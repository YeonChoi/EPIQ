using System;
using System.Collections.Generic;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;

namespace Epiq
{
    public class PrecusorMzCandidates : List<double>
    {
        public static string InstModel;

        public PrecusorMzCandidates(int ms2ScanNum, LcMsRun run, string instModel, int maxNumber = 3,
            double intThres = 0.5, bool debug = false)
        {
            var isolationWindow = run.GetIsolationWindow(ms2ScanNum);
            var precusorSpec = run.GetSpectrum(run.GetPrecursorScanNum(ms2ScanNum));
            var peakList = precusorSpec.GetPeakListWithin(isolationWindow.MinMz, isolationWindow.MaxMz);
            if (peakList.Count == 0) return;
            var filteredPeakList = ApplyIsolationWindowShapeFilter(isolationWindow, peakList, instModel);
            AddCandidiateMzs(filteredPeakList, maxNumber, intThres);

            if (debug)
            {
                Console.WriteLine("{0} {1}", ms2ScanNum, run.GetPrecursorScanNum(ms2ScanNum));
                Console.WriteLine("{0} {1}", isolationWindow.IsolationWindowLowerOffset,
                    isolationWindow.IsolationWindowUpperOffset);
                Console.WriteLine("{0} {1}", isolationWindow.MinMz, isolationWindow.MaxMz);
                var mzs = new List<double>();
                var intensities = new List<double>();
                foreach (var peak in peakList)
                {
                    mzs.Add(peak.Mz);
                    intensities.Add(peak.Intensity);
                }
                Console.WriteLine("mzs = [{0}];", string.Join(", ", mzs));
                Console.WriteLine("ints = [{0}];", string.Join(", ", intensities));
                var filteredMzs = new List<double>();
                var filteredIntensities = new List<double>();
                foreach (var peak in filteredPeakList)
                {
                    filteredMzs.Add(peak.Mz);
                    filteredIntensities.Add(peak.Intensity);
                }
                Console.WriteLine("filteredMzs = [{0}];", string.Join(", ", filteredMzs));
                Console.WriteLine("filteredInts = [{0}];", string.Join(", ", filteredIntensities));
                Console.WriteLine("CandidateMzs = [{0}];", string.Join(", ", this));
                Console.WriteLine();
            }
        }

        private List<Peak> ApplyIsolationWindowShapeFilter(IsolationWindow isolationWindow, List<Peak> peakList,
            string instModel)
        {
            if (instModel == "Q Exactive Orbitrap")
                return ApplyParabolicFilter(isolationWindow, peakList);
            if (instModel == "Orbitrap Fusion Lumos")
                return peakList;

            return peakList; // default is square filter.
        }

        private List<Peak> ApplyParabolicFilter(IsolationWindow isolationWindow, List<Peak> peakList)
        {
            var ret = new List<Peak>();

            var targetMz = isolationWindow.IsolationWindowTargetMz;
            foreach (var peak in peakList)
            {
                double coef;
                if (peak.Mz > targetMz)
                    coef = 1 - Math.Pow((peak.Mz - targetMz)/isolationWindow.IsolationWindowUpperOffset, 2);
                else
                    coef = 1 - Math.Pow((peak.Mz - targetMz)/isolationWindow.IsolationWindowLowerOffset, 2);
                var newPeak = new Peak(peak.Mz, coef*peak.Intensity);
                ret.Add(newPeak);
            }
            return ret;
        }

        private void AddCandidiateMzs(List<Peak> filteredPeakList, int maxNumber, double intThres)
        {
            filteredPeakList.Sort((firstObj, secondObj) => { return secondObj.Intensity.CompareTo(firstObj.Intensity); });
                // Sort reverse
            var maxIntensity = filteredPeakList[0].Intensity;
            for (var i = 0; i < filteredPeakList.Count; i++)
            {
                if (i >= maxNumber) break;
                if (filteredPeakList[i].Intensity/maxIntensity < intThres) break;
                Add(filteredPeakList[i].Mz);
            }
        }
    }
}