using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Sequence;
using InformedProteomics.Backend.MassSpecData;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.Utils;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using Constants = InformedProteomics.Backend.Data.Biology.Constants;

namespace Epiq
{
    public class XicCluster : XicMatrix, IQuantifiable
    {
        #region vars

        public const float XicClusterScorePrecision = 5.0E-37f;
        private readonly LcMsRun _run;
        private bool _valid;
        private readonly double _initSignalWidth;
        public int TemplateDefinedChannelCount { private set; get; }
        public Template[] Templates { private set; get; }
        public List<XicMatrix> SignificantCoelutedPeaks { private set; get; }
        public Ms2Result Id { private set; get; }
        
        public string[] TrainingStringDictionary { private set; get; }
        private readonly InterpolatedXic[] _iXics;
        private readonly List<int> _activeMzBins;
        private Dictionary<Composition, float>[] _possibleIsotopeCompositions;
        //private readonly Dictionary<int, Spectrum> _snSpecDictionary;
      
        public static bool Debug = false;
        public XicMatrix[] TemplateRecs, SigPeakRecs;  // for debug
       // public XicMatrix DcRec;
        public XicMatrix Rec;//
        public float XicClusterScore { private set; get; }
        //public float Cosine { private set; get; }
        //IQuantifiable members
        public int LabelCount { get; private set; }
        public float SignalPower { get; private set; }
        public float NoisePower { get; private set; }
        public float[] Quantities { get; private set; }

        public int GetQuantifiedLabelCount()
        {
            var ret = 0;
            foreach (var quantity in Quantities)
            {
                if (Math.Abs(quantity) > 0.1) ret++;
            }
            return ret;
        }

        public float GetSnr()
        {
            return SignalPower / (NoisePower + 1e-6f);
        }
        
        #endregion

        #region Constructor/Initition and Class getter

        public static XicCluster GetXicCluster(Ms2Result id, Tolerance tolerance, LcMsRun run, bool quantify)
        {
            var qf = new XicCluster(id, tolerance, run, quantify, false);
            return qf._valid ? qf : null;
        }

        public static XicCluster GetXicClusterForTraining(Ms2Result id, Tolerance tolerance, LcMsRun run, bool quantify)
        {
            var qf = new XicCluster(id, tolerance, run, quantify, true);
            return qf._valid ? qf : null;
        }

        private XicCluster(Ms2Result id, Tolerance tolerance, LcMsRun run, bool quantify, bool forTraining)
        {
            Id = id;
            _run = run;
            LabelCount = LabelList.LabelCount;
            //var isotopomerEnvelopes = GetIostopomerEnvolopes();//
            IsotopomerEnvelope[] impurityIsotopomerEnvelopes;
            var isotopomerEnvelopes = GetIostopomerEnvolopesComplex(out impurityIsotopomerEnvelopes);
          
            int[,] mzBinNumberPerCnIi;
            var mzBinCount = GetMzBinCount(Params.IsotopeIndex.Length, out mzBinNumberPerCnIi);
            _activeMzBins = GetActiveRowIndices(mzBinNumberPerCnIi, isotopomerEnvelopes);

            float[] mzs;
            _iXics = GetInterpolatableXics(mzBinNumberPerCnIi, mzBinCount, tolerance, out mzs);
            Mzs = mzs;

            float[] ets;
            Templates = Template.GetTemplates(_iXics, id, mzs, isotopomerEnvelopes, impurityIsotopomerEnvelopes,
                mzBinNumberPerCnIi, run,
                forTraining ? Params.CosineThresholdforTraining : Params.CosineThresholdForFitting, forTraining, out ets,
                out _initSignalWidth);
            if (Templates == null || run.GetElutionTime(Id.ScanNum) < ets[0] || run.GetElutionTime(Id.ScanNum) > ets[ets.Length - 1]) return;
            
            Rts = ets;

            TemplateDefinedChannelCount = 0;
            foreach (var cn in LabelList.LabelNumberArr)
            {
                var extendedTempalte = Templates[cn];
                if (extendedTempalte == null) continue;
                TemplateDefinedChannelCount++;
                // Console.WriteLine(cn + ": " + extendedTempalte.ApexEt);
            }
           
            if (TemplateDefinedChannelCount < Params.TemplateDefinedChannelCountThreshold) return;
            if (quantify)
            {
                CalculateQuantity(mzBinNumberPerCnIi);
                //Console.WriteLine("Before : {0}", String.Join("\t", Quantities));
                Quantities = LabelingEfficiencyCorrection.CorrectQuantities(Quantities, id);
                //Console.WriteLine("After : {0}", String.Join("\t", Quantities));
                //Console.WriteLine();
            }
            if (forTraining) SetTrainingStringDictionary();
            _valid = true;

        }

        private IsotopomerEnvelope[] GetIostopomerEnvolopes()
        {
            var comp =
                new Sequence(Id.UnmodifiedPeptide, Params.AminoAcidSet)
                    .Composition + new Composition(0, Id.Charge + 1, 0, 1, 0); 

            _possibleIsotopeCompositions = IsotopeEnvelopeCalculator.CalculateIsotopeCompositions(comp);
            var isos = new IsotopomerEnvelope[LabelCount];
            for (sbyte cn = 0; cn < isos.Length; cn++)
            {
                var compcn = comp;
                /*
                if (Params.PerLabelIsotopeComposition != null)
                {
                    for (var k = 0; k < Id.NumOfBoundLabels; k++)
                        compcn += Params.PerLabelIsotopeComposition[cn];
                }
                 */
                isos[cn] = IsotopeEnvelopeCalculator.GetIsotopomerEnvelop(compcn);
                // Math.Min(Params.IsotopeIndex.Length, massDiff + 2)); //
            }
            return isos;
        }

        private IsotopomerEnvelope[] GetIostopomerEnvolopesComplex(out IsotopomerEnvelope[] impurityIsotopomerEnvelopes)
        {
            var comp =
                new Sequence(Id.UnmodifiedPeptide, Params.AminoAcidSet)
                    .Composition + new Composition(0, Id.Charge + 2, 0, 1, 0);

            _possibleIsotopeCompositions = IsotopeEnvelopeCalculator.CalculateIsotopeCompositions(comp);
            var isos = new IsotopomerEnvelope[LabelCount];
            impurityIsotopomerEnvelopes = new IsotopomerEnvelope[LabelCount];
            for (sbyte cn = 0; cn < isos.Length; cn++)
            {
                var compcn = comp;
                /*if (Params.PerLabelIsotopeComposition != null)
                {
                    for (var k = 0; k < Id.NumOfBoundLabels; k++)
                        compcn += Params.PerLabelIsotopeComposition[cn];
                }*/
               /* if (cn == Id.LabelIndex)
                {
                    Console.WriteLine(Id.Precursor * Id.Charge - Id.IsotopeIdx);    
                    Console.WriteLine(compcn.Mass + 2 * cn * Id.NumOfBoundLabels * Atom.Get("2H").Mass + @"* " + cn);
                }
                */
                
                //Console.WriteLine(comp.ToPlainString() + " " + compcn.ToPlainString());
                impurityIsotopomerEnvelopes[cn] = Id.GetImpurityIsotopomerEnvelope(cn);
                isos[cn] = IsotopeEnvelopeCalculator.GetIsotopomerEnvelop(compcn);
                
                // Math.Min(Params.IsotopeIndex.Length, massDiff + 2)); //
            }
            return isos;
        }

        private int GetMzBinCount(int iiLength, out int[,] binNumbers)
        {
            var bins = new List<int>();
           // var maxIsotopeLength = 0;
          
            foreach (var cn in LabelList.LabelNumberArr)
            {
                for (sbyte ii = 0; ii < iiLength; ii++)
                {
                    var massDiffs = GetMassDiffs(cn, ii);
                    var bin = Constants.GetBinNum(massDiffs.Mean());
                    if (bins.Contains(bin)) continue;
                    bins.Add(bin);
                    //
                }
           //     maxIsotopeLength = Math.Max(maxIsotopeLength, iiLength);
            }
            bins.Sort();
            binNumbers = new int[LabelList.LabelNumberArr.Length,iiLength];

            foreach (var cn in LabelList.LabelNumberArr)
            {
                for (sbyte ii = 0; ii < iiLength; ii++)
                {
                    var massDiffs = GetMassDiffs(cn, ii);
                    var bin = bins.IndexOf(Constants.GetBinNum(massDiffs.Mean()));
                    binNumbers[cn, ii] = bin;
                }
                //Console.Write(binNumbers[cn, 0] + " "); // 
            }
            //Console.WriteLine();
            return bins.Count;
        }

        private float[] GetMassDiffs(sbyte toCn, sbyte toIi)
        {
            var massDiff = Id.GetMassDifference(0, toCn);//  numLables * (Params.ChannelToLabelMass[toCn] - Params.ChannelToLabelMass[0]);
            //return new []{channelDiff + (float)Constants.C13MinusC12* toIi};
            
            var isoCompositions = _possibleIsotopeCompositions[toIi].Keys;
            var ret = new float[isoCompositions.Count];
            var i = 0;
            foreach (var im in isoCompositions)
            {
                ret[i++] = (float)(massDiff + im.Mass);
            }

            return ret;
        }



        #endregion

        #region iXic define

        private Xic GetRecalibratedBaseLineCorrectedXic(HashSet<float> targetMzs, int targetSn, Tolerance tolerance, out float centerMz)
        {
            //centerMz = .0f;
            var minMz = float.PositiveInfinity;
            var maxMz = 0f;//float.NegativeInfinity;
            foreach (var mz in targetMzs)
            {
                minMz = Math.Min(minMz, mz);
                maxMz = Math.Max(maxMz, mz);
            }
            centerMz = targetMzs.Median();
            var tol = tolerance.GetToleranceAsTh(centerMz);
            var rtRange = new[] { _run.GetElutionTime(targetSn) - Params.MaxFeatureSpan, _run.GetElutionTime(targetSn) + Params.MaxFeatureSpan };

            var rawXic = _run.GetPrecursorExtractedIonChromatogram(minMz - tol, maxMz + tol, targetSn, rtRange);
            var xic = new Xic();
            foreach (var xp in rawXic)
            {
                xic.Add(new XicPoint(xp.ScanNum, xp.Mz, xp.Intensity));
            }

            if (xic.Count < Params.MinXicPointCntrInXic)
                return null;
            var xicMzs = new List<float>();
           
            var apexIndex = xic.BinarySearch(xic.GetNearestApex(Id.ScanNum));
            for (var i = apexIndex - 5; i < apexIndex + 5; i++)
            {
                if (i < 0 || i > xic.Count - 1) continue;
                xicMzs.Add((float)xic[i].Mz);
            }

            centerMz = xicMzs.Count > 0? (float)xicMzs.Mean() : centerMz;
            
           // 
            /*
            var baseLineDictionary = new Dictionary<int, Dictionary<double,float>>();

            for (var i=0;i<xic.Count;i++)
            {
                var xp = xic[i];
                float b;
                Dictionary<double, float> baseLindSubDictionary;
                if (!baseLineDictionary.TryGetValue(xp.ScanNum, out baseLindSubDictionary))
                {
                   baseLineDictionary[xp.ScanNum] = baseLindSubDictionary = new Dictionary<double, float>();
                }

                 if (!baseLindSubDictionary.TryGetValue(xic[i].Mz, out b))
                    {
                     var spec = _run.GetSpectrum(xp.ScanNum);
                     baseLindSubDictionary[xic[i].Mz] = b = spec == null? 0f : (float)spec.GetBaseLineAround(xic[i].Mz);
                    }

                // var baseLineCorrectedInt = xp.Intensity > b*2? xp.Intensity : xp.Intensity - b;
                //xic[i] = new XicPoint(xp.ScanNum, xp.Mz, baseLineCorrectedInt); 
            }*/
           
          //  var xicIntensities = new List<double>();
           // foreach (var t in xic)
           // {
           //     xicIntensities.Add(t.Intensity);
           // }
           // xicIntensities.Add(xicIntensities.Count == 0 ? 0 : xicIntensities.Median() * .1);           
           
            
           // var dc = xicIntensities.Count == 0? 0 : xicIntensities.Min();//xic.Aggregate(float.MaxValue, (current, t) => (float)Math.Min(current, t.Intensity));
           // if (dc > 0) { 
            //var max = xicIntensities.Count == 0 ? 1 : xicIntensities.Max();
           // for (var i = 0; i < xic.Count; i++)
            //{
                //xic[i] = new XicPoint(xic[i].ScanNum, xic[i].Mz, xic[i].Intensity - dc); 
           // }
            
            RemoveOverlappingXicPoints(xic, centerMz);

            
            return xic;//.GetSmoothedXic();
        }

        private HashSet<float> GetTargetMzs(float mz, int fromBin, int toBin, Dictionary<int, HashSet<Tuple<sbyte, sbyte>>> binToCnIis)
        {

            var fromCnIi = binToCnIis[fromBin];
            var toCnIi = binToCnIis[toBin];
            var ret = new HashSet<float>();
            
            foreach (var cnii in fromCnIi)
            {
                foreach (var cnii2 in toCnIi)
                {
                    var fromMassDiffs = GetMassDiffs(cnii.Item1, cnii.Item2);
                    var toMassDiffs = GetMassDiffs(cnii2.Item1, cnii2.Item2);
                    foreach (var r in fromMassDiffs)
                    {
                        foreach (var s in toMassDiffs)
                        {
                            ret.Add(mz + (s - r) / Id.Charge);
                        }
                    }
                }
            }
            return ret;
        }


        private InterpolatedXic[] GetInterpolatableXics(int[,] mzBinNumberPerCnIi, int mzBinCount, Tolerance tolerance, out float[] mzs)
        {
            var xis = new InterpolatedXic[mzBinCount];
            mzs = new float[mzBinCount];
            var idbin = mzBinNumberPerCnIi[Id.LabelIndex, Id.IsotopeIdx];

            var binToCnIis = new Dictionary<int, HashSet<Tuple<sbyte, sbyte>>>();
            foreach (var cn in LabelList.LabelNumberArr)
            {
                for (var ii = 0; ii < mzBinNumberPerCnIi.GetLength(1); ii++)
                {
                    var bin = mzBinNumberPerCnIi[cn, ii];
                    HashSet<Tuple<sbyte, sbyte>> cniiSet;
                    if (!binToCnIis.TryGetValue(bin, out cniiSet)) cniiSet = binToCnIis[bin] = new HashSet<Tuple<sbyte, sbyte>>();
                    cniiSet.Add(new Tuple<sbyte, sbyte>(cn, (sbyte)ii));
                }
            }
            
            foreach (var bin in _activeMzBins)
            {
                var targetMzs = GetTargetMzs(Id.Precursor, idbin, bin, binToCnIis);
                float centerMz;
                var xic = GetRecalibratedBaseLineCorrectedXic(targetMzs, Id.ScanNum, tolerance, out centerMz);
                mzs[bin] = centerMz;
                if (xic == null || xic.Count < Params.MinXicPointCntrInXic) continue;
                xis[bin] = new InterpolatedXic(xic, _run);
            }
           
            return xis;
        }



        private static void RemoveOverlappingXicPoints(Xic xic, float targetMz)
        {
            if (xic.Count < 1) return;
            var prevSn = xic[0].ScanNum;

            for (var i = 1; i < xic.Count; i++)
            {
                var currSn = xic[i].ScanNum;
                var snDiff = currSn - prevSn;
                prevSn = currSn;
                if (snDiff != 0) continue;
               // xic.RemoveAt(i - 1);
                if (xic[i - 1].Equals(xic[i]))
                {
                    xic.RemoveAt(i - 1);
                }
                else
                {
                    var prevMz = xic[i - 1].Mz;
                    var currMz = xic[i].Mz;
                    var pickCurr = Math.Abs(targetMz - prevMz) > Math.Abs(targetMz - currMz);
                    var selectedIntensity = xic[i].Intensity + xic[i - 1].Intensity;
                    var mz = pickCurr ? currMz : prevMz;
                    xic.RemoveAt(i - 1);
                    xic.RemoveAt(i - 1);
                    xic.Insert(i - 1, new XicPoint(currSn, mz, selectedIntensity));
                }
                i--;
            }
        }

        #endregion

        #region Matrix definition

        private void SetMatrix(int[,] bins, out Dictionary<int, Matrix<float>> templateSingalMatrixDictionary, out Matrix<float> signalMatrix)//int[,] bins, out Matrix<float> signalMatrix
        {
            M = Matrix<float>.Build.Dense(Mzs.Length, Rts.Length);
            signalMatrix = Matrix<float>.Build.Dense(Mzs.Length, Rts.Length, 0);
            templateSingalMatrixDictionary = new Dictionary<int, Matrix<float>>();
            var sigRange = new float[Mzs.Length][];
            
            foreach (var cn in LabelList.LabelNumberArr)
            {
                if (Templates[cn] == null) continue;
                var start = Templates[cn].SignalStart;//.SignalStart + ExtendedTemplates[cn].SignalWidth / 10;
                var end = Templates[cn].SignalStart + Templates[cn].SignalWidth;//ExtendedTemplates[cn].SignalStart + Templates[cn].SignalWidth - Templates[cn].SignalWidth / 10;

                var m = Matrix<float>.Build.Dense(Mzs.Length, Rts.Length, 0);
                
                for (var ii = 0; ii<bins.GetLength(1);ii++)
                {
                    var mzBin = bins[cn, ii];
                    if (sigRange[mzBin] == null)
                    {
                        sigRange[mzBin] = new float[2];
                        sigRange[mzBin][0] = float.PositiveInfinity;
                    }
                    sigRange[mzBin][0] = Math.Min(sigRange[mzBin][0], start);
                    sigRange[mzBin][1] = Math.Max(sigRange[mzBin][1], end);
                   
                    for (var etBin = 0; etBin < Rts.Length; etBin++)
                    {
                        if (Rts[etBin] < start) continue;
                        if (Rts[etBin] > end) break;

                        m[mzBin, etBin] = 1;
                    }
                }
                templateSingalMatrixDictionary[cn] = m;
            }

            
            foreach (var mzBin in _activeMzBins)
            {
                if (sigRange[mzBin] == null) continue;
                if (_iXics[mzBin] == null) continue;
                //Console.WriteLine(@"{0} {1} {2} {3}", Ets[0], Ets[Ets.Length-1], sigRange[mzBin][0], sigRange[mzBin][1]);

                for (var etBin = 0; etBin < Rts.Length; etBin++)
                { 
                    if (Rts[etBin] < sigRange[mzBin][0]) continue;
                    if (Rts[etBin] > sigRange[mzBin][1]) break;                  
                    signalMatrix[mzBin, etBin] = 1;
                }
            }
             
            //for (var mzBin = 0; mzBin < Mzs.Length; mzBin++)
            foreach (var mzBin in _activeMzBins)
            {
                if (sigRange[mzBin] == null) continue;
                var iXic = _iXics[mzBin];
                if (iXic == null) continue;
               // var dc = iXic.Ymin;// iXic.InterpolateAt(sigRange[mzBin][0]); // 
                AddInterpolatable(iXic, mzBin, sigRange[mzBin]);
            }
        }

        private void SetSignificantCoelutedPeaks(List<int> activeMzBins, int maxCountPerMzBin = 3)
        {
            SignificantCoelutedPeaks = new List<XicMatrix>();

            var delta = (Rts[1] - Rts[0]) * 2;

            foreach (var mzBin in activeMzBins)
            {
                var iXic = _iXics[mzBin];
                if (iXic == null) continue;
                var signalRange = new[] { float.PositiveInfinity, float.NegativeInfinity };
                foreach (var extendedTemplate in Templates)
                {
                    if (extendedTemplate == null) continue;
                    var r = extendedTemplate.GetSignalRange(mzBin, .25f);
                    if (r == null || r[0] < 0) continue;
                    signalRange[0] = Math.Min(signalRange[0], r[0]);
                    signalRange[1] = Math.Max(signalRange[1], r[1]);
                }
                if (float.IsPositiveInfinity(signalRange[0])) continue;

                var apexEts = iXic.FindApexEts(Rts, signalRange, .25f, maxCountPerMzBin);
                var ts = new List<XicMatrix>();
                var tmpApexEtList = new List<float>();
                foreach (var et in apexEts)
                {
                    float tmpApexEt;
                    var sxm = GetSignificantXicPeakMatrix(iXic, et, mzBin, Mzs, Rts, signalRange, Params.CosineThresholdForCoelutionFitting, out tmpApexEt);
                    if (sxm == null) continue;
                    tmpApexEtList.Add(tmpApexEt);
                    ts.Add(sxm);

                }
                tmpApexEtList.Sort();
                if (ts.Count == 0) continue;
                var prevTmpApexEt = tmpApexEtList[0];

                SignificantCoelutedPeaks.Add(ts[0]);
                for (var j = 1; j < tmpApexEtList.Count; j++)
                {
                    if (Math.Abs(tmpApexEtList[j] - prevTmpApexEt) < delta) continue;
                    SignificantCoelutedPeaks.Add(ts[j]);
                    prevTmpApexEt = tmpApexEtList[j];
                }
            }
        }
        #endregion

        #region Quantity calculation

        private Matrix<float> GetLeftMatrix(List<int> activeMzBinsIndices, Matrix<float> signalMatrix, Dictionary<int, Matrix<float>> templateSingalMatrixDictionary, 
            out List<Vector<float>> eVectors, out List<Vector<float>> sVectors, out List<Vector<float>> dcVectors, out List<int> evectorCns)
        {
            eVectors = new List<Vector<float>>();
            sVectors = new List<Vector<float>>();
            dcVectors = new List<Vector<float>>();
            //SetInterpolatedTotalIntensities();
            
            //_iXics;

            //foreach (var m in templateSingalMatrixDictionary.Values)
            //GetDcComponentMatrix(Mzs, Rts, 1f);
            evectorCns = new List<int>();
            //var dcv = GetDcComponentMatrix(Mzs, Rts, 0f).ToVector(activeMzBinsIndices, signalMatrix);
            for (var cn = 0; cn < Templates.Length; cn++)
            {
                var extendedTemplate = Templates[cn];
                if (extendedTemplate == null) continue;
                var etv = extendedTemplate.ToVector(activeMzBinsIndices, signalMatrix);
                if (etv.AbsoluteMaximum() <= 0) continue;
                eVectors.Add(etv);
                evectorCns.Add(cn);

                var m = templateSingalMatrixDictionary[cn];
                //dcv += GetDcComponentMatrix(Mzs, Rts, 1f).ToVector(activeMzBinsIndices, m);
            }
            //dcVectors.Add(dcv);

            foreach (var significantXicPeak in SignificantCoelutedPeaks)
            {
                var spv = significantXicPeak.ToVector(activeMzBinsIndices, signalMatrix);
                if (spv.AbsoluteMaximum() <= 0) continue;
                sVectors.Add(spv);
            }
            /*if (addDc)
            {
                for (var mzBin = 0; mzBin < Mzs.Length; mzBin++) //
                {
                    if (!activeMzBinsIndices.Contains(mzBin)) continue;

                    var dc = GetDcComponentMatrix(mzBin, Mzs, Rts, 1f); // .2 perfect for ln == 2
                    var dcv = dc.ToVector(activeMzBinsIndices, signalMatrix);
                    if (dcv.AbsoluteMaximum() <= 0) continue;
                    dcVectors.Add(dcv); // 
                }
            }
            
            /*
            for (var etBin = 0; etBin < Ets.Length; etBin++) //
            {
                //if (!activeMzBinsIndices.Contains(mzBin)) continue;

                var dc = GetDcComponentMatrixAlongMz(etBin, Mzs, Ets, .01f); // .2 perfect for ln == 2
                var dcv = dc.ToVector(activeMzBinsIndices, signalMatrix);
                if (dcv.AbsoluteMaximum() <= 0) continue;
               // dcVectors.Add(dcv); // 
            }*/
            //if(addDc) 
            //dcVectors.Add(GetDcComponentMatrix(Mzs, Rts, 1f).ToVector(activeMzBinsIndices, signalMatrix));
            
            var columnCount = eVectors.Count + sVectors.Count + dcVectors.Count;
            if (eVectors.Count<=0 || columnCount <= 0) return null;
            var leftMatrix = Matrix<float>.Build.Dense(eVectors[0].Count, columnCount);
            var columnIndex = 0;

            foreach (var v in eVectors) leftMatrix.SetColumn(columnIndex++, v);
            foreach (var v in sVectors) leftMatrix.SetColumn(columnIndex++, v);
            foreach (var v in dcVectors) leftMatrix.SetColumn(columnIndex++, v);

            foreach (var v in leftMatrix.Enumerate())
            {
                if (float.IsNaN(v)) return null;
            }

            return leftMatrix;
        }
        

        public List<int> GetActiveRowIndices(int[,] mzBinNumberPerCnIi, IsotopomerEnvelope[] isotopomerEnvelopes)
        {
            var ars = new HashSet<int>();
            foreach (var cn in LabelList.LabelNumberArr)
            {
                var ai = isotopomerEnvelopes[cn].MostAbundantIsotopeIndex;
                var iiset = new HashSet<int> { ai };
                if (ai == 0)
                {
                    iiset.Add(ai + 1);
                }
                else
                {
                    var env = isotopomerEnvelopes[cn].Envelope;
                    if (ai >= env.Length - 1 || env[ai - 1] > env[ai + 1]) iiset.Add(ai - 1);
                    else iiset.Add(ai + 1);
                }
                for (var ii = 0;ii < isotopomerEnvelopes[cn].Envelope.Length;ii++)
                {
                    var bin = mzBinNumberPerCnIi[cn, ii];
                    if (!iiset.Contains(ii)) continue;
                    //if (ii != isotopomerEnvelopes[cn].MostAbundantIsotopeIndex) continue;
                    //if (M!=null && M.Row(bin).AbsoluteMaximum() > 0)
                    ars.Add(bin);
                }
            }
            var arsList = ars.ToList();
            arsList.Sort();
            return arsList;
        }

        private void CalculateQuantity(int[,] mzBinNumberPerCnIi)
        {
            if (_activeMzBins.Count == 0) return;
            Matrix<float> signalMatrix;
            Dictionary<int, Matrix<float>> templateSingalMatrixDictionary;
            SetMatrix(mzBinNumberPerCnIi, out templateSingalMatrixDictionary, out signalMatrix);
            List<Vector<float>> eVectors, sVectors, dcVectors;

            var observed = ToVector(_activeMzBins, signalMatrix);
            SetSignificantCoelutedPeaks(_activeMzBins);
            Quantities = new float[LabelList.LabelNumberArr.Length];

            List<int> eVectorCns;
            var leftMatrix = GetLeftMatrix(_activeMzBins, signalMatrix, templateSingalMatrixDictionary, out eVectors, out sVectors, out dcVectors, out eVectorCns);
            if (leftMatrix == null) return;
            
            var inv = MatrixCalculation.MoorePenrosePsuedoinverse(leftMatrix);
            if (inv == null) return;
            var quantityVector = inv*observed;
           
            //var quantityVector = MatrixCalculation.NonNegativeLeastSquare(leftMatrix, observed);
           
            /*var qs = quantityVector.SubVector(0, eVectors.Count);
            if (qs.Min() <= 0)
            
                quantityVector = MatrixCalculation.NonNegativeLeastSquare(leftMatrix, observed);
           
            */
            for (var i = 0; i < eVectorCns.Count; i++)
            {
                var cn = eVectorCns[i];
                //var dc = quantityVector[eVectors.Count + sVectors.Count];
                var q = quantityVector[i];// + quantityVector[eVectors.Count + sVectors.Count];
                //if (q <= 0 && dc >= 0 || q >= 0 && dc <= 0) // q <= 0 && 
                {
                //    if (q + dc > 0)
                //        q += dc; //
                }
                if (q < 0) q = 0;

                //var m = templateSingalMatrixDictionary[cn];
                //var mv = GetDcComponentMatrix(Mzs, Rts, 1f).ToVector(_activeMzBins, m);
                
                //var dcR = dcVectors[0].PointwiseMultiply(mv).Sum();
                Quantities[cn] = q * Templates[cn].Area; //
                if (float.IsNaN(Quantities[cn])) return;

                /*if (Quantities[cn] <= 0 && dcR >= 0 || Quantities[cn] >= 0 && dcR <= 0)
                {
                    if (Quantities[cn] + dcR > 0)
                        Quantities[cn] += dcR; //
                }
                if (Quantities[cn] < 0) Quantities[cn] = 0;
                */

            }
       
            var recv = leftMatrix * quantityVector;
            var error = recv - observed;
       
            NoisePower = error.PointwisePower(2).Sum();// / CountNonZero(error);
            SignalPower = .0f;
            /*var sigRec = eVectors[0] * quantityVector[0];
            for (var i=1;i<eVectors.Count;i++)
            {
                sigRec += eVectors[i]*quantityVector[i];
            }*/
            /*if (dcVectors != null)
            {
                for (var j = 0; j < dcVectors.Count; j++)
                {
                    sigRec += dcVectors[j] * quantityVector[j + eVectors.Count + sVectors.Count];
                }
            }*/
            SignalPower = recv.PointwisePower(2).Sum();//sigRec.PointwisePower(2).Sum();

            if(IsQuantified()) _valid = true;//Quantities.Sum() > Params.MinQfeatureMeanQuantity;//true;//Snr >= Params.PeptideProteinSnrThreshold;
            if (!Debug) return;

            Rec = GetXicMatrixFromVector(recv, Rts, Mzs, _activeMzBins);
       
            TemplateRecs = new XicMatrix[Templates.Length];
            for (var i=0; i < eVectorCns.Count;i++)
            {
                var cn = eVectorCns[i];
                var extendedTemplate = Templates[cn];
                if (extendedTemplate == null) continue;
                var templateRecv = Vector<float>.Build.Dense(recv.Count);
                templateRecv += leftMatrix.Column(i) * quantityVector[i];
                TemplateRecs[cn] = GetXicMatrixFromVector(templateRecv, Rts, Mzs, _activeMzBins);
            }

            SigPeakRecs = new XicMatrix[sVectors.Count];
            var sci = eVectors.Count;
            for (var i = 0; i < sVectors.Count; i++)
            {
                var sigPeakRecv = leftMatrix.Column(sci)*quantityVector[sci];
                sci++;
                SigPeakRecs[i] = GetXicMatrixFromVector(sigPeakRecv, Rts, Mzs, _activeMzBins);
                
            }
        }

        

        #endregion

        #region MS2Score Calculation

        public void SetXicClusterScore(float ms2Score)
        {
            XicClusterScore = ms2Score;
        }


        public void SetXicClusterScore(List<Ms2Result> ids) // ids contains same unlabeled peptides
        {
            XicClusterScore = (float)Math.Log10(Id.Qvalue + 1e-3f);
            //var et = _run.GetElutionTime(Id.ScanNum);
            foreach (var oid in ids)
            {
                if (oid.IsotopeIdx > 1) continue;
                if ((Id.LabelIndex == oid.LabelIndex) || Id.Charge != oid.Charge) continue;
                var oet = _run.GetElutionTime(oid.ScanNum);
                if (oet > Rts[Rts.Length-1] || oet < Rts[0]) continue;
                if (Id.UnlabeledPeptide != oid.UnlabeledPeptide && Id.ArePeptidesTheSameExceptIle(oid)) continue;

                XicClusterScore += (float)Math.Log10(oid.Qvalue + 1e-3f);
                //XicClusterScore += (float)(Math.Log10(oid.SpecEValue+1e-100) - Math.Log10(Params.InitSpecEValueThreshold));
            }
            
        }

        #endregion

        #region Write & Setter

  
        public void WriteQuantities(StreamWriter sw, string var)
        {
            sw.WriteLine(@"clear " + var + @";");
            sw.Write(var + @"=[");
            foreach (var q in Quantities) sw.Write(q + ",");
            sw.WriteLine(@"];");
        }

        public void WriteRatio(StreamWriter sw, string var)
        {
            sw.WriteLine(@"clear " + var + @";");
            sw.Write(var + @"=[");
            foreach (var r in GetRatios()) sw.Write(r + ",");
            sw.WriteLine(@"];");
        }
        public void WriteXicPoints(StreamWriter sw, string var)
        {
            var xic = _run.GetPrecursorExtractedIonChromatogram(Mzs[0] - 2.1, Mzs[Mzs.Length - 1] + 2.1);
            sw.WriteLine(@"clear " + var + @";");
            sw.Write(var + @"=[");
            XicPoint ms2Xp = null;
            var iw = _run.GetIsolationWindow(Id.ScanNum);
            foreach (var x in xic)
            {
                var et = _run.GetElutionTime(x.ScanNum);
                if (x.ScanNum == _run.GetPrecursorScanNum(Id.ScanNum) && (iw.MinMz < x.Mz && iw.MaxMz > x.Mz)) { ms2Xp = x; }
                if (et < Rts[0] - .1 || et > Rts[Rts.Length - 1] + .1) continue;
                sw.Write(x.Mz + @"," + et + @"," + x.Intensity + @";");
            }
            sw.WriteLine(@"];");
            if (ms2Xp == null) return;
            sw.WriteLine(@"clear " + var + @"_ms2;");
            sw.Write(var + @"_ms2=[");
            sw.Write(ms2Xp.Mz + @"," + _run.GetElutionTime(ms2Xp.ScanNum) + @"," + ms2Xp.Intensity + @"," + iw.IsolationWindowTargetMz + @"," + Id.LabelIndex + @"," + Id.IsotopeIdx + @"," + Id.SpecEValue + @"," + Id.Precursor + @";");
            sw.WriteLine(@"];");
        }



        public void Write(StreamWriter sw)
        {
            //var sw = new StreamWriter(ResultsPath + @"QFM" + cntr++ + @".m");
            sw.WriteLine(@"clear qfm;");
            sw.WriteLine(@"clear xic;");
            sw.WriteLine(@"clear quantity;");
            sw.WriteLine(@"clear etmps;");
            sw.WriteLine(@"clear sigpeaks;");
            sw.WriteLine(@"clear rec;");
            sw.WriteLine(@"clear snr;");
            sw.WriteLine(@"snr=" + GetSnr() + @";");
            sw.WriteLine(@"templateCount=" + TemplateDefinedChannelCount + @";");
            sw.WriteLine(@"labelINdex=" + Id.LabelIndex + @";");

            //sw.WriteLine(@"bin=" + BinNumber + @";");

            Write(sw, @"qfm");
            WriteXicPoints(sw, @"xic");
            WriteQuantities(sw, @"quantity");
            WriteRatio(sw, @"ratio");

            sw.WriteLine(@"tempInfo=[");

            foreach (var t in Templates)
            {
                if (t == null) continue;
                sw.Write(t.Area + @"," + t.Sigma + @";");
            }
            sw.WriteLine(@"];");

            if (TemplateRecs != null)
            {
                for (var cn = 0; cn < TemplateRecs.Length; cn++)
                {
                    if (Templates[cn] == null) continue;
                    var templateRec = TemplateRecs[cn];
                    if (templateRec == null) continue;
                    templateRec.Write(sw, @"etmps{" + (cn + 1) + @"}");
                }
              //  ExtendedTemplates[0]

            }
            if (SigPeakRecs != null)
            {
                sw.WriteLine(@"sigpeaks={};");
                for (var mz = 0; mz < SigPeakRecs.Length; mz++)
                {
                    var sigPeakRec = SigPeakRecs[mz];
                    if (sigPeakRec == null) continue;
                    sigPeakRec.Write(sw, @"sigpeaks{" + (mz + 1) + @"}");
                }
            }
            if (Rec != null) Rec.Write(sw, @"rec");
           // if (DcRec!=null) DcRec.Write(sw, @"dcrec");
            sw.Close();
        }
        #endregion

        #region Training

        private void SetTrainingStringDictionary()
        {
            if (Templates[0] == null)
            {
                //Console.WriteLine("ExtTemplate null");
                return;
            }

            TrainingStringDictionary = new string[LabelList.LabelNumberArr.Length];
            var peptide = Id.UnmodifiedPeptide;
            var aaProportions = DShift.GetDshiftAaProportions(peptide);
            var initQxicEt = _run.GetElutionTime(Id.ScanNum);
            //var averageNormedSignalWidth = DShift.GetAverageNormedSignalWidth(ExtendedTemplates);

            foreach (var cn in LabelList.LabelNumberArr)
            {
                var ext = Templates[cn];
                if (ext == null) continue;

                //var predictedRtOffSet = DShift.GetRtOffset(Id, cn, _run.GetElutionTime(Id.ScanNum));
                //var measuredRtOffSet = Templates[0].RtPosition - ext.RtPosition; // Normalized ApexEt Diff
                //Console.WriteLine("{0}\t{1}\t{2}\t{3}", Id.Peptide, initQxicEt, predictedRtOffSet, measuredRtOffSet);

                var outStr = new StringBuilder();
                outStr.Append(peptide);
                outStr.Append('\t');
                outStr.Append(Id.Charge);
                outStr.Append('\t');
                outStr.Append(Id.SpecEValue);
                outStr.Append('\t');
                outStr.Append(cn);
                outStr.Append('\t');
                outStr.Append(ext.Cosine);
                outStr.Append('\t');
                outStr.Append((Templates[0].RtPosition - ext.RtPosition) / Params.RtSpan); // Normalized ApexEt Diff
                outStr.Append('\t');
                outStr.Append(Id.GetDetCount(cn));
                outStr.Append('\t');
                outStr.Append(DShift.GetNormedEt(initQxicEt));
                outStr.Append('\t');
                outStr.Append(peptide.Length);
                foreach (var aaPro in aaProportions)
                {
                    outStr.Append('\t');
                    outStr.Append(aaPro);
                }
                /*
                outStr.Append('\t');
                outStr.Append(ExtendedTemplates[0].ApexEt);
                outStr.Append('\t');
                outStr.Append(ext.ApexEt);
                outStr.Append('\t');
                outStr.Append(Params.GetNormedEt(ExtendedTemplates[0].ApexEt) - Params.GetNormedEt(ext.RtPosition));
                 */
                TrainingStringDictionary[cn] = outStr.ToString();
            }
        }

        public string GetDShiftTrainingString(sbyte channelNumber)
        {
            return TrainingStringDictionary == null ? null : TrainingStringDictionary[channelNumber];
        }


        #endregion


        public bool IsFullyQuantified()
        {
            return Quantities.Min() > 0;
        }

        public bool IsQuantified()
        {
            return Quantities.Max() > 0;
        }

        public float[] GetRatios()
        {
            var sum = Quantities.Sum();
            if (sum <= 0) return Quantities;
            var ratio = new float[LabelCount];
            for (var i = 0; i < ratio.Length; i++)
            {
                ratio[i] = Quantities[i] / sum;
            }
            return ratio;
        }
    }
}