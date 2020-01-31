using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using DeconTools.Backend;
using InformedProteomics.Backend.MassSpecData;
using NUnit.Framework;
using DEmain;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.Utils;
using NUnit.Framework.Constraints;
using Tolerance = InformedProteomics.Backend.Data.Spectrometry.Tolerance;

namespace InformedProteomics.Test.DE
{
    [TestFixture]
    class TestDe
    {
        public const string prefix = @"2";
        public const string BasePath = @"E:\MassSpec\2014_DM_Try_5plex\";//@"E:\MassSpec\201604_DM_Try_3plex\";//@"C:\scratch\";
        public const string TestRawFilePath = @"E:\MassSpec\DMDE\raw_data\2014-08-11(DM_5plex_mix)\5plex_M" + prefix + @"_Dimet_J70_NAQ1_0808_1.raw";
        public const string resultsPath = BasePath + @"DEFeatureResults\M" + prefix + @"_1\";
        public const string searchResultPath = BasePath + @"ChargeFixedSearchResults\5plex_M" + prefix + @"_Dimet_J70_NAQ1_0808_1.chargeFixed-merged.tsv";
        //BasePath + @"rawfiles\DM_1000_10_1_NAQ448_J100_0422.raw";

        static LcMsRun run = InMemoryLcMsRun.GetLcMsRun(TestRawFilePath, MassSpecDataType.XCaliburRun);
        Tolerance tolerance = new Tolerance(20);
        private double minMz = 634;//run.MinMs1Mz;// 634;//
        private double maxMz = 665;//run.MaxMs1Mz;//670;//
        private double etstart = 50;//run.GetElutionTime(run.MinLcScan);//50;//
        private double etend = 60;//run.GetElutionTime(run.MaxLcScan) + 1;//66;//


        [Test]
        public void TestDeconvolution()
        {
            double mzMargin = 0.5;
            double etMargin = 0.02;
            DEFeature.init();
            var ms2ContainingXicPackets = new Ms2ContainingXicPacketList(run, tolerance, minMz, maxMz, etstart, etend);
            int nChannels = DEParams.ChannelNumArr().Count();
            int maxIso0key = (nChannels-1)*10 + 1;

            Console.WriteLine("{0}", TestRawFilePath);
            Console.WriteLine("{0}", searchResultPath);
            Console.WriteLine("");

            var searchResult = new ChargeFixedSearchResults(searchResultPath);

            //Add MS2 search results
            foreach (XicPacket xpac in ms2ContainingXicPackets)
            {
                xpac.AddSearchResults(searchResult);
            }

            var deFeatureList = new DEFeatureList();
            foreach (XicPacket initXpac in ms2ContainingXicPackets)
            {
                if ((!initXpac.HasMs2Result()))
                    continue;
                var deFeature = new DEFeature(initXpac, run, tolerance, searchResult);
                deFeatureList.Add(deFeature);
            }

            int deFeatureFileIdx = 0;
            foreach (var deFeature in deFeatureList)
            {
                deFeature.DeconvoluteXicPackets(run);
                var path = string.Format(resultsPath + @"DeconvolutedDEFeatures\defeature{0:D5}.m", deFeatureFileIdx++);
                deFeature.WriteMatlabFile(path, mzMargin, etMargin, run);
            }

            
        }


        [Test]
        public void TestNoise()
        {
            RandomNoise noise = new RandomNoise(1000, 100, run, tolerance);
            foreach (var eachNoise in noise)
            {
                var inten = eachNoise[0];
                var et = eachNoise[1];
                Console.WriteLine("ElutionTime:\t{0}", string.Join(" ", et));
                Console.WriteLine("Intensity:\t{0}", string.Join(" ", inten));
            }
        }

        public void TestXicMatlabOut()
        {
            const string path = @"E:\MassSpec\DE\allXic.m";
            var xic = run.GetPrecursorExtractedIonChromatogram(minMz, maxMz);
            using (var fs = File.Create(path))
            {
                var xpac = new XicPacket(xic, run, xic.GetApex().Mz);
                xpac.WriteMatlabVariable(fs, "xic", run, etstart, etend);
               // var sepXicList = new XicPacketList(xic, run);
                   
            }
        }

        [Test]
        public void TestPeak()
        {
            var candidatePeaks = new HashSet<Peak>();
            for (byte c = 1; c <= 8; c++)
            {
                candidatePeaks.Add(new Peak(10, 659.326367316183));
                candidatePeaks.Add(new Peak(0, 659.326367316183));

            }                                                     
            Console.WriteLine(candidatePeaks.Count);
        }

        [Test]
        public void TestChargeDetermination()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            const string path = @"E:\MassSpec\DE_Trypsin_6plex\ms2XicPacketsMs2.m";
            const string mgfpath = @"E:\MassSpec\DE_Trypsin_6plex\DE_6plexing_2ug_J100_NAQ454_0114_charge_fixed.mgf";

            var ms2ContainingXicPackets = new Ms2ContainingXicPacketList(run, tolerance, minMz, maxMz, etstart, etend);
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            
            Console.WriteLine(@"RunTime " + elapsedTime);
            var threshold = .7;
            for (var method = 0; method <= 0; method++)
            {
                Console.WriteLine();
                Console.WriteLine(@"Scoring method: " + method);
               
                var chargeCntr = new Dictionary<int, int>();
                var chargeDeterminedCount = 0;
                var chargeCntrAllowingMultipleCharges = new Dictionary<int, int>();
                var testXicNum = 1;
                var diffCount = 0;
                var diffHighScoreCount = 0;

                using (var mgfWriter = new StreamWriter(mgfpath))
                {
                    using (var fs = File.Create(path))
                    {
                        var dummyScanNumber = 1;
                        foreach (var xicPac in ms2ContainingXicPackets)
                        {
                            // xicPac.SetChargeAndIsotopeIndex(run, tolerance, false, method);
                            xicPac.WriteMatlabVariable(fs, "xpMs2{" + testXicNum++ + "}", run, etstart, etend);
                            if (xicPac.MaxScoringCharge != xicPac.IsolationWindowCharge)
                            {
                                diffCount++;
                                if (xicPac.GetMaxChargeScore() > threshold) // && xicPac.Charge * 2 == xicPac.IsolationWindowCharge)
                                {
                                    //  xicPac.SetChargeAndIsotopeIndex(run, tolerance, true, method);
                                    diffHighScoreCount++;
                                }
                            }
                            if (xicPac.GetMaxChargeScore() < threshold) continue;
                            foreach (var c in xicPac.ChargeScores.Keys)
                            {
                                if (!(xicPac.ChargeScores[c] >= threshold)) continue;
                                if (!chargeCntrAllowingMultipleCharges.ContainsKey(c)) chargeCntrAllowingMultipleCharges[c] = 0;
                                chargeCntrAllowingMultipleCharges[c]++;

                                foreach (var ms2sn in xicPac.Ms2TriggeringScanNumbers)
                                {
                                    var spec = run.GetSpectrum(ms2sn);
                                    var ps = spec as ProductSpectrum;
                                    if (ps == null) continue;
                                    mgfWriter.WriteLine(ps.ToMgfString(c, dummyScanNumber++, ms2sn + " " + xicPac.IsolationWindowCharge));
                                }



                                //  xicPac.Ms2TriggeringScanNumbers

                            }

                            chargeDeterminedCount++;
                            if (!chargeCntr.ContainsKey(xicPac.MaxScoringCharge)) chargeCntr[xicPac.MaxScoringCharge] = 0;
                            chargeCntr[xicPac.MaxScoringCharge]++;
                        }
                    }
                }
                Console.WriteLine(@"Total MS2 containing XIC packets: " + ms2ContainingXicPackets.Count);
                Console.WriteLine(@"Total Charge Determined XIC packets: " + chargeDeterminedCount);
                Console.WriteLine(@"Total Xcal/our charge different XIC packets: " + diffCount);
                Console.WriteLine(@"Total Xcal/our charge different XIC packets with high score: " + diffHighScoreCount);

                for (var c =1;c<8;c++)
                {
                    if (!chargeCntr.ContainsKey(c)) continue;
                    Console.WriteLine(@"Max Charge {0} XIC Packets: {1}", c, chargeCntr[c]);
                }

                for (var c = 1; c < 8; c++)
                {
                    if (!chargeCntrAllowingMultipleCharges.ContainsKey(c)) continue;
                    Console.WriteLine(@"Charge {0} XIC Packets (allowing multiple charges): {1}", c, chargeCntrAllowingMultipleCharges[c]);
                }

                
            }
        }

        [Test]
        public void TestParsingChargeFixedSearchResults()
        {
            const string path = @"E:\MassSpec\DE_Trypsin_6plex\charge_fixed_search_results\DE_6plexing_2ug_J100_NAQ454_0114_charge_fixed.tsv";
            var testScanNums = new List<int> {37691, 37691, 41890, 37963, 37963};
            var testDataNames = new List<string> {"Peptide", "Charge", "Precursor", "QValue"};
            var parser = new ChargeFixedSearchResults(path);

            Console.WriteLine(@"Headers:");
            Console.WriteLine(String.Join(", ", parser.GetHeaders().ToArray()));
            Console.WriteLine(@"Number of Rows: {0}", parser.GetRows().Count);

            foreach (int scan in testScanNums)
            {
                Console.WriteLine("");
                Console.WriteLine(@"ScanNum: {0}", scan);
                for (int i = 0; i < parser.GetNumOfSearchResults(scan); i++)
                {
                    foreach (string dataname in testDataNames)
                    {
                        string data = parser.GetNstSearchDataOfScanNum(scan, i, dataname);
                        Console.WriteLine(@"{0} : {1}", dataname, data);
                    }
                    
                }
            }
        }

        [Test]
        public void TestGetEachChannelShape()
        {
            Console.WriteLine("{0}", TestRawFilePath);
            Console.WriteLine("{0}", searchResultPath);
            Console.WriteLine("");

            const string shapeFilePath = resultsPath + "ChannelShape.txt";
            int nChannels = DEParams.ChannelNumArr().Count();

            var ms2ContainingXicPackets = new Ms2ContainingXicPacketList(run, tolerance, minMz, maxMz, etstart, etend);
            var searchResult = new ChargeFixedSearchResults(searchResultPath);
            ms2ContainingXicPackets.AddSearchResults(searchResult);

            var deFeatureList = new DEFeatureList();
            foreach (XicPacket initXpac in ms2ContainingXicPackets)
            {
                if ((!initXpac.HasMs2Result()))
                    continue;
                var deFeature = new DEFeature(initXpac, run, tolerance, searchResult);
                deFeatureList.Add(deFeature);
            }
            deFeatureList.RemoveOverLappedDEFeature(tolerance);

            int deFeatureCount = 0;
            using (var fs = new StreamWriter(shapeFilePath))
            {
                fs.WriteLine("DEFeatureIdx\tPeptide\tChannelNum\tIsotopeIdx\tM/Z\tApex\tLeftWidth0.5\tRightWidth0.5\tLeftWidth0.7\tRightWidth0.7\tElutionTime\tIntensity");
                foreach (var deFeature in deFeatureList)
                {
                    if (deFeature.GetChannels().Count == nChannels)
                    {
                        deFeatureCount++;
                        var keys = new List<int>(deFeature.Keys);
                        keys.Sort();
                        foreach (int eachkey in keys)
                        {
                            XicPacket xpac = deFeature[eachkey];
                            double apexEt = run.GetElutionTime(xpac.GetCachedApex().ScanNum);

                            List<string> etList = new List<string>();
                            List<string> intensityList = new List<string>();
                            foreach (XicPoint xp in xpac)
                            {
                                etList.Add(run.GetElutionTime(xp.ScanNum).ToString("E"));
                                intensityList.Add(xp.Intensity.ToString("E"));
                            }
                            string etOut = string.Join(",", etList);
                            string intensityOut = string.Join(",", intensityList);
                            var width50p = xpac.GetWidth(run);
                            var width70p = xpac.GetWidth(run, 0.7);
                            fs.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}",
                                         deFeatureCount, deFeature.BestMs2Result().Peptide, DEFeature.GetChannel(eachkey), DEFeature.GetIsotopeIndex(eachkey),
                                         xpac.GetCachedApex().Mz, apexEt, width50p.Item1, width50p.Item2, width70p.Item1, width70p.Item2, etOut, intensityOut);
                        }
                    }
                }
            }
        }

        
        [Test]
        public void TestDEFeature()
        {
            double mzMargin = 0.5;
            double etMargin = 0.02;

            var ms2ContainingXicPackets = new Ms2ContainingXicPacketList(run, tolerance, minMz, maxMz, etstart, etend);
            int nChannels = DEParams.ChannelNumArr().Count();
            int maxIso0key = (nChannels-1)*10 + 1;

            Console.WriteLine("{0}", TestRawFilePath);
            Console.WriteLine("{0}", searchResultPath);
            Console.WriteLine("");

            var searchResult = new ChargeFixedSearchResults(searchResultPath);
            int totalMs2Points = 0;
            int totalMs2SearchedXic = 0;
            int totalMs2ConsistentXic = 0;
            int totalConflictedMs2ScansXic = 0;

            //Add MS2 search results
            foreach (XicPacket xpac in ms2ContainingXicPackets)
            {
                totalMs2Points += xpac.Ms2TriggeringScanNumbers.Count;
                xpac.AddSearchResults(searchResult);

                if (xpac.HasMs2Result())
                {
                    totalMs2SearchedXic++;
                    if (xpac.Ms2ResultsConsistent())
                        totalMs2ConsistentXic++;
                    else if (xpac.Ms2SearchedScanNum().Count > 1)
                        totalConflictedMs2ScansXic++;
                }
            }
            Console.WriteLine(@"Total MS2 containing XicPackets: {0}", ms2ContainingXicPackets.Count);
            Console.WriteLine(@"Total MS2 scans: {0}", totalMs2Points);
            Console.WriteLine(@"Total Xic with search result (satisfying QValue {0}, N-term DE) : {1}", XicPacket.Ms2IdQvalThreshold, totalMs2SearchedXic);
            Console.WriteLine(@"Total Xic with consistent search result : {0}", totalMs2ConsistentXic);
            Console.WriteLine(@"Total Xic carrying many MS2 Scans, with confilicting search results : {0}", totalConflictedMs2ScansXic);
            Console.WriteLine("");


            //Collect DEFeatures
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var deFeatureList = new DEFeatureList();

            foreach (XicPacket initXpac in ms2ContainingXicPackets)
            {
                if ((!initXpac.HasMs2Result()))
                    continue;
                var deFeature = new DEFeature(initXpac, run, tolerance, searchResult);
                deFeatureList.Add(deFeature);
            }
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine(@"Time used for making DEFeatures: {0}", elapsedTime);
            Console.WriteLine("");
            deFeatureList.PrintStats();


            /*
            int matidx = 0;
            foreach (var defeature in deFeatureList)
            {
                var path = string.Format(resultsPath + @"DEFeatures\defeature{0:D5}.m", matidx++);
                defeature.WriteMatlabFile(path, mzMargin, etMargin, run);
            }
             */

            deFeatureList.RemoveOverLappedDEFeature(tolerance);
            Console.WriteLine("Stat After removing overlapped DEFeatures:");
            deFeatureList.PrintStats();

            
            int failedAllChIdx = 0;
            int failedAllChIso0Idx = 0;
            int allChIso0Idx = 0;
            var failedAllChFile = new StreamWriter(resultsPath + "FailedAllChannelDEFeatures.txt");
            var failedAllChIso0File = new StreamWriter(resultsPath + "FailedAllChannelIso0DEFeatures.txt");
            var allChIso0File = new StreamWriter(resultsPath + "AllChannelIso0DEFeatures.txt");

            foreach (var defeature in deFeatureList)
            {
                if (defeature.GetChannels().Count < nChannels)
                {
                    var path = string.Format(resultsPath + @"FailedAllChannelDEFeatures\failed_defeature{0:D5}.m", failedAllChIdx);
                    defeature.WriteMatlabFile(path, mzMargin, etMargin, run);
                    var line = String.Format("{0:D5}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                                              failedAllChIdx,
                                              defeature.InitMs2Result.Charge,
                                              defeature.InitMs2Result.ChannelNum,
                                              defeature.InitMs2Result.IsotopeIdx,
                                              defeature.InitMs2Result.Qvalue,
                                              defeature.InitMs2Result.ScanNum,
                                              defeature.InitMs2Result.Peptide);
                    failedAllChFile.WriteLine(line);
                    failedAllChIdx++;
                }

                else
                {
                    bool allChannelIsotope0 = true;
                    for (int i = 0; i < maxIso0key; i += 10)
                    {
                        if (!defeature.ContainsKey(i))
                        {
                            allChannelIsotope0 = false;
                            break;
                        }
                    }

                    if (!allChannelIsotope0)
                    {
                        var path = string.Format(resultsPath + @"FailedAllChannelIso0DEFeatures\failediso0_defeature{0:D5}.m", failedAllChIso0Idx);
                        defeature.WriteMatlabFile(path, mzMargin, etMargin, run);
                        var line = String.Format("{0:D5}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                            failedAllChIso0Idx,
                            defeature.InitMs2Result.Charge,
                            defeature.InitMs2Result.ChannelNum,
                            defeature.InitMs2Result.IsotopeIdx,
                            defeature.InitMs2Result.Qvalue,
                            defeature.InitMs2Result.ScanNum,
                            defeature.InitMs2Result.Peptide);
                        failedAllChIso0File.WriteLine(line);
                        failedAllChIso0Idx++;
                    }
                    else
                    {
                        var path = string.Format(resultsPath + @"AllChannelIso0DEFeatures\defeature{0:D5}.m", allChIso0Idx);
                        defeature.WriteMatlabFile(path, mzMargin, etMargin, run);

                        var channel0Quantity = defeature[0].GetQuantity(run);
                        allChIso0File.Write("{0:D5}\t", allChIso0Idx);
                        for (int i = 0; i <maxIso0key; i += 10)
                        {
                            allChIso0File.Write("{0:F}\t", defeature[i].GetQuantity(run));
                        }
                        for (int i = 0; i < maxIso0key; i += 10)
                        {
                            allChIso0File.Write("{0:F}\t", 100*defeature[i].GetQuantity(run)/channel0Quantity);
                        }
                        var ms2Info = String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                            defeature.InitMs2Result.Charge,
                            defeature.InitMs2Result.ChannelNum,
                            defeature.InitMs2Result.IsotopeIdx,
                            defeature.InitMs2Result.Qvalue,
                            defeature.InitMs2Result.ScanNum,
                            defeature.InitMs2Result.Peptide);
                        allChIso0File.WriteLine(ms2Info);
                        allChIso0Idx++;
                    }
                }
            }
            failedAllChIso0File.Close();
            failedAllChFile.Close();
            allChIso0File.Close();
        }
 
 
        
        
        
        [Test]
        public void TestGatherTruePosDeFeatures()
        {
            var ms2ContainingXicPackets = new Ms2ContainingXicPacketList(run, tolerance, minMz, maxMz, etstart, etend);
            //const string searchResultPath = @"E:\MassSpec\DE_Trypsin_6plex\charge_fixed_search_results\DE_6plexing_2ug_J100_NAQ454_0114_charge_fixed-merged.tsv";
            const string searchResultPath = @"E:\MassSpec\DE_Trypsin_6plex\charge_fixed_search_results\DE-Mix1_2ug_J100_NAQ454_0119_charge_fixed-merged.tsv";
            //const string searchResultPath = @"E:\MassSpec\DE_Trypsin_6plex\charge_fixed_search_results\DE-Mix3_2ug_J100_NAQ454_0119_charge_fixed-merged.tsv";
            var searchResult = new ChargeFixedSearchResults(searchResultPath);
            int totalMs2Points = 0;
            int totalMs2SearchedXic = 0;
            int totalMs2ConsistentXic = 0;

            //Add MS2 search results
            foreach (XicPacket xpac in ms2ContainingXicPackets)
            {
                totalMs2Points += xpac.Ms2TriggeringScanNumbers.Count;
                xpac.AddSearchResults(searchResult);

                if (xpac.HasMs2Result())
                {
                    totalMs2SearchedXic++;

                    if (xpac.Ms2ResultsConsistent())
                    {
                        totalMs2ConsistentXic++;
                    }
                    
                    /*else
                    {
                        Console.WriteLine("this ms2Result is not consistent");
                        foreach (var m1 in xpac.Ms2ResultsList)
                        {
                            foreach (var m2 in m1)
                            {
                                m2.PrintValues();
                            }
                        }
                        Console.WriteLine("");
                    }*/
                }
            }
            Console.WriteLine(@"Total MS2 containing XicPackets: {0}", ms2ContainingXicPackets.Count);
            Console.WriteLine(@"Total MS2 scans: {0}", totalMs2Points);
            Console.WriteLine(@"Total Xic with search result (satisfying QValue {0}, N-term DE) : {1}", XicPacket.Ms2IdQvalThreshold, totalMs2SearchedXic);
            Console.WriteLine(@"Total Xic with consistent search result : {0}", totalMs2ConsistentXic);
            Console.WriteLine("");

            //Collect true positive DEFeatures
            Dictionary<string,int> DEFeatureCount = new Dictionary<string, int>();
            Dictionary<int,int> DEFeatureChannelCount = new Dictionary<int, int>() { {0, 0}, {1, 0}, {2, 0}, {3, 0}, {4, 0}, {5, 0}, {6, 0} };
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            HashSet<XicPacket> DEFeatureScannedXicPacket = new HashSet<XicPacket>();

            foreach (XicPacket initXpac in ms2ContainingXicPackets)
            {
                if (DEFeatureScannedXicPacket.Contains(initXpac))
                {
                    continue;
                }
                if ((!initXpac.HasMs2Result()) || (!initXpac.Ms2ResultsConsistent()))
                {
                    continue;
                }
                var DEFeature = new TruePosDEFeature(initXpac, run, ms2ContainingXicPackets, tolerance, DEFeatureScannedXicPacket);

                if (DEFeature.Count > 0)
                {
                    int[] channelFound = {0, 0, 0, 0, 0, 0};
                    int includedChannel = 0;

                    foreach (XicPacket deXpac in DEFeature)
                    {
                        channelFound[deXpac.Ms2ResultsSet.First().ChannelNum] += 1;
                        if (channelFound[deXpac.Ms2ResultsSet.First().ChannelNum] == 1)
                            includedChannel += 1;
                    }

                    DEFeatureChannelCount[includedChannel] += 1;
                    string channelFoundStr = string.Join(" ", channelFound);
                    if (DEFeatureCount.ContainsKey(channelFoundStr))
                        DEFeatureCount[channelFoundStr] += 1;
                    else
                        DEFeatureCount[channelFoundStr] = 1;
                }
            }

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine(@"Time used for making DEFeatures: {0}", elapsedTime);
            Console.WriteLine("");

            Console.WriteLine(@"Number of distinct channel in each DEFeature");
            for (var i = 0; i < 7; i++)
            {
                Console.WriteLine("{0}: {1}",i ,DEFeatureChannelCount[i]);
            }
            Console.WriteLine("");

            Console.WriteLine(@"Number of XicPacket in each channel of each DEFeature");
            List<string> keys = DEFeatureCount.Keys.ToList();
            keys.Sort();
            Console.WriteLine("0 1 2 3 4 5");
            foreach (var key in keys)
            {
                Console.Write("{0} : {1}\n", key, DEFeatureCount[key]);
            }
        }


        /*
        [Test]
        public void TestAppendingSearchResults()
        {
            var ms2ContainingXicPackets = new Ms2ContainingXicPacketList(run, tolerance, minMz, maxMz, etstart, etend);
            const string searchResultPath = @"E:\MassSpec\DE_Trypsin_6plex\charge_fixed_search_results\DE_6plexing_2ug_J100_NAQ454_0114_charge_fixed-merged.tsv";
            var searchResult = new ChargeFixedSearchResults(searchResultPath);
            int totalMs2Points = 0;
            int totalIdAppendedMs2 = 0;
            Dictionary<int,int> ms2NumChargesDist = new Dictionary<int, int>();
            Dictionary<int,int> ms2NumChannelsDist = new Dictionary<int, int>();
            Dictionary<int,int> ms2NumIdsDist = new Dictionary<int, int>();
            Dictionary<int,int> ms2ChannelCount = new Dictionary<int, int>() { {0, 0}, {1, 0}, {2, 0}, {3, 0}, {4, 0}, {5, 0}, };
            Dictionary<int,int> ms2IsotopeCount = new Dictionary<int, int>() { {0, 0}, {1, 0}, {2, 0}, {3, 0}, {4, 0}, {5, 0}, {6, 0}};

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (XicPacket xpac in ms2ContainingXicPackets)
            {
                xpac.AddSearchResults(searchResult);
                totalMs2Points += xpac.Ms2TriggeringScanNumbers.Count;

                // calculates search result stats
                foreach (List<Ms2Result> ms2ResultList in xpac.Ms2ResultsList)
                {
                    if (ms2ResultList.Count >= 1)
                        totalIdAppendedMs2++;

                    if (ms2NumIdsDist.ContainsKey(ms2ResultList.Count))
                        ms2NumIdsDist[ms2ResultList.Count]++;
                    else
                        ms2NumIdsDist[ms2ResultList.Count] = 1;

                    List<int> chargeList = new List<int>();
                    List<int> channelList = new List<int>();
                    foreach (Ms2Result ms2Result in ms2ResultList)
                    {
                        chargeList.Add(ms2Result.Charge);
                        channelList.Add(ms2Result.ChannelNum);
                        ms2ChannelCount[ms2Result.ChannelNum]++;
                        ms2IsotopeCount[ms2Result.IsotopeIdx]++;
                    }
                    HashSet<int> chargeSet = new HashSet<int>(chargeList);
                    HashSet<int> channelSet = new HashSet<int>(channelList);

                    if (ms2NumChargesDist.ContainsKey(chargeSet.Count))
                        ms2NumChargesDist[chargeSet.Count]++;
                    else
                        ms2NumChargesDist[chargeSet.Count] = 1;

                    if (ms2NumChannelsDist.ContainsKey(channelSet.Count))
                        ms2NumChannelsDist[channelSet.Count]++;
                    else
                        ms2NumChannelsDist[channelSet.Count] = 1;

                }
            }

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine(@"Time used for parsing MS2 search results: {0}", elapsedTime);

            // printing search result stats
            Console.WriteLine(@"Total MS2 containing XicPackets: {0}", ms2ContainingXicPackets.Count);
            Console.WriteLine(@"Total MS2 scans: {0}", totalMs2Points);
            Console.WriteLine(@"Total MS2 scans with ID (satisfying QValue {0}, N-term DE) : {1}", XicPacket.Ms2IdQvalThreshold, totalIdAppendedMs2);

            Console.WriteLine("");
            Console.WriteLine(@"Total IDed peptides in each channel");
            foreach (KeyValuePair<int, int> entry in ms2ChannelCount)
            {
                Console.WriteLine(@"{0}: {1}", entry.Key, entry.Value);
            }

            Console.WriteLine("");
            Console.WriteLine(@"Total IDed peptides with each Isotope Error");
            foreach (KeyValuePair<int, int> entry in ms2IsotopeCount)
            {
                Console.WriteLine(@"{0}: {1}", entry.Key, entry.Value);
            }

            Console.WriteLine("");
            Console.WriteLine(@"Distribution of Num of IDs of each MS2 scan");
            foreach (KeyValuePair<int, int> entry in ms2NumIdsDist)
            {
                Console.WriteLine(@"{0}: {1}", entry.Key, entry.Value);
            }

            Console.WriteLine("");
            Console.WriteLine(@"Distribution of Num of charges of each MS2 scan");
            foreach (KeyValuePair<int, int> entry in ms2NumChargesDist)
            {
                Console.WriteLine(@"{0}: {1}", entry.Key, entry.Value);
            }

            Console.WriteLine("");
            Console.WriteLine(@"Distribution of Num of channels of each MS2 scan");
            foreach (KeyValuePair<int, int> entry in ms2NumChannelsDist)
            {
                Console.WriteLine(@"{0}: {1}", entry.Key, entry.Value);
            }

        }
        */


        /*
        [Test]
        public void TestMs2NearbyXic()
        {
            var ms2ContainingXicList = new Ms2ContainingXicPacketList(run, tolerance);
            Console.WriteLine(@"MS2 containing XIC selection done..");
            var testXicNum = 1;
            var testSepXicNum = 1;
            var totalXicPointCount = 0;
            string path = @"D:\DMDE\Temp\ms2nearbyXic.m";
            path = @"E:\MassSpec\DE\ms2nearbyXic.m";
            using (var fs = File.Create(path))
            {
                for(var i=0;i<ms2ContainingXicList.Ms2ScanNumList.Count;i++)
                {
                    var testidx = ms2ContainingXicList.Ms2ScanNumList[i];
                    var spec = run.GetSpectrum(testidx);
                    
                    var ps = spec as ProductSpectrum;
                    double mz = ps.IsolationWindow.IsolationWindowTargetMz;
                    double et = run.GetElutionTime(testidx);

                    if (minMz > mz || maxMz < mz) continue;
                    if (etstart > et || etend < et) continue;

                    var xpac = new XicPacket(ms2ContainingXicList[i]);
                    xpac.SetMs2SpectraScanNum(run, tolerance);
                    xpac.WriteMatlabVariable(fs, "xpn2{" + testXicNum + "}", run);
                    
                    var sepXicList = new XicPacketList(ms2ContainingXicList[i], run);
                    foreach (var sepXic in sepXicList)
                    {
                        sepXic.SetChargeAndIsotopeIndex(run, tolerance);
                        if(sepXic.Charge > 1) Console.WriteLine(sepXic.Charge + " " + sepXic.IsotopeIdx + " " + sepXic.GetApex().Mz + " " + run.GetElutionTime(sepXic.GetApex().ScanNum));
                        sepXic.WriteMatlabVariable(fs, "sxpn2{" + testSepXicNum++ + "}", run);
                    }                    
                    totalXicPointCount += xpac.Count;
                    testXicNum++;
                }
            }
            Console.WriteLine(testXicNum);
            Console.WriteLine(testSepXicNum);
            Console.WriteLine(totalXicPointCount);
        }
        */

        /*
        [Test]
        public void RunForSunny()
        {
            var file = new StreamReader(@"E:\MassSpec\speclis.txt");
            var outFile = @"E:\MassSpec\speclisWithIntensities.txt";
            var outStringBuilder = new StringBuilder();
            string s;
            var specDictionary = new Dictionary<string, List<int>>();
            while ((s = file.ReadLine()) != null)
            {
                var token = s.Split('\t');
                if(!specDictionary.ContainsKey(token[0])) specDictionary[token[0]] = new List<int>();
                specDictionary[token[0]].Add(int.Parse(token[1]));
            }

            foreach (var specfile in specDictionary.Keys)
            {
                Console.WriteLine(specfile);
                var run = InMemoryLcMsRun.GetLcMsRun(@"E:\MassSpec\smORFNew\" + specfile, MassSpecDataType.MzMLFile);
                foreach (var sn in specDictionary[specfile])
                {
                    var spec = run.GetSpectrum(sn);
                    var ps = spec as ProductSpectrum;
                    var mz = ps.IsolationWindow.IsolationWindowTargetMz;
                    var nearXic = run.GetPrecursorExtractedIonChromatogram(mz, tolerance, sn);
                    outStringBuilder.AppendLine(specfile + @"\t" + sn + @"\t" + nearXic.GetArea());
                }
            }
            File.WriteAllText(outFile, outStringBuilder.ToString());
        }
        */
       
      
        /*
        [Test]
        public void TestStatistics()
        {
            Console.WriteLine(TestRawFilePath);
            var totalMs2Count = run.GetScanNumbers(2).Count;
            var bxps = new BinnedXicPackets(run, run.MinMs1Mz, run.MaxMs1Mz, BinSize, tolerance);
            var totalXpacCount = bxps.GetTotalXicPacketCount();
            var xpacList = bxps.GetXicPackets();
            var defCount = 0;
            var deftCount = 0;
            var xpacInDfSet = new HashSet<XicPacket>();
            var ms2InDfScanNumberSet = new HashSet<int>();
            var ms2TriggeredXpacIntensities = new StringBuilder();
            var ms2TriggeredXpacInDfIntensities = new StringBuilder();

            ms2TriggeredXpacIntensities.Append("ms2inXpacint=[");
            ms2TriggeredXpacInDfIntensities.Append("ms2inDFint=[");
            foreach (var xpac in xpacList)
            {
                if (xpac.SetMs2SpectraScanNum(run, tolerance).Count > 0)
                {
                    ms2TriggeredXpacIntensities.Append(xpac.GetCachedApex().Intensity + " ");
                }
                for (var charge = 6; charge >= 2; charge--)
                {
                    var mzdiff = channelDiff / charge;
                    var defeature = new DeFeature(bxps, run, xpac, tolerance, cosineCutoff, etdiff, mzdiff, numchannel);
                    if (defeature.Count < 4) continue;
                    defCount++;
                    foreach (var xpacIndf in defeature.Values)
                        xpacInDfSet.Add(xpacIndf);
                    if (defeature.TriggeredMs2ScanNumbers.Count > 0) deftCount++;
                    foreach (var sn in defeature.TriggeredMs2ScanNumbers) ms2InDfScanNumberSet.Add(sn);
                }
            }
            foreach (var xpac in xpacInDfSet)
            {
                if (xpac.SetMs2SpectraScanNum(run, tolerance).Count > 0)
                {
                    ms2TriggeredXpacInDfIntensities.Append(xpac.GetCachedApex().Intensity + " ");
                }
            }
            ms2TriggeredXpacIntensities.Append("];");
            ms2TriggeredXpacInDfIntensities.Append("];");
           

            Console.WriteLine("Total MS/MS spectra count : {0}", totalMs2Count);
            Console.WriteLine("Total XIC packet count : {0}", totalXpacCount);
            Console.WriteLine("DEF count : {0}", defCount);
            Console.WriteLine("XIC packets within DEFs count : " + xpacInDfSet.Count);
            Console.WriteLine("DEF triggering at least one MS/MS spectrum count : {0}", deftCount);
            Console.WriteLine("MS/MS spectra triggered within DEFs count : {0}", ms2InDfScanNumberSet.Count);
            System.IO.File.WriteAllText(@"E:\MassSpec\DE\intensitiesinxpac.m", ms2TriggeredXpacIntensities.ToString());
            System.IO.File.WriteAllText(@"E:\MassSpec\DE\intensitiesinde.m", ms2TriggeredXpacInDfIntensities.ToString());
        }
        */

        /*
        [Test]
        public void TestHello()
        {
            //Console.WriteLine(Normal.InvCDF(0, 1, .01));
            //Console.WriteLine(Normal.PDF(0, 1, -2.5));

            var run = InMemoryLcMsRun.GetLcMsRun(TestRawFilePath, MassSpecDataType.XCaliburRun);
            var tolerance = new Tolerance(10);
            int b = DateTime.Now.Millisecond;

            double minMz = run.MinMs1Mz;// 634;
            double maxMz = run.MaxMs1Mz;//670;
            const double binSize = 0.997;
            double etstart = run.GetElutionTime(run.MinLcScan);//56;
            double etend = run.GetElutionTime(run.MaxLcScan) + 1;//76;
            //400 - 460 -> 400 - 430
            //430 - 490 -> 430 - 460

            var bxps = new BinnedXicPackets(run, minMz, maxMz, binSize, tolerance);

           
            return;
            
            //test GetXicPacketsWithinTime
            const double searchMinMz = 400;
            const double searchMaxMz = 1600;
            //const double searchMinMz = 635;
            //const double searchMaxMz = 640;
            const double searchEtStart = 0;
            const double searchEtEnd = 120;
            //const double searchEtStart = 58;
            //const double searchEtEnd = 60;

            var xpacList = bxps.GetXicPacketsWithinTime(searchMinMz, searchMaxMz, searchEtStart, searchEtEnd);
            Console.WriteLine("xic packet count: {0}", xpacList.Count);
            var ms2TriggeredXpacCntr = 0;
            var ms2SpecScanNumSet = new HashSet<int>();
            foreach (var xpac in xpacList)
            {
                var ms2ScanNum = xpac.SetMs2SpectraScanNum(run, tolerance);
                foreach (var sn in ms2ScanNum) ms2SpecScanNumSet.Add(sn);
                if (ms2ScanNum.Count > 0) ms2TriggeredXpacCntr++;
            }
            Console.WriteLine("Total ms2 spectra count: {0}", run.GetScanNumbers(2).Count);
            
            Console.WriteLine("MS2 triggered xic packet count : {0}", ms2TriggeredXpacCntr);
            Console.WriteLine("xic packet triggered MS2 count: {0}", ms2SpecScanNumSet.Count);

            //System.IO.File.WriteAllText(@"C:\Users\Yeon\Source\Repos\DEAQ\tmp.m", bxps.GetMatlabVariables(minMz, maxMz, etstart, etend, xpacList));

            //test GetRelatedXicPackets
            const double massH = 1.007825;
            const double massD = 2.01410178;
            const double channelDiff = 4 * (massD - massH);
            const double etdiff = 0.04;
            const int numchannel = 6;
            const double consineCutoff = .7;
            int charge;
            double mzdiff;
            int relatedPacketCount;
            var MS2TriggeredRelatedPacketCount = 0;
            var RelatedPacketTriggeredMS2Count = new Dictionary<int, List<int>>();
           // var searchTolerance = new Tolerance(20);
            bool foundAll;

            var i = 0;
            
            for (charge = 2; charge <= 6; charge++)
            {
                mzdiff = channelDiff/charge;
                relatedPacketCount = 0;
                MS2TriggeredRelatedPacketCount = 0;
                RelatedPacketTriggeredMS2Count[charge] = new List<int>();
                foreach (var xpac in xpacList)
                {
                    //Console.WriteLine("test xic {0} {1}", xpac[0].Mz, run.GetElutionTime(xpac[0].ScanNum));
                    var relatedXpacList = bxps.GetRelatedXicPackets(xpac, tolerance, consineCutoff, etdiff, mzdiff, numchannel,
                        out foundAll);

                    if (!foundAll)
                    {                
                       
                        continue;
                    }
                    //Console.WriteLine("{0} {1} {2}", foundAll, relatedXpacList.Count, featurePacketCount);
                    //System.IO.File.WriteAllText(@"C:\Users\Yeon\Source\Repos\DEAQ\tmp" + i + ".m",
                    //    bxps.GetMatlabVariables(minMz, maxMz, etstart, etend, relatedXpacList));
                    relatedPacketCount++;
                    bool counted = false;
                    foreach(var rxpac in relatedXpacList)
                    {
                        var sns = rxpac.SetMs2SpectraScanNum(run, tolerance);
                        RelatedPacketTriggeredMS2Count[charge].AddRange(sns);
                        
                        if (!counted && sns.Count > 0)
                        {
                            MS2TriggeredRelatedPacketCount++;
                            counted = true;
                        }
                    }
                }
                Console.WriteLine("Number of related Xic packets found using charge {0} : {1}", charge, relatedPacketCount);
                Console.WriteLine("Number of MS2 triggered related Xic packets found using charge {0} : {1}", charge, MS2TriggeredRelatedPacketCount);
                Console.WriteLine("Number of related Xic packet triggered MS2 spectra found using charge {0} : {1}", charge, RelatedPacketTriggeredMS2Count[charge].Count);
                Console.WriteLine("Number of related Xic packet triggered unique MS2 spectra found using charge {0} : {1}", charge, new HashSet<int>(RelatedPacketTriggeredMS2Count[charge]).Count);
            }

            var uniqueScanNumberSet = new HashSet<int>();
            foreach (var key in RelatedPacketTriggeredMS2Count.Keys)
            {
                foreach (var sn in RelatedPacketTriggeredMS2Count[key])
                    uniqueScanNumberSet.Add(sn);
            }
            Console.WriteLine("Number of related Xic packet triggered unique MS2 spectra found : {0}", uniqueScanNumberSet.Count);

            //System.IO.File.WriteAllText(@"C:\Users\Yeon\Source\Repos\DEAQ\tmp.m", bxps.GetMatlabVariables(minMz, maxMz, etstart, etend));

            //Console.WriteLine();
        }*/

        /*
            var ms1ScanNum = run.GetScanNumbers(1);
            Console.WriteLine("{0} {1}", ms2ScanNum.Count, ms1ScanNum.Count    );
            foreach (int item in ms2ScanNum.Skip(10000).Take(10))
            {
                Console.Write(item);
                Console.Write(" ");
            } */

        /*      
        [Test]
        public void TestDEFeature()
        {
            double mzMargin = 0.5;
            double etMargin = 0.02; 

            var ms2ContainingXicPackets = new Ms2ContainingXicPacketList(run, tolerance, minMz, maxMz, etstart, etend);
            const string ResultsPath = BasePath + @"results_M3_2\";
            const string searchResultPath = BasePath + @"newChargeFixedSearchResults\5plex_M3_Dimet_J70_NAQ1_0808_2.chargeFixed-merged.tsv";

            Console.WriteLine("{0}", TestRawFilePath);
            Console.WriteLine("{0}", searchResultPath);
            Console.WriteLine("");

            var searchResult = new ChargeFixedSearchResults(searchResultPath);
            int totalMs2Points = 0;
            int totalMs2SearchedXic = 0;
            int totalMs2ConsistentXic = 0;
            int totalConflictedMs2ScansXic = 0;

            //Add MS2 search results
            foreach (XicPacket xpac in ms2ContainingXicPackets)
            {
                totalMs2Points += xpac.Ms2TriggeringScanNumbers.Count;
                xpac.AddSearchResults(searchResult);

                if (xpac.HasMs2Result())
                {
                    totalMs2SearchedXic++;

                    if (xpac.Ms2ResultsConsistent())
                    {
                        totalMs2ConsistentXic++;
                    }
                    else if (xpac.Ms2SearchedScanNum().Count > 1)
                    {
                        Console.WriteLine("\n{0}, {1}", totalConflictedMs2ScansXic, xpac.Ms2SearchedScanNum().Count);
                        foreach (var ms2Result in xpac.Ms2ResultsSet)
                        {
                            ms2Result.PrintValues();
                        }
                        var path = string.Format(ResultsPath + @"InconsistentXicPacket\XicPacket{0:D5}.m", totalConflictedMs2ScansXic);
                        totalConflictedMs2ScansXic++;
                        using (var fs = File.Create(path))
                        {
                            var writeMinMz = xpac.GetCachedApex().Mz - mzMargin;
                            var writeMaxMz = xpac.GetCachedApex().Mz + mzMargin;
                            var writeEtstart = run.GetElutionTime(xpac[0].ScanNum) - etMargin;
                            var writeEtend = run.GetElutionTime(xpac[xpac.Count-1].ScanNum) + etMargin;

                            var nearXic = run.GetPrecursorExtractedIonChromatogram(writeMinMz, writeMaxMz);
                            var nearXpac = new XicPacket(nearXic);
                            nearXpac.SetMs2SpectraScanNum(run, tolerance);
                            nearXpac.WriteMatlabVariable(fs, "allxic", run, writeEtstart, writeEtend);
                            xpac.WriteMatlabVariable(fs, "xp", run);
                        }
                    }
                }
            }
            Console.WriteLine(@"Total MS2 containing XicPackets: {0}", ms2ContainingXicPackets.Count);
            Console.WriteLine(@"Total MS2 scans: {0}", totalMs2Points);
            Console.WriteLine(@"Total Xic with search result (satisfying QValue {0}, N-term DE) : {1}", XicPacket.Ms2IdQvalThreshold, totalMs2SearchedXic);
            Console.WriteLine(@"Total Xic with consistent search result : {0}", totalMs2ConsistentXic);
            Console.WriteLine(@"Total Xic carrying many MS2 Scans, with confilicting search results : {0}", totalConflictedMs2ScansXic);
            Console.WriteLine("");


            //Collect DEFeatures
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var deFeatureList = new DEFeatureList();

            foreach (XicPacket initXpac in ms2ContainingXicPackets)
            {
                if ((!initXpac.HasMs2Result()))
                {
                    continue;
                }
                var deFeature = new DEFeature(initXpac, run, tolerance, searchResult);
                deFeatureList.Add(deFeature);

            }
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine(@"Time used for making DEFeatures: {0}", elapsedTime);
            Console.WriteLine("");
            deFeatureList.PrintStats();


            deFeatureList.RemoveOverLappedDEFeature(tolerance);

            Console.WriteLine("Stat After removing overlapped DEFeatures:");
            deFeatureList.PrintStats();


            int matlabfileIdx = 0;
            foreach (var defeature in deFeatureList)
            {
                bool allChannelIsotope0 = true;
                for (int i = 0; i < 41; i += 10)
                {
                    if (!defeature.ContainsKey(i))
                    {
                        allChannelIsotope0 = false;
                        break;
                    }
                }

                if (allChannelIsotope0)
                {
                    var path = string.Format(ResultsPath + @"DEFeature_Matlab\defeature{0:D5}.m", matlabfileIdx);

                    using (var fs = File.Create(path))
                    {
                        var writeMinMz = defeature.MinMz - mzMargin;
                        var writeMaxMz = defeature.MaxMz + mzMargin;
                        var writeEtstart = run.GetElutionTime(defeature.MinSn) - etMargin;
                        var writeEtend = run.GetElutionTime(defeature.MaxSn) + etMargin;

                        var xic = run.GetPrecursorExtractedIonChromatogram(writeMinMz, writeMaxMz);
                        var xpac = new XicPacket(xic);
                        xpac.WriteMatlabVariable(fs, "allxic", run, writeEtstart, writeEtend);

                        var channel0Quantity = defeature[0].GetQuantity(run);
                        Console.Write("{0:D5}\t", matlabfileIdx);
                        for (int i = 0; i < 41; i += 10)
                        {
                            //Console.Write("{0:F}\t", 100*defeature[i].GetQuantity(run)/channel0Quantity);
                            Console.Write("{0:F}\t", defeature[i].GetQuantity(run));
                            defeature[i].WriteMatlabVariable(fs, "xp{" + (i/10+1) + "}", run);
                        }
                        for (int i = 0; i < 41; i += 10)
                        {
                            Console.Write("{0:F}\t", 100*defeature[i].GetQuantity(run)/channel0Quantity);
                        }
                        Console.Write("{0}\t{1}\t{2}\t{3}\t{4}", defeature.InitMs2Result.Charge, defeature.InitMs2Result.ChannelNum, defeature.InitMs2Result.IsotopeIdx, defeature.InitMs2Result.Qvalue, defeature.InitMs2Result.ScanNum);
                        Console.Write("\n");
                        matlabfileIdx++;
                    }
                }
            }

        }*/

    }
}

