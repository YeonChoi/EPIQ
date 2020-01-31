using System;
using System.Collections.Generic;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;

namespace Epiq
{
    public class InterpolatedXic : Interpolatable
    {
        public float Mz { private set; get; }

        public InterpolatedXic(Xic xic, LcMsRun run, bool smooth = true)
        {
            var ets = new float[xic.Count];
            var ints = new float[xic.Count];
            var sxic = smooth ? xic.GetSmoothedXic() : xic;
            //Xic = sxic;
            var apex = xic.GetApex();
            Mz = apex == null? 0f : (float)apex.Mz;
            for (var i = 0; i < sxic.Count; i++)
            {
                ets[i] = (float) run.GetElutionTime(sxic[i].ScanNum);
                ints[i] = (float) sxic[i].Intensity;
            }

            SetInterpolator(ets, ints);
        }

        //public Xic Xic { get; private set; }

        public List<float> FindApexEts(float[] ets, float[] signalRange, float relativeIntensityThreshold, int maxCount)
        {
            var intensityEtDictionary = new Dictionary<float, float>();
            var i = 1;
            var prevDiff = 1.0;
            var prevEt = ets[0];
            var maxIntensity = Yapex - Ymin;
            float intensity;
            for (; i < ets.Length; i++)
            {
                var et = ets[i];
                if (et >= signalRange[0]) break;
                intensity = InterpolateAt(et) - Ymin;
                var diff = DifferentiateAt(et);
                if ((prevDiff >= 0) && (diff <= 0) && (intensity/maxIntensity > relativeIntensityThreshold))
                    intensityEtDictionary[intensity] = (prevEt + et)/2;
                prevDiff = diff;
                prevEt = et;
            }

            prevDiff = 1.0;
            prevEt = ets[i++];
            for (; i < ets.Length - 1; i++)
            {
                var et = ets[i];
                if (et <= signalRange[1]) continue;
                intensity = InterpolateAt(et) - Ymin;
                var diff = DifferentiateAt(et);
                if ((prevDiff >= 0) && (diff <= 0) && (intensity/maxIntensity > relativeIntensityThreshold))
                    intensityEtDictionary[intensity] = (prevEt + et)/2;
                prevDiff = diff;
                prevEt = et;
            }
            intensity = InterpolateAt(ets[ets.Length - 1]) - Ymin;
            if ((prevDiff >= 0) && (intensity/maxIntensity > relativeIntensityThreshold))
                intensityEtDictionary[intensity] = (prevEt + ets[ets.Length - 1])/2;

            var intensities = new List<float>();
            intensities.AddRange(intensityEtDictionary.Keys);
            intensities.Sort();
            intensities.Reverse();
            var apexEts = new List<float>();
            for (var j = 0; j < Math.Min(maxCount, intensities.Count); j++)
                apexEts.Add(intensityEtDictionary[intensities[j]]);

            return apexEts;
        }
    }
}