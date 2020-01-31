using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using MathNet.Numerics;

namespace Epiq
{
    public class PreProcessMs2Spec
    {
        private readonly Tolerance _tolerance;
        private List<string> _testRawFilePaths = new List<string>();
        private List<string> _mgfFilePaths = new List<string>();
        private bool _addMz;
        private bool _correctMz;
        private bool _correctCharge;

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

        
        public PreProcessMs2Spec(string rawFilePath, string mgfPath, Tolerance tolerance, bool addMz, bool correctMz, bool correctCharge)
        {
            _tolerance = tolerance;
            _addMz = addMz;
            _correctMz = correctMz;
            _correctCharge = correctCharge;

            if (!(addMz||correctCharge||correctMz))
                throw new Exception("Invalid correction option: Nothing will be corrected!");

            SetControl();
            SetFilePaths(rawFilePath, mgfPath);

            for (var i = 0; i < _testRawFilePaths.Count; i++)
            {
                var testRawFilePath = _testRawFilePaths[i];
                var mgfFilePath = _mgfFilePaths[i];
                Console.WriteLine(@"Processing " + testRawFilePath + @" ...");

                var correctedPsList = RunPreProcessing(testRawFilePath);
                CorrectedMgfWriter(mgfFilePath, correctedPsList);
            }
        }

        private void SetFilePaths(string rawFilePath, string mgfPath)
        {
            if (File.Exists(rawFilePath))
            {
                _testRawFilePaths.Add(rawFilePath);
                _mgfFilePaths.Add(mgfPath);
            }

            else if (Directory.Exists(rawFilePath))
            {
                foreach (var file in Directory.EnumerateFiles(rawFilePath))
                {
                    if (!file.ToUpper().EndsWith(".RAW")) continue;
                    _testRawFilePaths.Add(file);

                    string chargeOption = ".";

                    if (_addMz)
                        chargeOption += "MzAdded";
                    if (_correctMz && _correctCharge)
                        chargeOption += "MzAndChargeCorrected";
                    else if (_correctMz)
                        chargeOption += "MzCorrected";
                    else if (_correctCharge)
                        chargeOption += "ChargeCorrected";
                    _mgfFilePaths.Add(mgfPath + file.Substring(file.LastIndexOf('\\') + 1,
                                         file.LastIndexOf('.') - file.LastIndexOf('\\') - 1) + chargeOption + @".mgf");
                }
            }
        }


        private ConcurrentBag<CorrectedProductSpectrum> RunPreProcessing(string testRawFilePath)
        {
            var xcalReader = new XCaliburReader(testRawFilePath);
            var instModel = xcalReader.ReadInstModel();
            var run = InMemoryLcMsRun.GetLcMsRun(testRawFilePath, MassSpecDataType.XCaliburRun);
            Params.InitRtParams(run);
            var ms2ScanNumList = run.GetScanNumbers(2);

            Console.WriteLine(@"Inst Model is {0}", instModel);
            Console.WriteLine(@"Processing {0} spectra...", ms2ScanNumList.Count);

            var lockTarget = new object();
            var cntr = 0;
            var emptyCntr = 0;
            var newMzCntr = 0;
            var changedCount = 0;
            var correctedPsList = new ConcurrentBag<CorrectedProductSpectrum>();
            var chargeCounter = new ConcurrentDictionary<Tuple<int?, sbyte>, int>();

            //Parallel.ForEach(ms2ScanNumList, new ParallelOptions {MaxDegreeOfParallelism = Params.MaxParallelThreads},
            //    ms2ScanNum =>
            foreach (var ms2ScanNum in ms2ScanNumList)
                {
                    lock (lockTarget)
                        { if (++cntr%1000 == 0) Console.Write(@"{0} ", cntr); }

                    List<double> precusorMzCandidates;
                    if (_addMz)
                    {
                        precusorMzCandidates = new PrecusorMzCandidates(ms2ScanNum, run, instModel);
                        if (precusorMzCandidates.Count == 0)
                        {
                            Interlocked.Increment(ref emptyCntr);
                            continue;//return; //continue; //return;
                        }
                    }
                    else
                    {
                        precusorMzCandidates = new List<double> {run.GetIsolationWindow(ms2ScanNum).IsolationWindowTargetMz};
                    }
               
                    Interlocked.Add(ref newMzCntr, precusorMzCandidates.Count);

                    foreach (var precusorMz in precusorMzCandidates)
                    {
                        var cPs = new CorrectedProductSpectrum(ms2ScanNum, precusorMz, _tolerance, run, _correctMz, _correctCharge);
                        correctedPsList.Add(cPs);

                        if (cPs.ChargeCorrected)
                        {
                            //cPs.PrintDistribution();
                            changedCount++;
                            var counterKey = new Tuple<int?, sbyte>(cPs.Ps.IsolationWindow.Charge, cPs.CorrectedPrecursorCharge);
                            if (chargeCounter.ContainsKey(counterKey))
                                chargeCounter[counterKey] += 1;
                            else
                                chargeCounter[counterKey] = 1;
                        }
                    }
                }//);
            Console.WriteLine("\nNumber of empty isolation windows: {0}", emptyCntr);
            Console.WriteLine("Number of new target mzs: {0}", newMzCntr);
            Console.WriteLine("Number of charge changed spectrum: {0}", changedCount);
            foreach (var key in chargeCounter.Keys)
                Console.WriteLine("{0} -> {1} : {2}", key.Item1, key.Item2, chargeCounter[key]);

            return correctedPsList;
        }

        private void CorrectedMgfWriter(string mgfFilePath,
            ConcurrentBag<CorrectedProductSpectrum> correctedPsList)
        {
            Console.WriteLine("Writing mgf!\n");
            var dummySn = 1;
            using (var mgfWriter = new StreamWriter(mgfFilePath))
            {
                foreach (var correctedPs in correctedPsList)
                {
                    var psString = correctedPs.Ps.ToMgfString(correctedPs.CorrectedPrecursorCharge,
                                                              correctedPs.CorrectedPrecursorMz,
                                                              dummySn++,
                                                              correctedPs.Ps.ScanNum + " " + correctedPs.Ps.IsolationWindow.Charge
                                                              + " " + correctedPs.Ps.IsolationWindow.IsolationWindowTargetMz);
                    mgfWriter.WriteLine(psString);
                }
            }
        }

    }
}