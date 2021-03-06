﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using InformedProteomics.TopDown.Execution;

namespace MSPathFinderT
{
    public class Program
    {
        public const string Name = "MSPathFinderT";
        public const string Version = "0.91 (Feb 11, 2015)";
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        private const uint EnableExtendedFlags = 0x0080;

        public static void Main(string[] args)
        {
            var handle = Process.GetCurrentProcess().MainWindowHandle;
            SetConsoleMode(handle, EnableExtendedFlags);

            if (args.Length%2 != 0)
            {
                PrintUsageInfo("The number of arguments must be even.");
                return;
            }

            // initialize parameters
            var paramDic = new Dictionary<string, string>
            {
                {"-s", null},
                {"-d", null},
                {"-o", null},
                {"-m", "1"},
                {"-mod", null},
                {"-t", "10"},
                {"-f", "10"},
                {"-tda", "0"},
                {"-minLength", "21"},
                {"-maxLength", "500"},
                {"-minCharge", "2"},
                {"-maxCharge", "50"},
                {"-minFragCharge", "1"},
                {"-maxFragCharge", "20"},
                {"-minMass", "3000.0"},
                {"-maxMass", "50000.0"},
                {"-feature", null},
                {"-minProb", "0.1"}
            };

            for (var i = 0; i < args.Length/2; i++)
            {
                var key = args[2*i];
                var value = args[2*i + 1];
                if (!paramDic.ContainsKey(key))
                {
                    PrintUsageInfo("Invalid parameter: " + key);
                    return;
                }
                paramDic[key] = value;
            }

            var parameters = new TopDownInputParameters();
            var message = parameters.Parse(paramDic);
            if (message != null)
            {
                PrintUsageInfo(message);
                return;
            }

            Console.WriteLine(Name + " " + Version);
            parameters.Display();
            parameters.Write();

            foreach (var specFilePath in parameters.SpecFilePaths)
            {
                var topDownLauncher = new IcTopDownLauncher(
                    specFilePath,
                    parameters.DatabaseFilePath,
                    parameters.OutputDir,
                    parameters.AminoAcidSet,
                    parameters.MinSequenceLength,
                    parameters.MaxSequenceLength,
                    1, // max number of N-term cleavages
                    0, // max number of C-term cleavages
                    parameters.MinPrecursorIonCharge,
                    parameters.MaxPrecursorIonCharge,
                    parameters.MinProductIonCharge,
                    parameters.MaxProductIonCharge,
                    parameters.MinSequenceMass,
                    parameters.MaxSequenceMass,
                    parameters.PrecursorIonTolerancePpm,
                    parameters.ProductIonTolerancePpm,
                    parameters.Tda,
                    parameters.SearchMode,
                    parameters.FeatureFilePath,
                    parameters.FeatureMinProbability
                    );
                topDownLauncher.RunSearch();
            }
        }


        private static void PrintUsageInfo(string message = null)
        {
            if (message != null) Console.WriteLine("Error: " + message);
            Console.WriteLine(
                Name + " " + Version + "\n" +
                "Usage: " + Name + ".exe\n" +
                "\t-s SpectrumFile (*.raw)\n" +
                "\t-d DatabaseFile (*.fasta or *.fa)\n" +
                "\t[-o OutputFolder]\n" +
                "\t[-m SearchMode] (0: multiple internal cleavages, 1: single internal cleavage (default), 2: no internal cleavage)\n" +
                "\t[-mod ModificationFileName] (modification file, default: no modification)\n" +
                "\t[-t PrecursorToleranceInPpm] (e.g. 10, Default: 10)\n" +
                "\t[-f FragmentIonToleranceInPpm] (e.g. 10, Default: 10)\n" +
                "\t[-tda 0/1] (0: don't search decoy database (default), 1: search shuffled decoy database)\n" +
                "\t[-minLength MinSequenceLength] (minimum sequence length, default: 21)\n" +
                "\t[-maxLength MaxSequenceLength] (maximum sequence length, default: 500)\n" +
                "\t[-minCharge MinPrecursorCharge] (minimum precursor ion charge, default: 2)\n" +
                "\t[-maxCharge MaxPrecursorCharge] (maximum precursor ion charge, default: 50)\n" +
                "\t[-minFragCharge MinPrecursorCharge] (minimum fragment ion charge, default: 1)\n" +
                "\t[-maxFragCharge MaxPrecursorCharge] (maximum fragment ion charge, default: 20)\n" +
                "\t[-minMass MinSequenceMassInDa] (minimum sequence mass in Da, default: 3000.0)\n" +
                "\t[-maxMass MaxSequenceMassInDa] (maximum sequence mass in Da, default: 50000.0)\n" +
                "\t[-feature FeatureFile] (*.ms1ft, *_isos.csv, or *.msalign, default: Run ProMex)\n"
                );
        }

    }
}
