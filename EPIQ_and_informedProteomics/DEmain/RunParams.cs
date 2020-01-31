using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using InformedProteomics.Backend.Data.Spectrometry;

namespace Epiq
{
    public class RunParams
    {
        public string[][][] Raws;
        public string[][][] Tsvs;
        public string[][][] PsmTmpOut;
        public bool UsePredefinedLabelString;
        public string PredefinedLabelString;
        public string[] LabelStrings;
        public string FastaPath;
        public int RtShiftPredictionOption;
        public string[] RTshiftModelPath;
        public float Tolerence;
        public string MFileDir;
        public string QuantFileDir;
        public bool WriteExcel;
        public bool WriteMfiles;


        private Dictionary<Tuple<int, int, int>, Tuple<string, string>> _runDimensions = new Dictionary<Tuple<int, int, int>, Tuple<string, string>>();
        //private List<List<List<Tuple<string, string>>>> _runDimensions = new List<List<List<Tuple<string, string>>>>();
        private Dictionary<string, List<string>> _argsDict = new Dictionary<string, List<string>>();
        private const string SpecFileColRegexStr = @"^Spec File \d";
        private const string CustomLabelRegexStr = @"^Custom Label String [\d]*";

        public RunParams(string path)
        {
            foreach (var line in File.ReadAllLines(path))
            {
                if (line == String.Empty || line.Trim().StartsWith(@"#"))
                {
                    continue;
                }
                var fields = line.Trim().Split('\t');
                _argsDict[fields[0]] = fields.Skip(1).ToList();
            }
            ParseOptions();
            CollectLabelStrings();
            ParseSpecFileRows();
        }

        public void Run(bool overwrite=true)
        {
            PipeLine.Run(Raws,
                         Tsvs,
                         PsmTmpOut,
                         LabelStrings,
                         RTshiftModelPath,
                         FastaPath,
                         new Tolerance(Tolerence),
                         quantfileDir: QuantFileDir,
                         mfileDir: MFileDir,
                         overwrite: overwrite,
                         writeExcel: WriteExcel);
        }


        private void ParseOptions()
        {
            FastaPath = _argsDict["Fasta File Path"][0];

            RtShiftPredictionOption = Convert.ToInt32(_argsDict["RT Shift Prediction Option"][0]);

            if (RtShiftPredictionOption == 0)
            {
                RTshiftModelPath = new [] {(string) null, (string) null};
            }
            else if (RtShiftPredictionOption == 1)
            {
                var lcType = _argsDict["RT Shift Model LC Type"][0];
                var lcIdx = Array.IndexOf(BuiltInRtModels.RtModelLcTypes, lcType);
                if (lcIdx > -1)
                {
                    RTshiftModelPath = BuiltInRtModels.GetRtBuiltInRtModelPaths(lcType);
                    if (!File.Exists(RTshiftModelPath[0]))
                        throw new Exception(String.Format("Invalid Option: Cannot find RT Shift Model file {0}", RTshiftModelPath[0]));
                    if (!File.Exists(RTshiftModelPath[1]))
                        throw new Exception(String.Format("Invalid Option: Cannot find RT Shift Standard file {0}", RTshiftModelPath[1]));
                }
                else
                {
                    //If invalid LCTYPE - What should we do?
                }
            }
            else if (RtShiftPredictionOption == 2)
            {
                string rtshiftModelPath = null;
                string rtshiftStandardPath = null;
                if (_argsDict["RT Shift Model Path"].Count > 0 && File.Exists(_argsDict["RT Shift Model Path"][0]))
                {
                    rtshiftModelPath = _argsDict["RT Shift Model Path"][0];
                }
                else
                {
                    throw new Exception(String.Format("Invalid Option: Cannot find RT Shift Model file {0}", String.Join(";", _argsDict["RT Shift Model Path"])));
                }
                if (_argsDict["RT Shift Standard Path"].Count > 0 && File.Exists(_argsDict["RT Shift Standard Path"][0]))
                {
                    rtshiftStandardPath = _argsDict["RT Shift Standard Path"][0];
                }
                else
                {
                    throw new Exception(String.Format("Invalid Option: Cannot find RT Shift Standard file {0}", String.Join(";", _argsDict["RT Shift Standard Path"])));
                }

            
                RTshiftModelPath = new[] {rtshiftModelPath, rtshiftStandardPath};
            }
            else
            {
                throw new Exception(String.Format("Invalid Option: RT Shift Model LC Type = {0}"));
            }

            try
            {
                Tolerence = Convert.ToSingle(_argsDict["Tolerence"][0]);
                WriteExcel = Convert.ToBoolean(_argsDict["Write Excel"][0]);
                WriteMfiles = Convert.ToBoolean(_argsDict["Write XIC .m Files"][0]);
            }
            catch (Exception e)
            {
                //TODO notify which option is invalid
                throw new Exception(String.Format("Invalid Option:{0}", e.StackTrace), e);
            }
            try
            {
                QuantFileDir = _argsDict["Quantification Output Dir"][0];
                MFileDir = _argsDict["XIC mfile Dir"][0];
            }
            catch (Exception e)
            {
                if (_argsDict["Quantification Output Dir"].Count < 1)
                    throw new Exception("Quantification output path is not set properly", e);
                if (WriteMfiles && _argsDict["XIC mfile Dir"].Count < 1)
                    throw new Exception("XIC mfile path is not set properly.", e);
            }
            if (WriteMfiles)
            {
                try
                {
                    MFileDir = _argsDict["XIC mfile Dir"][0];
                }
                catch (Exception e)
                {
                    if (WriteMfiles && _argsDict["XIC mfile Dir"].Count < 1)
                        throw new Exception("XIC mfile path is not set properly.", e);
                }
            }
        }


        private void CollectLabelStrings()
        {
            try
            {
                UsePredefinedLabelString = Convert.ToBoolean(_argsDict["Use Predefined Labeling Scheme"][0]);
                if (UsePredefinedLabelString)
                {
                    PredefinedLabelString = _argsDict["Predefined Labeling Scheme"][0];
                    if (PredefinedLabelString == string.Empty || PredefinedLabelString == "")
                    {
                        throw new Exception(
                            "'Use Predefined Labeling Scheme' is True, but cannot find any predefined labeling scheme");
                    }
                    else
                    {
                        LabelStrings = LabelingSchemes.PredefinedLabelingSchemeToLabelStrings[PredefinedLabelString];
                    }
                    return;
                }

                var customLabelRegex = new Regex(CustomLabelRegexStr);
                var customLabelList = new List<string>();
                foreach (var firstCol in _argsDict.Keys)
                {
                    if (customLabelRegex.Match(firstCol).Success)
                    {
                        customLabelList.Add(_argsDict[firstCol][0]);
                    }
                }
                LabelStrings = customLabelList.ToArray();
                Console.WriteLine(String.Join(" ", LabelStrings));
            }
            catch (Exception e)
            {
               throw new Exception("Error while parsing 'Labeling Strings' part", e);
            }
        }


        private void ParseSpecFileRows()
        {
            var specFileRegex = new Regex(SpecFileColRegexStr);
            foreach (var firstCol in _argsDict.Keys)
            {
                if (specFileRegex.Match(firstCol).Success)
                {
                    ParseSpecFileEachRows(_argsDict[firstCol]);
                }
            }

            var nCond = _runDimensions.Keys.Max(x => x.Item1);
            Raws = new string[nCond][][];
            Tsvs = new string[nCond][][];
            PsmTmpOut = new string[nCond][][];
            for (var i = 0; i < nCond; i++)
            {
                var nRep = _runDimensions.Keys.Where(x => x.Item1 == i + 1).Max(x => x.Item2);
                Raws[i] = new string[nRep][];
                Tsvs[i] = new string[nRep][];
                PsmTmpOut[i] = new string[nRep][];
                for (var j = 0; j < nRep; j++)
                {
                    var nFrac = _runDimensions.Keys.Where(x => (x.Item1 == i + 1) && (x.Item2 == j + 1)).Max(x => x.Item3);
                    Raws[i][j] = new string[nFrac];
                    Tsvs[i][j] = new string[nFrac];
                    PsmTmpOut[i][j] = new string[nFrac];
                    for (var k = 0; k < nFrac; k++)
                    {
                        Raws[i][j][k] = _runDimensions[new Tuple<int, int, int>(i + 1, j + 1, k + 1)].Item1;
                        Tsvs[i][j][k] = _runDimensions[new Tuple<int, int, int>(i + 1, j + 1, k + 1)].Item2;
                        PsmTmpOut[i][j][k] = Path.ChangeExtension(Raws[i][j][k], ".bin");
                    }
                }
            }
        }


        private void ParseSpecFileEachRows(List<string> fields)
        {
            try
            {
                var condIdx = Convert.ToInt32(fields[0]);
                var repIdx = Convert.ToInt32(fields[1]);
                var fracIdx = Convert.ToInt32(fields[2]);
                var rawPath = fields[3];
                string tsvPath;
                if (fields.Count >= 5)
                    tsvPath = fields[4];
                else
                    tsvPath = Path.ChangeExtension(".raw", ".tsv");
                _runDimensions[new Tuple<int, int, int>(condIdx, repIdx, fracIdx)] = new Tuple<string, string>(rawPath, tsvPath);
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new ArgumentOutOfRangeException("Spectrum file path or index is not set properly.", e);
            }
        }

    }
}
