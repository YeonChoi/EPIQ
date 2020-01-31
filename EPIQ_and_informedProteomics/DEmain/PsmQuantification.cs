using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;

namespace Epiq
{
    public class PsmQuantification
    {
        public static QuantifiedPsmList RetreivePsmList(string psmQuantFile, bool training=false)
        {
            Console.Write(@"Retreiving quantified PSMs from file ... {0} ", psmQuantFile);
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(psmQuantFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            var psmList = (QuantifiedPsmList)formatter.Deserialize(stream);
            stream.Close();
            Console.WriteLine(@" - {0} PSMs retreived", psmList.Count);
            return psmList;
        }

        public static int Run(string specFilePath, string searchResultPath, Tolerance tolerance, string psmQuantFile, string xicClusterResultsPath = null)
        {
            Console.WriteLine(@"     **** Quantifying {0} identified in {1} ...", specFilePath, searchResultPath);
            var run = specFilePath.ToUpper().EndsWith(".MZML")? InMemoryLcMsRun.GetLcMsRun(specFilePath, MassSpecDataType.MzMLFile) : InMemoryLcMsRun.GetLcMsRun(specFilePath, MassSpecDataType.XCaliburRun);

            Params.InitRtParams(run);
            var filteredIds = new List<Ms2Result>();
            var tcntr = 0;

            foreach (var id in new Ms2ResultList(new SearchResults(searchResultPath, false, Params.InitSpecEValueThreshold)))
            {
                tcntr++;
                filteredIds.Add(id);
            }
            Console.WriteLine(@"Filtered psm count (spec-evalue threshold of {0:e2}) : {1} out of total psm count {2}", Params.InitSpecEValueThreshold, filteredIds.Count, tcntr);

            var psmList = GetQuantifiedPsmList(filteredIds, tolerance, run, xicClusterResultsPath!=null? xicClusterResultsPath + Path.DirectorySeparatorChar : null);
            psmList.TruncateQuantity(Params.MinLabelQuantity);
            //psmList.FilterByQvalue(Params.PepQvalueThreshold, Params.SnrThreshold, Params.MinQfeatureMeanQuantity); 
            //psmList.AssignPsmIds(psmIdPrefix);
           
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(psmQuantFile, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, psmList);
            stream.Close();
            
            return 0;
        }

        private static QuantifiedPsmList GetQuantifiedPsmList(List<Ms2Result> ids, Tolerance tolerance, LcMsRun run, string resultsPath = null)
        {
            var peptIdsDictionary = new Dictionary<string, List<Ms2Result>>();
            var psmsDictionary = new Dictionary<int, List<QuantifiedPsm>>();
            if (resultsPath != null) XicCluster.Debug = true;
            
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
           
            Console.Write(@"Defining&Quantifying {0} XicClusters : ", ids.Count);
            foreach (var id in ids)
            {
                currentIds.Add(id);
                if (currentIds.Count < 1000) continue;
                DefineAndQuantifyXicClusters(psmsDictionary, currentIds, peptIdsDictionary, tolerance, run, ref cntr, ref nextPercent, ids.Count, resultsPath);
                currentIds = new List<Ms2Result>();
            }
            if (currentIds.Count > 0)
                DefineAndQuantifyXicClusters(psmsDictionary, currentIds, peptIdsDictionary, tolerance, run, ref cntr, ref nextPercent, ids.Count, resultsPath);

            Console.WriteLine("Number of total psms saved to psmsDictionary : {0}", psmsDictionary.Count);

            var RetList = new List<QuantifiedPsm>();
            foreach (var psmTieList in psmsDictionary.Values)
            {
                foreach (var psm in psmTieList)
                {
                    RetList.Add(psm);
                }
            }

            return new QuantifiedPsmList(RetList);
        }


        private static void DefineAndQuantifyXicClusters(Dictionary<int, List<QuantifiedPsm>> psmsDictionary, List<Ms2Result> ids,
                                                         Dictionary<string, List<Ms2Result>> peptIdsDictionary, Tolerance tolerance, LcMsRun run,
                                                         ref int cntr, ref int nextPercent, int totalIdCount, string resultsPath = null)
        {
            var curCntr = cntr;
            var qcntr = 10;
            var next = nextPercent;
            
            var lockTarget = new object();

            Parallel.ForEach(ids, new ParallelOptions {MaxDegreeOfParallelism = Params.MaxParallelThreads}, (id, state) =>
            //foreach (var id in ids) // For non-paralle, debuging mode
            {

                var xicCluster = XicCluster.GetXicCluster(id, tolerance, run, true);
                if (xicCluster != null) xicCluster.SetXicClusterScore(peptIdsDictionary[id.UnlabeledPeptide]);

                lock (lockTarget)
                {

                    curCntr++;
                    var percent = 100 * curCntr / totalIdCount;
                    if (percent == next)
                    {
                        Console.Write(@"{0}% ", percent);
                        next += 10;
                    }

                    if (xicCluster == null) return; //continue; // For non-parallel, debuging mode


                    List<QuantifiedPsm> oxicClusterList;
                    if (!psmsDictionary.TryGetValue(id.ScanNum, out oxicClusterList))
                    {
                        psmsDictionary[id.ScanNum] = new List<QuantifiedPsm> {new QuantifiedPsm(xicCluster)};
                    }
                    else if (oxicClusterList[0].XicClusterScore > xicCluster.XicClusterScore)
                    {
                        psmsDictionary[id.ScanNum] = new List<QuantifiedPsm> {new QuantifiedPsm(xicCluster)};
                    }
                    else if (Math.Abs(oxicClusterList[0].XicClusterScore - xicCluster.XicClusterScore) < XicCluster.XicClusterScorePrecision &&
                             oxicClusterList[0].Id.SpecEValue > xicCluster.Id.SpecEValue)
                    {
                        psmsDictionary[id.ScanNum] = new List<QuantifiedPsm> {new QuantifiedPsm(xicCluster)};
                    }
                    else if (Math.Abs(oxicClusterList[0].XicClusterScore - xicCluster.XicClusterScore) < XicCluster.XicClusterScorePrecision &&
                             oxicClusterList[0].Id.SpecEValue == xicCluster.Id.SpecEValue &&
                             oxicClusterList[0].SignalPower < xicCluster.SignalPower)
                    {
                        psmsDictionary[id.ScanNum] = new List<QuantifiedPsm> {new QuantifiedPsm(xicCluster)};
                    }
                    else if (Math.Abs(oxicClusterList[0].XicClusterScore - xicCluster.XicClusterScore) < XicCluster.XicClusterScorePrecision &&
                             oxicClusterList[0].Id.SpecEValue == xicCluster.Id.SpecEValue &&
                             oxicClusterList[0].SignalPower == xicCluster.SignalPower  &&
                             Math.Abs(oxicClusterList[0].Id.PrecursorMzError) > Math.Abs(xicCluster.Id.PrecursorMzError))
                    {
                        psmsDictionary[id.ScanNum] = new List<QuantifiedPsm> {new QuantifiedPsm(xicCluster)};
                    }
                    else if (Math.Abs(oxicClusterList[0].XicClusterScore - xicCluster.XicClusterScore) <
                             XicCluster.XicClusterScorePrecision &&
                             oxicClusterList[0].Id.SpecEValue == xicCluster.Id.SpecEValue &&
                             oxicClusterList[0].SignalPower == xicCluster.SignalPower &&
                             Math.Abs(oxicClusterList[0].Id.PrecursorMzError) == Math.Abs(xicCluster.Id.PrecursorMzError))
                    {
                        psmsDictionary[id.ScanNum].Add(new QuantifiedPsm(xicCluster));
                    }


                    qcntr++;
                    if (resultsPath == null) return; //continue; For non-parallel, debuging mode
                    if (xicCluster.IsFullyQuantified() && xicCluster.GetSnr() > 10 && xicCluster.Quantities.Sum() > 1e9)
                        xicCluster.Write(new StreamWriter(resultsPath + id.SpecFile + "_"+ id.UnmodifiedPeptide + "_" + id.Charge + "_" + id.LabelIndex +
                                             "_" + id.IsotopeIdx + "_" + xicCluster.Id.NumOfBoundLabels + ".m"));

                }
            });
            //}  // For non-parallel, debuging mode
            cntr = curCntr; 
            nextPercent = next;
        }
    }


                    /*
            var outfile = new StreamWriter(@"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\checkRandom_livePsms.txt", true);
            outfile.Close();
                    if (id.ScanNum == 100003)
                    {
                        writer.Write("{0}\t{1}\t{2}", id.ScanNum, id.Peptide, xicCluster==null);
                        if (xicCluster != null)
                        {
                            var qpsm = new QuantifiedPsm(xicCluster);
                            writer.WriteLine("\t{0}\t{1}\t{2}", String.Join(";", qpsm.Cosines), String.Join(";", qpsm.Quantities), String.Join(";", qpsm.ChannelApexEts));
                            writer.WriteLine(xicCluster.Templates.ToString());
                            writer.WriteLine(xicCluster.Quantities.ToString());
                        }
                        else writer.WriteLine();
                        
                        state.Break();
                    }
                    */
}
