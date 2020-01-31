using System.Collections.Generic;
using System.Threading.Tasks;

namespace Epiq
{
    public class ProteinGroupQuantification
    {
        public static void UpdateDictionary(QuantifiedProteinGroupDictionary proteinGroupDictionary, QuantifiedPsmList psmList, short condition, short replicate = 0, bool assignProteinToPsm = false)
        {
           // var proteinGroupDictionary = new QuantifiedProteinGroupDictionary();
           
            List<QuantifiedProtein> proteinList;
            Dictionary<string, int> proteinNameDictionary;

            var bipartiteGraph = GetProteinList(psmList, Params.RatioSimilarityCosineThreshold, out proteinList, out proteinNameDictionary);
            PruneBipartiteGraph(psmList, proteinList, proteinNameDictionary, bipartiteGraph);
            proteinGroupDictionary.AddAndAssignMatchedProteinsToPsm(psmList, condition, replicate, proteinList, bipartiteGraph, Params.NumMatchedPsmsPerProtein, Params.NumMatchedPepsPerProtein, Params.ProteinQvalueThreshold, Params.SnrThreshold, Params.SnrThresholdForSingleHitProtein, assignProteinToPsm);
            // per condition list making then merge. psms should be updated accordingly... psms with c index -1 (?) are the psms matched to merged..?
            //return proteinGroupDictionary;
        }

        // no psm or protein lists are modified
        private static void PruneBipartiteGraph(List<QuantifiedPsm> psmList, List<QuantifiedProtein> proteinList, Dictionary<string, int> proteinNameDictionary, bool[][] adjMatrix)
        {
            Parallel.ForEach(proteinList, new ParallelOptions { MaxDegreeOfParallelism = Params.MaxParallelThreads },
              protein =>
              {
                  var i = proteinList.IndexOf(protein);
                  List<int> usedIndices;
                  var set = protein.SetMatchedPsmsAndQvalueScore(psmList, adjMatrix[i], out usedIndices);
                  adjMatrix[i] = new bool[psmList.Count];
                  if (!set) return;
                  foreach (var u in usedIndices) adjMatrix[i][u] = true;
              });

            // update rep protein(s) per peptide
            for (var j = 0; j < psmList.Count; j++)
            {
                var psm = psmList[j];
                var maxCosine = -100.0;
                foreach (var proteinName in psm.Id.Proteins)
                {
                    var i = proteinNameDictionary[proteinName];
                    if (!adjMatrix[i][j]) continue;
                    var cosine = proteinList[i].GetCosineWithPsmQuantity(psm);
                    if (cosine < maxCosine) continue;
                    maxCosine = cosine;
                }
                if (maxCosine <= -100) continue;

                foreach (var proteinName in psm.Id.Proteins)
                {
                    var i = proteinNameDictionary[proteinName];
                    if (!adjMatrix[i][j]) continue;
                    var cosine = proteinList[i].GetCosineWithPsmQuantity(psm);
                    if (cosine >= maxCosine)
                        adjMatrix[i][j] = true;
                    else adjMatrix[i][j] = false;
                }
            }
        }

        private static bool[][] GetProteinList(List<QuantifiedPsm> psmList, float cosineThrehsold, out List<QuantifiedProtein> proteinList, out Dictionary<string, int> proteinNameDictionary)
        {
            proteinNameDictionary = new Dictionary<string, int>();
            proteinList = new List<QuantifiedProtein>();

            // update proteinDictionary
            foreach (var psm in psmList)
                foreach (var proteinName in psm.Id.Proteins)
                {
                    if (proteinNameDictionary.ContainsKey(proteinName)) continue;
                    proteinNameDictionary[proteinName] = proteinList.Count;
                    proteinList.Add(new QuantifiedProtein(proteinName, cosineThrehsold));
                }

            // update adjMatrix
            var adjMatrix = new bool[proteinList.Count][];
            for (var i = 0; i < adjMatrix.Length; i++) adjMatrix[i] = new bool[psmList.Count];

            for (var j = 0; j < psmList.Count; j++)
            {
                var psm = psmList[j];
                //if (!psm.IsFullyQuantified()) continue;
                foreach (var proteinName in psm.Id.Proteins)
                {
                    var i = proteinNameDictionary[proteinName];
                    adjMatrix[i][j] = true;
                }
            }
            return adjMatrix;
        }
    }
}
