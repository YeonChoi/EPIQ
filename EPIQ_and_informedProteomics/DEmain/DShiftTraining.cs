using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;

namespace Epiq
{
    public class DShiftTraining
    {

        public static int Run(string rawPath, string searchResultPath, Tolerance tolerance, string psmQuantFile, bool writeXicClusters = false)
        {
            Console.WriteLine(@"     **** Training {0} identified in {1} ...", rawPath, searchResultPath);
            var run = InMemoryLcMsRun.GetLcMsRun(rawPath, MassSpecDataType.XCaliburRun);
            
            Params.InitRtParams(run);
            var filteredIds = new List<Ms2Result>();
            var tcntr = 0;

            foreach (var id in new Ms2ResultList(new SearchResults(searchResultPath, false, Params.TrainingSpecEvalueThreshold), false))
            {
                tcntr++;
                if (id.IsDecoy()) continue;
                filteredIds.Add(id);
            }
            Console.WriteLine(@"Filtered psm count (spec-evalue threshold of {0:e2}) : {1} out of total psm count {2}", Params.TrainingSpecEvalueThreshold, filteredIds.Count, tcntr);

            var psmList = GetTrainingPsmList(filteredIds, tolerance, run, writeXicClusters == false ? null : Directory.GetParent(psmQuantFile).FullName + Path.DirectorySeparatorChar);
            psmList.RemoveOverlappingXicClusters(Params.XicClusterOverapRtThreshold, tolerance);
            
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(psmQuantFile, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, psmList);
            stream.Close();

            return 0;
        }


        public static QuantifiedPsmList RetreivePsmList(string psmQuantFile, Tolerance tolerance)
        {
            Console.Write(@"Retreiving quantified PSMs from file ... {0} ", psmQuantFile);
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(psmQuantFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            var psmList = (QuantifiedPsmList)formatter.Deserialize(stream);
            stream.Close();
            Console.WriteLine(@" - {0} PSMs retreived", psmList.Count);
            return psmList;
        }

        private static QuantifiedPsmList GetTrainingPsmList(List<Ms2Result> ids, Tolerance tolerance, LcMsRun run, string resultPath = null)
        {
            var peptIdsDictionary = new Dictionary<string, List<Ms2Result>>();
            var psmsDictionary = new Dictionary<int, QuantifiedPsm>();
            if (resultPath != null) XicCluster.Debug = true;

            foreach (var id in ids)
            {
                List<Ms2Result> subIds;
                var pep = id.UnlabeledPeptide;
                if (!peptIdsDictionary.TryGetValue(pep, out subIds))
                    peptIdsDictionary[pep] = subIds = new List<Ms2Result>();
                subIds.Add(id);
            }

            var currentIds = new List<Ms2Result>();
            var cntr = 0;
            var nextPercent = 10;
            Console.Write(@"Defining {0} Training XicClusters : ", ids.Count);
            foreach (var id in ids)
            {
                currentIds.Add(id);
                if (currentIds.Count < 5000) continue;
                DefineTrainingXicClusters(psmsDictionary, currentIds, peptIdsDictionary, tolerance, run, ref cntr, ref nextPercent, ids.Count, resultPath);
                currentIds = new List<Ms2Result>();
            }
            if (currentIds.Count > 0)
                DefineTrainingXicClusters(psmsDictionary, currentIds, peptIdsDictionary, tolerance, run, ref cntr, ref nextPercent, ids.Count, resultPath);
            Console.WriteLine();

            return new QuantifiedPsmList(psmsDictionary.Values);
        }


        private static void DefineTrainingXicClusters(Dictionary<int, QuantifiedPsm> psmsDictionary, List<Ms2Result> ids, Dictionary<string, List<Ms2Result>> peptIdsDictionary,
            Tolerance tolerance, LcMsRun run, ref int cntr, ref int nextPercent, int totalIdCount, string resultPath = null)
        {
            var curCntr = cntr;
            var qcntr = 2;
            var next = nextPercent;
            var lockTarget = new object();

            Parallel.ForEach(ids, new ParallelOptions {MaxDegreeOfParallelism = Params.MaxParallelThreads}, id =>
            {
                var xicCluster = XicCluster.GetXicClusterForTraining(id, tolerance, run, false);
                if (xicCluster != null) xicCluster.SetXicClusterScore(peptIdsDictionary[id.UnlabeledPeptide]);


                lock (lockTarget)
                {
                    curCntr++;
                    var percent = 100 * curCntr / totalIdCount;
                    if (percent == next)
                    {
                        Console.Write(@"{0}% ", percent);
                        next += 2;
                    }

                    if (xicCluster == null) return;
                    if (xicCluster.TrainingStringDictionary == null) return;

                    QuantifiedPsm oxicCluster;
                    if (!psmsDictionary.TryGetValue(id.ScanNum, out oxicCluster))
                    {
                        psmsDictionary[id.ScanNum] = new QuantifiedPsm(xicCluster);
                    }
                    else if (oxicCluster.XicClusterScore > xicCluster.XicClusterScore)
                    {
                        psmsDictionary[id.ScanNum] = new QuantifiedPsm(xicCluster);
                    }
                    else if (Math.Abs(oxicCluster.XicClusterScore - xicCluster.XicClusterScore) < XicCluster.XicClusterScorePrecision &&
                             oxicCluster.Id.SpecEValue > xicCluster.Id.SpecEValue)
                    {
                        psmsDictionary[id.ScanNum] = new QuantifiedPsm(xicCluster);
                    }
                    else if (Math.Abs(oxicCluster.XicClusterScore - xicCluster.XicClusterScore) < XicCluster.XicClusterScorePrecision &&
                             oxicCluster.Id.SpecEValue == xicCluster.Id.SpecEValue &&
                             oxicCluster.SignalPower == xicCluster.SignalPower &&
                             Math.Abs(oxicCluster.Id.PrecursorMzError) > Math.Abs(xicCluster.Id.PrecursorMzError))
                    {
                        psmsDictionary[id.ScanNum] = new QuantifiedPsm(xicCluster);
                    }
                    else if (Math.Abs(oxicCluster.XicClusterScore - xicCluster.XicClusterScore) < XicCluster.XicClusterScorePrecision &&
                             oxicCluster.Id.SpecEValue == xicCluster.Id.SpecEValue &&
                             oxicCluster.SignalPower == xicCluster.SignalPower &&
                             Math.Abs(oxicCluster.Id.PrecursorMzError) == Math.Abs(xicCluster.Id.PrecursorMzError) &&
                             oxicCluster.Id.Idindex > xicCluster.Id.Idindex)
                    {
                        psmsDictionary[id.ScanNum] = new QuantifiedPsm(xicCluster);
                    }



                    qcntr++;
                    if (resultPath != null) xicCluster.Write(new StreamWriter(resultPath + id.UnmodifiedPeptide + "_" + id.ScanNum + "_" + qcntr + ".m"));
                }
                Thread.Sleep(150);
            });
            cntr = curCntr;
            nextPercent = next;
        }
    }
}
