using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DEmain;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using NUnit.Framework;

namespace InformedProteomics.Test.DE
{
    [TestFixture]
    class TestDeTools
    {
        Tolerance tolerance = new Tolerance(20);
        
        [Test]
        public void TestRunManyChargeDetermination()
        {
            string[] rawPathArr =
            {
				@"E:\MassSpec\201604_DM_3plex\rawfiles\DM_1000_10_1_NAQ448_J100_0422.raw",
            };

            for (int i = 0; i < rawPathArr.Count(); i++)
            {
                var rawPath = rawPathArr[i];
                var mgfPath = Path.ChangeExtension(rawPath, ".chargeFixed.mgf");
                LcMsRun eachrun = InMemoryLcMsRun.GetLcMsRun(rawPath, MassSpecDataType.XCaliburRun);
                RunChargeDetermination(eachrun, mgfPath);
            }
        }

        [Test]
        public void TestRunManyShape()
        {
         const string BasePath = @"E:\MassSpec\2014_DM_Try_5plex\";//@"E:\MassSpec\201604_DM_Try_3plex\";//@"C:\scratch\";

            string[] rawPathArr =
            {
                @"E:\MassSpec\DMDE\raw_data\2014-08-11(DM_5plex_mix)\5plex_M1_Dimet_J70_NAQ1_0808_1.raw",
                @"E:\MassSpec\DMDE\raw_data\2014-08-11(DM_5plex_mix)\5plex_M1_Dimet_J70_NAQ1_0808_2.raw",
                @"E:\MassSpec\DMDE\raw_data\2014-08-11(DM_5plex_mix)\5plex_M2_Dimet_J70_NAQ1_0808_1.raw",
                @"E:\MassSpec\DMDE\raw_data\2014-08-11(DM_5plex_mix)\5plex_M2_Dimet_J70_NAQ1_0808_2.raw",
                @"E:\MassSpec\DMDE\raw_data\2014-08-11(DM_5plex_mix)\5plex_M3_Dimet_J70_NAQ1_0808_1.raw",
                @"E:\MassSpec\DMDE\raw_data\2014-08-11(DM_5plex_mix)\5plex_M3_Dimet_J70_NAQ1_0808_2.raw",
            };
            string[] resultsPathArr =
            {
                BasePath + @"DEFeatureResults\M1_1\",
                BasePath + @"DEFeatureResults\M1_2\",
                BasePath + @"DEFeatureResults\M2_1\",
                BasePath + @"DEFeatureResults\M2_2\",
                BasePath + @"DEFeatureResults\M3_1\",
                BasePath + @"DEFeatureResults\M3_2\",
                
            };
            string[] searchResultArr =
            {
                BasePath + @"ChargeFixedSearchResults\5plex_M1_Dimet_J70_NAQ1_0808_1.chargeFixed-merged.tsv",
                BasePath + @"ChargeFixedSearchResults\5plex_M1_Dimet_J70_NAQ1_0808_2.chargeFixed-merged.tsv",
                BasePath + @"ChargeFixedSearchResults\5plex_M2_Dimet_J70_NAQ1_0808_1.chargeFixed-merged.tsv",
                BasePath + @"ChargeFixedSearchResults\5plex_M2_Dimet_J70_NAQ1_0808_2.chargeFixed-merged.tsv",
                BasePath + @"ChargeFixedSearchResults\5plex_M2_Dimet_J70_NAQ1_0808_1.chargeFixed-merged.tsv",
                BasePath + @"ChargeFixedSearchResults\5plex_M2_Dimet_J70_NAQ1_0808_2.chargeFixed-merged.tsv",
            };

            for (int i = 0; i < rawPathArr.Count(); i++)
            {
                var rawPath = rawPathArr[i];
                var resultPath = resultsPathArr[i];
                var searchResultPath = searchResultArr[i];
                GetEachChannelShape(rawPath, searchResultPath, resultPath);
            }
        }

        public void RunChargeDetermination(LcMsRun thisRun, string mgfpath)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            double minMz = thisRun.MinMs1Mz;
            double maxMz = thisRun.MaxMs1Mz;
            double etstart = thisRun.GetElutionTime(thisRun.MinLcScan);
            double etend = thisRun.GetElutionTime(thisRun.MaxLcScan) + 1;

            var ms2ContainingXicPackets = new Ms2ContainingXicPacketList(thisRun, tolerance, minMz, maxMz, etstart, etend);
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
                var diffCount = 0;
                var diffHighScoreCount = 0;

                using (var mgfWriter = new StreamWriter(mgfpath))
                {
                    var dummyScanNumber = 1;
                    foreach (var xicPac in ms2ContainingXicPackets)
                    {
                        // xicPac.SetChargeAndIsotopeIndex(thisRun, tolerance, false, method);
                        if (xicPac.MaxScoringCharge != xicPac.IsolationWindowCharge)
                        {
                            diffCount++;
                            if (xicPac.GetMaxChargeScore() > threshold)
                                // && xicPac.Charge * 2 == xicPac.IsolationWindowCharge)
                            {
                                //  xicPac.SetChargeAndIsotopeIndex(thisRun, tolerance, true, method);
                                diffHighScoreCount++;
                            }
                        }
                        if (xicPac.GetMaxChargeScore() < threshold) continue;
                        foreach (var c in xicPac.ChargeScores.Keys)
                        {
                            if (!(xicPac.ChargeScores[c] >= threshold)) continue;
                            if (!chargeCntrAllowingMultipleCharges.ContainsKey(c))
                                chargeCntrAllowingMultipleCharges[c] = 0;
                            chargeCntrAllowingMultipleCharges[c]++;

                            foreach (var ms2sn in xicPac.Ms2TriggeringScanNumbers)
                            {
                                var spec = thisRun.GetSpectrum(ms2sn);
                                var ps = spec as ProductSpectrum;
                                if (ps == null) continue;
                                mgfWriter.WriteLine(ps.ToMgfString(c, dummyScanNumber++,
                                    ms2sn + " " + xicPac.IsolationWindowCharge));
                            }



                            //  xicPac.Ms2TriggeringScanNumbers

                        }

                        chargeDeterminedCount++;
                        if (!chargeCntr.ContainsKey(xicPac.MaxScoringCharge)) chargeCntr[xicPac.MaxScoringCharge] = 0;
                        chargeCntr[xicPac.MaxScoringCharge]++;
                    }
                }
                Console.WriteLine(@"Total MS2 containing XIC packets: " + ms2ContainingXicPackets.Count);
                Console.WriteLine(@"Total Charge Determined XIC packets: " + chargeDeterminedCount);
                Console.WriteLine(@"Total Xcal/our charge different XIC packets: " + diffCount);
                Console.WriteLine(@"Total Xcal/our charge different XIC packets with high score: " + diffHighScoreCount);

                for (var c = 1; c < 8; c++)
                {
                    if (!chargeCntr.ContainsKey(c)) continue;
                    Console.WriteLine(@"Max Charge {0} XIC Packets: {1}", c, chargeCntr[c]);
                }

                for (var c = 1; c < 8; c++)
                {
                    if (!chargeCntrAllowingMultipleCharges.ContainsKey(c)) continue;
                    Console.WriteLine(@"Charge {0} XIC Packets (allowing multiple charges): {1}", c,
                        chargeCntrAllowingMultipleCharges[c]);
                }
            }
        }

        public void GetEachChannelShape(string TestRawFilePath, string searchResultPath, string resultsPath)
        {
            LcMsRun run = InMemoryLcMsRun.GetLcMsRun(TestRawFilePath, MassSpecDataType.XCaliburRun);
            Tolerance tolerance = new Tolerance(20);
            double minMz = run.MinMs1Mz;// 634;//
            double maxMz = run.MaxMs1Mz;//670;//
            double etstart = run.GetElutionTime(run.MinLcScan);//50;//
            double etend = run.GetElutionTime(run.MaxLcScan) + 1;//66;//


            Console.WriteLine("{0}", TestRawFilePath);
            Console.WriteLine("{0}", searchResultPath);
            Console.WriteLine("");

            string shapeFilePath = resultsPath + "ChannelShape.txt";
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
    }



}
