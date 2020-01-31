using System;
using System.Collections.Generic;
using System.Linq;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.MassSpecData;
using InformedProteomics.Backend.Utils;
using MathNet.Numerics.Statistics;

namespace Epiq
{
    public class Template : XicMatrix
    {
        private const float SpanMultFactor = 1.1f; // ets will span shape * spanFactor
        public float Cosine { get; private set; }
        public float SignalWidth { get; private set; }
        public float SignalStart { get; private set; }
        public float RtPosition { get; private set; }
        public float MzPosition { get; private set; }
        public float Area { get; private set; }
        public float Sigma { get; private set; }
        public List<sbyte> ActiveMzBins { get; private set; }

        public static int GetIsotopeSearchRange(sbyte cn, Ms2Result id, IsotopomerEnvelope isoEnvelope)
        {
            var isotopeSearchRange = isoEnvelope.MostAbundantIsotopeIndex + 2;
            return Math.Min(isotopeSearchRange, isoEnvelope.Envelope.Length);
        }

        private Template(float[,] intensities, float[] rts, float[] mzs, float area, float cosine,
            XicShape template)
            : base(intensities, rts, mzs)
        {
            Cosine = cosine;
            Area = area;
            SignalStart = template.Xstart;
            SignalWidth = template.GetXSpan();
            RtPosition = template.RtPosition;
            MzPosition = template.MzPosition;
            ActiveMzBins = new List<sbyte>();
            for (var row = 0; row < M.RowCount; row++)
            {
                if (M.Row(row).Max() <= 0) continue;
                ActiveMzBins.Add((sbyte)row);
            }
            //Area = GetArea();//shape.Area;
            Sigma = template.Sigma;
        }

        
        private static float GetConcatenatedCosine(XicShape template, InterpolatedXic[] iXics, int cn,
            int[,] mzBinNumberPerCnIi, double[] isotopeEnvelope, int isotopeSearchRange, bool dis = false)
        {
            var templateInts = new List<float>();
            var iXicInts = new List<float>();
            var ets = template.GetEts();
            var ti = template.InterpolateAt(ets);
            var prevXiEndIntensity = .0f;
            for (var ii = 0; ii < isotopeSearchRange; ii++)
            {
                var iXic = iXics[mzBinNumberPerCnIi[cn, ii]];
                var xi = iXic == null ? new float[ets.Length] : iXic.InterpolateAt(ets);
                for (var i = 0; i < xi.Length; i++)
                {
                    iXicInts.Add(xi[i] - xi[0] + prevXiEndIntensity);
                    templateInts.Add(ti[i]*(float) isotopeEnvelope[ii]);
                }
                prevXiEndIntensity = xi[xi.Length - 1] - xi[0] + prevXiEndIntensity;
            }
            if (dis)
            {
                Console.Write(@"xic=[");
                foreach (var it in iXicInts) Console.Write(it + @" ");
                Console.WriteLine(@"];");

                Console.Write(@"temp=[");
                foreach (var it in templateInts) Console.Write(it + @" ");
                Console.WriteLine(@"];");
            }
            return FitScoreCalculator.GetCosine(templateInts.ToArray(), iXicInts.ToArray());
            //FitScoreCalculator.GetPearsonCorrelation(templateInts.ToArray(), iXicInts.ToArray(), false);
        }
        

        private static XicShape GetInitialBestXicShape(InterpolatedXic[] iXics, Ms2Result id,
            int[,] mzBinNumberPerCnIi, IsotopomerEnvelope[] isoEnvelope, LcMsRun run, bool forTraining, out sbyte bestCn)
        {
            XicShape bestInitTemplate = null;
            var maxCosine = -100.0;
            bestCn = 0;
            var initEt = (float) run.GetElutionTime(id.ScanNum);
            var cn = id.LabelIndex;
            //Console.WriteLine(cn);
            var isotopeSearchRange = GetIsotopeSearchRange(cn, id, isoEnvelope[cn]);
            for (var ii = 0; ii < isotopeSearchRange; ii++)
            {
                var iXic = iXics[mzBinNumberPerCnIi[cn, ii]];
                var xicShape = XicShape.GetXicShape(iXic, initEt, null, forTraining);
                if (xicShape == null) continue;
                var cosine = GetConcatenatedCosine(xicShape, iXics, cn, mzBinNumberPerCnIi, isoEnvelope[cn].Envelope, isotopeSearchRange);
                //Console.WriteLine(corr);

                if (cosine < maxCosine) continue;
                maxCosine = cosine;
                bestInitTemplate = xicShape;
                bestCn = cn;
            }
            return bestInitTemplate;
        }

        private static XicShape GetLocalBestXicShape(InterpolatedXic[] iXics, Ms2Result id,
            int[,] mzBinNumberPerCnIi, int cn, int prevCn, XicShape prevXicShape, float initEt,
            double initWidth, IsotopomerEnvelope[] isoEnvelope, bool forTraining)
        {
            if (iXics[mzBinNumberPerCnIi[cn, isoEnvelope[cn].MostAbundantIsotopeIndex]] == null) return null;
            var rtOffset = DShift.GetRtOffset(id, (sbyte) prevCn, (sbyte) cn, initEt, forTraining);
            //Console.WriteLine(prevCn + " " + cn + " " + etOffset);
            XicShape localBestTemplate = null;
            var localMaxCosine = -100.0;
            var isotopeSearchRange = GetIsotopeSearchRange((sbyte) cn, id, isoEnvelope[cn]);
            for (var ii = 0; ii < isotopeSearchRange; ii++)
            {
                var iXic = iXics[mzBinNumberPerCnIi[cn, ii]];
                if (iXic == null) continue;
                //var prev = iXic == null? 0 : iXic.Ymax;
                var xicShape = XicShape.GetXicShape(iXic, prevXicShape, rtOffset, forTraining);
                if (xicShape == null) continue;
                var cosine = //xicShape.Cosine;//GetCorrMean(shape, iXics, cn, mzBinNumberPerCnIi, isotopeSearchRange);
                    GetConcatenatedCosine(xicShape, iXics, cn, mzBinNumberPerCnIi, isoEnvelope[cn].Envelope, isotopeSearchRange); //
                if (cosine < localMaxCosine) continue;
                //if (forTraining && cosine < Params.CosineThresholdforTraining) continue;
                localMaxCosine = cosine;
                localBestTemplate = xicShape;
            }

            return localBestTemplate ??
                   (forTraining
                       ? null
                       : XicShape.GetXicShapeWithRtOffset(prevXicShape, rtOffset));
        }

        private static float[] DefineRts(XicShape[] templates)
        {
            var signalStart = float.PositiveInfinity;
            var signalEnd = float.NegativeInfinity;
            var spans = new List<float>();
            foreach (var t in templates)
            {
                if (t == null) continue;
                signalStart = Math.Min(signalStart, t.Xstart);
                signalEnd = Math.Max(signalEnd, t.Xend);
                spans.Add(t.GetXSpan());
            }

            //var span = signalEnd - signalStart;
            var medianSpan = Math.Abs(spans.Median());
            signalStart -= (SpanMultFactor - 1)/2*medianSpan;
            signalEnd += (SpanMultFactor - 1)*medianSpan;

            var ets = new float[Params.RtBinCount];
            for (var i = 0; i < ets.Length; i++)
                ets[i] = signalStart + (signalEnd - signalStart)/(ets.Length - 1)*i;
            return ets;
        }


        public static Template[] GetTemplates(InterpolatedXic[] iXics, Ms2Result id, float[] mzs,
            IsotopomerEnvelope[] isoEnvelopes, IsotopomerEnvelope[]  impEnvelopes, int[,] mzBinNumberPerCnIi, LcMsRun run, float cosineThreshold,
            bool forTraining, out float[] rts, out double initWidth)
        {
            rts = null;
            initWidth = -double.MinValue;
            sbyte bestCn;
            var initEt = (float) run.GetElutionTime(id.ScanNum);
            var xicShapes = new XicShape[LabelList.LabelNumberArr.Length];
            var bestInitXicShape = GetInitialBestXicShape(iXics, id, mzBinNumberPerCnIi, isoEnvelopes, run,
                forTraining, out bestCn);

            if (bestInitXicShape == null) return null;
            initWidth = bestInitXicShape.GetXSpan();

            var prevShape = bestInitXicShape;
            var prevCn = bestCn;

            for (var idx = id.LabelListOfId.RtSortedCns.IndexOf(bestCn); idx < id.LabelListOfId.RtSortedCns.Count; idx++)
            {
                var cn = id.LabelListOfId.RtSortedCns[idx];
                xicShapes[cn] = GetLocalBestXicShape(iXics, id, mzBinNumberPerCnIi, cn, prevCn, prevShape, initEt,
                    initWidth, isoEnvelopes, forTraining);
                if (xicShapes[cn] == null) continue;
                prevCn = cn;
                prevShape = xicShapes[cn];
            }
            prevCn = bestCn;
            prevShape = xicShapes[bestCn] == null ? bestInitXicShape : xicShapes[bestCn];

            for (var idx = id.LabelListOfId.RtSortedCns.IndexOf(bestCn); idx >= 0; idx--)
            {
                var cn = id.LabelListOfId.RtSortedCns[idx];
                xicShapes[cn] = GetLocalBestXicShape(iXics, id, mzBinNumberPerCnIi, cn, prevCn, prevShape, initEt,
                    initWidth, isoEnvelopes, forTraining);
                if (xicShapes[cn] == null) continue;
                prevCn = cn;
                prevShape = xicShapes[cn];
            }


            rts = DefineRts(xicShapes);
            var ret = new Template[xicShapes.Length];
            //prevCn = bestCn;
            bestCn = -1;
            var maxCosine = -2f;
            for (var cn = (sbyte) 0; cn < xicShapes.Length; cn++)
            {
                if (xicShapes[cn] == null) continue;
                if (maxCosine > xicShapes[cn].Cosine) continue;
                maxCosine = xicShapes[cn].Cosine;
                bestCn = cn;
            }
            
            //var cnList = new List<sbyte>();

            
            
            prevCn = bestCn;
            for (var cn = (sbyte)Math.Max(0, (int)prevCn); cn < LabelList.LabelNumberArr.Length; cn++)// cnList.Add(cn);
            {
                //Console.WriteLine(Params.LabelNumArr.Length + " " + cn + " " + (templates[cn] == null));
                if (xicShapes[cn] == null) continue;
                var impurityConsideredEnv = isoEnvelopes[cn].GetConvolutedEnvelope(impEnvelopes[cn]);
                        
                var isotopeSearchRange = GetIsotopeSearchRange(cn, id, isoEnvelopes[cn]);
                var cosine = GetConcatenatedCosine(xicShapes[cn], iXics, cn, mzBinNumberPerCnIi,
                    isoEnvelopes[cn].Envelope, isotopeSearchRange); //
                if (cosine <= cosineThreshold)
                {
                    if (forTraining) continue;
                    if (prevCn >= 0)
                    {
                        cosine = GetConcatenatedCosine(xicShapes[prevCn], iXics, cn, mzBinNumberPerCnIi,
                        isoEnvelopes[cn].Envelope, isotopeSearchRange); //

                        ret[cn] = GetTemplate(xicShapes[prevCn], mzBinNumberPerCnIi[cn, 0], mzs, rts, cosine,
                            impurityConsideredEnv, DShift.GetRtOffset(id, prevCn, cn, initEt));
                        continue;
                    }
                }
                prevCn = cn;
                ret[cn] = GetTemplate(xicShapes[cn], mzBinNumberPerCnIi[cn, 0], mzs, rts, cosine,
                    impurityConsideredEnv);

              //  Console.WriteLine(cn + " " + xicShapes[cn].Area + " " + xicShapes[cn].Sigma + " " + ret[cn].Sigma);
                
            }

            prevCn = bestCn;
            for (var cn = (sbyte)(prevCn - 1); cn >= 0; cn--)// cnList.Add(cn);
            {
                //Console.WriteLine(Params.LabelNumArr.Length + " " + cn + " " + (templates[cn] == null));
                if (xicShapes[cn] == null) continue;
                var impurityConsideredEnv = isoEnvelopes[cn].GetConvolutedEnvelope(impEnvelopes[cn]);
                
                var isotopeSearchRange = GetIsotopeSearchRange(cn, id, isoEnvelopes[cn]);
                var cosine = GetConcatenatedCosine(xicShapes[cn], iXics, cn, mzBinNumberPerCnIi,
                    isoEnvelopes[cn].Envelope, isotopeSearchRange); //
               if (cosine <= cosineThreshold)
                {
                    if (forTraining) continue;
                    if (prevCn >= 0)
                    {
                        cosine = GetConcatenatedCosine(xicShapes[prevCn], iXics, cn, mzBinNumberPerCnIi,
                        isoEnvelopes[cn].Envelope, isotopeSearchRange); //

                        ret[cn] = GetTemplate(xicShapes[prevCn], mzBinNumberPerCnIi[cn, 0], mzs, rts, cosine,
                            impurityConsideredEnv, DShift.GetRtOffset(id, prevCn, cn, initEt));
                        continue;
                    }
                }
                prevCn = cn;
                ret[cn] = GetTemplate(xicShapes[cn], mzBinNumberPerCnIi[cn, 0], mzs, rts, cosine,
                    impurityConsideredEnv);

               // Console.WriteLine(cn + " " + xicShapes[cn].Area + " " + xicShapes[cn].Sigma + " " + ret[cn].Sigma);
            }

           // Console.WriteLine(id.UnmodifiedPeptide);
            
            return ret;
        }

        public float[] GetSignalRange(int row, float relativeIntensityThreshold)
        {
            if (!ActiveMzBins.Contains((sbyte) row)) return null;
            var s = -1.0f;
            var e = -1.0f;
            var t = M.Row(row);
            var i = 0;
            var threshold = t.Max()*relativeIntensityThreshold;
            for (; i < Rts.Length; i++)
            {
                if (t[i] < threshold) continue;
                s = Rts[i];
                break;
            }

            for (; i < Rts.Length; i++)
            {
                e = Rts[i];
                if (t[i] < threshold) break;
            }
            return new[] {s, e};
        }

        public float[] GetSignalRange(float relativeIntensityThreshold)
        {
            var s = -1.0f;
            var e = -1.0f;
            var t = M.Row(ActiveMzBins[0]);
            var i = 0;
            var threshold = t.Max()*relativeIntensityThreshold;
            for (; i < Rts.Length; i++)
            {
                if (t[i] < threshold) continue;
                s = Rts[i];
                break;
            }

            for (; i < Rts.Length; i++)
            {
                e = Rts[i];
                if (t[i] < threshold) break;
            }
            return new[] {s, e};
        }


        private static Template GetTemplate(XicShape shape, int monoIsotopicMzBin,
            float[] mzs, float[] ets, float cosine, IsotopomerEnvelope isoEnvelope, float etShift = 0f)
        {
            var intensities = new float[mzs.Length, ets.Length];
            float area;
            if (Math.Abs(etShift) > 0)
                shape = XicShape.GetXicShapeWithRtOffset(shape, etShift);

            SetIsotopeMultipledIntensities(intensities, shape, monoIsotopicMzBin, ets, isoEnvelope, out area);
            var ret = new Template(intensities, ets, mzs, area, cosine, shape);
            return ret;
        }

        private static void SetIsotopeMultipledIntensities(float[,] intensities, XicShape template,
            int mzBin, float[] ets, IsotopomerEnvelope isoEnvelop, out float area)
        {
            //var isoSum = isoEnvelop.Sum();
            var max = 0f;
            //var dc = .1f;
            area = (float)isoEnvelop.Envelope.Sum() * template.Area;
            for (var row = Math.Max(0, mzBin -isoEnvelop.MonoIsotopeIndex); row < Math.Min(isoEnvelop.Envelope.Length - isoEnvelop.MonoIsotopeIndex + mzBin, intensities.GetLength(0)); row++)
            {
                var iostope = isoEnvelop.Envelope[row - mzBin + isoEnvelop.MonoIsotopeIndex];
                for (var col = 0; col < intensities.GetLength(1); col++)
                {
                    //if (ets[col] < shape.SignalStart || rts[col] > shape.SignalEnd) continue; // 
                    intensities[row, col] = (float) (template.InterpolateAt(ets[col])*iostope);
                    max = Math.Max(max, intensities[row, col]);
                }
                //area += (float) iostope*;
            }
            if (max <= 0) return;
            for (var row = Math.Max(0, mzBin - isoEnvelop.MonoIsotopeIndex); row < Math.Min(isoEnvelop.Envelope.Length - isoEnvelop.MonoIsotopeIndex + mzBin, intensities.GetLength(0)); row++)
                for (var col = 0; col < intensities.GetLength(1); col++)
                    intensities[row, col] /= max;
            area /= max;
            
        }
    }
}