using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using MathNet.Numerics.Statistics;

namespace Epiq
{
    public class IsotopeImpurityMeasurement
    {
        private readonly double zeroPeakIntCutOff = 50000000;
        private readonly double trimmedPortionForMean = 0.2;
        private LcMsRun _run;
        private Tolerance _1stTolerance;
        private Tolerance _2ndTolerance = new Tolerance(2.5);
        private List<Ms2Result> _filteredIds;
        private Dictionary<char, HashSet<string>> _usedAtoms;
        private HashSet<char> _allowedLabeledSites = new HashSet<char>();
        private string _outPath;

        public IsotopeImpurityMeasurement(string raw, string tsv, Tolerance tolerance, string usedAtoms, string outPath, string labelSiteSetStr)
        {
            foreach (var site in labelSiteSetStr.Split('|'))
            {
                if (site.Length > 1) throw new Exception(String.Format("labeling site should be one character. {0} was given.", site));
                _allowedLabeledSites.Add(Convert.ToChar(site));
            }

            _run = InMemoryLcMsRun.GetLcMsRun(raw, MassSpecDataType.XCaliburRun);
            _1stTolerance = tolerance;
            Params.InitRtParams(_run);
            _filteredIds = new List<Ms2Result>();
            ParseUsedAtomString(usedAtoms);
            _outPath = outPath;

            var tcntr = 0;
            foreach (var id in new Ms2ResultList(new SearchResults(tsv, false, Params.TrainingSpecEvalueThreshold), retainBestOutofSameUnlabeledPeptide:true))
            {
                tcntr++;
                if (id.IsDecoy()) continue;
                _filteredIds.Add(id);
            }
            Console.WriteLine(@"Filtered psm count (spec-evalue threshold of {0:e2}) : {1} out of total psm count {2}",
                                Params.TrainingSpecEvalueThreshold, _filteredIds.Count, tcntr);

        }

        public int Run()
        {
            var overToleranceAtNeg1 = 0;
            var overTolerenceAtNeg2 = 0;
            var recalCntr = 0;
            var resultStringStrList = new List<string>();
            var measuredNeg1Ratios = new List<double>();
            var measuredNeg2Ratios = new List<double>();

            foreach (var id in _filteredIds)
            {
                if (id.NumOfBoundLabels > 1) continue; // ignore multiply labeled peptides
                var labeledAaCounts = id.GetLabeledAaCounts();
                bool unAllowedLabel = false;
                foreach (var aa in labeledAaCounts.Keys)
                {
                    if (!_allowedLabeledSites.Contains(aa) && labeledAaCounts[aa] > 0)
                    {
                        unAllowedLabel = true;
                    }
                }
                if (unAllowedLabel) continue;


                var recaledXic = GetRecalibratedAndSmoothedXic(id);
                if (recaledXic == null) continue;
                recalCntr++;
                double neg1Ratio = 0;
                double neg2Ratio = 0;


                var zeroPeakInt = recaledXic.GetApex().Intensity;
                var zeroPeakMz = (float) recaledXic.GetApex().Mz;

                var zeroMzSet = new HashSet<float> {zeroPeakMz};
                var neg1Mzs = NegativeOneMzList(id, id.Charge, new HashSet<float>(zeroMzSet));
                var neg2Mzs = NegativeOneMzList(id, id.Charge, neg1Mzs);

                var neg1MeanMz = neg1Mzs.Mean();
                var neg2MeanMz = neg2Mzs.Mean();

                var ms = _run.GetSpectrum(recaledXic.GetApexScanNum());

                var neg1Peak = ms.FindPeak(neg1MeanMz, _2ndTolerance);
                if (neg1Peak != null && neg1Mzs.Count > 0)
                    neg1Ratio = neg1Peak.Intensity / zeroPeakInt;

                var neg2Peak = ms.FindPeak(neg2MeanMz, _2ndTolerance);
                if (neg2Peak != null && neg2Mzs.Count > 0)
                    neg2Ratio = neg2Peak.Intensity / zeroPeakInt;

                var usedForNeg1 = false;
                var usedForNeg2 = false;

                if (zeroPeakInt > zeroPeakIntCutOff)
                {
                    if (Math.Abs(neg1Ratio) > 0.00001)
                    {
                        measuredNeg1Ratios.Add(neg1Ratio);
                        usedForNeg1 = true;
                    }
                    if (Math.Abs(neg2Ratio) > 0.00001)
                    {
                        measuredNeg2Ratios.Add(neg2Ratio);
                        usedForNeg2 = true;
                    }
                }

                resultStringStrList.Add(
                    String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}",
                        id.Precursor, id.IsotopeIdx, zeroPeakMz, neg1Mzs.Mean(), neg2Mzs.Mean(), zeroPeakInt,
                        neg1Ratio, neg2Ratio, id.Peptide, id.Charge, id.ScanNum, recaledXic.GetApexScanNum(), usedForNeg1, usedForNeg2));
            }

            using (var fileWriter = new StreamWriter(_outPath))
            {
                Console.WriteLine("Measured Ratio Counts : {0} {1}", measuredNeg1Ratios.Count, measuredNeg2Ratios.Count);
                Console.WriteLine("Median Ratios : {0} {1}", measuredNeg1Ratios.Median(), measuredNeg2Ratios.Median());

                fileWriter.WriteLine("Intensity cutoff for monoisotopic peak : {0}", zeroPeakIntCutOff);
                fileWriter.WriteLine("m/z tolerance for -1, -2 peaks : {0}", _2ndTolerance);
                fileWriter.WriteLine("Measured Ratio Counts : {0} {1}", measuredNeg1Ratios.Count, measuredNeg2Ratios.Count);
                fileWriter.WriteLine();
                fileWriter.WriteLine("\t-1 Ratio\t-2 Ratio");
                fileWriter.WriteLine("Median Ratios\t{0}\t{1}", measuredNeg1Ratios.Median(), measuredNeg2Ratios.Median());
                fileWriter.WriteLine();
                fileWriter.WriteLine("Precursor Mz\tIded Isotope Index\tMonoiso Mz\t-1 Mz\t-2 Mz\tMonoiso Peak Intensity\t-1 Ratio\t-2 Ratio"
                                     + "\tPeptide\tCharge\tID ScanNum\tApex ScanNum\tUsed for -1\tUsed for -2");
                foreach (var str in resultStringStrList)
                {
                    fileWriter.WriteLine(str);
                }
            }

            return 0;


            /*
            var neg1Peaks = new HashSet<Peak>();
            foreach (var mz in neg1Mzs)
            {
                var peak = ms.FindPeak(mz, _2ndTolerance);
                if (peak != null)
                    neg1Peaks.Add(peak);
            } 
            double neg1Ints = 0;
            foreach (var peak in neg1Peaks)
            {
                neg1Ints += peak.Intensity;
            }
            neg1Ratio = neg1Ints / zeroPeakInt;

            var neg2Peaks = new HashSet<Peak>();
            foreach (var mz in neg2Mzs)
            {
                var peak = ms.FindPeak(mz, _2ndTolerance);
                if (peak != null)
                    neg2Peaks.Add(peak);
            } 
            double neg2Ints = 0;
            foreach (var peak in neg2Peaks)
            {
                neg2Ints += peak.Intensity;
            }
            neg2Ratio = neg2Ints / zeroPeakInt;
             */
        }

        private void ParseUsedAtomString(string usedAtoms)
        {
            _usedAtoms = new Dictionary<char, HashSet<string>>();
            
            foreach (var perLabel in usedAtoms.Split(';'))
            {
                var token = perLabel.Split('_');
                var aa = token[0][0];
                _usedAtoms[aa] = new HashSet<string>();
                for (int i = 1; i < token.Length; i++)
                {
                    _usedAtoms[aa].Add(token[i]);
                }
            }
        }

        private HashSet<float> NegativeOneMzList(Ms2Result id, sbyte charge, HashSet<float> zeroMzs)
        {
            // TODO : memoization of this calculation is possible and very easy but not very necessary -_-;;;;
            var ret = new HashSet<float>();
            var aaCounts = id.GetLabeledAaCounts();
            var usedAtomsInThisId = new HashSet<string>();

            foreach (var labeledAa in aaCounts.Keys)
            {
                var aaCount = aaCounts[labeledAa];
                if (aaCount == 0) continue;

                 usedAtomsInThisId.UnionWith(_usedAtoms[labeledAa]);
            }

            foreach (var mz in zeroMzs)
            {
                var mass = mz * charge;
                foreach (var atom in usedAtomsInThisId)
                {
                    var neg1Mass = float.MaxValue;
                    if (atom == "13C")
                        neg1Mass = Convert.ToSingle(mass - Constants.C13MinusC12);
                    else if (atom == "15N")
                        neg1Mass = Convert.ToSingle(mass - (15.0001088982 - 14.0030740048));
                    else if (atom == "D")
                        neg1Mass = Convert.ToSingle(mass - (2.01410177811 - 1.00782503224));
                    else if (atom == "H")
                        neg1Mass = Convert.ToSingle(mass - 1.00782503224);
                    else if (atom == "Proton")
                        neg1Mass = Convert.ToSingle(mass - Constants.Proton);

                    ret.Add(neg1Mass / charge);
                }
            }
            return ret; 
        }

        private Xic GetRecalibratedAndSmoothedXic(Ms2Result id)
        {
            var monoMz = id.Precursor - Convert.ToSingle(id.IsotopeIdx * Constants.C13MinusC12 / id.Charge); // find monoisotopic mz

            var tol = _1stTolerance.GetToleranceAsTh(id.Precursor);
            var rtRange = new[] { _run.GetElutionTime(id.ScanNum) - Params.MaxFeatureSpan, _run.GetElutionTime(id.ScanNum) + Params.MaxFeatureSpan };

            var xic = _run.GetPrecursorExtractedIonChromatogram(monoMz - tol, monoMz + tol, id.ScanNum, rtRange);

            if (xic.Count < Params.MinXicPointCntrInXic)
                return null;
            var xicMzs = new List<float>();

            var apexIndex = xic.BinarySearch(xic.GetNearestApex(id.ScanNum));
            for (var i = apexIndex - 5; i < apexIndex + 5; i++)
            {
                if (i < 0 || i > xic.Count - 1) continue;
                xicMzs.Add((float)xic[i].Mz);
            }
            monoMz = (float)xicMzs.Mean();
            RemoveOverlappingXicPoints(xic, monoMz);

            return xic.GetSmoothedXic();
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

    }
}
