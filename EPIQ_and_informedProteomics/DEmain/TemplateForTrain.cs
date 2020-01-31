using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using IntervalTreeLib;
using MathNet.Numerics;

namespace Epiq
{
    public class TemplateForTrain : List<QuantifiedPsm>
    {
        public TemplateForTrain(string testRawFilePath, string searchResultFilePath, Tolerance tolerance,
            string resultPath, string chargeOption = @"", float[] mzRange = null, float[] etRange = null)
        {
            SetControl();
            Console.WriteLine(@"Raw File: {0}", testRawFilePath);
            Console.WriteLine(@"Search Result File: {0}", searchResultFilePath);
            var run = InMemoryLcMsRun.GetLcMsRun(testRawFilePath, MassSpecDataType.XCaliburRun);
            Params.InitRtParams(run);
            IsotopeEnvelopeCalculator.SetMaxNumIsotopes(Params.IsotopeIndex.Length);


            var searchResults = new SearchResults(searchResultFilePath, chargeOption.Length > 0,
                Params.TrainingSpecEvalueThreshold);

            var filteredIds = new List<Ms2Result>();
            var tcntr = 0;

            foreach (var id in new Ms2ResultList(searchResults, false))
            {
                tcntr++;
                if (!id.ChannelIdentified() || (id.SpecEValue > Params.TrainingSpecEvalueThreshold)) continue; //
                if ((mzRange != null) && ((id.Precursor < mzRange[0]) || (id.Precursor > mzRange[1]))) continue;
                var et = run.GetElutionTime(id.ScanNum);
                if ((etRange != null) && ((et < etRange[0]) || (et > etRange[1]))) continue;
                if (et > run.GetElutionTime(run.MaxLcScan) - 5.0) continue;

                filteredIds.Add(id);
            }

            Console.WriteLine(@"Qfeature generating MS2 ID count (spec-evalue threshold of {0:e2}) : {1} out of {2}",
                Params.TrainingSpecEvalueThreshold, filteredIds.Count, tcntr);

            ExtractQfeatures(filteredIds, tolerance, run);
            RemoveOverlaps(Params.XicClusterOverapRtThreshold, tolerance);
            Console.WriteLine(@"Number of Qfms : {0}", Count);
            WriteTrainingFile(resultPath);
        }

        private static void SetControl()
        {
            if (Control.TryUseNativeCUDA())
            {
                Console.WriteLine(@"Using CUDA for matrix calculation...");
                Control.UseNativeCUDA();
            }
            else if (Control.TryUseNativeMKL())
            {
                Console.WriteLine(@"Using MKL for matrix calculation...");
                Control.UseNativeMKL();
            }
            else if (Control.TryUseNativeOpenBLAS())
            {
                Console.WriteLine(@"Using OpenBLAS for matrix calculation...");
                Control.UseNativeOpenBLAS();
            }
        }


        private void ExtractQfeatures(List<Ms2Result> ids, Tolerance tolerance, LcMsRun run)
        {
            Console.Write(@"Extracting Qfeature training informations : ");
            var cntr = 0;
            var qcntr = 10;

            var next = 1;
            var lockTarget = new object();
            var peptIdsDictionary = new Dictionary<string, List<Ms2Result>>();
            foreach (var id in ids)
            {
                List<Ms2Result> subIds;
                var pep = id.UnlabeledPeptide;
                if (!peptIdsDictionary.TryGetValue(pep, out subIds))
                    peptIdsDictionary[pep] = subIds = new List<Ms2Result>();
                subIds.Add(id);
            }

            var qfmNullCnt = 0;
            var qfmStrNullCnt = 0;

            Parallel.ForEach(ids, new ParallelOptions {MaxDegreeOfParallelism = Params.MaxParallelThreads}, id =>
            {
                var qfm = XicCluster.GetXicClusterForTraining(id, tolerance, run, false);
                if (qfm != null) qfm.SetXicClusterScore(peptIdsDictionary[id.UnlabeledPeptide]);

                lock (lockTarget)
                {
                    cntr++;
                    var percent = 100*cntr/ids.Count;
                    if ((cntr > 0) && (percent >= next))
                    {
                        Console.Write(@"{0}% ", percent);
                        next += 1;
                    }

                    if (qfm == null)
                    {
                        qfmNullCnt++;
                        return;
                    }
                    if (qfm.TrainingStringDictionary == null)
                    {
                        qfmStrNullCnt++;
                        return;
                    }

                    //if (qfm == null || qfm.TrainingStringDictionary == null) continue;return;
                    Add(new QuantifiedPsm(qfm));
                    qcntr++;
                }
            });
            Console.WriteLine();
            Console.WriteLine(@"qfm null: {0} , qfm training str null: {1}", qfmNullCnt, qfmStrNullCnt);
            Console.WriteLine();
        }

        private void RemoveOverlaps(float overlapEtThreshold, Tolerance tolerance)
        {
            Console.Write(@"Generating interval tree for {0} Qfeatures ...", Count);

            var qfmIndexToArrayListDictionary = new Dictionary<int, int>();
            for (var i = 0; i < Count; i++)
                qfmIndexToArrayListDictionary[this[i].Index] = i;

            var tree = new IntervalTree<int, float>();
            foreach (var qfm in this)
            {
                var mz = qfm.Mz;
                var tol = (float) tolerance.GetToleranceAsTh(mz);
                tree.AddInterval(mz - tol, mz + tol, qfm.Index);
            }
            tree.Build();

            Console.Write(@"Done. Now pruning ... ");

            var toRemoveQfmIndexSet = new ConcurrentDictionary<int, byte>();

            Parallel.ForEach(this, new ParallelOptions {MaxDegreeOfParallelism = Params.MaxParallelThreads}, psm =>
            {
                if (toRemoveQfmIndexSet.ContainsKey(psm.Index)) return;
                var mz = psm.Mz;
                foreach (var oqfmIndex in tree.Get(mz))
                {
                    if (toRemoveQfmIndexSet.ContainsKey(oqfmIndex)) continue;
                    if (oqfmIndex == psm.Index) continue;
                    var opsm = this[qfmIndexToArrayListDictionary[oqfmIndex]];
                    if (psm.Id.Charge != opsm.Id.Charge) continue;
                    if (psm.Id.ArePeptidesTheSameExceptIle(opsm.Id)) continue;
                    if ((psm.RtStart > opsm.RtEnd) || (psm.RtEnd < opsm.RtStart))
                        continue;
                    if (Math.Min(psm.RtEnd - opsm.RtStart, opsm.RtEnd - psm.RtStart) < overlapEtThreshold*(psm.RtEnd - psm.RtStart))
                        continue;
                    if (psm.GetSnr() < opsm.GetSnr())
                    {
                        toRemoveQfmIndexSet[psm.Index] = 1;
                    }
                    else if (psm.GetSnr() > opsm.GetSnr())
                    {
                        toRemoveQfmIndexSet[oqfmIndex] = 1;
                    }
                    else
                    {
                        if (psm.Id.SpecEValue >= opsm.Id.SpecEValue) toRemoveQfmIndexSet[psm.Index] = 1;
                        else toRemoveQfmIndexSet[oqfmIndex] = 1;
                    }
                }
            });

            var toRemoveListIndex = new List<int>();
            foreach (var qfmIndex in toRemoveQfmIndexSet.Keys)
                toRemoveListIndex.Add(qfmIndexToArrayListDictionary[qfmIndex]);
            toRemoveListIndex.Sort();
            var offset = 0;
            foreach (var i in toRemoveListIndex)
                RemoveAt(i + offset--);
            Console.WriteLine(@"Done remaining Qfeature count : {0}", Count);
        }

        private void WriteTrainingFile(string resultPath)
        {
            Console.WriteLine(@"Writing files ...");
            using (var txtWriter = new StreamWriter(resultPath))
            {
                foreach (var qfm in this)
                    foreach (var cn in LabelList.LabelNumberArr)
                    {
                        if (qfm == null) throw new Exception(@"qfm is null");
                        if (qfm.TrainingDictionary == null) throw new Exception(@"training dictionary is null");
                        var trainingStr = qfm.TrainingDictionary[cn];
                        if (trainingStr == null) continue;
                        var parsedStr = trainingStr.Split('\t');
                        var normedEtDiff = parsedStr[0];
                        var dCount = parsedStr[1];
                        var normedRepreEt = parsedStr[2];
                        var pepLen = parsedStr[3];
                        var aaRatio = parsedStr[4];

                        txtWriter.WriteLine("{0} 1:{1} 2:{2} 3:{3} 4:{4}", normedEtDiff, dCount, normedRepreEt, pepLen,
                            aaRatio);
                    }
            }

            /*

            var pandasPath = Path.ChangeExtension(resultPath, @".tsv");
            using (var pandasWriter = new StreamWriter(pandasPath))
            {
                var index = 0;
                pandasWriter.WriteLine("DEFeatureIdx\tNormedApexEtDiff\tDnum\tNormedRepreEt\tPeptideLength\t{0}Ratio" +
                                       "\tPeptide\tCharge\tSpecEValue\tChannel\tNormedInitSignalWidth", DShift.DshiftAAs);
                foreach (var qfm in this)
                {
                    if (qfm == null) throw new Exception(@"qfm is null");
                    if (qfm.TrainingDictionary == null) throw new Exception(@"training dictionary is null");
                    foreach (var cn in LabelList.LabelNumberArr)
                    {
                        var trainingStr = qfm.TrainingDictionary[cn];
                        if (trainingStr == null) continue;
                        pandasWriter.WriteLine("{0}\t{1}", index, trainingStr);
                    }
                    index++;
                }
            }
             */
        }
    }
}