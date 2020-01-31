using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using InformedProteomics.Backend.Utils;

namespace Epiq
{
    public class XicShape : Interpolatable
    {
        private const float MinSigma = .1f;
        private const float MaxSigma = 2.0f;
        private const float StartMinPdf = 1e-2f;//1e-2f; // don't touch!!
        private const float EndMinPdf = 1e-2f;
      //  private const float FinalStartMinPdf = 1e-3f;
     //   private const float FinalEndMinPdf = 1e-3f;

        private const float InitWidthMargin = 6f;
        private const float WidthMargin = 3f; // 1.5
        private const float SigmaMargin = 3f;
        private readonly bool _fit;
        //public float SignalStart { private set; get; }
        //public float SignalEnd { private set; get; }
        public float Area { private set; get; }
        public float Cosine { get; private set; }
        public float Sigma { get; private set; }
        public float RtPosition { get; private set; }
        public float MzPosition { get; private set; }
        //public static float MaxIntensity = 1e9f;
       // public static float MinIntensity = 1e4f;

        
        private static ConcurrentDictionary<float, Tuple<float, float>> _lognormalSignalPdfRange;
     //   private static ConcurrentDictionary<float, Tuple<float, float>> _lognormalFinalPdfRange;
       // private static ConcurrentDictionary<float, ConcurrentDictionary<float, float>> _lognormalDensity;

        public static XicShape GetXicShapeWithRtOffset(XicShape it, float rtOffset)
        {
            return new XicShape(it, rtOffset, it.Cosine);
        }

        public static XicShape GetXicShape(InterpolatedXic iXic, float initRt, float[] excludeRtRange = null, bool forTraining = false)
        {   
            if (iXic == null) return null;
            float[] apexRange;
            float width;
            DefineRtRanges(iXic, initRt, StartMinPdf, out apexRange, out width);
            var template = GetXicShape(iXic, apexRange, width, InitWidthMargin, null, excludeRtRange,-1, forTraining);
            if (template == null || template.Xstart > initRt || template.Xend < initRt) return null;
            
            return template;
        }

        public static XicShape GetXicShape(InterpolatedXic iXic, XicShape previousXicShape, float rtOffset, bool forTraining = false)
        {
            if (iXic == null) return null;
            float[] apexRange;
            DefineRtRanges(previousXicShape, rtOffset, out apexRange, forTraining);
            //Console.WriteLine(etOffset);
            var sigmaRange = new[] { previousXicShape.Sigma / SigmaMargin, previousXicShape.Sigma * SigmaMargin };
            return GetXicShape(iXic, apexRange, previousXicShape.GetXSpan(), WidthMargin, sigmaRange, null, previousXicShape.Xapex + rtOffset, forTraining);            
        }

        private static XicShape GetXicShape(InterpolatedXic iXic, float[] apexRange, float width, float widthMargin, float[] sigmaRange = null, float[] excludeRtRange = null, float predictedApex = -1f,  bool forTraining = false)
        {
            if (iXic == null) return null;
            var it = new XicShape(iXic, predictedApex, apexRange, width, widthMargin, sigmaRange, excludeRtRange, forTraining);
            return it._fit ? it : null;
        }

        private static void DefineRtRanges(Interpolatable previousXicShape, float rtOffset, out float[] apexRange, bool forTraining = false)
        {
            var apexRt = previousXicShape.Xapex + (forTraining? 0 : rtOffset);
            var rtDelta = forTraining ? Params.RtDeltaTraining : previousXicShape.GetXSpan() / 5; // TODO

          // if (forTraining)
          //  {
                apexRange = new[] { apexRt - rtDelta, apexRt + rtDelta };
           // }
           // else
            /*{
                apexRange = etOffset >= 0
                    ? new[] {prevTemplate.Xapex - etDelta/10, apexEt + etDelta}
                    : new[] {apexEt - etDelta, prevTemplate.Xapex + etDelta/10};
            }*/
            //Console.WriteLine("{0}        {1}", rtOffset, rtDelta);
        }

        private static void DefineRtRanges(Interpolatable iXic, float initRt, float tolerance, out float[] apexRange, out float width)
        {
            apexRange = new float[2];
            var rtBinSize = iXic.GetXSpan() / iXic.PointCount / 5;
            var prevDiff = iXic.DifferentiateAt(initRt);
            var right = prevDiff > 0;
            var apexRt = initRt;
            for (; apexRt < iXic.Xend && apexRt > iXic.Xstart; apexRt += (right ? rtBinSize : -rtBinSize))
            {
                var diff = iXic.DifferentiateAt(apexRt);
                if (prevDiff*diff <= 0) break;
                prevDiff = diff;
            }

            var apexInt = iXic.InterpolateAt(apexRt);
            apexRange[0] = apexRange[1] = apexRt;
            for (; apexRange[1] < iXic.Xend; apexRange[1] += rtBinSize)
            {
                var intensity = iXic.InterpolateAt(apexRange[1]);
                if ((apexInt - iXic.Ymin) * Params.RelativeIntensityThresholdForPeakDetection > intensity - iXic.Ymin) break;
            }

            for (; apexRange[0] > iXic.Xstart; apexRange[0] -= rtBinSize)
            {
                var intensity = iXic.InterpolateAt(apexRange[0]);
                if ((apexInt - iXic.Ymin) * Params.RelativeIntensityThresholdForPeakDetection > intensity - iXic.Ymin) break;
            }

            var prevIntensity = iXic.InterpolateAt(apexRange[1]);
            var rt = apexRange[1] + rtBinSize;
            for (; rt < iXic.Xend; rt += rtBinSize)
            {
                var intensity = iXic.InterpolateAt(rt);
                if (prevIntensity > intensity * (1 + tolerance)) break;
                if (intensity - iXic.Ymin < (apexInt - iXic.Ymin) * tolerance) break;
                prevIntensity = intensity;
            }
            width = rt;

            prevIntensity = iXic.InterpolateAt(apexRange[0]);
            rt = apexRange[0] - rtBinSize;
            for (; rt > iXic.Xstart; rt -= rtBinSize)
            {
                var intensity = iXic.InterpolateAt(rt);
                if (prevIntensity > intensity * (1 + tolerance)) break;
                if (intensity - iXic.Ymin < (apexInt - iXic.Ymin) * tolerance) break;
                prevIntensity = intensity;
            }
            width = width - rt;
        }

        private XicShape(XicShape it, float rtOffset, float cosine)
            : base(it, rtOffset)
        {
            //SignalStart = it.SignalStart + etOffset;
            //SignalEnd = it.SignalEnd + etOffset;
            RtPosition = it.RtPosition + rtOffset;
            MzPosition = it.MzPosition;
            Sigma = it.Sigma;
            Area = it.Area;
            Cosine = cosine;
            _fit = true;
        }

        
        private XicShape(InterpolatedXic iXic, float predictedApex, float[] apexRange, float width, float widthMargin, float[] sigmaRange = null, float[] excludeRtRange = null, bool forTraining = false)
        {
            var binNumber = forTraining? Params.BinNumberForTemplateFittingForTraining : Params.BinNumberForTemplateFitting;
            var minWidth = Math.Max(width / widthMargin, Params.MinFeatureSpan);
            
            var maxWidth = width * widthMargin;
            var finalPeakStartRt = 0f;

            var minSigma = sigmaRange == null ? MinSigma : sigmaRange[0];
            var maxSigma = sigmaRange == null ? MaxSigma : sigmaRange[1];

            Cosine = -100;

            if (forTraining)
            {
                if (apexRange[0] >= iXic.Xapex || apexRange[1] <= iXic.Xapex)
                    return;
            }

            float apexRt = 0;

            if (apexRange[0] <= iXic.Xapex && iXic.Xapex <= apexRange[1])
            {
                apexRt = iXic.Xapex;
            }
            else if (!forTraining && predictedApex > 0)
            {
                apexRt = predictedApex;
            }
            //apexRt = predictedApex;// TODO
            if (apexRt<=0 || excludeRtRange != null && excludeRtRange[0] < apexRt && excludeRtRange[1] > apexRt) return;
            var rtStartRange = new[] { apexRt - maxWidth / 2, apexRt - minWidth / 2};
        
            for (var j = 0; j < binNumber; j ++)
            {
                var peakStartRt = rtStartRange[0] + (rtStartRange[1] - rtStartRange[0])/(binNumber - 1)*j;
                if (peakStartRt < 0 || peakStartRt > apexRt) continue;

                for (var sigma = MinSigma; sigma < MaxSigma; sigma *= (float)Math.Pow(MaxSigma/MinSigma, 1.0/binNumber/5))
                {
                    if (sigma < minSigma || sigma > maxSigma) continue;
                    float[] rts, ints;
                    float area;
                    GetLogNormalTemplate(apexRt, peakStartRt, forTraining? Params.TemplateLengthforTraining : Params.TemplateLength, sigma, minWidth, maxWidth, false, out rts, out ints, out area);
                    
                    if (rts == null) continue;
                    if (excludeRtRange != null && peakStartRt < excludeRtRange[0] && rts[rts.Length - 1] > excludeRtRange[1]) continue;
                    var interpolatedXic = iXic.InterpolateAt(rts);
                    /*var si = -1;
                    var ei = int.MaxValue;
                    for (var m = 0; m < ets.Length; m++)
                    {
                        var et = ets[m];
                        if (iXic.Xstart > et) si = m + 1;
                        if (iXic.Xend >= et) continue;
                        ei = m - 1;
                        break;
                    }*/

                   // var interpolatedMax = interpolatedXic.Max();
                   // var intMax = ints.Max();
                    //var intMin = interpolatedXic.Min();
                    //for (var l = 0; l < interpolatedXic.Length; l++)
                    //{
                     //   interpolatedXic[l] = Math.Max(0, interpolatedXic[l] - intMin);
                    //}
                    //for (var l = 0; l < ints.Length; l++)
                    //    ints[l] -= intMax;
                    var cosine = FitScoreCalculator.GetCosine(interpolatedXic, ints);
                    if (Cosine >= cosine) continue;
                    Cosine = cosine;
                    Sigma = sigma;
                    RtPosition = apexRt;
                    MzPosition = iXic.Mz;
        
                    finalPeakStartRt = peakStartRt;
                    // iXic.Xapex == apexEt && cosine > .95f ? interpolatedXic : 
                    //bestParams = new[] { apexEt, peakStartEt, sigma };
                }
            }

            if (Cosine <= -100) return;
            float[] bestRts, bestInts;
            float finalArea;
            GetLogNormalTemplate(RtPosition, finalPeakStartRt, 300, Sigma, minWidth, maxWidth, true, out bestRts, out bestInts, out finalArea);
            Area = finalArea;
            
            var bestInterpolatedXic = iXic.InterpolateAt(bestRts);
           // var bestintMin = bestInterpolatedXic.Min();
           // for (var l = 0; l < bestInterpolatedXic.Length; l++)
           // {
           //     bestInterpolatedXic[l] = Math.Max(0, bestInterpolatedXic[l] - bestintMin);
           // }
            /*if (FitScoreCalculator.GetCosine(bestInterpolatedXic, bestInts) > .98f)
            {
                var a = bestInts.Sum();
                var b = bestInterpolatedXic.Sum();
                for (var k = 0; k < bestInterpolatedXic.Length; k++)
                {
                    bestInts[k] = bestInterpolatedXic[k]/b*a; 
                }
            }*/
            SetInterpolator(bestRts, bestInts);
           // float area;
           // GetFinalLogNormalTemplate(bestParams[0], bestParams[1], 500, bestParams[2], bestRange, out bestEts, out bestInts, out area);
            
            //Console.WriteLine(SignalStart + " " + bestEts[0] + " " + SignalEnd + " " + bestEts[bestEts.Length-1]);
            //SignalStart = bestEts[0];
            //SignalEnd = bestEts[bestEts.Length - 1];
                       
            // if (bestEts == null) return;
            //Area = bestParams[0] - bestEts[0];
            
           // Area = .0f;
            /*
            var r = bestInts;
            var prev = r[0];
            for (var i = 1; i < r.Length; i++)
            {
                var curr = r[i];
               // Area += (prev + curr) / 2 * (bestEts[i] - bestEts[i - 1]);
                prev = curr;
            }*/
            
            

            _fit = true;
        }


        public static float NormalizeIntensities(float[] ints)
        {
            var max = ints.Max();
            //var min = ints.Min();
            if (max <= 0) return max;
            for (var i = 0; i < ints.Length; i++) ints[i] = ints[i] / max;
            return max;
        }


        private static Tuple<float, float> GetLogNormalPdfRange(float sigma)
        {
            if (_lognormalSignalPdfRange == null) _lognormalSignalPdfRange = new ConcurrentDictionary<float, Tuple<float, float>>();
           // if (_lognormalFinalPdfRange == null) _lognormalFinalPdfRange = new ConcurrentDictionary<float, Tuple<float, float>>();

           // var lognormalPdfRange = final ? _lognormalFinalPdfRange : _lognormalSignalPdfRange;
            Tuple<float, float> range;
            if (_lognormalSignalPdfRange.TryGetValue(sigma, out range)) return range;

            var mu = sigma * sigma;
            var sp = -1.0;
            var ep = -1.0;
            var pdf = new MathNet.Numerics.Distributions.LogNormal(mu, sigma);
            var pdfmode = pdf.Density(1);
            var pdfmins = pdfmode * StartMinPdf;
            var pdfmine = pdfmode * EndMinPdf;
            for (var p = .0; p < 30; p += .0001)
            {
                var d = pdf.Density(p);//MathNet.Numerics.Distributions.LogNormal.PDF(mu, sigma, p);//
                if (sp <= 0)
                {
                    if (d >= pdfmins) sp = p;
                }
                else
                {
                    if (p < 1 || d >= pdfmine) continue;
                    ep = p;
                    break;
                }
            }
            if (ep <= 0) ep = 30;
            if (ep <= 0 || sp <= 0 || sp >= ep)
            {
                Console.WriteLine(@"error in log normal {0} {1} ", sp, ep);
            }
            return _lognormalSignalPdfRange[sigma] = new Tuple<float, float>((float)sp, (float)ep);
        }

        private static void GetLogNormalTemplate(float apexRt, float startRt, int templetLength, float sigma, float minWidth, float maxWidth, bool calculateArea, out float[] rts, out float[] ints, out float area)
        {
            rts = null;
            ints = null;
            area = 0;
            var r = GetLogNormalPdfRange(sigma);
            var endRt = (r.Item2 - r.Item1) * (apexRt - startRt) / (1 - r.Item1) + startRt;
            if (endRt - startRt < minWidth || endRt - startRt > maxWidth) return;
            
            var mu = sigma * sigma;

            var deno = (templetLength - 1)/2;
            var deltaRt = (apexRt - startRt) / deno;
            var deltaP = (1 - r.Item1) / deno;

            var i = 0;
            for (; i < deno * 5; i++)
            {
                if (startRt + deltaRt * i > endRt) break;
                
                //(float)MathNet.Numerics.Distributions.LogNormal.PDF(mu, sigma, r.Item1 + deltaP * i);
            }

            rts = new float[i];
            ints = new float[i];
            var pdf = new MathNet.Numerics.Distributions.LogNormal(mu, sigma);
           
            for (i = 0; i < rts.Length; i++)
            {
                rts[i] = startRt + deltaRt * i;
                ints[i] = (float)pdf.Density(r.Item1 + deltaP * i); //GetDensity(sigma, r.Item1 + deltaP*i); //;
            }

            area = 0;
            if(calculateArea) area = deltaRt / deltaP;// / (pdf.CumulativeDistribution(r.Item2) - pdf.CumulativeDistribution(r.Item1)));

        }

        public float[] GetEts()
        {
            var xs = new float[PointCount];
            for (var i = 0; i < xs.Length; i++)
            {
                xs[i] = Xstart + GetXSpan() * i / (xs.Length - 1);
            }
            return xs;
        }
        //SSGPYGGGGQYFAKPR_16786_1827
        /*
        private static void GetFinalLogNormalTemplate(float apexEt, float prevStartEt, int templetLength, float sigma, Tuple<float, float> prevR, out float[] ets, out float[] ints, out float area)
        {
            //var prevR = GetLogNormalPdfRange(sigma, prevStartMinPdf, prevEndMinPdf);
            var r = GetLogNormalPdfRange(sigma);
            var startEt = apexEt - (apexEt - prevStartEt) / (1 - prevR.Item1) * (1 - r.Item1); 
            var endEt = (r.Item2 - r.Item1) * (apexEt - startEt) / (1 - r.Item1) + startEt;
           // Console.WriteLine(apexEt + " " + prevStartEt + " " + prevR.Item1 + " " + r.Item1 + " " + sigma); // 46.22144 45.73628 0.9925 0.0001 0.002511887
            ets = new float[templetLength];
            ints = new float[templetLength];

            var mu = sigma * sigma;
            var pdf = new MathNet.Numerics.Distributions.LogNormal(mu, sigma);

            var deltaEt = (endEt - startEt) / (templetLength - 1);
            var deltaP = (r.Item2 - r.Item1) / (templetLength - 1);

            for (var i = 0; i < templetLength; i++)
            {
                ets[i] = startEt + deltaEt * i;
                ints[i] = (float) pdf.Density(r.Item1 + deltaP * i);//GetDensity(sigma, r.Item1 + deltaP * i);//(float)MathNet.Numerics.Distributions.LogNormal.PDF(mu, sigma, r.Item1 + deltaP * i);      
               // Console.Write(ints[i] + " ");
            }
           // Console.WriteLine();

           // area = (apexEt - startEt);

            area = deltaEt /deltaP;
         }
       */
    }
}
