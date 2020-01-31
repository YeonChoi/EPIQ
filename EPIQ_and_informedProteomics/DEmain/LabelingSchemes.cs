using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epiq
{
    public static class LabelingSchemes
    {
        public static readonly List<string> LabelSites = new List<string>();
        public static readonly Dictionary<string, string> DictLabelSitesToShort = new Dictionary<string, string>();
        public static readonly Dictionary<string, string> DictLabelSitesToLong = new Dictionary<string, string>();
        public static readonly string LabelFreeName = "Label free";

        private static readonly string AminoAcids = @"ACDEFGHIKLMNPQRSTUVWY";
        private static readonly List<Tuple<string, string>> LabelSiteLongAndShortNames = new List<Tuple<string, string>> 
        {
            new Tuple<string, string>("N-term", "^"),
            new Tuple<string, string>("C-term", "$"),
        };

        static LabelingSchemes()
        {
            foreach (var shortAndLongName in LabelSiteLongAndShortNames)
            {
                LabelSites.Add(shortAndLongName.Item1);
                DictLabelSitesToShort[shortAndLongName.Item1] = shortAndLongName.Item2;
                DictLabelSitesToLong[shortAndLongName.Item2] = shortAndLongName.Item1;
            }

            for (int i=0;i<AminoAcids.Length;i++)
            {
                var aa = AminoAcids[i].ToString();
                LabelSites.Add(aa);
                DictLabelSitesToShort[aa] = aa;
                DictLabelSitesToLong[aa] = aa;

                var nTermShort = "^" + aa;
                var nTermLong = "N term + " + aa;
                var cTermShort = aa + "$";
                var cTermLong = "C term + " + aa;

                LabelSites.Add(nTermLong);
                DictLabelSitesToShort[nTermLong] = nTermShort;
                DictLabelSitesToLong[nTermShort] = nTermLong;

                LabelSites.Add(cTermLong);
                DictLabelSitesToShort[cTermLong] = cTermShort;
                DictLabelSitesToLong[cTermShort] = cTermLong;
            }
        }

        public static readonly object[] PredefinedLabelingSchemes = new string[]
        {
            "Diethylation 6plex",
            "Diethylation 5plex",
            "Diethylation 3plex C13",
            "Dimethylation 3plex",
            "Dimethylation 5plex",
            "SILAC 6plex",
            LabelFreeName
        };

        public static List<string[]> TokenizeLabelingScheme(string[] labelingScheme)
        {
            var tokenizedStrs = new List<List<string>>();

            var numLabel = 0;
            foreach (var labelstr in labelingScheme)
            {
                var token = labelstr.Split(' ');
                numLabel = Math.Max(numLabel, token.Length - 1);
            }
            for (var i = 0; i < numLabel; i++)
            {
                tokenizedStrs.Add(new List<string>());
            }

            foreach (var labelstr in labelingScheme)
            {
                var token = labelstr.Split(' ');
                var labelSiteList = token[0].Split('|');
                foreach (var labelSite in labelSiteList)
                {
                    for (var i = 0; i < numLabel; i++)
                    {
                        tokenizedStrs[i].Add(labelSite + " " + token[i+1]);
                    }
                }
            }

            var retList = new List<string[]>();
            foreach (var strList in tokenizedStrs)
            {
                retList.Add(strList.ToArray());
            }
            return retList;
        }

        public static readonly Dictionary<string, string[]> PredefinedLabelingSchemeToLabelStrings = new Dictionary<string, string[]>()
        {
            {"Diethylation 6plex", 
                new[]
                {
                    "^ 0_56.06260 2_58.07515 4_60.08771 6_62.10026 8_64.11281 10_66.12537",
                    "K 0_56.06260 2_58.07515 4_60.08771 6_62.10026 8_64.11281 10_66.12537"
                }},
            {"Diethylation 5plex", 
                new[]
                {
                    "^ 2_58.07515 4_60.08771 6_62.10026 8_64.11281 10_66.12537", 
                    "K 2_58.07515 4_60.08771 6_62.10026 8_64.11281 10_66.12537"
                }},
            {"Diethylation 3plex C13", 
                new []
                {
                    "^ 0_56.06260 0_58.069310 0_60.076020", 
                    "K 0_56.06260 0_58.069310 0_60.076020"
                }},
            {"Dimethylation 3plex", 
                new []
                {
                    "^ 0_28.0313 4_32.0564 6_36.0757",
                    "K 0_28.0313 4_32.0564 6_36.0757"
                }},
            {"Dimethylation 5plex", 
                new []
                {
                    "^ 0_28.0313 2_30.0439 4_32.0564 6_34.0690 6_36.0757",
                    "K 0_28.0313 2_30.0439 4_32.0564 6_34.0690 6_36.0757"
                }},
            {"SILAC 6plex", 
                new[]
                {
                    "K 0_0.000000 0_1.9940697 4_4.025107 0_6.020129026 8_8.0502139672 9_11.05056404995",
                    "R 0_0.000000 0_1.9940697 0_3.9881395 7_7.0439372213 7_9.0380070077 7_11.0320767941",
                }},
            {LabelFreeName,
                new []
                {
                    ". 0_0"
                }},
        };

    }
}
