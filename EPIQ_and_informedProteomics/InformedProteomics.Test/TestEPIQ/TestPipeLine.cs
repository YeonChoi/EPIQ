using System;
using System.Collections.Generic;
using Epiq;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.MassSpecData;
using NUnit.Framework;

namespace InformedProteomics.Test.TestEpiq
{
    [TestFixture]
    internal class TestPipeLine
    {
        private static string[][][][] GetTsvAndBinStrs(string[][][] raw)
        {
            var ret = new string[2][][][];
            ret[0] = new string[raw.Length][][];
            ret[1] = new string[raw.Length][][];

            for (var i = 0; i < raw.Length; i++)
            {
                ret[0][i] = new string[raw[i].Length][];
                ret[1][i] = new string[raw[i].Length][];

                for (var j = 0; j < raw[i].Length; j++)
                {
                    ret[0][i][j] = new string[raw[i][j].Length];
                    ret[1][i][j] = new string[raw[i][j].Length];

                    for (var k = 0; k < raw[i][j].Length; k++)
                    {
                        ret[0][i][j][k] = raw[i][j][k].Replace("RawFiles", "SearchResults").Replace(".raw", ".tsv");
                        ret[1][i][j][k] = raw[i][j][k].Replace("RawFiles", "TmpResults").Replace(".raw", ".bin");
                    }
                }
            }
            return ret;
        }


        [Test]
        public void TestLabelingEfficiencyCorrection()
        {
            new LabelingEfficiencyCorrection();

            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index
            var fasta = @"E:\MassSpec\PaperData\DB\hsa_UP000005640_cRAP_added\hsa_UP000005640_cRAP_added.revCat.fasta";

            //E:\MassSpec\PaperData\DE6_Tryp_QE\

            var svrPaths = new[] {@"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RtModel\D-shifts_libSVMData_UniquePepSampled_ForPaperSILAC6plexLumos_ThermoUltimate3000_130m.standardized.s4t2g0.1c0.1.model",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RtModel\D-shifts_libSVMData_UniquePepSampled_ForPaperSILAC6plexLumos_ThermoUltimate3000_130m.standard" };
            var overwrite = true;
            var writeExcel = true;
            var writeXic = true;
            var quantfileDir = @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\IncorTest";
            string mfileDir = null;

            raws = new[]
            {  new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RawFiles\SILAC_eq_1-1_NCP1_J100_20180220.raw"
                    }
                    , 
                },
                
            };

            var tp = GetTsvAndBinStrs(raws);
            PipeLine.Run(raws, tp[0], tp[1], LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["upSILAC 6plex"], svrPaths, fasta, tolernace, mfileDir, quantfileDir, overwrite,
                writeExcel);


        }


        [Test]
        public void TestDE6GetXicValues()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index
            var fasta = @"E:\MassSpec\PaperData\DB\hsa_UP000005640\hsa_UP000005640_cRAP_added.revCat.fasta";

            //E:\MassSpec\PaperData\DE6_Tryp_QE\

            var svrPaths = new[] {@"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RTmodel\D-shifts_libSVMData_UniquePepSampled_ForPaperQE_ThermoUltimate3000_125m.standardized.s4t2g1c1.model",
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RTmodel\D-shifts_libSVMData_UniquePepSampled_ForPaperQE_ThermoUltimate3000_125m.standard" };
            var overwrite = true;
            var writeExcel = true;
            var writeXic = true;
            var quantfileDir = @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\";
            var mfileDir = @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\";

            raws = new[]
            {  new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RawFiles\DE_1_1_NAQ448C1_J100_0913.raw"
                    }
                    , 
                },
                
            };

            var tp = GetTsvAndBinStrs(raws);
            PipeLine.Run(raws, tp[0], tp[1], LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["Diethylation 6plex"], svrPaths, fasta, tolernace, mfileDir, quantfileDir, overwrite,
                writeExcel);
        }


        [Test]
        public void TestUpSilacEqui()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index
            var fasta = @"E:\MassSpec\PaperData\DB\hsa_UP000005640\hsa_UP000005640_cRAP_added.revCat.fasta";

            //E:\MassSpec\PaperData\DE6_Tryp_QE\

            var svrPaths = new [] {   @"E:\MassSpec\PaperData\DE6\D-shifts_libSVMData_UniquePepSampled_ThermoUltimate3000_130m_20171218Lumos.standardized.s4t2g1c1.model",
                @"E:\MassSpec\PaperData\DE6\D-shifts_libSVMData_UniquePepSampled_ThermoUltimate3000_130m_20171218Lumos.standard" };
            var overwrite = false;
            var writeExcel = true;
            var writeXic = true;
            var quantfileDir = @"E:\MassSpec\PaperData\upSILAC\20171215_eq_mol_test\QuantResults_RtDeltaTrainingUsed\";
            var mfileDir = @"E:\MassSpec\PaperData\upSILAC\20171215_eq_mol_test\mFiles_RtDeltaTrainingUsed\";

            raws = new[]
            {  new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\upSILAC\20171215_eq_mol_test\RawFiles\Silac_eq_NCP1_J100_20171215.raw"
                    }
                    , 
                },
                
            };

            var tp = GetTsvAndBinStrs(raws);
            PipeLine.Run(raws, tp[0], tp[1], LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["upSILAC 6plex"], svrPaths, fasta, tolernace, mfileDir, quantfileDir, overwrite,
                writeExcel);
        }

        [Test]
        public void TestDE3()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index
            var fasta = @"E:\MassSpec\PaperData\DE3_LysC_QE\hsa_UP000005640_cRAP_added.revCat.fasta";
            var svrPaths = new[]
            {
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard"
            };

            var overwrite = true;
            var writeExcel = false;
            var writeXicClusters = false;
            var mfileDir = @"E:\MassSpec\PaperData\DE3_LysC_QE\mFiles\";
            var quantfileDir = @"E:\MassSpec\PaperData\DE3_LysC_QE\quantFiles\";

            raws = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_1_1\RawFiles\DE_1_1_1_NCP1C2_J100_0519_01.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_1_1\RawFiles\DE_1_1_1_NCP1C2_J100_0519_02.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_1_1\RawFiles\DE_1_1_1_NCP1C2_J100_0519_03_RE.raw"
                    }
                }
                ,
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_5_10\RawFiles\DE_1_5_10_NCP1C1_J100_0521_01.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_5_10\RawFiles\DE_1_5_10_NCP1C1_J100_0521_03.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_5_10\RawFiles\DE_1_5_10_NCP1C2_J100_0521_02.raw"
                    }
                }
                ,
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_10_20\RawFiles\DE_1_10_20_01_J100_0928.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_10_20\RawFiles\DE_1_10_20_02_J100_0928.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_10_20\RawFiles\DE_1_10_20_03_J100_0928.raw"
                    }
                },
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_20_50\RawFiles\DE_1_20_50_01_NCP2C1_J100_1031.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_20_50\RawFiles\DE_1_20_50_02_NCP2C1_J100_1031.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_20_50\RawFiles\DE_1_20_50_03_NCP2C1_J100_1031.raw"
                    }
                }
            };
            var strs = GetTsvAndBinStrs(raws);
            PipeLine.Run(raws, strs[0], strs[1], @"DE3", svrPaths, fasta, tolernace, mfileDir, quantfileDir, overwrite,
                writeExcel);
        }


        [Test]
        public void TestDE3Xeno()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index
            var fasta = @"E:\MassSpec\PaperData\DE3_LysC_QE\PHROG_Database_cRAP_added.revCat.fasta";
            var svrPaths = new[]
            {
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard"
            };

            var overwrite = false;
            var writeExcel = true;
            var writeXicClusters = false;
            var mfileDir = @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\mFiles\";
            var quantfileDir = @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\quantFiles\";

            raws = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN01.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN02.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN03.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN04.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN05.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN06.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN07.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN08.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN09.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN10.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN11.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN12.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN13.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN14.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN15.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN16.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN17.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN18.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN19.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN20.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN21.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN22.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN23.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0814_J100_NCP1_FN24.raw",
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN01.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN02.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN03.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN04.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN05.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN06.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN07.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN08.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN09.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN10.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN11.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN12.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN13.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN14.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN15.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN16.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN17.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN18.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN19.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN20.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN21.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN22.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN23.raw",
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\RawFiles\Embryo_1_8_13_0818_SET02_J100_NCP1_FN24.raw",
                    },

                }
            };
            var strs = GetTsvAndBinStrs(raws);
            PipeLine.Run(raws, strs[0], strs[1], @"DE3", svrPaths, fasta, tolernace, null, quantfileDir, overwrite,
                writeExcel);
        }

        [Test]
        public void TestSY_BP2plex()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index
            var fasta = @"D:\p\tiny\SY_BP2plex\Unist_uniprot-human20170806.revCat.fasta";
            var svrPaths = new[]
            {
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard"
            };

            var overwrite = true;
            var writeExcel = true;
            var writeXicClusters = false;
            var mfileDir = @"D:\p\tiny\SY_BP2plex\mFiles\";
            var quantfileDir = @"D:\p\tiny\SY_BP2plex\QuantResults\";

            raws = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_1_1\RawFiles\DM_1_1_1_01_NCP1_J100_0510.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_1_1\RawFiles\DM_1_1_1_02_NCP1_J100_0510.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_1_1\RawFiles\DM_1_1_1_03_NCP1_J100_0510.raw"
                    }
                }
                ,
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_5_10\RawFiles\DM_1_5_10_01_NCP1_J100_0510.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_5_10\RawFiles\DM_1_5_10_02_NCP1_J100_0510.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_5_10\RawFiles\DM_1_5_10_03_NCP1_J100_0510.raw"
                    }
                }
                ,
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_10_20\RawFiles\DM_1_10_20_01_NCP2C1_J100_1031.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_10_20\RawFiles\DM_1_10_20_02_NCP2C1_J100_1031.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_10_20\RawFiles\DM_1_10_20_03_NCP2C1_J100_1031.raw"
                    }
                },
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_20_50\RawFiles\DM_1_20_50_01_NCP2_J100_1019.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_20_50\RawFiles\DM_1_20_50_02_NCP2_J100_1019.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_20_50\RawFiles\DM_1_20_50_03_NCP2_J100_1019.raw"
                    }
                }
            };
            var strs = GetTsvAndBinStrs(raws);
            PipeLine.Run(raws, strs[0], strs[1], @"DM3", svrPaths, fasta, tolernace, mfileDir, quantfileDir, overwrite, writeExcel);
        }


        [Test]
        public void TestDM3()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index
            var fasta = @"E:\MassSpec\PaperData\DE3_LysC_QE\hsa_UP000005640_cRAP_added.revCat.fasta";
            var svrPaths = new[]
            {
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard"
            };

            var overwrite = true;
            var writeExcel = false;
            var writeXicClusters = false;
            var mfileDir = @"E:\MassSpec\PaperData\DM3_LysC_QE\mFiles\";
            var quantfileDir = @"E:\MassSpec\PaperData\DM3_LysC_QE\quantFiles\";

            raws = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_1_1\RawFiles\DM_1_1_1_01_NCP1_J100_0510.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_1_1\RawFiles\DM_1_1_1_02_NCP1_J100_0510.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_1_1\RawFiles\DM_1_1_1_03_NCP1_J100_0510.raw"
                    }
                }
                ,
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_5_10\RawFiles\DM_1_5_10_01_NCP1_J100_0510.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_5_10\RawFiles\DM_1_5_10_02_NCP1_J100_0510.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_5_10\RawFiles\DM_1_5_10_03_NCP1_J100_0510.raw"
                    }
                }
                ,
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_10_20\RawFiles\DM_1_10_20_01_NCP2C1_J100_1031.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_10_20\RawFiles\DM_1_10_20_02_NCP2C1_J100_1031.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_10_20\RawFiles\DM_1_10_20_03_NCP2C1_J100_1031.raw"
                    }
                },
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_20_50\RawFiles\DM_1_20_50_01_NCP2_J100_1019.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_20_50\RawFiles\DM_1_20_50_02_NCP2_J100_1019.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DM3_LysC_QE\DM3_1_20_50\RawFiles\DM_1_20_50_03_NCP2_J100_1019.raw"
                    }
                }
            };
            var strs = GetTsvAndBinStrs(raws);
            PipeLine.Run(raws, strs[0], strs[1], @"DM3", svrPaths, fasta, tolernace, mfileDir, quantfileDir, overwrite,
                writeExcel);
        }

        [Test]
        public void TestDE3_2()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index
            var fasta = @"E:\MassSpec\PaperData\DE3_LysC_QE\hsa_UP000005640_cRAP_added.revCat.fasta";
            var svrPaths = new[]
            {
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard"
            };

            var overwrite = true;
            var writeExcel = false;
            var writeXicClusters = false;
            var mfileDir = @"E:\MassSpec\PaperData\DE3_LysC_QE\mFilesSimple\";
            var quantfileDir = @"E:\MassSpec\PaperData\DE3_LysC_QE\quantFilesSimple\";

            raws = new[]
            {
               
                 new[]
                {
                    new[]
                    {
                        // @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_1_1\RawFiles\DE_1_1_1_NCP1C2_J100_0519_01.raw"
                        // @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_20_50\RawFiles\DE_1_20_50_01_NCP2C1_J100_1031.raw"
                        @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_10_20\RawFiles\DE_1_10_20_01_J100_0928.raw"
                    },
                }
            };

            var strs = GetTsvAndBinStrs(raws);
            PipeLine.Run(raws, strs[0], strs[1], @"DE3", svrPaths, fasta, tolernace, mfileDir, quantfileDir, overwrite,
                writeExcel);
        }

        [Test]
        public void TestDE6()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index
            
            var fasta = @"E:\MassSpec\PaperData\DE3_LysC_QE\hsa_UP000005640_cRAP_added.revCat.fasta";

            var svrPaths = new[]
            {
                @"E:\MassSpec\PaperData\SVM\D-shifts_libSVMData_UniquePepSampled_ThermoUltimate3000_130m_20171218Lumos_highCos.standardized.s4t2g1c1.model",
                @"E:\MassSpec\PaperData\SVM\D-shifts_libSVMData_UniquePepSampled_ThermoUltimate3000_130m_20171218Lumos_highCos.standard"
            };
            var overwrite = true;
            var writeExcel = true;
            var mfileDir = @"E:\MassSpec\PaperData\DE6\mFiles\";
            var quantfileDir = @"E:\MassSpec\PaperData\DE6\quantFiles\";

            raws = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE6\RawFiles\DE6plex_SCX_Fr4_NCP1_J100_20171204.raw"
                    },
                   
                },
               
                /*
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170708_DE6plex_mixture_QE\RawFiles\DE_1_1_1_NAQ444_J100_0708.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170708_DE6plex_mixture_QE\RawFiles\DE_1_1_2_NAQ444_J100_0708.raw"
                    }
                },
                
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170708_DE6plex_mixture_QE\RawFiles\DE_12_1_1_NAQ444_J100_0708.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170708_DE6plex_mixture_QE\RawFiles\DE_12_1_2_NAQ444_J100_0708.raw"
                    }
                },

                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170708_DE6plex_mixture_QE\RawFiles\DE_15_1_1_NAQ444_J100_0708.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170708_DE6plex_mixture_QE\RawFiles\DE_15_1_2_NAQ444_J100_0708.raw"
                    }
                },

                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170708_DE6plex_mixture_QE\RawFiles\DE6plex_1_1_1_NCP2_J100_0621.raw"
                    },
                },*/
                

                //DE6Plex_3ug_NCP2_J90_0228
            };
            var tp = GetTsvAndBinStrs(raws);

            PipeLine.Run(raws, tp[0], tp[1], @"DE", svrPaths, fasta, tolernace, mfileDir, quantfileDir,
                overwrite, writeExcel);
        }

        [Test]
        public void TestupSILAC()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index

            var fasta = @"E:\MassSpec\PaperData\DE3_LysC_QE\hsa_UP000005640_cRAP_added.revCat.fasta";

            var svrPaths = new[]
            {
                @"E:\MassSpec\PaperData\DE6\D-shifts_libSVMData_UniquePepSampled_ThermoUltimate3000_130m_20171218Lumos.standardized.s4t2g1c1.model",
                @"E:\MassSpec\PaperData\DE6\D-shifts_libSVMData_UniquePepSampled_ThermoUltimate3000_130m_20171218Lumos.standard"
            };
            var overwrite = true;
            var writeExcel = true;
            var mfileDir = @"E:\MassSpec\PaperData\upSILAC\mFiles\";
            var quantfileDir = @"E:\MassSpec\PaperData\upSILAC\quantFiles\";

            raws = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\upSILAC\RawFiles\Silac_eq_NCP1_J100_20171215.raw"
                    },
                   
                },
            };
            var tp = GetTsvAndBinStrs(raws);

            PipeLine.Run(raws, tp[0], tp[1], @"DE", svrPaths, fasta, tolernace, null, quantfileDir,
                overwrite, writeExcel);
        }
        [Test]
        public void TestDE6DEP()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index

            var fasta = @"E:\MassSpec\ProteinDBs\H_sapiens_Uniprot_SPROT_2013-05-01.revCat.fasta";

            var svrPaths = new[]
            {
                 @"E:\MassSpec\PaperData\SVM\D-shifts_libSVMData_UniquePepSampled_ThermoUltimate3000_130m_20171218Lumos.standardized.s4t2g1c1.model",
                @"E:\MassSpec\PaperData\SVM\D-shifts_libSVMData_UniquePepSampled_ThermoUltimate3000_130m_20171218Lumos.standard"
            };
            var overwrite = true;
            var writeExcel = true;
            var mfileDir = @"E:\MassSpec\PaperData\DE6_DEP_QE\mFiles\";
            var quantfileDir = @"E:\MassSpec\PaperData\DE6_DEP_QE\quantFiles\";

            raws = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE6_DEP_QE\RawFiles\DE_1_20_NAQ448C1_J100_0913.raw"
                    },
                },
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\DE6_DEP_QE\RawFiles\DE_1_4_NAQ448C1_J100_0913.raw"
                    },
                },
               
            };
            var tp = GetTsvAndBinStrs(raws);

            PipeLine.Run(raws, tp[0], tp[1], @"DE", svrPaths, fasta, tolernace, mfileDir, quantfileDir,
                overwrite, writeExcel);
        }

        [Test]
        public void TestDE6_30_20_10_1_5_10()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index
            var fasta = @"E:\MassSpec\PaperData\DB\hsa_UP000005640\hsa_UP000005640_cRAP_added.revCat.fasta";


            //E:\MassSpec\PaperData\DE6_Tryp_QE\

            var svrPaths = new[]
            {
                @"E:\MassSpec\PaperData\SVM\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
                @"E:\MassSpec\PaperData\SVM\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard"
            };
            var overwrite = true;
            var writeExcel = true;
            var writeXic = false;
            var mfileDir = @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\mFiles\";
            var quantfileDir = @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\quantFiles\";

            raws = new[]
            {  new[]
                {
                  
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\DE_SCX_Fr1_NCP2_J100_0720.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\DE_SCX_Fr2_NCP2_J100_0720.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\DE_SCX_Fr3_NCP2_J100_0720.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\DE_SCX_Fr4_NCP2_J100_0720.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\DE_SCX_Fr5_NCP2_J100_0720.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\DE_SCX_Fr6_NCP2_J100_0720.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\DE_SCX_Fr7_NCP2_J100_0720.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\DE_SCX_Fr8_NCP2_J100_0720.raw",
                    }
                    , 
                   /*  new[]
                    {
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\set2\DE_SCX_Fr1_NCP2_J100_0721.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\set2\DE_SCX_Fr2_NCP2_J100_0721.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\set2\DE_SCX_Fr3_NCP2_J100_0721.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\set2\DE_SCX_Fr4_NCP2_J100_0721.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\set2\DE_SCX_Fr5_NCP2_J100_0721.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\set2\DE_SCX_Fr6_NCP2_J100_0721.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\set2\DE_SCX_Fr7_NCP2_J100_0721.raw",
                        @"E:\MassSpec\PaperData\20170720_DE6plex_SCX_30_20_QE\RawFiles\set2\DE_SCX_Fr8_NCP2_J100_0721.raw",
                    }*/
                   /*  new[]
                    {
                        @"E:\MassSpec\PaperData\DE6_DEP_QE\RawFiles\DE_1_1_NAQ448C1_J100_0913.raw"
                    },*/
                  
                },
                
            };

            var tp = GetTsvAndBinStrs(raws);
            PipeLine.Run(raws, tp[0], tp[1], @"DE", svrPaths, fasta, tolernace, null, quantfileDir, overwrite,
                writeExcel);
        }


        [Test]
        public void TestDE6PlexQEIsotopeImpurityMeasurement()
        {
            var tolernace = new Tolerance(5);

            var filePathPerLabelDictionary = new Dictionary<sbyte, Tuple<string, string, string, string>>()
            {
                { 0, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\RawFiles\DE_test_C_NCP2C1_J100_1126.raw",
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\SearchResults\DE_test_C_NCP2C1_J100_1126.tsv",
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\MeasuredII\QE_label_1.tsv",
                    "^_;K_"
                    )},
                { 1, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\RawFiles\DE_test_L1_NCP2C1_J100_1126.raw",
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\SearchResults\DE_test_L1_NCP2C1_J100_1126.tsv",
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\MeasuredII\QE_label_2.tsv",
                    "^_D;K_D"
                    )},
                { 2, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\RawFiles\DE_test_L2_NCP2C1_J100_1126.raw",
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\SearchResults\DE_test_L2_NCP2C1_J100_1126.tsv",
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\MeasuredII\QE_label_3.tsv",
                    "^_D;K_D"
                    )},
                { 3, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\RawFiles\DE_test_L3_NCP2C1_J100_1126.raw",
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\SearchResults\DE_test_L3_NCP2C1_J100_1126.tsv",
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\MeasuredII\QE_label_4.tsv",
                    "^_D;K_D"
                    )},
                { 4, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\RawFiles\DE_test_L4_NCP2C1_J100_1126.raw",
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\SearchResults\DE_test_L4_NCP2C1_J100_1126.tsv",
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\MeasuredII\QE_label_5.tsv",
                    "^_D;K_D"
                    )},
                { 5, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\RawFiles\DE_test_L5_NCP2C1_J100_1126.raw",
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\SearchResults\DE_test_L5_NCP2C1_J100_1126.tsv",
                    @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\LabelCorrection\MeasuredII\QE_label_6.tsv",
                    "^_D;K_D"
                    )},
            };

            var toCheckLabelSites = new List<string> {"^|K"};
            PipeLine.RunIsotopeImpurityMeasurement(LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["Diethylation 6plex"],
                                                   filePathPerLabelDictionary, tolernace, toCheckLabelSites);
        }


        [Test]
        public void TestDE6PlexLumosIsotopeImpurityMeasurement()
        {
            var tolernace = new Tolerance(5);

            var filePathPerLabelDictionary = new Dictionary<sbyte, Tuple<string, string, string, string>>()
            {
               { 0, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\RawFiles\DE_Label_test_L1_J100_NAQ448_0728_C1.raw",
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\SearchResults\DE_Label_test_L1_J100_NAQ448_0728_C1.tsv",
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\MeasuredII\label_1.tsv",
                    "^_;K_"
                    )},
                { 1, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\RawFiles\DE_Label_test_L2_J100_NAQ448_0728_C1.raw",
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\SearchResults\DE_Label_test_L2_J100_NAQ448_0728_C1.tsv",
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\MeasuredII\label_2.tsv",
                    "^_D;K_D"
                    )},
                { 2, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\RawFiles\DE_Label_test_L3_J100_NAQ448_0728_C1.raw",
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\SearchResults\DE_Label_test_L3_J100_NAQ448_0728_C1.tsv",
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\MeasuredII\label_3.tsv",
                    "^_D;K_D"
                    )},
                { 3, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\RawFiles\DE_Label_test_L4_J100_NAQ448_0728_C1.raw",
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\SearchResults\DE_Label_test_L4_J100_NAQ448_0728_C1.tsv",
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\MeasuredII\label_4.tsv",
                    "^_D;K_D"
                    )},
                { 4, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\RawFiles\DE_Label_test_L5_J100_NAQ448_0728_C1.raw",
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\SearchResults\DE_Label_test_L5_J100_NAQ448_0728_C1.tsv",
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\MeasuredII\label_5.tsv",
                    "^_D;K_D"
                    )},
                { 5, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\RawFiles\DE_Label_test_L6_J100_NAQ448_0728_C1.raw",
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\SearchResults\DE_Label_test_L6_J100_NAQ448_0728_C1.tsv",
                    @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\LabelCorrection\20170728\MeasuredII\label_6.tsv",
                    "^_D;K_D"
                    )},
            };

            var toCheckLabelSites = new List<string> {"^|K"};
            PipeLine.RunIsotopeImpurityMeasurement(LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["Diethylation 6plex"],
                                                   filePathPerLabelDictionary, tolernace, toCheckLabelSites);
        }


        [Test]
        public void TestSilacK14IsotopeImpurityMeasurement()
        {
            var tolernace = new Tolerance(5);

            var filePathPerLabelDictionary = new Dictionary<sbyte, Tuple<string, string, string, string>>()
            {
                { 0, new Tuple<string, string, string, string>(
                    @"D:\p\tiny\SILAC_K14\RawFiles\K14only_J100_NAQ448_20180813.raw",
                    @"D:\p\tiny\SILAC_K14\SearchResults\K14only_J100_NAQ448_20180813.tsv",
                    @"D:\p\tiny\SILAC_K14\MeasuredII\K14.tsv",
                    "K_13C_15N_D"
                    )},
            };
            var toCheckLabelSites = new List<string> {"K",};

            PipeLine.RunIsotopeImpurityMeasurement(new[] { "K 7_14.061101" },
                                                   filePathPerLabelDictionary, tolernace, toCheckLabelSites);
        }



        [Test]
        public void TestSilac6PlexIsotopeImpurityMeasurement()
        {
            var tolernace = new Tolerance(5);

            var filePathPerLabelDictionary = new Dictionary<sbyte, Tuple<string, string, string, string>>()
            {
                { 0, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\RawFiles\S1_NCP1_J100_20180127.raw",
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\SearchResults\S1_NCP1_J100_20180127.tsv",
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\MeasuredII\S1.tsv",
                    "K;R"
                    )},
                { 1, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\RawFiles\S2_NCP1_J100_20180127.raw",
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\SearchResults\S2_NCP1_J100_20180127.tsv",
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\MeasuredII\S2.tsv",
                    "K_15N;R_15N"
                    )},
                { 2, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\RawFiles\S3_NCP1_J100_20180127.raw",
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\SearchResults\S3_NCP1_J100_20180127.tsv",
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\MeasuredII\S3.tsv",
                    "K_D;R_15N"
                    )},
                { 3, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\RawFiles\S4_NCP1_J100_20180127.raw",
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\SearchResults\S4_NCP1_J100_20180127.tsv",
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\MeasuredII\S4.tsv",
                    "K_13C;R_D"
                    )},
                { 4, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\RawFiles\S5_NCP1_J100_20180127.raw",
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\SearchResults\S5_NCP1_J100_20180127.tsv",
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\MeasuredII\S5.tsv",
                    "K_D;R_D_15N"
                    )},
                { 5, new Tuple<string, string, string, string>(
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\RawFiles\S6_NCP1_J100_20180127.raw",
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\SearchResults\S6_NCP1_J100_20180127.tsv",
                    @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\LabelCorrection\MeasuredII\S6.tsv",
                    "K_D_15N;R_D_15N"
                    )},
            };
            var toCheckLabelSites = new List<string> {"K", "R"};

            PipeLine.RunIsotopeImpurityMeasurement(LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["upSILAC 6plex"],
                                                   filePathPerLabelDictionary, tolernace, toCheckLabelSites);
        }


        [Test]
        public void TestSilac6PlexNanoAcquityTraining()
        {
            var tolernace = new Tolerance(5);
            var overwrite = true;
            var writeXicClusters = false;
            string[] raws;
            string[] tsvs;
            string[] psmTmpOutFiles;
            string[] trainingOutfile;

            raws = new[]
            {
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\RawFiles\silac_6plex_eq-1_1_naq444_j100_20181227.raw",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\RawFiles\silac_6plex_eq-1_2_naq444_j100_20181227.raw",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\RawFiles\silac_6plex_eq-2_1_naq444_j100_20181227.raw",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\RawFiles\silac_6plex_eq-2_2_naq444_j100_20181227.raw"
            };

            tsvs = new[]
            {
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\SearchResults\silac_6plex_eq-1_1_naq444_j100_20181227.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\SearchResults\silac_6plex_eq-1_2_naq444_j100_20181227.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\SearchResults\silac_6plex_eq-2_1_naq444_j100_20181227.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\SearchResults\silac_6plex_eq-2_2_naq444_j100_20181227.tsv"
            };

            psmTmpOutFiles = new[]
            {
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\RawFiles\silac_6plex_eq-1_1_naq444_j100_20181227.bin",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\RawFiles\silac_6plex_eq-1_2_naq444_j100_20181227.bin",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\RawFiles\silac_6plex_eq-2_1_naq444_j100_20181227.bin",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\RawFiles\silac_6plex_eq-2_2_naq444_j100_20181227.bin"
            };

            trainingOutfile = new[]
            {
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\TrainingFiles\silac_6plex_eq-1_1_naq444_j100_20181227.training.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\TrainingFiles\silac_6plex_eq-1_2_naq444_j100_20181227.training.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\TrainingFiles\silac_6plex_eq-2_1_naq444_j100_20181227.training.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_nanoACQUITY\TrainingFiles\silac_6plex_eq-2_2_naq444_j100_20181227.training.tsv"
            };
            PipeLine.RunTraining(raws, tsvs, psmTmpOutFiles, LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["upSILAC 6plex"],
                                 tolernace, trainingOutfile, overwrite, writeXicClusters);
        }





        [Test]
        public void TestSilac6PlexTraining()
        {
            var tolernace = new Tolerance(5);
            var overwrite = true;
            var writeXicClusters = false;
            var removeContam = true;
            var removeDecoy = true;
            string[] raws;
            string[] tsvs;
            string[] psmTmpOutFiles;
            string[] trainingOutfile;

            raws = new[]
            {
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RawFiles\SILAC_eq_1_NCP1_J100_20180206.raw",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RawFiles\SILAC_eq_2_NCP1_J100_20180206.raw",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RawFiles\SILAC_eq_3_NCP1_J100_20180206.raw",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RawFiles\SILAC_eq_1-1_NCP1_J100_20180220.raw",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RawFiles\SILAC_eq_1-2_NCP1_J100_20180220.raw",

            };

            tsvs = new[]
            {
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\SearchResults\SILAC_eq_1_NCP1_J100_20180206.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\SearchResults\SILAC_eq_2_NCP1_J100_20180206.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\SearchResults\SILAC_eq_3_NCP1_J100_20180206.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\SearchResults\SILAC_eq_1-1_NCP1_J100_20180220.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\SearchResults\SILAC_eq_1-2_NCP1_J100_20180220.tsv",
            };

            psmTmpOutFiles = new[]
            {
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RawFiles\SILAC_eq_1_NCP1_J100_20180206.bin",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RawFiles\SILAC_eq_2_NCP1_J100_20180206.bin",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RawFiles\SILAC_eq_3_NCP1_J100_20180206.bin",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RawFiles\SILAC_eq_1-1_NCP1_J100_20180220.bin",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RawFiles\SILAC_eq_1-2_NCP1_J100_20180220.bin",
            };

            trainingOutfile = new[]
            {
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RtModelTrainingData\SILAC_eq_1_NCP1_J100_20180206.training.test.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RtModelTrainingData\SILAC_eq_2_NCP1_J100_20180206.training.test.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RtModelTrainingData\SILAC_eq_3_NCP1_J100_20180206.training.test.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RtModelTrainingData\SILAC_eq_1-1_NCP1_J100_20180220.training.test.tsv",
                @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\RtModelTrainingData\SILAC_eq_1-2_NCP1_J100_20180220.training.test.tsv",
            };
            PipeLine.RunTraining(raws, tsvs, psmTmpOutFiles, LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["upSILAC 6plex"],
                                 tolernace, trainingOutfile, overwrite, writeXicClusters);
        }

        [Test]
        public void TestDm5Training()
        {
            var tolernace = new Tolerance(5);
            var overwrite = true;
            var writeXicClusters = false;
            string[] raws;
            string[] tsvs;
            string[] psmTmpOutFiles;
            string[] trainingOutfile;

            raws = new[]
            {
                @"E:\MassSpec\PaperData\DM5_QE_oldLC\RawFiles\5plex_M1_Dimet_J70_NAQ1_0808_1.raw",
                @"E:\MassSpec\PaperData\DM5_QE_oldLC\RawFiles\5plex_M1_Dimet_J70_NAQ1_0808_2.raw",
            };

            tsvs = new[]
            {
                @"E:\MassSpec\PaperData\DM5_QE_oldLC\SearchResults\5plex_M1_Dimet_J70_NAQ1_0808_1.tsv",
                @"E:\MassSpec\PaperData\DM5_QE_oldLC\SearchResults\5plex_M1_Dimet_J70_NAQ1_0808_2.tsv",
            };

            psmTmpOutFiles = new[]
            {
                @"E:\MassSpec\PaperData\DM5_QE_oldLC\RawFiles\5plex_M1_Dimet_J70_NAQ1_0808_1.bin",
                @"E:\MassSpec\PaperData\DM5_QE_oldLC\RawFiles\5plex_M1_Dimet_J70_NAQ1_0808_2.bin",
            };

            trainingOutfile = new[]
            {
                @"E:\MassSpec\PaperData\DM5_QE_oldLC\RtTraining\RtModelTrainingData\5plex_M1_Dimet_J70_NAQ1_0808_1.tsv",
                @"E:\MassSpec\PaperData\DM5_QE_oldLC\RtTraining\RtModelTrainingData\5plex_M1_Dimet_J70_NAQ1_0808_2.tsv",
            };

            PipeLine.RunTraining(raws, tsvs, psmTmpOutFiles, LabelingSchemes.PredefinedLabelingSchemeToLabelStrings["Dimethylation 5plex"],
                                 tolernace, trainingOutfile, overwrite, writeXicClusters);
        }


        [Test]
        public void TestDE6LumosWatersLcTraining()
        {
            var tolernace = new Tolerance(5);
            var overwrite = true;
            var writeXicClusters = false;
            string[] raws;
            string[] tsvs;
            string[] psmTmpOutFiles;
            string[] trainingOutfile;

            raws = new[]
            {
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE6plex_1_1_1_CT1_IT30_NAQ444_J100_20171124.raw",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE6plex_1_1_2_CT1_IT30_NAQ444_J100_20171124.raw",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE6plex_1_1_3_CT1_IT30_NAQ444_J100_20171124.raw",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE6plex_1_1_4_CT1_IT40_NAQ444_J100_20171124.raw",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE6plex_mixture_CT1_200min_J100_NAQ444_20171116.raw",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE_1_1_1_NAQ1D_J100_20171018.raw",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE_1_1_2_NAQ1D_APD_J100_20171018.raw",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE_1_1_2_NAQ1D_J100_20171018.raw",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE_1_1_2_NAQ1D_J100_20171029.raw",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE_1_1_3_5ug_NAQ1D_J100_20171018.raw",
            };

            tsvs = new[]
            {
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\SearchResults\DE6plex_1_1_1_CT1_IT30_NAQ444_J100_20171124.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\SearchResults\DE6plex_1_1_2_CT1_IT30_NAQ444_J100_20171124.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\SearchResults\DE6plex_1_1_3_CT1_IT30_NAQ444_J100_20171124.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\SearchResults\DE6plex_1_1_4_CT1_IT40_NAQ444_J100_20171124.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\SearchResults\DE6plex_mixture_CT1_200min_J100_NAQ444_20171116.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\SearchResults\DE_1_1_1_NAQ1D_J100_20171018.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\SearchResults\DE_1_1_2_NAQ1D_APD_J100_20171018.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\SearchResults\DE_1_1_2_NAQ1D_J100_20171018.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\SearchResults\DE_1_1_2_NAQ1D_J100_20171029.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\SearchResults\DE_1_1_3_5ug_NAQ1D_J100_20171018.tsv",
            };

            psmTmpOutFiles = new[]
            {
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE6plex_1_1_1_CT1_IT30_NAQ444_J100_20171124.bin",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE6plex_1_1_2_CT1_IT30_NAQ444_J100_20171124.bin",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE6plex_1_1_3_CT1_IT30_NAQ444_J100_20171124.bin",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE6plex_1_1_4_CT1_IT40_NAQ444_J100_20171124.bin",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE6plex_mixture_CT1_200min_J100_NAQ444_20171116.bin",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE_1_1_1_NAQ1D_J100_20171018.bin",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE_1_1_2_NAQ1D_APD_J100_20171018.bin",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE_1_1_2_NAQ1D_J100_20171018.bin",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE_1_1_2_NAQ1D_J100_20171029.bin",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RawFiles\DE_1_1_3_5ug_NAQ1D_J100_20171018.bin",
            };

            trainingOutfile = new[]
            {
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RtModelTrainingData\DE6plex_1_1_1_CT1_IT30_NAQ444_J100_20171124.training.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RtModelTrainingData\DE6plex_1_1_2_CT1_IT30_NAQ444_J100_20171124.training.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RtModelTrainingData\DE6plex_1_1_3_CT1_IT30_NAQ444_J100_20171124.training.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RtModelTrainingData\DE6plex_1_1_4_CT1_IT40_NAQ444_J100_20171124.training.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RtModelTrainingData\DE6plex_mixture_CT1_200min_J100_NAQ444_20171116.training.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RtModelTrainingData\DE_1_1_1_NAQ1D_J100_20171018.training.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RtModelTrainingData\DE_1_1_2_NAQ1D_APD_J100_20171018.training.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RtModelTrainingData\DE_1_1_2_NAQ1D_J100_20171018.training.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RtModelTrainingData\DE_1_1_2_NAQ1D_J100_20171029.training.tsv",
                @"E:\MassSpec\PaperData\DE6_4hr_gradient\RtModelTrainingData\DE_1_1_3_5ug_NAQ1D_J100_20171018.training.tsv",
            };

            PipeLine.RunTraining(raws, tsvs, psmTmpOutFiles, "DE", tolernace, trainingOutfile, overwrite, writeXicClusters);
        }


        [Test]
        public void TestDE6LumosTraining()
        {
            var tolernace = new Tolerance(5);
            var overwrite = true;
            var writeXicClusters = false;
            string[] raws;
            string[] tsvs;
            string[] psmTmpOutFiles;
            string[] trainingOutfile;

            raws = new[]
            {
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_1_N_J100_NCP1_1228.raw",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_1_O_J100_NCP1_1229.raw",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_2_N_J100_NCP1_1228.raw",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_2_O_J100_NCP1_1230.raw",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_N_J100_NCP1_1228.raw",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_NCP1_J100_20171215.raw",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_NCP1_J100_20171218_1.raw",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_O_J100_NCP1_1228.raw",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_2_NCP1_J100_20171218.raw",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_2_NCP1_J100_20171218_2.raw",
            };

            tsvs = new[]
            {
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\SearchResults\DE6plex_1_1_1_1_N_J100_NCP1_1228.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\SearchResults\DE6plex_1_1_1_1_O_J100_NCP1_1229.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\SearchResults\DE6plex_1_1_1_2_N_J100_NCP1_1228.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\SearchResults\DE6plex_1_1_1_2_O_J100_NCP1_1230.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\SearchResults\DE6plex_1_1_1_N_J100_NCP1_1228.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\SearchResults\DE6plex_1_1_1_NCP1_J100_20171215.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\SearchResults\DE6plex_1_1_1_NCP1_J100_20171218_1.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\SearchResults\DE6plex_1_1_1_O_J100_NCP1_1228.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\SearchResults\DE6plex_1_1_2_NCP1_J100_20171218.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\SearchResults\DE6plex_1_1_2_NCP1_J100_20171218_2.tsv",
            };

            psmTmpOutFiles = new[]
            {
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_1_N_J100_NCP1_1228.bin",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_1_O_J100_NCP1_1229.bin",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_2_N_J100_NCP1_1228.bin",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_2_O_J100_NCP1_1230.bin",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_N_J100_NCP1_1228.bin",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_NCP1_J100_20171215.bin",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_NCP1_J100_20171218_1.bin",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_1_O_J100_NCP1_1228.bin",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_2_NCP1_J100_20171218.bin",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RawFiles\DE6plex_1_1_2_NCP1_J100_20171218_2.bin",
            };

            trainingOutfile = new[]
            {
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RtModelTrainingData\DE6plex_1_1_1_1_N_J100_NCP1_1228.training.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RtModelTrainingData\DE6plex_1_1_1_1_O_J100_NCP1_1229.training.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RtModelTrainingData\DE6plex_1_1_1_2_N_J100_NCP1_1228.training.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RtModelTrainingData\DE6plex_1_1_1_2_O_J100_NCP1_1230.training.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RtModelTrainingData\DE6plex_1_1_1_N_J100_NCP1_1228.training.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RtModelTrainingData\DE6plex_1_1_1_NCP1_J100_20171215.training.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RtModelTrainingData\DE6plex_1_1_1_NCP1_J100_20171218_1.training.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RtModelTrainingData\DE6plex_1_1_1_O_J100_NCP1_1228.training.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RtModelTrainingData\DE6plex_1_1_2_NCP1_J100_20171218.training.tsv",
                @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\RtTraining\RtModelTrainingData\DE6plex_1_1_2_NCP1_J100_20171218_2.training.tsv",
            };

            PipeLine.RunTraining(raws, tsvs, psmTmpOutFiles, "DE", tolernace, trainingOutfile, overwrite, writeXicClusters);
        }


        [Test]
        public void TestDE6QeTraining()
        {
            var tolernace = new Tolerance(5);
            var overwrite = true;
            var writeXicClusters = false;
            string[] raws;
            string[] tsvs;
            string[] psmTmpOutFiles;
            string[] trainingOutfile;

            raws = new[]
            {
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RawFiles\DE_1_1_NAQ448C2_J100_0825.raw",
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RawFiles\DE_12_1_NAQ448C2_J100_0825.raw",
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RawFiles\DE_1_1_NAQ448C1_J100_0913.raw",
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RawFiles\DE_1_1_2_NAQ448C1_J100_0913.raw",
            };

            tsvs = new[]
            {
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\SearchResults\DE_1_1_NAQ448C2_J100_0825.tsv",
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\SearchResults\DE_12_1_NAQ448C2_J100_0825.tsv",
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\SearchResults\DE_1_1_NAQ448C1_J100_0913.tsv",
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\SearchResults\DE_1_1_2_NAQ448C1_J100_0913.tsv",
            };

            psmTmpOutFiles = new[]
            {
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RawFiles\DE_1_1_NAQ448C2_J100_0825.bin",
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RawFiles\DE_12_1_NAQ448C2_J100_0825.bin",
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RawFiles\DE_1_1_NAQ448C1_J100_0913.bin",
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RawFiles\DE_1_1_2_NAQ448C1_J100_0913.bin",
            };

            trainingOutfile = new[]
            {
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RtModelTrainingData\DE_1_1_NAQ448C2_J100_0825.training.tsv",
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RtModelTrainingData\DE_12_1_NAQ448C2_J100_0825.training.tsv",
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RtModelTrainingData\DE_1_1_NAQ448C1_J100_0913.training.tsv",
                @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\RtModelTrainingData\DE_1_1_2_NAQ448C1_J100_0913.training.tsv",
            };

            PipeLine.RunTraining(raws, tsvs, psmTmpOutFiles, "DE", tolernace, trainingOutfile, overwrite, writeXicClusters);
        }

        [Test]
        public void TestNterminomics()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index
            string[][][] tsvs;
            string[][][] psmTmpOutFiles;
            var fasta = @"E:\MassSpec\PaperData\DE3_LysC_QE\hsa_UP000005640_cRAP_added.revCat.fasta";
            var svrPaths = new[]
            {
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_Lumos_WatersLC_4hrG.standardized.s4t2g1c1.model",
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_Lumos_WatersLC_4hrG.standard"
            };
            var overwrite = false;
            var writeExcel = true;
            var mfileDir = @"E:\MassSpec\NtermSunny\mFiles\";
            var quantfileDir = @"E:\MassSpec\NtermSunny\quantFiles\";

            raws = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\RawFiles\Control_1_J100_NAQ448C1_0325.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\RawFiles\Control_2_J100_NAQ448C1_0325.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\RawFiles\Control_3_J100_NAQ448C1_0325.raw"
                    }
                },
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\RawFiles\Naa40_1_J100_NAQ448C1_0325.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\RawFiles\Naa40_2_J100_NAQ448C1_0325.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\RawFiles\Naa40_3_J100_NAQ448C1_0325.raw"
                    }
                }
            };

            tsvs = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\SearchResults\Control_1_J100_NAQ448C1_0325.tsv"
                    },
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\SearchResults\Control_2_J100_NAQ448C1_0325.tsv"
                    },
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\SearchResults\Control_3_J100_NAQ448C1_0325.tsv"
                    }
                },
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\SearchResults\Naa40_1_J100_NAQ448C1_0325.tsv"
                    },
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\SearchResults\Naa40_2_J100_NAQ448C1_0325.tsv"
                    },
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\SearchResults\Naa40_3_J100_NAQ448C1_0325.tsv"
                    }
                }
            };

            psmTmpOutFiles = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\TmpResults\Control_1_J100_NAQ448C1_0325.bin"
                    },
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\TmpResults\Control_2_J100_NAQ448C1_0325.bin"
                    },
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\TmpResults\Control_3_J100_NAQ448C1_0325.bin"
                    }
                },
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\TmpResults\Naa40_1_J100_NAQ448C1_0325.bin"
                    },
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\TmpResults\Naa40_2_J100_NAQ448C1_0325.bin"
                    },
                    new[]
                    {
                        @"E:\MassSpec\NtermSunny\TmpResults\Naa40_3_J100_NAQ448C1_0325.bin"
                    }
                }
            };

            string[] labelingString =
            {
                "^ 0_42.011 0_45.029",
                "K 0_45.029 0_45.029"
            };
            PipeLine.Run(raws, tsvs, psmTmpOutFiles, labelingString, svrPaths, fasta, tolernace, mfileDir, quantfileDir,
                overwrite, writeExcel);
        }

        [Test]
        public void TestZebra()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index
            string[][][] tsvs;
            string[][][] psmTmpOutFiles;
            var fasta = @"";
            var svrPaths = new[]
            {
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_Lumos_WatersLC_4hrG.standardized.s4t2g1c1.model",
                @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_Lumos_WatersLC_4hrG.standard"
            };
            var overwrite = true;
            var writeExcel = false;
            var mfileDir = @"E:\MassSpec\PaperData\Zebra\mFiles\";
            var quantfileDir = @"E:\MassSpec\PaperData\Zebra\quantFiles\";

            raws = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\Zebra\RawFiles\Zebrafish_only_light_3ug_J100_NAQ448_0510.raw"
                    }
                }
                /*new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\Zebra\RawFiles\ZF_SDT_NCP1_J100_0429.raw"
                    },
                },
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\Zebra\RawFiles\ZF_urea_NCP1_J100_0429.raw"
                    },
                }*/
            };

            tsvs = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\Zebra\SearchResults\Zebrafish_only_light_3ug_J100_NAQ448_0510.tsv"
                    }
                }
                /*new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\Zebra\SearchResults\ZF_SDT_NCP1_J100_0429.tsv"
                    },
                },
                 new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\Zebra\SearchResults\ZF_urea_NCP1_J100_0429.tsv"
                    },
                }*/
            };

            psmTmpOutFiles = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\Zebra\TmpResults\Zebrafish_only_light_3ug_J100_NAQ448_0510.bin"
                        // E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_1_1\Result\DE_1_1_1_NCP1C2_J100_0519_01.bin"
                    }
                }
                /*new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\Zebra\Result\ZF_SDT_NCP1_J100_0429.bin"
                        // E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_1_1\Result\DE_1_1_1_NCP1C2_J100_0519_01.bin"
                    }
                },
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\Zebra\Result\ZF_urea_NCP1_J100_0429.bin"
                        // E:\MassSpec\PaperData\DE3_LysC_QE\DE3_1_1_1\Result\DE_1_1_1_NCP1C2_J100_0519_01.bin"
                    }
                }
                */
            };

            var labelingString = new[]
            {
                "^|K 0_56.06260"
            };

            PipeLine.Run(raws, tsvs, psmTmpOutFiles, labelingString, svrPaths, fasta, tolernace, mfileDir, quantfileDir,
                overwrite, writeExcel);
        }

        [Test]
        public void TestZebraSimple()
        {
            var tolernace = new Tolerance(5);
            string[][][] raws; // condition index, replicate index, fractionation index
            string[][][] tsvs;
            string[][][] psmTmpOutFiles;
            var fasta =
                @"E:\MassSpec\PaperData\20170614_ZF_label_mixed_Lumos\danRer_UP000000437_cRAP_added.revCat.fasta";
            var svrPaths = new[]
            {
                @"E:\MassSpec\PaperData\SVM\D-shifts_libSVMData_UniquePepSampled_WatersLC2hr_Lumos_ForPaper.standardized.s4t2g1c1.model",
                @"E:\MassSpec\PaperData\SVM\D-shifts_libSVMData_UniquePepSampled_WatersLC2hr_Lumos_ForPaper.standard"
            };

            var overwrite = false;
            var writeExcel = true;
            var writeXicClusters = false;
            var mfileDir = @"E:\MassSpec\PaperData\20170614_ZF_label_mixed_Lumos\mFiles\";
            var quantfileDir = @"E:\MassSpec\PaperData\20170614_ZF_label_mixed_Lumos\quantFiles\";

            raws = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170614_ZF_label_mixed_Lumos\RawFiles\ZF_Label_mix_1_2ug_J100_NAQ448_0614_C1.raw"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170614_ZF_label_mixed_Lumos\RawFiles\ZF_Label_mix_2_2ug_J100_NAQ448_0614_C1.raw"
                    }
                }
            };

            tsvs = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170614_ZF_label_mixed_Lumos\SearchResults\ZF_Label_mix_1_2ug_J100_NAQ448_0614_C1.tsv"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170614_ZF_label_mixed_Lumos\SearchResults\ZF_Label_mix_2_2ug_J100_NAQ448_0614_C1.tsv"
                    }
                }
            };

            psmTmpOutFiles = new[]
            {
                new[]
                {
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170614_ZF_label_mixed_Lumos\TmpResults\ZF_Label_mix_1_2ug_J100_NAQ448_0614_C1.bin"
                    },
                    new[]
                    {
                        @"E:\MassSpec\PaperData\20170614_ZF_label_mixed_Lumos\TmpResults\ZF_Label_mix_2_2ug_J100_NAQ448_0614_C1.bin"
                    }
                }
            };

            PipeLine.Run(raws, tsvs, psmTmpOutFiles, @"DE", svrPaths, fasta, tolernace, mfileDir, quantfileDir,
                overwrite, writeExcel);
        }


    }
}