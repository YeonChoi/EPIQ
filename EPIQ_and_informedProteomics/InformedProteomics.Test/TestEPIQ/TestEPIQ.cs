using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Accord.MachineLearning;
using DeconTools.Backend.Utilities;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.MassSpecData;
using NUnit.Framework;
using InformedProteomics.Backend.Data.Spectrometry;
using IntervalTreeLib;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using Epiq;
using Microsoft.Office.Interop.Excel;

namespace InformedProteomics.Test.TestEpiq
{
    [TestFixture]
    class TestEpiq
    {

        
        #region DE3 names
        /*
        public static readonly string[] Names =
        {
            @"DE_1_1_1_NCP1C2_J100_0519_01",
            @"DE_1_1_1_NCP1C2_J100_0519_02",
            @"DE_1_1_1_NCP1C2_J100_0519_03_RE",
            @"DE_1_10_20_01_J100_0928",
            @"DE_1_10_20_02_J100_0928",
            @"DE_1_10_20_03_J100_0928",
            @"DE_1_20_50_01_NCP2C1_J100_1031",
            @"DE_1_20_50_02_NCP2C1_J100_1031",
            @"DE_1_20_50_03_NCP2C1_J100_1031",
        };

        public static readonly string[] BasePaths =
        {
            @"20161031_DEC13_3plex_LysC\",
            @"20161031_DEC13_3plex_LysC\",
            @"20161031_DEC13_3plex_LysC\",
            @"20161031_DEC13_3plex_LysC\",
            @"20161031_DEC13_3plex_LysC\",
            @"20161031_DEC13_3plex_LysC\",
            @"20161031_DEC13_3plex_LysC\",
            @"20161031_DEC13_3plex_LysC\",
            @"20161031_DEC13_3plex_LysC\",
        };
        */
        #endregion

        #region DM3 names
        /*public static readonly string[] Names =
        {
            @"DM_1_1_1_01_NCP1_J100_0510",
            @"DM_1_1_1_02_NCP1_J100_0510",
            @"DM_1_1_1_03_NCP1_J100_0510",
            @"DM_1_20_50_01_NCP2_J100_1019",
            @"DM_1_20_50_02_NCP2_J100_1019",
            @"DM_1_20_50_03_NCP2_J100_1019",
            @"DM_1_10_20_01_J100_NAQ448C1_1031",
            @"DM_1_10_20_01_J100_NAQ448C1_1031",
            @"DM_1_10_20_01_J100_NAQ448C1_1031",
        };

        public static readonly string[] BasePaths =
        {
            @"20161108_DM_3plex_LysC\",
            @"20161108_DM_3plex_LysC\",
            @"20161108_DM_3plex_LysC\",
            @"20161108_DM_3plex_LysC\",
            @"20161108_DM_3plex_LysC\",
            @"20161108_DM_3plex_LysC\",
            @"20161108_DM_3plex_LysC\",
            @"20161108_DM_3plex_LysC\",
            @"20161108_DM_3plex_LysC\",
        };*/
        #endregion

        #region DE names
        
        public static readonly string[] Names =
        {
            @"DE_1_50_NAQ448C1_J100_0913",//0
            @"DE_1_20_NAQ448C1_J100_0913",
            @"DE_1_10_NAQ448C1_J100_0913",
            @"DE_1_8_NAQ448C1_J100_0913",
            @"DE_1_4_NAQ448C1_J100_0913",
            @"DE_1_1_NAQ448C1_J100_0913",//5
            @"DE_1_1_2_NAQ448C1_J100_0913",
            @"DE_1_1_NAQ448C2_J100_0825",
            @"DE_12_1_NAQ448C2_J100_0825",
            @"DE_1_10_4ug_NAQ448C1_J100_0921",
            @"DE_1_10_8ug_NAQ448C1_J100_0921",//10
            @"DE_1_20_4ug_NAQ448C1_J100_0921",
            @"DE_1_20_8ug_NAQ448C1_J100_0921",
            @"DE_1_50_1ug_High_resolution_NAQ448C1_J100_0921",
            @"DE_1_50_4ug_NAQ448C1_J100_0921", // 
            @"DE_1_50_8ug_NAQ448C1_J100_0921",//15
            @"DE_1_100_4ug_NAQ448C1_J100_0921",
            @"DE_1_100_8ug_NAQ448C1_J100_0921",
        };
        public static readonly string[] BasePaths =
        {
            @"20160913_DE_Try_6plex\",
            @"20160913_DE_Try_6plex\",
            @"20160913_DE_Try_6plex\",
            @"20160913_DE_Try_6plex\",
            @"20160913_DE_Try_6plex\",
            @"20160913_DE_Try_6plex\",
            @"20160913_DE_Try_6plex\",
            @"20160825_DE6plex\",
            @"20160825_DE6plex\",
            @"20160922_DE_Try_6plex\",
            @"20160922_DE_Try_6plex\",
            @"20160922_DE_Try_6plex\",
            @"20160922_DE_Try_6plex\",
            @"20160922_DE_Try_6plex\",
            @"20160922_DE_Try_6plex\",
            @"20160922_DE_Try_6plex\",
            @"20160922_DE_Try_6plex\",
            @"20160922_DE_Try_6plex\",
        };
        
        
        #endregion


        #region DE Lumos names
        
        /*
        public static readonly string[] Names =
        {
            @"DE_1_1_60K_4G_J100_NAQ448C1_1031",
            @"DE_1_20_60K_4G_J100_NAQ448C1_1031",
            @"DE_1_1_60K_4G_IW04_J100_NAQ448C1_1105",
            @"DE_1_1_60K_4G_IW_J100_NAQ448C1_1031",
            @"DE_3ug_1_100_120K_J100_NCP1_1001",
            @"DE_3ug_1_50_500K_J100_NCP1_1001",
            @"DE_3ug_1_50_240K_J100_NCP1_1001",
            @"DE_3ug_1_50_120K_J100_NCP1_1001",
        };
        public static readonly string[] BasePaths =
        {
            @"20161031_DE_6plex_Try_4hr\",
            @"20161031_DE_6plex_Try_4hr\",
            @"20161102_DE6plex_4hrG_IW\",
            @"20161102_DE6plex_4hrG_IW\",
            @"20161001_DE_Try_6plex\",
            @"20161001_DE_Try_6plex\",
            @"20161001_DE_Try_6plex\",
            @"20161001_DE_Try_6plex\",
        };*/
        #endregion

        
        #region DE Training names

        public static readonly string[] TrainingNames =
        {
            @"DE_1_1_NAQ448C2_J100_0825",
            @"DE_12_1_NAQ448C2_J100_0825",
            @"DE_1_1_NAQ448C1_J100_0913",
            @"DE_1_1_2_NAQ448C1_J100_0913",
            @"DE_1_1_60K_4G_J100_NAQ448C1_1031",
            @"DE_1_1_60K_4G_IW04_J100_NAQ448C1_1105",
            @"DE_1_1_60K_4G_IW_J100_NAQ448C1_1031",
        };
        public static readonly string[] TrainingBasePaths =
        {
            @"20160825_DE6plex\",
            @"20160825_DE6plex\",
            @"20160913_DE_Try_6plex\",
            @"20160913_DE_Try_6plex\",
            @"20161031_DE_6plex_Try_4hr\",
            @"20161102_DE6plex_4hrG_IW\",
            @"20161102_DE6plex_4hrG_IW\",
        };
         
        

        #endregion

        public static void SetPath(int index)
        {
            Name = Names[index];//@"DE_3ug_1_50_120K_J100_NCP1_1001";//";//@"DE_1_20_4ug_NAQ448C1_J100_0921"//DE_12_1_NAQ448C2_J100_0825";//@"DE_1_4_NAQ448C1_J100_0913";//@"DE_1_50_1ug_High_resolution_NAQ448C1_J100_0921";//@"DE_1_4_NAQ448C1_J100_0913";//DE_1_50_1ug_High_resolution_NAQ448C1_J100_0921";//@"DE_1_1_NAQ448C2_J100_0825"; //
            BasePath = @"E:\MassSpec\" + BasePaths[index];//@"E:\MassSpec\20161001_DE_Try_6plex\";//@"E:\MassSpec\20160825_DE6plex\";//; //
            TestRawFilePath = BasePath + @"RawFiles\"+Name+".raw";
            MgfDirPath = BasePath + @"CorrectedMgfs\";
            SearchResultPath = BasePath + @"SearchResults\"+Name+".tsv";
            ResultsPath = @"C:\Scratch\" + Name + @"\"; //BasePath + @"QFeatureResults\" + Name + @"\";
        }

        public static void SetTrainingPath(int index)
        {
            Name = TrainingNames[index];
            BasePath = @"E:\MassSpec\" + TrainingBasePaths[index];
            TestRawFilePath = BasePath + @"RawFiles\"+Name+".raw";
            MgfDirPath = BasePath + @"CorrectedMgfs\";
            TrainingPath = BasePath + @"TrainingData\" + Name + ".txt";
            SearchResultPath = BasePath + @"SearchResults\"+Name+".tsv";
        }


        public static string Name = Names[0];//@"DE_3ug_1_50_120K_J100_NCP1_1001";//";//@"DE_1_20_4ug_NAQ448C1_J100_0921"//DE_12_1_NAQ448C2_J100_0825";//@"DE_1_4_NAQ448C1_J100_0913";//@"DE_1_50_1ug_High_resolution_NAQ448C1_J100_0921";//@"DE_1_4_NAQ448C1_J100_0913";//DE_1_50_1ug_High_resolution_NAQ448C1_J100_0921";//@"DE_1_1_NAQ448C2_J100_0825"; //
        public static string BasePath = @"E:\MassSpec\" + BasePaths[0];//@"E:\MassSpec\20161001_DE_Try_6plex\";//@"E:\MassSpec\20160825_DE6plex\";//; //
        public static string TestRawFilePath = BasePath + @"RawFiles\"+Name+".raw";
        public static string MgfDirPath = BasePath + @"CorrectedMgfs\";
        public static string TrainingPath = BasePath + @"TrainingData\" + Name + ".txt";
        public static string SearchResultPath = BasePath + @"SearchResults\"+Name+".tsv";
        public static string ResultsPath = @"C:\Scratch\" + Name + @"\"; //BasePath + @"QFeatureResults\" + Name + @"\";

        public static Tolerance Tolerance = new Tolerance(10);

        private static LcMsRun Run;//InMemoryLcMsRun.GetLcMsRun(TestRawFilePath, MassSpecDataType.XCaliburRun);
        private static readonly float[] mzRange = null;
        private static readonly float[] etRange = null;// {20f, 40f};
        private static readonly bool overWrite = true;


        [Test]
        public void TestWriteExcel()
        {
            Application xlApp = new Application();
            if (xlApp == null)
            {
                throw new Exception("Excel is not properly installed");
            }
            var xlWorkBook = xlApp.Workbooks.Add();
            var xlWorkSheet1 = (Worksheet)xlWorkBook.Worksheets.Item[1];
            xlWorkSheet1.Cells[1, 1] = "ID";
            xlWorkSheet1.Cells[1, 2] = "Name";
            xlWorkSheet1.Cells[2, 1] = "1";
            xlWorkSheet1.Cells[2, 2] = "Moon";
            xlWorkSheet1.Cells[3, 1] = "2";
            xlWorkSheet1.Cells[3, 2] = "Hong";

            xlWorkSheet1.Activate();
            xlWorkSheet1.Application.ActiveWindow.SplitRow = 1;
            xlWorkSheet1.Application.ActiveWindow.FreezePanes = true;

            var xlWorkSheet2 = (Worksheet)xlWorkBook.Worksheets.Add();
            xlWorkSheet2.Cells[2, 2] = "Excel TEST~~~~~~!~!~!~!";
            xlWorkBook.SaveAs(@"C:\Users\yeon\Desktop\MrPresident.xlsx");

            xlApp.Quit();
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);

        }
        
        [Test]
        public void TestIntervalTree()
        {
            IntervalTree<int, int> tree = new IntervalTree<int, int>();
            tree.AddInterval(0, 10, 100);
            tree.AddInterval(20, 30, 200);

            var result = tree.Get(1, 23);
            foreach(var r in result)
                Console.WriteLine(r);
        }

        [Test]
        public void TestKmeans()
        {
            double[][] observations = 
            {
                new double[] { -5, -2, -1 },
                new double[] { -5, -5, -6 },
                new double[] {  2,  1,  1 },
                new double[] {  1,  1,  2 },
                new double[] {  1,  2,  2 },
                new double[] {  3,  1,  2 },
                new double[] { 11,  5,  4 },
                new double[] { 15,  5,  6 },
                new double[] { 10,  5,  6 },
            };
            var kmeans = new KMeans(3);
            
            
            var ret = kmeans.Learn(observations);

            var e = ret.Decide(observations);
            foreach (var t in e)
            {
                Console.WriteLine(t);
            }
        }

        [Test]
        public void TestPinv()
        {
            var carray = new[,]{
            {0.0372f ,   0.2869f},
            {0.6861f ,   0.7071f},
            {0.6233f  ,  0.6245f},
            {0.6344f  ,  0.6170f}
            };
            var X = Matrix<float>.Build.DenseOfArray(carray);
            
            //Console.WriteLine(XicMatrix.MoorePenrosePsuedoinverseBefore(X));
            Console.WriteLine(MatrixCalculation.MoorePenrosePsuedoinverse(X));
        }

        [Test]
        public void TestThermoCharge()
        {
            
            Console.Write("Name\tInst Model");
            int[] chargeCandidates = new int[] {0, 1, 2, 3, 4, 5, 6, 7};
            foreach (var charge in chargeCandidates)
            {
                Console.Write("\t{0}", charge);
            }
            for (var index = 0; index < Names.Length; index++)
            {
                var chargeCounter = new Dictionary<int?, int>();
                SetPath(index);
                var xcalReader = new XCaliburReader(TestRawFilePath);
                var instModel = xcalReader.ReadInstModel();
                Run = InMemoryLcMsRun.GetLcMsRun(TestRawFilePath, MassSpecDataType.XCaliburRun);
                var ms2ScanNumList = Run.GetScanNumbers(2);

                Console.WriteLine();
                Console.Write(BasePaths[index] + Name + "\t" + instModel);
                foreach (var ms2Sn in ms2ScanNumList)
                {
                    var spec = Run.GetSpectrum(ms2Sn);
                    var ps = spec as ProductSpectrum;
                    if (ps.IsolationWindow.Charge==null)
                        Console.WriteLine("Charge is null!");

                    if (chargeCounter.ContainsKey(ps.IsolationWindow.Charge))
                        chargeCounter[ps.IsolationWindow.Charge] += 1;
                    else
                        chargeCounter[ps.IsolationWindow.Charge] = 1;
                }
                foreach (var charge in chargeCandidates)
                {
                    if (chargeCounter.ContainsKey(charge))
                        Console.Write("\t{0}", chargeCounter[charge]);
                    else
                        Console.Write("\t0");
                }
            }

        }


        [Test]
        public void TestNterminomics()
        {
            var tolerance = new Tolerance(5);
            var overwrite = true;
            var merge = false;
            var chargeOption = "";

            string[] labelStr =
            {
                "^ 0_42.011 0_45.029",
                "K 0_45.029 0_45.029"
            };
            Params.InitParams(labelStr);
            var basePath = @"D:\tiny\N-terminomics\";
            var rawFilePath = basePath + @"RawFiles\";
            var searchFilePath = basePath + @"SearchResults\";
            var resultPath = basePath + @"Results\";
            //QuantifiedProteinGroupDictionary.GetQuantifiedProteins(rawFilePath, searchFilePath, resultPath, tolerance, overwrite, merge, chargeOption:chargeOption);
        }



        [Test]
        public void TestMzRefinedResults()
        {
            var tolerance = new Tolerance(5);
            var overwrite = true;
            var merge = false;
            var chargeOption = "MzCorrected";

            Params.InitParams(@"DE",
             @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
             @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard");
            var basePath = @"E:\MassSpec\DevelopeData\20170410_DE6plex_New_ratio_QE\";
            var rawFilePath = basePath + @"RawFiles\";
            var searchFilePath = basePath + @"SearchResults\";
            var resultPath = basePath + @"MzCorrectedResults\";
           // QuantifiedProteinGroupDictionary.GetQuantifiedProteins(rawFilePath, searchFilePath, resultPath, tolerance, overwrite, merge, chargeOption:chargeOption);
        }

        [Test]
        public void TestMzRefining()
        {
            var tolerance = new Tolerance(10);
            var addMz = true;
            var correctMz = true;
            var correctCharge = false;

            Params.InitParams(@"DE");
            var basePath = @"E:\MassSpec\DevelopeData\20170410_DE6plex_New_ratio_QE\";
            var rawFilePath = basePath + @"RawFiles\";
            var mgfPath = basePath + @"MzRefinedMgfs\";

          //  new PreProcessMs2Spec(rawFilePath, mgfPath, tolerance, addMz, correctMz, correctCharge);
        }


        [Test]
        public void TestMzCandidiates()
        {
            
            var xcalReader = new XCaliburReader(TestRawFilePath);
            var instModel = xcalReader.ReadInstModel();
            Run = InMemoryLcMsRun.GetLcMsRun(TestRawFilePath, MassSpecDataType.XCaliburRun);
            var ms2ScanNumList = Run.GetScanNumbers(2);

            Console.WriteLine();
            Console.WriteLine(@"Inst Model is {0}", instModel);
            Console.WriteLine(@"Processing {0} spectrum from {1} ...", ms2ScanNumList.Count, Name);
            var emptyCntr = 0;
            foreach (var ms2ScanNum in ms2ScanNumList)
            {
                var precusorMzCandidates = new PrecusorMzCandidates(ms2ScanNum, Run, instModel);
                if (precusorMzCandidates.Count == 0) emptyCntr++;
            }
            Console.WriteLine("empty isolation windows: {0}", emptyCntr);
        }

        [Test]
        public void PutSpecNumber()
        {
            var sw = new StreamWriter(@"E:\MassSpec\IsoQuant\SearchResults\VPS4B1J.tsv");
            foreach (var line in File.ReadLines(@"E:\MassSpec\IsoQuant\SearchResults\merged.tsv"))
            {
                if (line.StartsWith(@"#"))
                {
                    sw.WriteLine(line);
                    continue;
                }
                var token = line.Split('\t');
                var title = token[3];
                var sn = title.Substring(title.LastIndexOf("=") + 1, title.LastIndexOf('"') - title.LastIndexOf("=") - 1 );
                for (var i = 0; i < token.Length-1; i++)
                {
                    if(i!=2)
                        sw.Write(token[i]+"\t");
                    else
                        sw.Write(sn + "\t");
                }

                sw.WriteLine(token[token.Length-1]);

            }
            sw.Close();
    }
        
        [Test]
        public void TestPreProcessMs2Spec()
        {
            Params.InitParams(@"DE");
            var mgfDirPath = @"E:\MassSpec\PreProcessedMgfs\";
            if (!Directory.Exists(mgfDirPath)) Directory.CreateDirectory(mgfDirPath);
            for (var index = 0; index < Names.Length; index++)
            {
                SetPath(index);
                var mgfpath = mgfDirPath + Name + ".chargeCorrected.mgf";
                Console.WriteLine();
                Console.WriteLine(@"Processing {0}...", Name);
                //new Old_PreProcessMs2Spec(TestRawFilePath, Tolerance, mgfpath);
            }
        }


        [Test]
        public void TestQFMTrain()
        {
            Params.InitParams(@"DE");
            var chargeOption = @"";
            var trainingNames = new string[]
            {
                "DE6plex_Equi_2ug_2_NCP1_J100_0411",
                "DE6plex_Equi_2ug_3_NCP1_J100_0411",
                "DE6plex_Equi_2ug_4_NCP1_J100_0411",
                "DE6plex_Equi_2ug_5_NCP1_J100_0411",
                "DE6plex_Equi_2ug_6_NCP1_J100_0411",
                "DE6plex_Equi_2ug_7_NCP1_J100_0411",
            };

            for (var index = 0; index < trainingNames.Length; index++)
            {
                Name = trainingNames[index];
                TestRawFilePath = @"E:\MassSpec\PaperData\DE6_Try_1_1_1_1_1_1_ThermoLC2hr\RawFiles\" + Name + ".raw";
                TrainingPath = @"E:\MassSpec\PaperData\DE6_Try_1_1_1_1_1_1_ThermoLC2hr\TrainingData\" + Name + ".txt";
                SearchResultPath = @"E:\MassSpec\PaperData\DE6_Try_1_1_1_1_1_1_ThermoLC2hr\SearchResults_cRAP_MSConvert\" + Name + (chargeOption.Length == 0 ? @"" : @"." + chargeOption) + @".tsv";

                Console.WriteLine();
                Console.WriteLine(@"Processing " + Name + @" ...");
                //SearchResultPath = @"E:\MassSpec\PrecursorAddedSearchResults\" + Name + (chargeOption.Length == 0 ? @"" : @"." + chargeOption) + @".tsv";
                if (!File.Exists(SearchResultPath)) continue;
                new TemplateForTrain(TestRawFilePath, SearchResultPath, Tolerance, TrainingPath, chargeOption);
            }
        }

        [Test]
        public void TestQE2hr()
        {
            Params.InitParams(@"DE",
             dshiftModelPath: @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
             dshiftStandardPath: @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard");
            TestRawFilePath = @"E:\MassSpec\20160913_DE_Try_6plex\RawFiles\";
            SearchResultPath = @"E:\MassSpec\PrecursorAddedSearchResults\";
            ResultsPath = @"C:\Scratch\";
            var chargeOption = @"chargeCorrected";
           // QuantifiedProteinGroupDictionary.GetQuantifiedProteins(TestRawFilePath, SearchResultPath, ResultsPath, Tolerance, overWrite, false, false, chargeOption);
        }

      
        [Test]
        public void TestQFMProtein()
        {//DE_1_100_8ug_NAQ448C1_J100_0921
            Params.InitParams(@"DE", 
              @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
              @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard");
            var chargeOption = @"";//@"";
            float[] mzRange = null;//new []{500f,550};
            for (var index = 0; index < 2; index++)
            {
                SetPath(index);
                if(chargeOption.Length>0) SearchResultPath = @"E:\MassSpec\PrecursorAddedSearchResults\" + Name + (chargeOption.Length == 0 ? @"" : @"." + chargeOption) + @".tsv";
                if (!File.Exists(SearchResultPath)) continue;
              //  QuantifiedProteinGroupDictionary.GetQuantifiedProteins(TestRawFilePath, SearchResultPath, ResultsPath, Tolerance, false, false, false, chargeOption);
            }
        }


        [Test]
        public void TestQFMProteinLumos()
        {
            Params.InitParams(@"DE",
              @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_Lumos_WatersLC_4hrG.standardized.s4t2g1c1.model",
              @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_Lumos_WatersLC_4hrG.standard");
            var chargeOption = @"";
            float[] mzRange = null;//{500,550};// null};
            for (var index = 1; index < Names.Length; index++)
            {
                SetPath(index);
                SearchResultPath = @"E:\MassSpec\PrecursorAddedSearchResults\" + Name + (chargeOption.Length == 0 ? @"" : @"." + chargeOption) + @".tsv";
                if (!File.Exists(SearchResultPath)) continue;
               // QuantifiedProteinGroupDictionary.GetQuantifiedProteins(TestRawFilePath, SearchResultPath, ResultsPath, Tolerance, false, false, false, chargeOption, mzRange, etRange);
            }
        }
        [Test]
        public void TestQFMFractionation()
        {
            Params.InitParams(@"DE",
              @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
              @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard");
            var chargeOption = @"";//chargeCorrected";
            TestRawFilePath = @"E:\MassSpec\20160820_DE6plex_IEF_LysC\RawFiles\";
            SearchResultPath = @"E:\MassSpec\20160820_DE6plex_IEF_LysC\SearchResults\";

            //20160820_DE6plex_IEF_LysC
            
            ResultsPath = @"C:\Scratch\";
          //  QuantifiedProteinGroupDictionary.GetQuantifiedProteins(TestRawFilePath, SearchResultPath, ResultsPath, Tolerance, true, false, false, chargeOption);

        }


       [Test]
       public void TestDE3()
       {
           Params.InitParams(@"DE3",
            @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
            @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard");
           ResultsPath = @"E:\MassSpec\ForPaper\DE3\DE3_1_10_20\";
           TestRawFilePath = ResultsPath + @"RawFiles\";
           SearchResultPath = ResultsPath + @"SearchResults\";
           var chargeOption = @"";
         //  QuantifiedProteinGroupDictionary.GetQuantifiedProteins(TestRawFilePath, SearchResultPath, ResultsPath, Tolerance, false, true, false, chargeOption);
       }
       
       [Test]
       public void TestDM3()
       {
           Params.InitParams(@"DM3",
           @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
           @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard"); TestRawFilePath = @"E:\MassSpec\DE3_1_20_50\RawFiles\";
           TestRawFilePath = @"E:\MassSpec\DM3_1_1_1\RawFiles\";
           SearchResultPath = @"E:\MassSpec\PrecursorAddedSearchResults\";
           ResultsPath = @"C:\Scratch\";
           var chargeOption = @"chargeCorrected";
         //  QuantifiedProteinGroupDictionary.GetQuantifiedProteins(TestRawFilePath, SearchResultPath, ResultsPath, Tolerance, false, true, false, chargeOption);
       }
        /*
       [Test]
       public void Test4hr()
       {
           Params.InitParams(@"DE", @"E:\MassSpec\20160825_DE6plex\SVMModels\D-shifts_libSVM_model_logN_all_NoAA_margin=0.txt");
           TestRawFilePath = @"E:\MassSpec\20161101_DE6plex_4hrG\RawFiles\";
           SearchResultPath = @"E:\MassSpec\20161101_DE6plex_4hrG\SearchResults\";
           ResultsPath = @"C:\Scratch\";
           var chargeOption = @"chargeCorrected";
           QuantifiedProteinList.GetQuantifiedProteins(TestRawFilePath, SearchResultPath, ResultsPath, Tolerance, overWrite, false, false, chargeOption);
       }
        */

        public string[] SILACNames =
        {
            @"13C6_13C615N2_1on1_J100_NAQ448C1_1216",
            @"D4_13C615N2_1on1_J100_NAQ448C1_1216",
            @"D4_D8_1on1_J100_NAQ448C1_1216",
            @"D8_13C6_1on1_J100_NAQ448C1_1216",
        };

        public string[] SILACLabelStrings =
        {
            "K 0_6.020129 0_8.014199",
            "K 4_4.025107 0_8.014199",
            "K 4_4.025107 8_8.050214",
            "K 8_8.050214 0_6.020129",

        };

        public string[] SILACTriplet =
        {
            "K 0_0 4_4.0251 0_8.0142",
            "R 0_0 0_6.02013 0_10.00827"
        };
      //  private static string SILAC2StringK = "K 0_0 0_4.025107 0_8.014199";
      //  private static string SILAC2StringR = "R 0_0 0_6.02013 0_10.00827"; // should be changed so diff aas have diff mz shifts det numbers


        [Test]
        public void TestPreProcessSILACTest()
        {
            var rawDirPath = @"E:\MassSpec\20161219_SILAC_2plex_Try\RawFiles\";
            var mgfDirPath = @"E:\MassSpec\20161219_SILAC_2plex_Try\PreProcessedMgfs\";
            if (!Directory.Exists(mgfDirPath)) Directory.CreateDirectory(mgfDirPath);

            for (var index = 0; index < SILACNames.Length; index++)
            {
                var name = SILACNames[index];
                Params.InitParams(SILACLabelStrings[index]);

                var rawPath = rawDirPath + name + @".raw";
                var mgfPath = mgfDirPath + name + ".chargeCorrected.mgf";
                Console.WriteLine();
                Console.WriteLine(@"Processing {0}...", name);
                //new Old_PreProcessMs2Spec(rawPath, Tolerance, mgfPath);
            }
        }
        

        [Test]
        public void TestQuantifySILACTest()
        {
            var rawDirPath = @"E:\MassSpec\20161219_SILAC_2plex_Try\RawFiles\";
            var searchDirPath = @"E:\MassSpec\20161219_SILAC_2plex_Try\SearchResults\";
            var resultDirPath = @"E:\MassSpec\20161219_SILAC_2plex_Try\NoDshiftQuantResults\";
            if (!Directory.Exists(resultDirPath)) Directory.CreateDirectory(resultDirPath);

            var chargeOption = @"";
            for (var index = 0; index < SILACNames.Length; index++)
            //var index = 1;
            {
                var name = SILACNames[index];
                Params.InitParams(SILACLabelStrings[index],
              @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_Lumos_WatersLC_4hrG.standardized.s4t2g1c1.model",
              @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_Lumos_WatersLC_4hrG.standard");

                var rawPath = rawDirPath + name + @".raw";
                var searchPath = searchDirPath + name + chargeOption + ".tsv";
                var resultPath = resultDirPath + name + @"\";
                Console.WriteLine();
                Console.WriteLine(@"Processing {0}...", searchPath);
                if (!Directory.Exists(resultPath)) Directory.CreateDirectory(resultPath);
            //    QuantifiedProteinGroupDictionary.GetQuantifiedProteins(rawPath, searchPath, resultPath, Tolerance, true, false, false, chargeOption, mzRange, etRange);

            }
        }

        [Test]
        public void TestIsoQuant()
        {
            var rawPath = @"E:\MassSpec\IsoQuant\RawFiles\";
            var searchPath = @"E:\MassSpec\IsoQuant\SearchResults\";
            
            var chargeOption = @"";
           
            var resultDirPath = (@"E:\MassSpec\IsoQuant\Results\");
            if (!Directory.Exists(resultDirPath)) Directory.CreateDirectory(resultDirPath);

            Params.InitParams(SILACTriplet,
               @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
               @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard");

            // var rawPath = rawDirPath + name + @".raw";
            //var searchPath = searchDirPath + name + chargeOption + ".tsv";
            //var resultPath = resultDirPath +  + @"\";
            Console.WriteLine();
            Console.WriteLine(@"Processing {0}...", resultDirPath);
            if (!Directory.Exists(resultDirPath)) Directory.CreateDirectory(resultDirPath);
          //  QuantifiedProteinGroupDictionary.GetQuantifiedProteins(rawPath, searchPath, resultDirPath, Tolerance, true, false, false, chargeOption, mzRange, etRange);
            
        }

        [Test]
        public void TestPreProcessXenopusEmbryo()
        {
            Params.InitParams("DE3");
            var basePath = @"E:\MassSpec\";
            var embryoSet = new string[]
            {
                @"20170119_DEC13_3plex_Try_XenopusEmbryo_SET01\",
                @"20170119_DEC13_3plex_Try_XenopusEmbryo_SET02\",
            };

            foreach (var setName in embryoSet)
            {
                Console.WriteLine("\r\nProcessing " + setName + @"...");
                var rawDirPath = basePath + setName + @"RawFiles\";
                var mgfDirPath = basePath + setName + @"PreProcessedMgfs\";
                
                if (!Directory.Exists(mgfDirPath)) Directory.CreateDirectory(mgfDirPath);
                //new PreProcessMs2Spec(rawDirPath, Tolerance, mgfDirPath);
            }
        }


        [Test]
        public void TestQuantifyXenopusEmbryo()
        {
            Params.InitParams(@"DE3",
             @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
             @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard");
            var chargeOption = @""; // 
            var basePath = @"E:\MassSpec\";
            var embryoSet = new []
            {   
                @"20170119_DEC13_3plex_LysC_XenopusEmbryo\",
            };

            foreach (var setName in embryoSet)
            {
                //Console.WriteLine("\t **** Processing Set" + setName + "...");
                TestRawFilePath = basePath + setName + @"RawFiles\";
                SearchResultPath = basePath + setName + @"SearchResults\";
                ResultsPath = basePath + setName + @"Results\";

            //    QuantifiedProteinGroupDictionary.GetQuantifiedProteins(TestRawFilePath, SearchResultPath, ResultsPath, Tolerance, false, true, false, chargeOption);
            }
        }


        [Test]
        public void TestQuantifyDE6PlexForPaper()
        {
            Params.InitParams(@"DE",
             @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
             @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard");
            var chargeOption = @""; // 
            var basePath = //@"E:\MassSpec\20170324_DE6plex_new_ratio_QE\";//
@"E:\MassSpec\20170302_DE6plex_mixture_QE\";
           
            TestRawFilePath = basePath + @"RawFiles\";
            SearchResultPath = basePath + @"SearchResults\";
            ResultsPath = basePath + @"Results\";

          //  QuantifiedProteinGroupDictionary.GetQuantifiedProteins(TestRawFilePath, SearchResultPath, ResultsPath, Tolerance, false, false, false, chargeOption);
        }

        [Test]
        public void TestQuantifyDE6PlexForPaperLumos()
        {
            Params.InitParams(@"DE", //
               @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_Lumos_WatersLC_4hrG.standardized.s4t2g1c1.model",
               @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_Lumos_WatersLC_4hrG.standard"); 
            var chargeOption = @""; // 
            var basePath = @"E:\MassSpec\20170228_DE6plex_mixture_Lumos\";

            TestRawFilePath = basePath + @"RawFiles\";
            SearchResultPath = basePath + @"SearchResults\";
            ResultsPath = basePath + @"Results\";

          //  QuantifiedProteinGroupDictionary.GetQuantifiedProteins(TestRawFilePath, SearchResultPath, ResultsPath, Tolerance, true, false, false, chargeOption);

        }

        [Test]
        public void TestQuantifyDE5PlexForPaper()
        {
            Params.InitParams(@"DE",
             @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
             @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard");
            var chargeOption = @""; // 
            var basePath = @"E:\MassSpec\ForPaper\20170324_DE6plex_Try_newratio\";

            TestRawFilePath = basePath + @"RawFiles\";
            SearchResultPath = basePath + @"SearchResults\";
            ResultsPath = basePath + @"Results\";

          //  QuantifiedProteinGroupDictionary.GetQuantifiedProteins(TestRawFilePath, SearchResultPath, ResultsPath, Tolerance, true, false, false, chargeOption);

        }
        [Test]
        public void GetCorrelationBetweenTwoTsvs()
        {
            var tsv1 =
                @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_10_20\Results\DE_1_10_20_01_J100_0928\ProteinList.tsv";
                  //@"E:\MassSpec\PaperData\DE3_LysC_XenopusEmbryo_QE\Results\Embryo_1_8_13_0814_J100_NCP1_FN01/ProteinList.tsv";
            var tsv2 = @"E:\MassSpec\PaperData\MaxQuantResults\DE_1_10_20_requantify\combined\txt\proteinGroups.txt";
   // @"E:\MassSpec\PaperData\DE3_LysC_XenopusEmbryo_QE\Results\Embryo_1_8_13_0814_J100_NCP1_FN01/proteinGroups_DE_MQ_Original file.txt";     

            var outDir = @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_10_20\Results\DE_1_10_20_01_J100_0928\";
            var outm = outDir + @"corr.m";
//@"E:\MassSpec\PaperData\DE3_LysC_XenopusEmbryo_QE\Results\Embryo_1_8_13_0814_J100_NCP1_FN01/corr.m";

            var outOnly1Tsv = outDir + @"only1.tsv";
            var outOnly2Tsv = outDir + @"only2.tsv";
            var outBothTsv =outDir +  @"both.tsv";

            var dic1 = new Dictionary<HashSet<string>, List<double>>(HashSet<string>.CreateSetComparer());
            var dic2 = new Dictionary<HashSet<string>, List<double>>(HashSet<string>.CreateSetComparer());

            var pro1 = 1;
            var pro2 = 1;

            var qs1 = new[] { 3, 4, 5 };
            var qs2 = new[] {103,104,105};//{108,109,110};//{ 116, 117, 118 };//112,113,114

          //  qs1 = new[] { 5, 6, 7 };
         //   qs2 = new[] { 5, 6, 7 };

         //   qs2 = qs1 = new[] { 2, 3, 4,5,6,7 };
            //qs2 = new[] { 51, 57, 63};
            
            //qs1 = new[] { 69, 75, 81 };
            //qs2 = new[] { 87, 93, 99 };
            //69 75 81 ratio set 1 
            //87 93 99 ratio set 2 
            // 51 57 63 ratio set sum
            foreach (var line in File.ReadLines(tsv1))
            {
                var tokens = line.Split('\t');

                if (line.StartsWith(@"Protein"))
                {
                    foreach (var i in qs1) Console.Write(tokens[i] + @" ");
                    Console.WriteLine();
                    continue;
                }
                var intensities = new List<double>();
                var nan = false;
                foreach (var i in qs1)
                {
                    if (double.IsNaN(double.Parse(tokens[i]))) nan = true;
                    intensities.Add(double.Parse(tokens[i]));
                }
                if (nan) continue;
                var k = new HashSet<string>();
                foreach (var tt in tokens[pro1].Split(';'))
                {
                    if (tt.Length == 0) continue;
                    k.Add(tt);
                }
                dic1[k] = intensities;
            }

            foreach (var line in File.ReadLines(tsv2))
            {
                var tokens = line.Split('\t');
                if (line.StartsWith(@"Protein"))
                {
                    foreach (var i in qs2) Console.Write(tokens[i] + @" ");
                    Console.WriteLine();
                    continue;
                }
                var intensities = new List<double>();
                var nan = false;
                foreach (var i in qs2)
                {
                    if (double.IsNaN(double.Parse(tokens[i]))) nan = true;
                    intensities.Add(double.Parse(tokens[i]));
                }
                if (nan) continue;
                var k = new HashSet<string>();
                foreach (var tt in tokens[pro2].Split(';'))
                {
                    if (tt.Length == 0) continue;
                    k.Add(tt);
                }
                dic2[k] = intensities;
            }

            var btsv = new StreamWriter(outBothTsv);
            var o1tsv = new StreamWriter(outOnly1Tsv);
            var o2tsv = new StreamWriter(outOnly2Tsv);

            btsv.WriteLine("ProteinGroup1\tProteinGroup2\tIntensities1 (L)\tIntensities1 (M)\tIntensities1 (H)\tIntensities2 (L)\tIntensities2 (M)\tIntensities2 (H)");
            o1tsv.WriteLine("ProteinGroup\tIntensities (L)\tIntensities (M)\tIntensities (H)");
            o2tsv.WriteLine("ProteinGroup\tIntensities (L)\tIntensities (M)\tIntensities (H)");

            var overlaps = new HashSet<HashSet<string>>();
            foreach (var pr in dic1.Keys)
            {
                var overlap = new HashSet<string>();
                foreach (var pr2 in dic2.Keys)
                {
                    if (pr.Intersect(pr2).Any())
                    {
                        overlap = pr2;
                        break;
                    }
                }
                if (overlap.Any()) // both
                {
                    overlaps.Add(overlap);
                    foreach (var p in pr) btsv.Write(p + @";");
                    btsv.Write("\t");
                    foreach (var p in overlap) btsv.Write(p + @";");
                    btsv.Write("\t");
                    foreach (var i in dic1[pr]) btsv.Write("{0}\t", i);
                    //btsv.Write("\t");
                    foreach (var i in dic2[overlap]) btsv.Write("{0}\t", i);
                    btsv.WriteLine();
                }
                else // dic1 only
                {
                    foreach (var p in pr) o1tsv.Write(p + @";");
                    o1tsv.Write("\t");
                    foreach (var i in dic1[pr]) o1tsv.Write("{0}\t", i);
                    o1tsv.WriteLine();
                }
            }
            foreach (var pr in dic2.Keys)
            {
                if (overlaps.Contains(pr)) continue;
                //dic2 only
                foreach (var p in pr) o2tsv.Write(p + @";");
                o2tsv.Write("\t");
                foreach (var i in dic2[pr]) o2tsv.Write("{0}\t", i);
                o2tsv.WriteLine();
            }

            btsv.Close();
            o1tsv.Close();
            o2tsv.Close();


            var m = new StreamWriter(outm);

            m.WriteLine(@"int1=[");
            foreach (var pr in dic1.Keys)
            {
                if (!dic2.ContainsKey(pr)) continue;
                if (dic1[pr].Min() == 0 || dic2[pr].Min() == 0) continue;
                foreach (var i in dic1[pr]) m.Write(@"{0},", i);
                m.Write(@" %");
                foreach (var p in pr) m.Write(p + ";");
                m.WriteLine();
            }
            m.WriteLine(@"];");

            m.WriteLine(@"int2=[");
            foreach (var pr in dic1.Keys)
            {
                if (!dic2.ContainsKey(pr)) continue;
                if (dic1[pr].Min() == 0 || dic2[pr].Min() == 0) continue;
                foreach (var i in dic2[pr]) m.Write(@"{0},", i);
                m.Write(@" %");
                foreach (var p in pr) m.Write(p + ";");
                m.WriteLine();
            }
            m.WriteLine(@"];");


            m.Close();




        }

        [Test]
        public void TestRegx()
        {
            var peptide = @"+123PEPTIDEK+123";
            var pattern = @"^[ACDEFGHIKLMNPQRSTUVWY]";
            var regx = new Regex(pattern);
            Console.WriteLine(regx.IsMatch(peptide));
            //^+56.063 +56.063AVFVDLEPTVIDEVR
        }


        [Test]
        public void MergePDMQTsvs()
        {
            var pdtsv = @"C:\Users\kyowon\Downloads\PD_TMT_MS3_matching.csv";
            var mqtsv = @"C:\Users\kyowon\Downloads\MQ_DE_3plex_matching.csv";
            var mqouttsv = @"C:\Users\kyowon\Downloads\MQ_DE_3plex_PD_TMT_MS3_matching.csv";
            var pdProteins = new HashSet<string>();
            foreach (var protein in File.ReadLines(pdtsv))
            {
                if (protein.StartsWith(@"Accession") || protein.Length == 0) continue;
                pdProteins.Add(protein);
            }

            var sw = new StreamWriter(mqouttsv);

            foreach (var l in File.ReadLines(mqtsv))
            {
                if (l.StartsWith(@"Protein IDs"))
                {
                    sw.WriteLine(l);
                    continue;
                }

                var tokens = l.Split(',');
                var proteins = tokens[0].Split(';');
                var inter = pdProteins.Intersect(proteins).GetEnumerator();
                if (inter.MoveNext())
                {
                    var ip = inter.Current;
                    sw.Write(ip);
                    for (var i = 0; i < proteins.Length; i++)
                    {
                        var p = proteins[i];
                        if (ip == p) continue;
                        sw.Write(@";{0}", p);
                    }
                    for (var i = 1; i < tokens.Length; i++)
                    {
                        sw.Write(@",{0}", tokens[i]);
                    }
                    sw.WriteLine();
                }
                else sw.WriteLine(l);

            }

            sw.Close();


        }


        /*
        [Test]
        public void TestChargeDetermination()
        {
            Tolerance Tolerance = new Tolerance(5);

            float[] etRange = null;//new[] {};
            bool overWrite = true;
            IsotopeEnvelopeCalculator.SetMaxNumIsotopes(Params.IsotopeIndex.Length);

            for (var index = 0; index < Names.Length; index++)
            {
                SetPath(index);
                Run = InMemoryLcMsRun.GetLcMsRun(TestRawFilePath, MassSpecDataType.XCaliburRun);

                var lockTarget = new object();
                var cntr = 0;
                var changedCount = 0;
                var ms2ScanNumList = Run.GetScanNumbers(2);
                var correctedPsList = new ConcurrentBag<Old_CorrectedProductSpectrum>();
                var chargeCounter = new ConcurrentDictionary<Tuple<int?, sbyte>, int>();

                Console.WriteLine();
                Console.WriteLine(@"Processing {0} spectrum from {1} ...", ms2ScanNumList.NumOfBoundLabels, Name);

                Parallel.ForEach(ms2ScanNumList,
                    new ParallelOptions {MaxDegreeOfParallelism = Params.MaxParallelThreads}, ms2ScanNum =>
                    {
                        if (etRange != null &&
                            (Run.GetElutionTime(ms2ScanNum) < etRange[0] || UpdateDictionary.GetElutionTime(ms2ScanNum) > etRange[1]))
                            return;
                        var cPs = new Old_CorrectedProductSpectrum(ms2ScanNum, Run.GetIsolationWindow(ms2ScanNum).IsolationWindowTargetMz, Tolerance, UpdateDictionary);
                        correctedPsList.Add(cPs);

                        if (cPs.ChargeCorrected)
                        {
                            var counterKey = new Tuple<int?, sbyte>(cPs.Ps.IsolationWindow.Charge, cPs.minErrorCharge);
                            if (chargeCounter.ContainsKey(counterKey))
                                chargeCounter[counterKey] += 1;
                            else
                                chargeCounter[counterKey] = 1;
                        }

                        lock (lockTarget)
                        {
                            if (cPs.ChargeCorrected)
                            {
                                changedCount++;
                                //cPs.PrintDistribution();
                            }
                            if (++cntr%1000 == 0) Console.Write(@"{0} ", cntr);
                        }
                    });
                Console.WriteLine("Number of charge changed spectrum: {0}", changedCount);
                foreach (var key in chargeCounter.Keys)
                {
                    Console.WriteLine("{0} -> {1} : {2}", key.Item1, key.Item2, chargeCounter[key]);
                }
                Console.WriteLine("Writing mgf!\n");


                #region mgf writing

                if (!Directory.Exists(MgfDirPath)) Directory.CreateDirectory(MgfDirPath);

                int dummySn = 1;
                var mgfpath = MgfDirPath + Name + ".chargeCorrected.mgf";
                using (var mgfWriter = new StreamWriter(mgfpath))
                {
                    foreach (var correctedPs in correctedPsList)
                    {
                        string psString;
                        if (correctedPs.ChargeCorrected)
                        {
                            psString = correctedPs.Ps.ToMgfString(correctedPs.minErrorCharge,
                                correctedPs.CorrectedPrecusorMz,
                                dummySn++,
                                correctedPs.Ps.ScanNum + " " + correctedPs.Ps.IsolationWindow.Charge
                                + " " + correctedPs.Ps.IsolationWindow.IsolationWindowTargetMz);
                        }
                        else if (correctedPs.MzCorrected)
                        {
                            psString = correctedPs.Ps.ToMgfString(correctedPs.Ps.IsolationWindow.Charge,
                                correctedPs.CorrectedPrecusorMz,
                                dummySn++,
                                correctedPs.Ps.ScanNum + " " + correctedPs.Ps.IsolationWindow.Charge
                                + " " + correctedPs.Ps.IsolationWindow.IsolationWindowTargetMz);
                        }
                        else
                        {
                            psString = correctedPs.Ps.ToMgfString(correctedPs.Ps.IsolationWindow.Charge,
                                correctedPs.Ps.IsolationWindow.IsolationWindowTargetMz,
                                dummySn++,
                                correctedPs.Ps.ScanNum + " " + correctedPs.Ps.IsolationWindow.Charge
                                + " " + correctedPs.Ps.IsolationWindow.IsolationWindowTargetMz);
                        }
                        mgfWriter.WriteLine(psString);
                    }
                }

                #endregion
            }
        }

         */
        
    }
}