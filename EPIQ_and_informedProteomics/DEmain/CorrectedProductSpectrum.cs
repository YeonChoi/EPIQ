using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using MathNet.Numerics.LinearAlgebra;

namespace Epiq
{
    public class CorrectedProductSpectrum
    {
        private static readonly int minXicLength = 5;

        private readonly LcMsRun _run;
        private readonly Tolerance _tolerance;
        private readonly double _targetPrecursorMz;
        private XicPoint _localApex;

        public bool ChargeCorrected;
        public bool MzCorrected;

        public ProductSpectrum Ps { get; private set; }
        public double CorrectedPrecursorMz { get; private set; }
        public sbyte CorrectedPrecursorCharge { get; private set; }

        public CorrectedProductSpectrum(int ms2ScanNum, double precusorMz, Tolerance tolerance, LcMsRun run, bool correctMz, bool correctCharge)
        {
            _tolerance = tolerance;
            _run = run;
            _targetPrecursorMz = precusorMz;
            var spec = _run.GetSpectrum(ms2ScanNum);
            Ps = spec as ProductSpectrum;
            CorrectedPrecursorMz = _targetPrecursorMz;
            CorrectedPrecursorCharge = (sbyte)Ps.IsolationWindow.Charge;

            if (correctMz)
                CorrectMzAndEt();
            if (correctCharge)
                CorrectCharge();
        }

        private void CorrectCharge()
        {
            throw new NotImplementedException("Charge correction is not implemented");
            if (_localApex == null) return;
            _run.GetSpectrum(_localApex.ScanNum);

            /*
            var ints = new List<double>();
            var mzs = new List<double>();
            foreach (var peak in spec.Peaks)
            {
                mzs.Add(peak.Mz);
                ints.Add(peak.Intensity);
            }
            Console.WriteLine(@"mzs=[" + String.Join(", ", mzs) + @"];");
            Console.WriteLine(@"ints=[" + String.Join(", ", ints) + @"];");

            var cints = new List<double>();
            var cmzs = new List<double>();
            foreach (var peak in spec.GetBaseLineCorrectedPeaks())
            {
                cmzs.Add(peak.Mz);
                cints.Add(peak.Intensity);
            }
            Console.WriteLine(@"cmzs=[" + String.Join(", ", cmzs) + @"];");
            Console.WriteLine(@"cints=[" + String.Join(", ", cints) + @"];");

            Console.WriteLine();
             */


        }


        private void CorrectMzAndEt()
        {
            if (Ps == null) return;

            var etRange = new[]
            {
                _run.GetElutionTime(Ps.ScanNum) - Params.MaxFeatureSpan,
                _run.GetElutionTime(Ps.ScanNum) + Params.MaxFeatureSpan
            };
            var initXic = _run.GetPrecursorExtractedIonChromatogram(_targetPrecursorMz,
                new Tolerance(_tolerance.GetValue()*3, _tolerance.GetUnit()), Ps.ScanNum, etRange);
            RemoveSnOverlappingXicPoints(initXic, _targetPrecursorMz);
            if (initXic.Count < minXicLength) return;

            var interpolatableInitXic = new InterpolatedXic(initXic, _run);
            var direction = interpolatableInitXic.DifferentiateAt((float) _run.GetElutionTime(Ps.ScanNum));
            _localApex = initXic.GetOneDirectionalNearestApex(Ps.ScanNum, direction > 0, false);

            CorrectedPrecursorMz = _localApex.Mz;
            MzCorrected = true;
        }


        private static void RemoveSnOverlappingXicPoints(Xic xic, double targetMz)
        {
            if (xic.Count < 1) return;
            var prevSn = xic[0].ScanNum;

            for (var i = 1; i < xic.Count; i++)
            {
                var currSn = xic[i].ScanNum;
                var snDiff = currSn - prevSn;
                prevSn = currSn;
                if (snDiff != 0) continue;

                var prevMz = xic[i - 1].Mz;
                var currMz = xic[i].Mz;
                if (Math.Abs(targetMz - prevMz) > Math.Abs(targetMz - currMz))
                    xic.RemoveAt(i - 1);
                else
                    xic.RemoveAt(i);
                i--;
            }
        }

    }
}