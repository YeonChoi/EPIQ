using System;
using System.Collections.Generic;
using System.ComponentModel;
using Accord.MachineLearning.VectorMachines.Learning;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Sequence;
using InformedProteomics.Backend.MassSpecData;
using MathNet.Numerics.Statistics;

namespace Epiq
{
    public class Params
    {
        public static bool UseLabelingEfficiencyCorrection = false; // TODO read this from runparams file

        public static readonly sbyte[] IsotopeIndex = {0, 1, 2, 3, 4,}; // 
        // public const int MaxNumConsecutiveScansWithoutPeak = 10;
        public static readonly int TemplateDefinedChannelCountThreshold = 1; // # channel /2 + 1;
        public static readonly float CosineThresholdForFitting = .3f; // the lower the more qxic
        public static readonly float CosineThresholdForCoelutionFitting = .5f; // the lower the more qxic

        public static readonly float RatioSimilarityCosineThreshold = .8f;
            //  1 - .25 * label number ?? // .95 for 3 0.85 for 6..

        public static readonly float PsmQvalueThreshold = .01f;
        public static readonly float PepQvalueThreshold = .01f;
        public static readonly float ProteinQvalueThreshold = .01f;
        public static readonly float InitSpecEValueThreshold = 1E-8f;//1E-8f;
        public static readonly float XicClusterOverapRtThreshold = .8f;
        public static readonly float MinLabelQuantity = -0f;
        public static readonly int NumMatchedPepsPerProtein = 1;
        public static readonly int NumMatchedPsmsPerProtein = 1;
        public static readonly int NumMatchedPsmsPerPeptide = 1;
        public static readonly float SnrThreshold = 8f; // 8f in normal;
        public static readonly float SnrThresholdForSingleHitProtein = 12f; // 12f in normal;
        //public static readonly float CosineThreshold = -100f;
        public static float MinMeanQuantity = -0f;
        public static readonly int MinXicPointCntrInXic = 2; //
        // public static readonly double MinQfeatureSnr = 10.0;
        public static readonly float RelativeIntensityThresholdForPeakDetection = 0.2f;
        public static readonly int MaxParallelThreads = -1; //8; -1 for unlimited number of concurrent threads.
        public static readonly int TemplateLength = 20; // 40 for training
        public static readonly int RtBinCount = 30;
        public static readonly int BinNumberForTemplateFitting = 15; // 50 for training
        /* Constants for D-shift prediction */
        private static readonly int RtNormalizeMinPercentile = 20;
        private static readonly int RtNormalizeMaxPercentile = 80;
       // private static readonly float EtDeltaCoef = 0.002f;//0.002f
        public static readonly float MaxFeatureSpanCoef = 0.06f;
        public static readonly float MinFeatureSpanCoef = 0.001f;

        /* Training */
        public static readonly int TemplateLengthforTraining = 80; 
        public static readonly int BinNumberForTemplateFittingForTraining = 50; 
        public static readonly float CosineThresholdforTraining = 0.9f;
        public static readonly double TrainingSpecEvalueThreshold = 1E-12;
        private static readonly float RtDeltaCoefTraining = 0.007f;

        /* SVM model of D-shift */
        public static IntPtr DShiftModelIntPtr;

        /* Run-time determined Et Parameters */
        public static float RtNormMin { get; private set; }
        public static float RtNormMax { get; private set; }
        public static float RtSpan { get; private set; }
        public static float RtDeltaTraining { get; private set; }
        public static float MaxFeatureSpan { get; private set; }
        public static float MinFeatureSpan { get; private set; }

        /* Enzyme */
        public static Enzyme Enzyme = Enzyme.Trypsin;
        
        /* Amino Acid set */
        public static AminoAcidSet AminoAcidSet = AminoAcidSet.GetStandardAminoAcidSetWithCarboamidomethylCys();

        /* Label Parameters */

        //public static string LabelSite;
        //public static Dictionary<sbyte, float> ChannelToLabelMass = new Dictionary<sbyte, float>();
        //public static Dictionary<string, sbyte> LabelMassToChannel = new Dictionary<string, sbyte>();
        //public static sbyte[] LabelNumberArr;
        //public static readonly Dictionary<sbyte, int> DeuteriumCount = new Dictionary<sbyte, int>();

            
        public static Composition[] PerLabelIsotopeComposition;
        public static void InitParams(string labelString, string dshiftModelPath = null, string dshiftStandardPath = null)
        {
            string[] labelStrings;
            if (labelString == "DE") labelStrings = LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["Diethylation 6plex"];
            else if (labelString == "DENoDet") labelStrings = LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["Diethylation 6plex No Deuterium"];
            else if (labelString == "DE5") labelStrings = LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["Diethylation 5plex"];
            else if (labelString == "DM") labelStrings = LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["Dimethylation 5plex"];
            else if (labelString == "DE3") labelStrings = LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["Diethylation 3plex C13"];
            else if (labelString == "DM3") labelStrings = LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["Dimethylation 3plex"];
            else throw new Exception(String.Format("Unknown labelString: {0}", labelString));

            InitParams(labelStrings, dshiftModelPath, dshiftStandardPath);

            /*
            if (labelString == "DE")
            {
                PerLabelIsotopeComposition = new Composition[6];
                for (var i = 0; i < 6; i++)
                {
                    PerLabelIsotopeComposition[i] = new Composition(4, 8 - i * 2,0,0,0);
                }
            }
            else if (labelString == "DE3")
            {
                PerLabelIsotopeComposition = new Composition[3];
                for (var i = 0; i < 3; i++)
                {
                    PerLabelIsotopeComposition[i] = new Composition(4 - i * 2, 8, 0, 0, 0);
                }
            }
            else if (labelString == "DM3")
            {
                PerLabelIsotopeComposition = new Composition[3]; //{"^|K 0_28.0313 4_32.0564 6_36.0757"};
                PerLabelIsotopeComposition[0] = new Composition(2, 4, 0, 0, 0);
                PerLabelIsotopeComposition[1] = new Composition(2, 0, 0, 0, 0);
                PerLabelIsotopeComposition[2] = new Composition(0, -2, 0, 0, 0);
            }
             */
        }

        public static void InitParams(string[] labelStrings, string dshiftModelPath = null, 
                                      string dshiftStandardPath = null)
        {
            Console.WriteLine();
            Console.WriteLine("Using RT prediction model {0}", dshiftModelPath);
            Console.WriteLine("Using RT prediction model standard {0}", dshiftStandardPath);

            LabelingEfficiencyCorrection.ParseLabelingEfficiencyValues(UseLabelingEfficiencyCorrection);
            Console.WriteLine();

            if (!LabelList.ParseLabels(labelStrings)) InitParams(labelStrings[0], dshiftModelPath, dshiftStandardPath);
            DShift.ReadFiles(dshiftModelPath, dshiftStandardPath);


            /*
            if (!LabelList.ParseLabels(labelStrings))
            {
                Console.WriteLine("Label strings:");
                foreach (var str in labelStrings) Console.WriteLine(str);
                throw new Exception("Invalid label string; See above strings");
            }
             */
            //TODO : what if it fails to init parameters?
        }


        /*private static void ParseLabelString(string labelString)
        {
            var fields = labelString.Split(' ');
            LabelSite = fields[0];

            var dCountMassList = new List<Tuple<sbyte, float>>(); //Parse
            for(var i=1; i<fields.Length; i++)
            {
                var dCountMassPair  = fields[i].Split('_');
                var dCount = Convert.ToSByte(dCountMassPair[0]);
                var mass = Convert.ToSingle(dCountMassPair[1]);
                dCountMassList.Add(new Tuple<sbyte, float>(dCount, mass));
            }
            dCountMassList = dCountMassList.OrderBy(x => x.Item2).ThenBy(x => x.Item1).ToList();

            LabelNumberArr = new sbyte[dCountMassList.NumOfBoundLabels];
            for (sbyte i = 0; i < dCountMassList.NumOfBoundLabels; i++) //Save
            {
                LabelNumberArr[i] = i;
                ChannelToLabelMass[i] = dCountMassList[i].Item2;
                LabelMassToChannel["+" + dCountMassList[i].Item2.ToString("F3")] = i;
                DeuteriumCount[i] = dCountMassList[i].Item1;
            }
        }
        */

        public static void InitRtParams(LcMsRun run)
        {
            var ms2ScanRtList = new List<double>();
            for (var sn = run.MinLcScan; sn <= run.MaxLcScan; sn++)
            {
                if (run.GetMsLevel(sn) != 2) continue;
                ms2ScanRtList.Add(run.GetElutionTime(sn));
            }
            RtNormMin = (float) Statistics.Percentile(ms2ScanRtList, RtNormalizeMinPercentile);
            RtNormMax = (float) Statistics.Percentile(ms2ScanRtList, RtNormalizeMaxPercentile);
            RtSpan = RtNormMax - RtNormMin;

            MaxFeatureSpan = MaxFeatureSpanCoef*RtSpan;
            MinFeatureSpan = MinFeatureSpanCoef*RtSpan;

           // EtDelta = EtDeltaCoef*RtSpan;
            RtDeltaTraining = RtDeltaCoefTraining*RtSpan;

            Console.WriteLine(@"%RtMin: {0}, RtMax: {1}", RtNormMin, RtNormMax);
            Console.WriteLine(@"%RtSpan: {0}", RtSpan);
            Console.WriteLine(@"%RtDelta For Training: {0}", RtDeltaTraining);
            Console.WriteLine(@"%Max Feature Span: {0}, Min Feature Span: {1}", MaxFeatureSpan, MinFeatureSpan);
            //Console.WriteLine();
        }
    }
}