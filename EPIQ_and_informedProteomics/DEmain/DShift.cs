using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using LibSVMsharp;

namespace Epiq
{
    public static class DShift
    {
        private static IntPtr _dShiftModelIntPtr;
        private static bool _noRtShiftModel = false;

        private static readonly Dictionary<string, StandardFactor> _featureStandardFactors =
            new Dictionary<string, StandardFactor>();

        private static string _standardPath;
        private static string _modelPath;
        public static List<Regex> DshiftAaRegexList { get; private set; }

        public static void ReadFiles(string modelPath, string standardPath)
        {
            DshiftAaRegexList = new List<Regex>();
            foreach (var aa in LabelList.DeuteratedLabelingSites)
            {
                DshiftAaRegexList.Add(new Regex("[" + aa + "]"));
            }

            if ((modelPath != null) && (standardPath != null))
            {
                _modelPath = modelPath;
                _standardPath = standardPath;
                _dShiftModelIntPtr = SVMModel.Allocate(SVM.LoadModel(_modelPath));
                SetStandardFactors();
                _noRtShiftModel = false;
            }
            else
            {
                _noRtShiftModel = true;
            }
        }

        private static void SetStandardFactors()
        {
            foreach (var line in File.ReadLines(_standardPath))
            {
                var fields = line.Split(' ');
                var name = fields[0];
                var sf = new StandardFactor
                {
                    Mean = Convert.ToDouble(fields[1]),
                    Std = Convert.ToDouble(fields[2])
                };
                _featureStandardFactors[name] = sf;
            }
        }

        private static double GetStandardizedValue(double input, string featureName)
        {
            StandardFactor sf;
            try
            {
                sf = _featureStandardFactors[featureName];
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("{0} cannot be found in standard file {1}", featureName,
                    _standardPath));
            }
            return (input - sf.Mean)/sf.Std;
        }


        public static double[] GetDshiftAaProportions(string unmodPep)
        {
            var ret = new double[DshiftAaRegexList.Count];
            foreach (var x in ret) { if (x != 0.0) { Console.WriteLine("Achtung!@!"); } }
            if (unmodPep.Length == 0)
                return ret;

            for(var i=0; i<DshiftAaRegexList.Count; i++)
            {
                ret[i] = Convert.ToDouble(DshiftAaRegexList[i].Matches(unmodPep).Count)/Convert.ToDouble(unmodPep.Length);
            }
            return ret;
        }

        public static double GetNormedEt(double centerEt)
        {
            return (centerEt - Params.RtNormMin)/Params.RtSpan;
        }

        private static SVMNode[] GetSvmNode(int dCount, double et, int pepLen, double[] aaRatio)
        {
            // Dnum Et PepLength AARatio AvragePeakWidth
            //var ret = new SVMNode[5];
            var ret = new SVMNode[3 + DshiftAaRegexList.Count];
            ret[0] = new SVMNode(1, GetStandardizedValue(Convert.ToDouble(dCount), @"Dnum"));
            ret[1] = new SVMNode(2, GetStandardizedValue(et, @"NormedRepreEt"));
            ret[2] = new SVMNode(3, GetStandardizedValue(Convert.ToDouble(pepLen), @"PeptideLength"));
            for (var i = 0; i < DshiftAaRegexList.Count; i++)
            {
                ret[i+3] = new SVMNode(i+4, GetStandardizedValue(aaRatio[i], string.Format("{0}Ratio", DshiftAaRegexList[i])));
            }
            // Warning: DeuteratedLabelingSites are sorted alphabetically. 
            //ret[4] = new SVMNode(5, GetStandardizedValue(width, @"NormedInitSignalWidth"));

            return ret;
        }

        public static float GetRtOffset(Ms2Result id, sbyte fromCn, sbyte toCn, double centerEt, bool forTraining = false)
        {
            //return 0;//
            if (forTraining || (fromCn == toCn)) return 0;
            if (id.GetDetCount(fromCn) == id.GetDetCount(toCn)) return 0;
            if (_noRtShiftModel) return 0;
            return GetRtOffset(id, fromCn, centerEt) - GetRtOffset(id, toCn, centerEt);
        }

        public static float GetRtOffset(Ms2Result id, sbyte channel, double centerEt, bool forTraining = false)
        {
            //return 0;//TODO
            if (forTraining)
                return 0;

            var dCount = id.GetDetCount(channel);
            if (dCount == 0) return 0;
            var normedEt = GetNormedEt(centerEt);
            var unmodPep = id.UnmodifiedPeptide;
            var pepLen = unmodPep.Length;
            var aaRatio = GetDshiftAaProportions(unmodPep);
            //var normedWidth = initSignalWidth/Params.RtSpan;

            return Params.RtSpan*
                   (float) SVM.Predict(_dShiftModelIntPtr, GetSvmNode(dCount, normedEt, pepLen, aaRatio));
        }

        private class StandardFactor
        {
            public double Mean;
            public double Std;
        }


        /*
        public static double GetAverageNormedSignalWidth(ExtendedTemplate[] extendedTemplates)
        {
            if (extendedTemplates == null) throw new Exception("Error: Cannot calculate GetAverageNormed: ExtendedTemplate[] is null.");

            var templateExists = false;
            double summedWidth = 0;
            for (int cn=0; cn< extendedTemplates.Length; cn++)
            {
                if (extendedTemplates[cn] == null) continue;
                templateExists = true;
                summedWidth += extendedTemplates[cn].SignalWidth;
            }
            if (!templateExists) throw new Exception("Error: Cannot calculate GetAverageNormed: ExtendedTemplate[] is empty");

            return (summedWidth/extendedTemplates.Length)/Params.RtSpan;
        }
         */
    }
}