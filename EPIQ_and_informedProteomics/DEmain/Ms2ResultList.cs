using System;
using System.Collections.Generic;

namespace Epiq
{
    public class Ms2ResultList : List<Ms2Result>
    {
        public Ms2ResultList(SearchResults searchResult, bool retainBestOutofSameUnlabeledPeptide = false)
        {
            if (retainBestOutofSameUnlabeledPeptide)
            {
                var results = new Dictionary<string, Dictionary<sbyte, Ms2Result>>(); // unmodifiedpeptide 
                var idIndex = 0;
                foreach (var sn in searchResult.GetScanNums())
                {
                    for (var i = 0; i < searchResult.GetNumOfSearchResults(sn); i++)
                    {
                        idIndex++;
                        var id = new Ms2Result(searchResult, sn, i, idIndex);
                        if (id.LabelIndex < 0) continue; //
                        if (id.IsContam()) continue;
                        if (id.IsotopeIdx > 2) continue;
                       
                        Dictionary<sbyte, Ms2Result> subResults;
                        //if ((Id.LabelIndex == oid.LabelIndex) || Id.Charge != oid.Charge || !oid.SameUnmodifiedPeptide(Id)) continue;
                        //Console.WriteLine(id.LabelIndex + " " + Params.ChannelToLabelMass[id.LabelIndex]);
                        if (!results.TryGetValue(id.UnlabeledPeptide, out subResults))
                            results[id.UnlabeledPeptide] = new Dictionary<sbyte, Ms2Result> { { id.Charge, id } };
                        else
                        {
                            Ms2Result result;
                            if (!subResults.TryGetValue(id.Charge, out result))
                                subResults[id.Charge] = id;
                            else if (result.SpecEValue > id.SpecEValue) subResults[id.Charge] = id;
                        }
                    }
                }
                foreach (var v in results.Values)
                    AddRange(v.Values);
            }
            else
            {
                var idIndex = 0;
                foreach (var sn in searchResult.GetScanNums())
                    for (var i = 0; i < searchResult.GetNumOfSearchResults(sn); i++)
                    {
                        idIndex++;
                        var id = new Ms2Result(searchResult, sn, i, idIndex);
                        if (id.LabelIndex < 0) continue; // 
                        if (id.IsContam()) continue;
                        if (id.IsotopeIdx > 1) continue;
                        Add(new Ms2Result(searchResult, sn, i, idIndex));
                    }
            }
            GetPsmAndProteinCount(Params.PsmQvalueThreshold);
        }


        private void GetPsmAndProteinCount(float fdrThreshold)
        {
            var pepScoreDic = new Dictionary<string, float>();
            foreach (var id in this)
            {
                var pep = id.UnlabeledPeptide + (id.IsDecoy() ? "X" : "");
                float score;
                if (!pepScoreDic.TryGetValue(pep, out score)) score = float.PositiveInfinity;
                score = Math.Min(score, id.Qvalue);
                pepScoreDic[pep] = score;
            }
            var pepDic = new Dictionary<float, List<string>>();
            foreach (var pep in pepScoreDic.Keys)
            {
                List<string> peps;
                var score = pepScoreDic[pep];
                if (pepDic.TryGetValue(score, out peps)) peps.Add(pep);
                else pepDic[score] = new List<string> {pep};
            }

            var scores = new List<float>();
            scores.AddRange(pepDic.Keys);
            scores.Sort();

            var numDecoy = 0;
            var numTarget = 0;
            var fdr = new float[scores.Count];
            for (var i = 0; i < scores.Count; i++)
            {
                var score = scores[i];
                foreach (var pep in pepDic[score])
                    if (pep.EndsWith("X")) numDecoy++;
                    else numTarget++;
                fdr[i] = Math.Min((float) numDecoy/(numDecoy + numTarget), 1.0f);
            }

            var qValue = new float[fdr.Length];
            qValue[fdr.Length - 1] = fdr[fdr.Length - 1];
            for (var i = fdr.Length - 2; i >= 0; i--)
                qValue[i] = Math.Min(qValue[i + 1], fdr[i]);

            var psmCounts = new int[10];
            var idedPsmCounts = new int[10];

            var proteinCount = 0;
            var proteinPsmCntrDictionary = new Dictionary<string, int>();
            var proteinPeptidesDictionary = new Dictionary<string, HashSet<string>>();
            var proteins = new HashSet<string>();
            var scanNumSet = new HashSet<int>();
            foreach (var id in this)
            {
                var score = id.Qvalue;
                var index = scores.BinarySearch(score);
                if (index < 0) index = ~index;
                var qval = qValue[Math.Min(index, qValue.Length - 1)];
                psmCounts[id.Charge]++;
                if (qval > fdrThreshold) continue;
                scanNumSet.Add(id.ScanNum);
                idedPsmCounts[id.Charge]++;
                foreach (var protein in id.Proteins)
                {
                    if (!proteinPsmCntrDictionary.ContainsKey(protein)) proteinPsmCntrDictionary[protein] = 0;
                    if (!proteinPeptidesDictionary.ContainsKey(protein))
                        proteinPeptidesDictionary[protein] = new HashSet<string>();
                    proteins.Add(protein);
                    proteinPsmCntrDictionary[protein]++;
                    proteinPeptidesDictionary[protein].Add(id.UnlabeledPeptide);
                }
            }

            foreach (var protein in proteins)
            {
                if (!proteinPsmCntrDictionary.ContainsKey(protein)) continue;
                if (!proteinPeptidesDictionary.ContainsKey(protein)) continue;
                if (proteinPsmCntrDictionary[protein] < Params.NumMatchedPsmsPerProtein) continue;
                if (proteinPeptidesDictionary[protein].Count < Params.NumMatchedPepsPerProtein) continue;
                proteinCount++;
            }
            Console.Write(@"# PSMs per charge (ided/total): ");
            for (var c = 0; c < idedPsmCounts.Length; c++)
            {
                if (idedPsmCounts[c] <= 0) continue;
                Console.Write(@" z {0} - {1}/{2} ", c, idedPsmCounts[c], psmCounts[c]);
            }
            Console.WriteLine(@",# Scan Numbers: {0} ,# Proteins: {1} at FDR threshold of {2}", scanNumSet.Count,
                proteinCount, fdrThreshold);
        }
    }
}