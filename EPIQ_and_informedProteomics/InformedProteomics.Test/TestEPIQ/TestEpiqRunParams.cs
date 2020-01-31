using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Accord;
using Epiq;
using InformedProteomics.Backend.Data.Spectrometry;
using NUnit.Framework;

namespace InformedProteomics.Test.TestEpiq
{
    [TestFixture]
    internal class TestEpiqRunParams
    {
        [Test]
        public void run_paperdata_1to1_vsMQ()
        {
            var pathL0L2L4 = @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\DE6_1to1_QE_ThermoLC_vsMQ\EPIQL0L2L4\EPIQ_run_params_L0L2L4.tsv";
            new RunParams(pathL0L2L4).Run();

            var pathL1L3L5 = @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\DE6_1to1_QE_ThermoLC_vsMQ\EPIQL1L3L5\EPIQ_run_params_L1L3L5.tsv";
            new RunParams(pathL1L3L5).Run();
        }


        [Test]
        public void run_SILAC_K14()
        {
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.SILAC_K14Envs);
            Params.UseLabelingEfficiencyCorrection = false;

            var path = @"D:\p\tiny\SILAC_K14\1to1QuantResults\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }


        [Test]
        public void run_paperdata_SILAC6plex_noRT()
        {
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.SILAC6EnvsPerLabelSite);
            Params.UseLabelingEfficiencyCorrection = true;

            var path = @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\QuantResults_noRT_RTdelta1over100\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }

        [Test]
        public void run_paperdata_1toR_usedInPaperOnly()
        {
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.DE6QeEnvsPerLabelSite);
            var path = @"E:\MassSpec\PaperData\DE6_1toR_usedInPaperOnly\QuantResults\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }

        [Test]
        public void run_paperdata_SILAC6plex()
        {
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.SILAC6EnvsPerLabelSite);
            Params.UseLabelingEfficiencyCorrection = true;

            var path = @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\QuantResults_isoforms\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }


        [Test]
        public void run_paperdata_DE6plex_singlechannel()
        {
            var path = @"E:\MassSpec\RevisionData\R1Point3-NumberOfId\DE6\SingleChannel\QuantResults_Channel2\EPIQ_run_params.txt";
            new RunParams(path).Run();

            path = @"E:\MassSpec\RevisionData\R1Point3-NumberOfId\DE6\SingleChannel\QuantResults_Channel3\EPIQ_run_params.txt";
            new RunParams(path).Run();

            path = @"E:\MassSpec\RevisionData\R1Point3-NumberOfId\DE6\SingleChannel\QuantResults_Channel4\EPIQ_run_params.txt";
            new RunParams(path).Run();

            path = @"E:\MassSpec\RevisionData\R1Point3-NumberOfId\DE6\SingleChannel\QuantResults_Channel5\EPIQ_run_params.txt";
            new RunParams(path).Run();

            path = @"E:\MassSpec\RevisionData\R1Point3-NumberOfId\DE6\SingleChannel\QuantResults_Channel6\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }

        [Test]
        public void run_paperdata_SILAC6plex_singlechannel()
        {
            var path = @"E:\MassSpec\RevisionData\R1Point3-NumberOfId\SILAC6\SingleChannel\QuantResults_Channel2\EPIQ_run_params.txt";
            new RunParams(path).Run();

            path = @"E:\MassSpec\RevisionData\R1Point3-NumberOfId\SILAC6\SingleChannel\QuantResults_Channel3\EPIQ_run_params.txt";
            new RunParams(path).Run();

            path = @"E:\MassSpec\RevisionData\R1Point3-NumberOfId\SILAC6\SingleChannel\QuantResults_Channel4\EPIQ_run_params.txt";
            new RunParams(path).Run();

            path = @"E:\MassSpec\RevisionData\R1Point3-NumberOfId\SILAC6\SingleChannel\QuantResults_Channel5\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }

        [Test]
        public void run_paperdata_SILAC6_heatShock()
        {
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.SILAC6EnvsPerLabelSite);

            //var path = @"E:\MassSpec\PaperData\HeatShock_TotalProtein\QuantResults\EPIQ_run_params.txt";
            //var path = @"E:\MassSpec\PaperData\HeatShock_Pulse\QuantResults\EPIQ_run_params.txt";
            var path = @"E:\MassSpec\PaperData\HeatShock_TotalProtein\QuantResults_Phospho_all\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }


        [Test]
        public void run_paperdata_SILAC6_nanoACQUITY()
        {
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.SILAC6EnvsPerLabelSite);

            var path = @"E:\MassSpec\PaperData\SILAC6_HDR\QuantResults_trained\EPIQ_run_params.txt";
            new RunParams(path).Run();

            path = @"E:\MassSpec\PaperData\SILAC6_HDR\QuantResults_wideII_trained\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }

        [Test]
        public void run_paperdata_SILAC6_HILIC_HDR()
        {
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.SILAC6EnvsPerLabelSite);

            //var path = @"E:\MassSpec\PaperData\SILAC6_HILIC_HDR\QuantResults_isoformDB\EPIQ_run_params.txt";

            var path = @"E:\MassSpec\PaperData\SILAC6_HILIC32_HDR\QuantResults\EPIQ_run_params.txt";
            new RunParams(path).Run();

        }

        [Test]
        public void run_paperdata_FractionNumberTest()
        {
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.SILAC6EnvsPerLabelSite);
            //for (int i = 32; i > 0; i--)
            int i = 22;
            {
                var path = String.Format(@"C:\Scratch\SILAC6_HILIC32\QuantResults_byFraction\QuantResults_Fr1-{0}\EPIQ_run_params.txt", i, i);
                Console.WriteLine(path);
                Console.WriteLine();
                new RunParams(path).Run(overwrite:false);
            }
            //for (int i = 32; i > 0; i--)
            i = 2;
            {
                var path = String.Format(@"C:\Scratch\SILAC6_HILIC32\QuantResults_byFraction\QuantResults_Fr1-{0}\EPIQ_run_params.txt", i, i);
                Console.WriteLine(path);
                Console.WriteLine();
                new RunParams(path).Run(overwrite:false);
            }

            /*
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.SILAC6EnvsPerLabelSite);
            for (int i = 24; i > 0; i--)
            {
                var path = String.Format(@"C:\Scratch\SILAC6_HILIC\QuantResults_byFraction\QuantResults_Fr1-{0}\EPIQ_run_params_Fr1-{1}.txt", i, i);
                Console.WriteLine(path);
                Console.WriteLine();
                new RunParams(path).Run(overwrite:false);
            }
             */

            /*
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.DE6LumosEnvsPerLabelSite);
            for (int i = 8; i > 0; i--)
            {
                var path = String.Format(@"C:\Scratch\DE6_SCX\QuantResults_byFraction\QuantResults_Fr1-{0}\EPIQ_run_params_Fr1-{1}.txt", i, i);
                Console.WriteLine(path);
                Console.WriteLine();
                new RunParams(path).Run(overwrite:false);
            }
             */
        }


        [Test]
        public void run_SILAC3_HILIC_test()
        {
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.SILAC6EnvsPerLabelSite);

            var path = @"E:\MassSpec\HILIC test\QuantResults\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }


        [Test]
        public void run_paperdata_SILAC6plex_NoCorrection()
        {
            IsotopeImpurityValues.SetDefaultImpurityValues();
            Params.UseLabelingEfficiencyCorrection = false;

            var path = @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\QuantResults_finalRtModel_NoII_NoIncorporationRate\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }

        [Test]
        public void run_paperdata_SILAC6plex_NoLe()
        {
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.SILAC6EnvsPerLabelSite);
            Params.UseLabelingEfficiencyCorrection = false;

            var path = @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\QuantResults_finalRtModel_SILACII_NoIncorporationRate\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }


        [Test]
        public void run_paperdata_SILAC6plex_NoII()
        {
            IsotopeImpurityValues.SetDefaultImpurityValues();
            Params.UseLabelingEfficiencyCorrection = true;

            var path = @"E:\MassSpec\PaperData\SILAC6_Lumos_SILACLC\QuantResults_finalRtModel_NoII_WithIncorporationRate\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }

        [Test]
        public void run_revisiondata_DE6plex_singlechannel()
        {
            var path = @"E:\MassSpec\RevisionData\R1Point3-NumberOfId\DE6\SingleChannel\QuantResults_Channel1\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }

        [Test]
        public void run_revisiondata_DE6plex_singleshot()
        {
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.DE6LumosEnvsPerLabelSite);

            var path = @"E:\MassSpec\RevisionData\R1Point3-NumberOfId\DE6\6plexSingleShot\QuantResults\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }

        [Test]
        public void run_revisiondata_DE6plex_diff_gradient()
        {
            IsotopeImpurityValues.SetDefaultImpurityValues();
            var path = @"E:\MassSpec\RevisionData\R1Point4-2_diff_gradient\DE6\QuantResults\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }

        [Test]
        public void run_paperdata_1toR()
        {
            //IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.DE6QeEnvsPerLabelSite);
            //var path = @"E:\MassSpec\PaperData\DE6_1toR_QE_ThermoLC\QuantResults\EPIQ_run_params.txt";
            //new RunParams(path).Run();

            //IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.DE6QeEnvsPerLabelSite);
            //var path = @"E:\MassSpec\PaperData\DE6_2to1_12to1_15to1_QE_ThermoLC\QuantResults\EPIQ_run_params.txt";
            //new RunParams(path).Run();

            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.DE6QeEnvsPerLabelSite);
            var path = @"E:\MassSpec\PaperData\DE6_1toR_usedInPaperOnly\QuantResults_isoforms\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }

        [Test]
        public void run_paperdata_scx()
        {
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.DE6LumosEnvsPerLabelSite);

            var path = @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\QuantResults_isoforms\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }


        [Test]
        public void run_paperdata_scx_noRT()
        {
            IsotopeImpurityValues.SetHardCodedImpurityValues(IsotopeImpurityValues.DE6LumosEnvsPerLabelSite);

            var path = @"E:\MassSpec\PaperData\DE6_20_10_Lumos_ThermoLC\QuantResults_noRT_RTdelta1over100\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }


        [Test]
        public void run_paperdata_DM5()
        {
            var path = @"E:\MassSpec\PaperData\DM5_QE_oldLC\QuantResults\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }


        [Test]
        public void run_paperdata_DM3()
        {
            var path = @"E:\MassSpec\PaperData\DM3_byJJH\QuantResults\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }



        [Test]
        public void run_BP2plex()
        {
            var path = @"D:\p\tiny\BP2plex_2018\20180909\QuantResults_80_101\EPIQ_run_params.txt";
            new RunParams(path).Run();
        }


    }
}
