using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.Database;
using MathNet.Numerics;
using Microsoft.Office.Interop.Excel;

namespace Epiq
{
    public class PipeLine
    {
        private static string[][][] _specFilePaths; // condition index, replicate index, fractionation index
        private static string[][][] _tsvs;
        private static string[][][] _psmTmpOutFiles;

        private static QuantifiedPsmList[][][] _psmLists;
        private static QuantifiedPeptideDictionary[][] _peptideDictionariesPerConditionReplicate;
        private static QuantifiedProteinGroupDictionary[][] _proteinGroupDictionariesPerConditionReplicate;

        private static QuantifiedPeptideDictionary[] _peptideDictionariesPerCondition;
        private static QuantifiedProteinGroupDictionary[] _proteinGroupDictionariesPerCondition;
        
        private static QuantifiedPeptideDictionary _peptideDictionary;
        private static QuantifiedProteinGroupDictionary _proteinGroupDictionary;

        public static int Run(string[][][] specFilePaths, string[][][] tsvs, string[][][] psmTmpOutFiles, string[] labelStrings, string[] svrPaths, string proteinFasta,
            Tolerance tolerance, string mfileDir = null, string quantfileDir = null, bool overwrite = false, bool writeExcel = false)
        {
            SetControl();
            IsotopeEnvelopeCalculator.SetMaxNumIsotopes(Params.IsotopeIndex.Length);
            Params.InitParams(labelStrings, svrPaths[0], svrPaths[1]);
            _specFilePaths = specFilePaths;
            _tsvs = tsvs;
            _psmTmpOutFiles = psmTmpOutFiles;
            RunPsmQuantificationPerSpecFile(tolerance, overwrite, mfileDir);
            RunProteinPeptideQuantification(tolerance);
            WriteMfiles(mfileDir);
            var fastaDb = new FastaDatabase(proteinFasta);
            if(writeExcel) WriteExcelFiles(quantfileDir, fastaDb);
            WriteTsvFiles(quantfileDir, fastaDb);

            return 0;
        }
        
        public static int Run(string[][][] specFilePaths, string[][][] tsvs, string[][][] psmTmpOutFiles, string labelString, string[] svrPaths, string proteinFasta, Tolerance tolerance, string mfileDir = null,
            string quantfileDir = null, bool overwrite = false, bool writeExcel = false)
        {
            return Run(specFilePaths, tsvs, psmTmpOutFiles, new[] { labelString }, svrPaths, proteinFasta, tolerance, mfileDir, quantfileDir, overwrite, writeExcel);
        }

        public static int RunTraining(string[] specFilePaths, string[] tsvs, string[] psmTmpOutFiles, string labelString, Tolerance tolerance, string[] trainingOutfiles, bool overwrite, bool writeXicClusters = false)
        {
            return RunTraining(specFilePaths, tsvs, psmTmpOutFiles, new[] {labelString}, tolerance, trainingOutfiles, overwrite, writeXicClusters);
        }

        public static int RunTraining(string[] specFilePaths, string[] tsvs, string[] psmTmpOutFiles, string[] labelStrings, Tolerance tolerance, string[] trainingOutfiles, bool overwrite, bool writeXicClusters = false)
        {
            SetControl();
            IsotopeEnvelopeCalculator.SetMaxNumIsotopes(Params.IsotopeIndex.Length);
            Params.InitParams(labelStrings);

            RunPsmTraining(tolerance, specFilePaths, tsvs, psmTmpOutFiles, overwrite, writeXicClusters);

            for (var i=0; i<psmTmpOutFiles.Length; i++)
            {
                var psmOutFile = psmTmpOutFiles[i];
                var trainingOutPath = trainingOutfiles[i];
                var trainingPsmLists = PsmQuantification.RetreivePsmList(psmOutFile, training:true);
                if (!Directory.GetParent(trainingOutPath).Exists) Directory.GetParent(trainingOutPath).Create();
                trainingPsmLists.WriteTrainingFile(trainingOutPath);
            }
            return 0;
        }


        public static int RunIsotopeImpurityMeasurement(string[] labelStrings, Dictionary<sbyte, Tuple<string, string, string, string>> filePathPerLabelDictionary, 
                                                        Tolerance tolerance, List<string> toCheckLabelSiteList)
        {
            SetControl();
            IsotopeEnvelopeCalculator.SetMaxNumIsotopes(Params.IsotopeIndex.Length);
            IsotopeImpurityValues.SetDefaultImpurityValues();
            var tokenedLabelList = LabelingSchemes.TokenizeLabelingScheme(labelStrings);

            for (sbyte i=0; i<tokenedLabelList.Count; i++)
            {
                foreach (var labelSiteSetStr in toCheckLabelSiteList)
                {
                    // TODO check if no file specified
                    Params.InitParams(tokenedLabelList[i]);
                    var specFilePath = filePathPerLabelDictionary[i].Item1;
                    var searchFilePath = filePathPerLabelDictionary[i].Item2;
                    var outFilePath = filePathPerLabelDictionary[i].Item3.Replace(".tsv", "_" + labelSiteSetStr.Replace("|", "_") + ".tsv");
                    var usedAtoms = filePathPerLabelDictionary[i].Item4;

                    Console.WriteLine("Processing {0} for label site {1}", specFilePath, labelSiteSetStr);
                    var iiMeasure = new IsotopeImpurityMeasurement(specFilePath, searchFilePath, tolerance, usedAtoms, outFilePath, labelSiteSetStr);
                    iiMeasure.Run();
                }
            }

            return 0;
        }


        private static void RunProteinPeptideQuantification(Tolerance tolerance)
        {
            _psmLists = new QuantifiedPsmList[_specFilePaths.Length][][];
            _peptideDictionariesPerConditionReplicate = new QuantifiedPeptideDictionary[_specFilePaths.Length][];
            _peptideDictionariesPerCondition = new QuantifiedPeptideDictionary[_specFilePaths.Length];
            _proteinGroupDictionariesPerConditionReplicate = new QuantifiedProteinGroupDictionary[_specFilePaths.Length][];
            _proteinGroupDictionariesPerCondition = new QuantifiedProteinGroupDictionary[_specFilePaths.Length];
            
            _peptideDictionary = new QuantifiedPeptideDictionary();
            _proteinGroupDictionary = new QuantifiedProteinGroupDictionary();

            for (short condition = 1; condition <= _specFilePaths.Length; condition++)
            {
                var c = condition - 1;
                _psmLists[c] = new QuantifiedPsmList[_specFilePaths[c].Length][];
                _peptideDictionariesPerConditionReplicate[c] = new QuantifiedPeptideDictionary[_specFilePaths[c].Length];
                _peptideDictionariesPerCondition[c] = new QuantifiedPeptideDictionary();

                _proteinGroupDictionariesPerConditionReplicate[c] = new QuantifiedProteinGroupDictionary[_specFilePaths[c].Length];
                _proteinGroupDictionariesPerCondition[c] = new QuantifiedProteinGroupDictionary();

                var psmListsPerCondition = new QuantifiedPsmList();
                for (short replicate = 1; replicate <= _specFilePaths[c].Length; replicate++)
                {
                    var r = replicate - 1;
                    _psmLists[c][r] = new QuantifiedPsmList[_specFilePaths[c][r].Length];
                    var psmListsPerReplicate = new QuantifiedPsmList();
                    _peptideDictionariesPerConditionReplicate[c][r] = new QuantifiedPeptideDictionary();
                    _proteinGroupDictionariesPerConditionReplicate[c][r] = new QuantifiedProteinGroupDictionary();

                    for (short fraction = 1; fraction <= _specFilePaths[c][r].Length; fraction++)
                    {
                        var f = fraction - 1;
                        var psmOutFile = _psmTmpOutFiles[c][r][f];
                        var psmList = PsmQuantification.RetreivePsmList(psmOutFile);

                        psmList.FilterByQvalue(Params.PsmQvalueThreshold);
                        psmList.RemoveOverlappingXicClusters(Params.XicClusterOverapRtThreshold, tolerance);
                        
                        var psmIdPrefix = String.Format("c{0}r{1}f{2}", condition, replicate, fraction);
                        psmList.AssignPsmIds(psmIdPrefix);
                        _psmLists[c][r][f] = psmList;
                        psmListsPerCondition.AddRange(psmList);
                        psmListsPerReplicate.AddRange(psmList);
                    }

                    psmListsPerReplicate.FilterByPeptideSimilarity(Params.RatioSimilarityCosineThreshold);
                    psmListsPerReplicate.FilterBySnrAndMeanQuantity(Params.SnrThreshold, Params.MinMeanQuantity);

                   // psmListsPerReplicate.FilterByCosine(Params.CosineThreshold);
                    
                    for (short fraction = 1; fraction <= _specFilePaths[c][r].Length; fraction++)
                    {
                        var f = fraction - 1;
                        if (_specFilePaths[c][r].Length <= 1) continue;
                        PeptideQuantification.UpdateDictionary(_peptideDictionary, _psmLists[c][r][f], condition, replicate, fraction);
                        PeptideQuantification.UpdateDictionary(_peptideDictionariesPerConditionReplicate[c][r], _psmLists[c][r][f], condition, replicate, fraction);
                    }

                    //if (_specFilePaths[c].Length <= 1) continue;
                    ProteinGroupQuantification.UpdateDictionary(_proteinGroupDictionary, psmListsPerReplicate, condition, replicate);
                    ProteinGroupQuantification.UpdateDictionary(_proteinGroupDictionariesPerCondition[c], psmListsPerReplicate, condition, replicate);
                    ProteinGroupQuantification.UpdateDictionary(_proteinGroupDictionariesPerConditionReplicate[c][r], psmListsPerReplicate, condition, replicate);
                    
                    PeptideQuantification.UpdateDictionary(_peptideDictionary, psmListsPerReplicate, condition, replicate);
                    PeptideQuantification.UpdateDictionary(_peptideDictionariesPerCondition[c], psmListsPerReplicate, condition, replicate);
                    PeptideQuantification.UpdateDictionary(_peptideDictionariesPerConditionReplicate[c][r], psmListsPerReplicate, condition, replicate);
                    _peptideDictionariesPerConditionReplicate[c][r].Filter(Params.PepQvalueThreshold, Params.SnrThreshold);
               

                    Console.WriteLine(@"Condition: {0} replicate: {1}", condition, replicate);
                   // _proteinGroupDictionariesPerConditionReplicate[condition][replicate] = ProteinGroupQuantification.Run(psmListsPerReplicate);
                    //
                }
                psmListsPerCondition.FilterByPeptideSimilarity(Params.RatioSimilarityCosineThreshold);
                psmListsPerCondition.FilterBySnrAndMeanQuantity(Params.SnrThreshold, Params.MinMeanQuantity);

                //psmListsPerCondition.FilterByCosine(Params.CosineThreshold);
                    
                ProteinGroupQuantification.UpdateDictionary(_proteinGroupDictionary, psmListsPerCondition, condition, 0, true);
                ProteinGroupQuantification.UpdateDictionary(_proteinGroupDictionariesPerCondition[c], psmListsPerCondition, condition, 0, true);
               
                
                PeptideQuantification.UpdateDictionary(_peptideDictionary, psmListsPerCondition, condition);
                PeptideQuantification.UpdateDictionary(_peptideDictionariesPerCondition[c], psmListsPerCondition, condition);
                _peptideDictionariesPerCondition[c].Filter(Params.PepQvalueThreshold, Params.SnrThreshold);
                
                //
                
                for (short replicate = 1; replicate <= _specFilePaths[c].Length; replicate++)
                {
                    var r = replicate - 1;
                    for (short fraction = 1; fraction <= _specFilePaths[c][r].Length; fraction++)
                    {
                        var f = fraction - 1;
                        _psmLists[c][r][f] = _psmLists[c][r][f].GetIntersectingPsms(psmListsPerCondition);
                    }
                }
                Console.WriteLine(@"Condition: {0}", condition);
                //_proteinGroupDictionary[condition] = ProteinGroupQuantification.Run(psmListsPerCondition, true); 
                //_peptideDictionary[condition] = PeptideQuantification.Run(psmListsPerCondition);
            }
            _peptideDictionary.Filter(Params.PepQvalueThreshold, Params.SnrThreshold);
        }

        private static void RunPsmQuantificationPerSpecFile(Tolerance tolerance, bool overwrite, string xicClusterResultsPath=null)
        {
            for (var condition = 1; condition <= _specFilePaths.Length; condition++)
            {
                var c = condition - 1;
                for (var replicate = 1; replicate <= _specFilePaths[c].Length; replicate++)
                {
                    var r = replicate - 1;
                    for (var fraction = 1; fraction <= _specFilePaths[c][r].Length; fraction++)
                    {
                        var f = fraction - 1;
                        var specFilePath = _specFilePaths[c][r][f];
                        var tsv = _tsvs[c][r][f];
                        var psmOutFile = _psmTmpOutFiles[c][r][f];
                        if (!Directory.GetParent(psmOutFile).Exists) Directory.GetParent(psmOutFile).Create();
                        if (!overwrite && File.Exists(psmOutFile)) continue;
                        PsmQuantification.Run(specFilePath, tsv, tolerance, psmOutFile, xicClusterResultsPath);
                    }
                }
            }
        }

        private static void RunPsmTraining(Tolerance tolerance, string[] specFilePaths, string[] tsvs, string[] psmTmpOutFiles, bool overwrite, bool writeXicClusters = false)
        {
           
            for (var i = 0; i < specFilePaths.Length; i++)
            {
                var specFilePath = specFilePaths[i];
                var tsv = tsvs[i];
                var psmOutFile = psmTmpOutFiles[i];
                if (!Directory.GetParent(psmOutFile).Exists) Directory.GetParent(psmOutFile).Create();
                if (!overwrite && File.Exists(psmOutFile)) continue;
                DShiftTraining.Run(specFilePath, tsv, tolerance, psmOutFile, writeXicClusters);
            }
        }


        private static void SetControl()
        {
            if (Control.TryUseNativeCUDA())
            {
                Console.WriteLine(@"Using CUDA for matrix calculation...");
                Control.UseNativeCUDA();
            }
            else if (Control.TryUseNativeMKL())
            {
                Console.WriteLine(@"Using MKL for matrix calculation...");
                Control.UseNativeMKL();
            }
            else if (Control.TryUseNativeOpenBLAS())
            {
                Console.WriteLine(@"Using OpenBLAS for matrix calculation...");
                Control.UseNativeOpenBLAS();
            }
        }

        private static void WriteMfiles(string dir)
        {
            if (dir == null) return;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            for (var condition = 1; condition <= _specFilePaths.Length; condition++)
            {
                var c = condition - 1;
                for (var replicate = 1; replicate <= _specFilePaths[c].Length; replicate++)
                {
                    var r = replicate - 1;
                    for (var fraction = 1; fraction <= _specFilePaths[c][r].Length; fraction++)
                    {
                        var f = fraction - 1;
                        _psmLists[c][r][f].WriteMfile(string.Format(@"{0}psm_c{1}r{2}f{3}.m", dir, condition, replicate, fraction));
                    }
                    //if (_specFilePaths[c].Length <= 1) continue;
                    //_peptideDictionariesPerConditionReplicate[c][r].WriteMfile(string.Format(@"{0}pep_c{1}r{2}.m", dir, condition, replicate));
                    //_proteinGroupDictionariesPerConditionReplicate[c][r].WriteMfile(string.Format(@"{0}protein_c{1}r{2}.m", dir, condition, replicate));
                }
            }
            _peptideDictionary.WriteMfile(string.Format(@"{0}pep.m", dir));
            _proteinGroupDictionary.WriteMfile(string.Format(@"{0}protein.m", dir));
        }

        private static int WriteExcelFiles(string dir, FastaDatabase fastaDb)
        {
            if (dir == null) return -1;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!Directory.Exists(dir + @"\Psms")) Directory.CreateDirectory(dir + @"\Psms");

            if (!Directory.Exists(dir + @"\PeptidesPerCondition")) Directory.CreateDirectory(dir + @"\PeptidesPerCondition");
            if (!Directory.Exists(dir + @"\ProteinGroupsPerCondition")) Directory.CreateDirectory(dir + @"\ProteinGroupsPerCondition");

            if (!Directory.Exists(dir + @"\PeptidesPerConditionReplicate")) Directory.CreateDirectory(dir + @"\PeptidesPerConditionReplicate");
            if (!Directory.Exists(dir + @"\ProteinGroupsPerConditionReplicate")) Directory.CreateDirectory(dir + @"\ProteinGroupsPerConditionReplicate");

            Application xlApp = new Application();
            if (xlApp == null) return 1;
            
            fastaDb.Read();
            try
            {
                for (var condition = 1; condition <= _specFilePaths.Length; condition++)
                {
                    var c = condition - 1;
                    for (var replicate = 1; replicate <= _specFilePaths[c].Length; replicate++)
                    {
                        var r = replicate - 1;
                        for (var fraction = 1; fraction <= _specFilePaths[c][r].Length; fraction++)
                        {
                            var f = fraction - 1;
                            var tsv = _tsvs[c][r][f];
                            var givenFileName = Path.GetFileNameWithoutExtension(_specFilePaths[c][r][f]);
                            var file = string.Format(@"{0}psms_c{1}r{2}f{3}_{4}.xlsx", dir + @"Psms" + Path.DirectorySeparatorChar, condition, replicate, fraction, givenFileName);
                            
                           // Console.WriteLine(file);
                            _psmLists[c][r][f].WriteExcel(file, xlApp, tsv);
                            
                        }
                        //if (_specFilePaths[c].Length <= 1) continue;

                        var repCommonPrefix = "_" + GetCommonPrefix(_specFilePaths[c][r]);
                        _peptideDictionariesPerConditionReplicate[c][r].WriteExcel(string.Format(@"{0}peptides_c{1}r{2}{3}.xlsx", dir + @"PeptidesPerConditionReplicate" + Path.DirectorySeparatorChar , condition, replicate, repCommonPrefix), xlApp, fastaDb);
                        _proteinGroupDictionariesPerConditionReplicate[c][r].WriteExcel(string.Format(@"{0}proteinGroups_c{1}r{2}{3}.xlsx", dir + @"ProteinGroupsPerConditionReplicate" + Path.DirectorySeparatorChar, condition, replicate, repCommonPrefix), xlApp, fastaDb);     
                    }
                    var condCommonPrefix = "_" + GetCommonPrefix(_specFilePaths[c]);
                    _peptideDictionariesPerCondition[c].WriteExcel(string.Format(@"{0}peptides_c{1}{2}.xlsx", dir + @"PeptidesPerCondition" + Path.DirectorySeparatorChar, condition, condCommonPrefix), xlApp, fastaDb);
                    _proteinGroupDictionariesPerCondition[c].WriteExcel(string.Format(@"{0}proteinGroups_c{1}{2}.xlsx", dir + @"ProteinGroupsPerCondition" + Path.DirectorySeparatorChar, condition, condCommonPrefix), xlApp, fastaDb);     
                }
                var allCommonPrefix = "_" + GetCommonPrefix(_specFilePaths);
                _peptideDictionary.WriteExcel(string.Format(@"{0}peptides{1}.xlsx", dir, allCommonPrefix), xlApp, fastaDb);
                _proteinGroupDictionary.WriteExcel(string.Format(@"{0}proteinGroups{1}.xlsx", dir, allCommonPrefix), xlApp, fastaDb);
            }
            catch (COMException)
            {
                return 2;
            }
            finally
            {
                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);
            }

            return 0;
        }

        private static int WriteTsvFiles(string dir, FastaDatabase fastaDb)
        {
            if (dir == null) return -1;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!Directory.Exists(dir + @"Psms")) Directory.CreateDirectory(dir + @"Psms");

            if (!Directory.Exists(dir + @"PeptidesPerCondition")) Directory.CreateDirectory(dir + @"PeptidesPerCondition");
            if (!Directory.Exists(dir + @"ProteinGroupsPerCondition")) Directory.CreateDirectory(dir + @"ProteinGroupsPerCondition");

            if (!Directory.Exists(dir + @"PeptidesPerConditionReplicate")) Directory.CreateDirectory(dir + @"PeptidesPerConditionReplicate");
            if (!Directory.Exists(dir + @"ProteinGroupsPerConditionReplicate")) Directory.CreateDirectory(dir + @"ProteinGroupsPerConditionReplicate");

            fastaDb.Read();
            
            for (var condition = 1; condition <= _specFilePaths.Length; condition++)
            {
                var c = condition - 1;
                for (var replicate = 1; replicate <= _specFilePaths[c].Length; replicate++)
                {
                    var r = replicate - 1;
                    for (var fraction = 1; fraction <= _specFilePaths[c][r].Length; fraction++)
                    {
                        var f = fraction - 1;
                        var tsv = _tsvs[c][r][f];
                        var givenFileName = Path.GetFileNameWithoutExtension(_specFilePaths[c][r][f]);
                        var tsvfile = string.Format(@"{0}psms_c{1}r{2}f{3}_{4}.tsv", dir + @"Psms" + Path.DirectorySeparatorChar, condition, replicate, fraction, givenFileName);

                        // Console.WriteLine(file);
                        _psmLists[c][r][f].WriteTsv(tsvfile, tsv);
                            
                    }
                    //if (_specFilePaths[c].Length <= 1) continue;
                    var repCommonPrefix = "_" + GetCommonPrefix(_specFilePaths[c][r]);
                    _peptideDictionariesPerConditionReplicate[c][r].WriteTsv(string.Format(@"{0}peptides_c{1}r{2}{3}.tsv", dir + @"PeptidesPerConditionReplicate" + Path.DirectorySeparatorChar, condition, replicate, repCommonPrefix), fastaDb);
                    _proteinGroupDictionariesPerConditionReplicate[c][r].WriteTsv(string.Format(@"{0}proteinGroups_c{1}r{2}{3}.tsv", dir + @"ProteinGroupsPerConditionReplicate" + Path.DirectorySeparatorChar, condition, replicate, repCommonPrefix), fastaDb);
                        
                }
                var condCommonPrefix = "_" + GetCommonPrefix(_specFilePaths[c]);
                _peptideDictionariesPerCondition[c].WriteTsv(string.Format(@"{0}peptides_c{1}{2}.tsv", dir + @"PeptidesPerCondition" + Path.DirectorySeparatorChar, condition, condCommonPrefix), fastaDb);
                _proteinGroupDictionariesPerCondition[c].WriteTsv(string.Format(@"{0}proteinGroups_c{1}{2}.tsv", dir + @"ProteinGroupsPerCondition" + Path.DirectorySeparatorChar, condition, condCommonPrefix), fastaDb);
                   
            }
            var allCommonPrefix = "_" + GetCommonPrefix(_specFilePaths);
            _peptideDictionary.WriteTsv(string.Format(@"{0}peptides{1}.tsv", dir, allCommonPrefix), fastaDb);
            _proteinGroupDictionary.WriteTsv(string.Format(@"{0}proteinGroups{1}.tsv", dir, allCommonPrefix), fastaDb);
            
            

            return 0;
        }


        private static string GetCommonPrefix(string[] paths)
        {
            if (paths.Length < 1) return "";
            if (paths.Length == 1) return Path.GetFileNameWithoutExtension(paths[0]);

            var fileNames = (from eachpath in paths select Path.GetFileNameWithoutExtension(eachpath)).ToList();
            Console.WriteLine(String.Join(" ", fileNames));
            var commonPrefix = new string(fileNames.First().Substring(0, fileNames.Min(x => x.Length)).TakeWhile((c, i) => fileNames.All(s => s[i]==c)).ToArray());
            return commonPrefix;
        }


        private static string GetCommonPrefix(string[][] paths)
        {
            string[] flattenedPaths = paths.SelectMany(x => x).ToArray();
            return GetCommonPrefix(flattenedPaths);
        }


        private static string GetCommonPrefix(string[][][] paths)
        {
            string[] flattenedPaths = paths.SelectMany(x => x).ToArray().SelectMany(x=>x).ToArray();
            return GetCommonPrefix(flattenedPaths);
        }


    }
}
