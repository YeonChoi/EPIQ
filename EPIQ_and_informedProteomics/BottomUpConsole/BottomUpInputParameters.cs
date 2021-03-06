﻿using System;
using System.Collections.Generic;
using System.IO;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Sequence;

namespace MSPathFinder
{
    public class BottomUpInputParameters
    {
        public const string ParameterFileExtension = ".param";

        public IEnumerable<string> SpecFilePaths { get; set; }
        public string DatabaseFilePath { get; set; }
        public string OutputDir { get; set; }
        public AminoAcidSet AminoAcidSet { get; set; }
        public Enzyme Enzyme { get; set; }
        public int NumTolerableTermini { get; set; }
        public bool Tda { get; set; }
        public double PrecursorIonTolerancePpm { get; set; }
        public double ProductIonTolerancePpm { get; set; }
        public int MinSequenceLength { get; set; }
        public int MaxSequenceLength { get; set; }
        public int MinPrecursorIonCharge { get; set; }
        public int MaxPrecursorIonCharge { get; set; }
        public int MinProductIonCharge { get; set; }
        public int MaxProductIonCharge { get; set; }

        private IEnumerable<SearchModification> _searchModifications;
        private int _maxNumDynModsPerSequence;

        public void Display()
        {
            Console.WriteLine("SpecFilePath: ");
            foreach(var specFilePath in SpecFilePaths) Console.WriteLine("\t{0}", specFilePath);
            Console.WriteLine("DatabaseFilePath: " + DatabaseFilePath);
            Console.WriteLine("OutputDir: " + OutputDir);
            Console.WriteLine("Enzyme: " + Enzyme.Name);
            Console.WriteLine("NumTolerableTermini: " + NumTolerableTermini);
            Console.WriteLine("Tda: " + Tda);
            Console.WriteLine("PrecursorIonTolerancePpm: " + PrecursorIonTolerancePpm);
            Console.WriteLine("ProductIonTolerancePpm: " + ProductIonTolerancePpm);
            Console.WriteLine("MinSequenceLength: " + MinSequenceLength);
            Console.WriteLine("MaxSequenceLength: " + MaxSequenceLength);
            Console.WriteLine("MinPrecursorIonCharge: " + MinPrecursorIonCharge);
            Console.WriteLine("MaxPrecursorIonCharge: " + MaxPrecursorIonCharge);
            Console.WriteLine("MinProductIonCharge: " + MinProductIonCharge);
            Console.WriteLine("MaxProductIonCharge: " + MaxProductIonCharge);
            Console.WriteLine("MaxDynamicModificationsPerSequence: " + _maxNumDynModsPerSequence);
            Console.WriteLine("Modifications: ");
            foreach (var searchMod in _searchModifications)
            {
                Console.WriteLine(searchMod);
            }
        }

        public void Write()
        {
            foreach (var specFilePath in SpecFilePaths)
            {
                var outputFilePath = OutputDir + Path.DirectorySeparatorChar +
                                           Path.GetFileNameWithoutExtension(specFilePath) + ParameterFileExtension;

                using (var writer = new StreamWriter(outputFilePath))
                {
                    writer.WriteLine("SpecFile\t" + Path.GetFileName(specFilePath));
                    writer.WriteLine("DatabaseFile\t" + Path.GetFileName(DatabaseFilePath));
                    writer.WriteLine("Enzyme\t" + Enzyme.Name);
                    writer.WriteLine("NumTolerableTermini\t" + NumTolerableTermini);
                    writer.WriteLine("Tda\t" + Tda);
                    writer.WriteLine("PrecursorIonTolerancePpm\t" + PrecursorIonTolerancePpm);
                    writer.WriteLine("ProductIonTolerancePpm\t" + ProductIonTolerancePpm);
                    writer.WriteLine("MinSequenceLength\t" + MinSequenceLength);
                    writer.WriteLine("MaxSequenceLength\t" + MaxSequenceLength);
                    writer.WriteLine("MinPrecursorIonCharge\t" + MinPrecursorIonCharge);
                    writer.WriteLine("MaxPrecursorIonCharge\t" + MaxPrecursorIonCharge);
                    writer.WriteLine("MinProductIonCharge\t" + MinProductIonCharge);
                    writer.WriteLine("MaxProductIonCharge\t" + MaxProductIonCharge);
                    writer.WriteLine("MaxDynamicModificationsPerSequence\t" + _maxNumDynModsPerSequence);
                    foreach (var searchMod in _searchModifications)
                    {
                        writer.WriteLine("Modification\t" + searchMod);
                    }
                }                
            }
        }

        public string Parse(Dictionary<string, string> parameters)
        {
            var message = CheckIsValid(parameters);
            if (message != null) return message;

            var specFilePath = parameters["-s"];
            if (Directory.Exists(specFilePath)) // Directory
            {
                SpecFilePaths = Directory.GetFiles(specFilePath, "*.raw");
            }
            else
            {
                SpecFilePaths = new[] {specFilePath};
            }

            DatabaseFilePath = parameters["-d"];

            var outputDir = parameters["-o"] ?? Environment.CurrentDirectory;
            if (outputDir[outputDir.Length - 1] == Path.DirectorySeparatorChar) outputDir = outputDir.Remove(outputDir.Length - 1);
            if (!Directory.Exists(outputDir))
            {
                if (File.Exists(outputDir) && !File.GetAttributes(outputDir).HasFlag(FileAttributes.Directory))
                {
                    return "OutputDir " + outputDir + " is not a directory!";
                }
                Directory.CreateDirectory(outputDir);
            }
            OutputDir = outputDir;

            var modFilePath = parameters["-mod"];
            if (modFilePath != null)
            {
                var parser = new ModFileParser(modFilePath);
                _searchModifications = parser.SearchModifications;
                _maxNumDynModsPerSequence = parser.MaxNumDynModsPerSequence;

                if (_searchModifications == null) return "Error while parsing " + modFilePath + "!";

                AminoAcidSet = new AminoAcidSet(_searchModifications, _maxNumDynModsPerSequence);
            }
            else
            {
                AminoAcidSet = new AminoAcidSet();
                _searchModifications = new SearchModification[0];
            }

            var enzymeId = Convert.ToInt32(parameters["-e"]);
            Enzyme enzyme;
            switch (enzymeId)
            {
                case 0:
                    enzyme = Enzyme.UnspecificCleavage;
                    break;
                case 1:
                    enzyme = Enzyme.Trypsin;
                    break;
                case 2:
                    enzyme = Enzyme.Chymotrypsin;
                    break;
                case 3:
                    enzyme = Enzyme.LysC;
                    break;
                case 4:
                    enzyme = Enzyme.LysN;
                    break;
                case 5:
                    enzyme = Enzyme.GluC;
                    break;
                case 6:
                    enzyme = Enzyme.ArgC;
                    break;
                case 7:
                    enzyme = Enzyme.AspN;
                    break;
                case 8:
                    enzyme = Enzyme.Alp;
                    break;
                case 9:
                    enzyme = Enzyme.NoCleavage;
                    break;
                default:
                    return "Invalid enzyme ID (" + enzymeId + ") for parameter -e";
            }
            Enzyme = enzyme;

            NumTolerableTermini = Convert.ToInt32(parameters["-ntt"]);
            if (NumTolerableTermini < 0 || NumTolerableTermini > 2)
            {
                return "Invalid value (" + NumTolerableTermini + ") for parameter -m";
            }

            PrecursorIonTolerancePpm = Convert.ToDouble(parameters["-t"]);
            ProductIonTolerancePpm = Convert.ToDouble(parameters["-f"]);

            var tdaVal = Convert.ToInt32(parameters["-tda"]);
            if (tdaVal != 0 && tdaVal != 1)
            {
                return "Invalid value (" + tdaVal + ") for parameter -tda";
            }
            Tda = (tdaVal == 1);

            MinSequenceLength = Convert.ToInt32(parameters["-minLength"]);
            MaxSequenceLength = Convert.ToInt32(parameters["-maxLength"]);
            if (MinSequenceLength > MaxSequenceLength)
            {
                return "MinSequenceLength (" + MinSequenceLength + ") is larger than MaxSequenceLength (" + MaxSequenceLength + ")!";
            }

            MinPrecursorIonCharge = Convert.ToInt32(parameters["-minCharge"]);
            MaxPrecursorIonCharge = Convert.ToInt32(parameters["-maxCharge"]);
            if (MinSequenceLength > MaxSequenceLength)
            {
                return "MinPrecursorCharge (" + MinPrecursorIonCharge + ") is larger than MaxPrecursorCharge (" + MaxPrecursorIonCharge + ")!";
            }

            MinProductIonCharge = Convert.ToInt32(parameters["-minFragCharge"]);
            MaxProductIonCharge = Convert.ToInt32(parameters["-maxFragCharge"]);
            if (MinSequenceLength > MaxSequenceLength)
            {
                return "MinFragmentCharge (" + MinProductIonCharge + ") is larger than MaxFragmentCharge (" + MaxProductIonCharge + ")!";
            }

            return null;
        }

        static string CheckIsValid(Dictionary<string, string> parameters)
        {
            foreach (var keyValuePair in parameters)
            {
                var key = keyValuePair.Key;
                var value = keyValuePair.Value;
                if (keyValuePair.Value == null && keyValuePair.Key != "-mod" && keyValuePair.Key != "-o")
                {
                    return "Missing required parameter " + key + "!";
                }

                if (key.Equals("-s"))
                {
                    if (value == null)
                    {
                        return "Missing parameter " + key + "!";
                    }
                    if (!File.Exists(value) && !Directory.Exists(value))
                    {
                        return "File not found: " + value + "!";
                    }
                    if (Directory.Exists(value)) continue;
                    var extension = Path.GetExtension(value);
                    if (!Path.GetExtension(value).ToLower().Equals(".raw"))
                    {
                        return "Invalid extension for the parameter " + key + " (" + extension + ")!";
                    }
                }
                else if (key.Equals("-d"))
                {
                    if (value == null)
                    {
                        return "Missing parameter " + key + "!";
                    }
                    if (!File.Exists(value))
                    {
                        return "File not found." + value + "!"; 
                    }
                    var extension = Path.GetExtension(value).ToLower();
                    if (!extension.Equals(".fa") && !extension.Equals(".fasta"))
                    {
                        return "Invalid extension for the parameter " + key + " (" + extension + ")!";
                    }
                }
                else if (key.Equals("-o"))
                {

                }
                else if (key.Equals("-mod"))
                {
                    if (value != null && !File.Exists(value))
                    {
                        return "File not found." + value + "!";
                    }
                }
                else
                {
                    double num;
                    if (!double.TryParse(value, out num))
                    {
                        return "Invalid value (" + value + ") for the parameter " + key + "!";
                    }
                }
            }

            return null;
        }
    }
}
