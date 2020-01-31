using System;
using System.Collections.Generic;
using System.Linq;
using InformedProteomics.Backend.Data.Composition;

namespace Epiq
{
    [Serializable]
    public class LabelList
    {
        private static List<Label[]> _labels; // labels
        private readonly float[] _massShifts;
        public static sbyte LabelCount { get; private set; }
        public List<sbyte> RtSortedCns = new List<sbyte>();
        
        public LabelList(Ms2Result id)
        {
            var labelsPerLabelIndex = new List<Label>[LabelNumberArr.Length];
            DetCounts = new sbyte[LabelNumberArr.Length];
            ImpurityIsotopomerEnvelopes = new IsotopomerEnvelope[LabelNumberArr.Length];
            _massShifts = new float[LabelNumberArr.Length];
            for (var cn = 0; cn < labelsPerLabelIndex.Length; cn++) labelsPerLabelIndex[cn] = new List<Label>();

            foreach (var labels in _labels)
                for (var i = 0; i < labels[id.LabelIndex].NumBound(id.Peptide); i++)
                    foreach (var label in labels)
                        labelsPerLabelIndex[label.Channel].Add(label);
            foreach (var lbs in labelsPerLabelIndex) lbs.Sort();

            NumOfBoundLabels = (sbyte) labelsPerLabelIndex[0].Count;
            UnlabeledPeptide = GetUnlabeledPeptide(id, labelsPerLabelIndex);

            var cnDetMassList = new List<Tuple<int, sbyte, float>>();

            for (var cn = 0; cn < labelsPerLabelIndex.Length; cn++)
            {
                DetCounts[cn] = (sbyte) GetDetCount((sbyte) cn, labelsPerLabelIndex);
                _massShifts[cn] = GetMassShift((sbyte) cn, labelsPerLabelIndex);
                ImpurityIsotopomerEnvelopes[cn] = GetImpurityIsotopomerEnvelope((sbyte)cn, labelsPerLabelIndex);
                cnDetMassList.Add(new Tuple<int, sbyte, float>(cn, DetCounts[cn], _massShifts[cn]));
            }
            foreach (var tup in cnDetMassList.OrderBy(x => x.Item2).ThenBy(x => x.Item3).ToList() )
            {
                RtSortedCns.Add((sbyte)tup.Item1);
            }
        }


        public static sbyte[] LabelNumberArr { get; private set; }
        public sbyte NumOfBoundLabels { get; private set; }
        public sbyte[] DetCounts { get; private set; }
        public string UnlabeledPeptide { get; private set; }
        public IsotopomerEnvelope[] ImpurityIsotopomerEnvelopes { get; private set; }
        public static List<string> DeuteratedLabelingSites { get; private set; }

        public static bool ParseLabels(string[] labelString)
        {
            _labels = new List<Label[]>();
            //_labeledAminoAcids = new HashSet<string>();

            DeuteratedLabelingSites = new List<string>();

            Console.WriteLine(@"Used Labels: ");
            foreach (var labelstr in labelString)
            {
                if (labelString.Length == 1 && !labelstr.Contains(' ')) return false;
                var token = labelstr.Split(' ');
                var labelSiteList = token[0].Split('|');
                LabelNumberArr = new sbyte[token.Length - 1];
                foreach (var labelSite in labelSiteList)
                {
                    var envs = IsotopeImpurityValues.GetEnv(labelSite[0]);

                    var deuterated = false;
                    var labels = new Label[LabelNumberArr.Length];
                    for (var i = 0; i < LabelNumberArr.Length; i++)
                    {
                        LabelNumberArr[i] = (sbyte) i;
                        var dCountMassPair = token[i + 1].Split('_');
                        var dCount = Convert.ToSByte(dCountMassPair[0]);
                        var mass = Convert.ToSingle(dCountMassPair[1]);
                        if (dCount > 0) deuterated = true; 

                        var label = new Label(labelSite[0], mass, dCount, (sbyte) i, envs[i]);
                        labels[i] = label;
                        Console.Write(label + @" ");
                    }
                    _labels.Add(labels);
                    if (deuterated && (labelSite != "^") && (labelSite !="$")) DeuteratedLabelingSites.Add(labelSite); // ignore N or C term since they are consistent.
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
            LabelCount = (sbyte)LabelNumberArr.Length;
            DeuteratedLabelingSites.Sort();
            Console.WriteLine("Considered deuterated sites: {0}", String.Join(", ", DeuteratedLabelingSites));
            Console.WriteLine();
            return true;
        }


        public string GetUnlabeledPeptide(Ms2Result id, List<Label>[] labelsPerChannel)
        {
            var pep = id.Peptide;
            foreach (var label in labelsPerChannel[id.LabelIndex])
                pep = label.GetUnlabeledPeptide(pep);
            return pep;
        }

        public static Dictionary<char, int> GetLabeledAaCounts(Ms2Result id)
        {
            var ret = new Dictionary<char, int>();
            foreach (var label in _labels)
            {
                ret[label[id.LabelIndex].AminoAcid] = label[id.LabelIndex].NumBound(id.Peptide); // Assumes all channel contains same number of label site.
            }
            return ret;
        }

        public static sbyte GetLabelIndex(Ms2Result id)
        {
            sbyte cn = -1;
            foreach (var labels in _labels)
            {
                foreach (var label in labels)
                {
                    if (label.NumBound(id.Peptide) == 0) continue;
                    cn = label.Channel;
                    break;
                }
                if (cn >= 0) break;
            }
            foreach (var labels in _labels)
                foreach (var label in labels)
                {
                    if (label.Channel != cn) continue;
                    if (label.HasUnbound(id.Peptide)) return -1;
                }

            return cn;
        }

        private IsotopomerEnvelope GetImpurityIsotopomerEnvelope(sbyte cn, List<Label>[] labelsPerChannel)
        {
            IsotopomerEnvelope env = null;
            foreach (var label in labelsPerChannel[cn])
            {
                env = env == null ? label.ImpurityIsotopeEnvelope : env.GetConvolutedEnvelope(label.ImpurityIsotopeEnvelope);
            }
            return env;
        }

        private int GetDetCount(sbyte cn, List<Label>[] labelsPerChannel)
        {
            //return _labelsPerChannel[cn].Aggregate(0, (current, l) => current + l.DetCount);
            return labelsPerChannel[cn].Aggregate(0, (current, label) => current + label.DetCount);
        }

        private float GetMassShift(sbyte cn, List<Label>[] labelsPerChannel)
        {
            return labelsPerChannel[cn].Aggregate(0f, (current, label) => current + label.MassShift);
        }

        public float GetMassDifference(sbyte fromCn, sbyte toCn)
        {
            return _massShifts[toCn] - _massShifts[fromCn];
        }

        public int GetDetDifference(sbyte fromCn, sbyte toCn)
        {
            return DetCounts[toCn] - DetCounts[fromCn];
        }
    }
}