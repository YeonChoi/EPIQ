using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Composition;

namespace MMS1Quant
{
    public class PreCalculatedMzDiff
    {
        private static readonly ConcurrentDictionary<Tuple<int, sbyte, int>, List<double>> MzDiffMap;
        private static readonly ConcurrentDictionary<Tuple<int, sbyte, int>, int[,]> MzBinNumberMap;
        private static readonly ConcurrentDictionary<Tuple<int, sbyte, int>, double> MzSpanMap;

        static PreCalculatedMzDiff()
        {
            MzDiffMap = new ConcurrentDictionary<Tuple<int, sbyte, int>, List<double>>();
            MzBinNumberMap = new ConcurrentDictionary<Tuple<int, sbyte, int>, int[,]>();
            MzSpanMap = new ConcurrentDictionary<Tuple<int, sbyte, int>, double>();
        }

        public static List<double> GetMzDiffs(int numOfLabels, sbyte charge, int isotopeEnvelopeLen, out int[,] mzBinNumbers, out double mzSpan)
        {
            var key = new Tuple<int, sbyte, int>(numOfLabels, charge, isotopeEnvelopeLen);

            if (MzDiffMap.ContainsKey(key))
            {
                mzBinNumbers = MzBinNumberMap[key];
                mzSpan = MzSpanMap[key];
                return MzDiffMap[key];
            }

            var mzDictionary = new Dictionary<int, double>();
            mzBinNumbers = new int[Params.ChannelNumArr().Length, isotopeEnvelopeLen];
            foreach (var cn in Params.ChannelNumArr())
            {
                for (sbyte ii = 0; ii < isotopeEnvelopeLen; ii++)
                {
                    var massDiff = GetMassDiff(numOfLabels, cn, 0, ii, 0);
                    var bin = Constants.GetBinNum(massDiff);
                    var mz = massDiff / charge;
                    mzDictionary[bin] = mz;
                    mzBinNumbers[cn, ii] = bin;
                }
            }

            var mzDiffs = mzDictionary.Values.ToList();
            mzDiffs.Sort();
            foreach (var cn in Params.ChannelNumArr())
            {
                for (sbyte ii = 0; ii < isotopeEnvelopeLen; ii++)
                {
                    var mz = mzDictionary[mzBinNumbers[cn, ii]];
                    mzBinNumbers[cn, ii] = mzDiffs.IndexOf(mz);
                }
            }
            mzSpan = mzDiffs[mzDiffs.Count - 1] - mzDiffs[0];

            MzSpanMap[key] = mzSpan;
            MzBinNumberMap[key] = mzBinNumbers;
            MzDiffMap[key] = mzDiffs;
            return mzDiffs;
        }

        private static double GetMassDiff(int numLables, sbyte toCn, sbyte fromCn, sbyte toIi, sbyte fromIi)
        {
            return numLables * (Params.ChannelToLabelMass()[toCn] - Params.ChannelToLabelMass()[fromCn]) + (toIi - fromIi) * Constants.C13MinusC12;
        }
    }
}
