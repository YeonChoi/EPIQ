using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Composition;
using InformedProteomics.Backend.Data.Sequence;

namespace Epiq
{
    [Serializable]
    public class Ms2Result
    {
        /* Search Result Headers
         * #SpecFile	SpecID	ScanNum	Title	FragMethod	Precursor	IsotopeError	
         * PrecursorError(ppm)	Charge	Peptide	Proteins	DeNovoScore	MSGFScore	SpecEValuEValue	QValue	PepQValue */

        //Static Values
        public const string SpecFileHeader = "#SpecFile";
        public const string PeptideHeader = "Peptide";
        public const string ChargeHeader = "Charge";
        public const string IsotopeIdxHeader = "IsotopeError";
        public const string QvalueHeader = "QValue";
        public const string ProteinHeader = "Protein";
        public const string DeNovoScoreHeader = "DeNovoScore";
        public const string MSGFScoreHeader = "MSGFScore";
        public const string SpecEValueHeader = "SpecEValue";
        public const string EValueHeader = "EValue";
        public const string PepQValueHeader = "PepQValue";
        public const string PrecursorHeader = "Precursor";
        public const string PrecursorMzErrorHeader = "PrecursorError(ppm)";

        public const string DecoyStartsWith = "XXX_";
        public const string ContamStartsWith = "CCC_";

        //  private static string LabelRegexStr;
        // private static Regex LabelRegex;
        private const string ModRegexStr = @"\+\d*\.\d*";
        private static readonly Regex ModRegex = new Regex(ModRegexStr);
        private const string PrePostRegexStr = @"\(pre=[-ACDEFGHIKLMNPQRSTUVWYX],post=[-ACDEFGHIKLMNPQRSTUVWYX]\)";
        public readonly LabelList LabelListOfId;


        public Ms2Result(SearchResults resultParser, int scannum, int idx, int idIndex)
        {
            if (!resultParser.ScanNumExists(scannum)) return;
            Idindex = idIndex;
            ScanNum = scannum;
            Peptide = "";
            Qvalue = -1;
            SpecEValue = -1;
            Charge = -1;
            IsotopeIdx = -1;
            LabelIndex = -1;
            Precursor = -1;
            PrecursorMzError = -0;

            ReadResult(resultParser, scannum, idx);
            LabelIndex = LabelList.GetLabelIndex(this);
            //Console.WriteLine(LabelIndex + " " + Peptide);
            //CountNumofLabels();
            if (LabelIndex >= 0)
            {
                LabelListOfId = new LabelList(this);
                UnlabeledPeptide = LabelListOfId.UnlabeledPeptide;
                //UnlabeledNoIlePeptide = UnlabeledPeptide.Replace('I', 'L');
                UnmodifiedPeptide = GetUnmodifiedPeptide();
                NumOfBoundLabels = LabelListOfId.NumOfBoundLabels;
            }
        }

        //   private static readonly Regex LysModRegex = new Regex(LysModRegexStr);


//        static Ms2Result()
        //       {
        //          SetLabelRegex();
        //     }
        /*static void SetLabelRegex()
        {
            string labelSite = Params.LabelSite;
            //LabelRegexStr = String.Format(@"({0})({1})", labelSite, @"\+\d+\.\d+"); //TODO Should be changed to specific modification mass
            LabelRegexStr = @"(^|K)(\+\d*\.\d*)";
            Console.WriteLine(LabelRegexStr);
            LabelRegex = new Regex(LabelRegexStr);
        }*/

        //Instance Values
        //public short DeNovoScore { get; private set; }
        //public short MSGFScore { get; private set; }
        public int Idindex { get; private set; }
        public float SpecEValue { get; private set; }
        //public float EValue { get; private set; }
        public float PepQValue { get; private set; }

        public string SpecFile { get; private set; }
        public int ScanNum { get; private set; }
        public string Peptide { get; private set; }
        public string UnlabeledPeptide { get; private set; }
       // public string UnlabeledNoIlePeptide { get; private set; }
        public string UnmodifiedPeptide { get; private set; }
        public string[] Proteins { get; private set; }
        public float Qvalue { get; private set; }
        public sbyte Charge { get; private set; }
        public sbyte IsotopeIdx { get; private set; }
        public sbyte LabelIndex { get; private set; }
        public float Precursor { get; private set; }
        public float PrecursorMzError { get; private set; }
        public sbyte NumOfBoundLabels { get; private set; }


        public bool QvalueCutoffPassed(double qvalCutoff)
        {
            return Qvalue <= qvalCutoff;
        }

        public bool ChannelIdentified()
        {
            return LabelIndex != -1;
        }


        private void ReadResult(SearchResults resultParser, int scannum, int idx)
        {
            SpecFile = resultParser.GetNstSearchDataOfScanNum(scannum, idx, SpecFileHeader);
            Peptide = resultParser.GetNstSearchDataOfScanNum(scannum, idx, PeptideHeader);
            // DeNovoScore = Convert.ToInt16(resultParser.GetNstSearchDataOfScanNum(scannum, idx, DeNovoScoreHeader));
            //MSGFScore = Convert.ToInt16(resultParser.GetNstSearchDataOfScanNum(scannum, idx, MSGFScoreHeader));
            SpecEValue = Convert.ToSingle(resultParser.GetNstSearchDataOfScanNum(scannum, idx, SpecEValueHeader));
            // EValue = Convert.ToSingle(resultParser.GetNstSearchDataOfScanNum(scannum, idx, EValueHeader));
            PepQValue = Convert.ToSingle(resultParser.GetNstSearchDataOfScanNum(scannum, idx, PepQValueHeader));

            Proteins =
                Regex.Replace(resultParser.GetNstSearchDataOfScanNum(scannum, idx, ProteinHeader), PrePostRegexStr, "")
                    .Split(';');
            Qvalue = Convert.ToSingle(resultParser.GetNstSearchDataOfScanNum(scannum, idx, QvalueHeader));
            Charge = Convert.ToSByte(resultParser.GetNstSearchDataOfScanNum(scannum, idx, ChargeHeader));
            IsotopeIdx = Convert.ToSByte(resultParser.GetNstSearchDataOfScanNum(scannum, idx, IsotopeIdxHeader));
            Precursor = Convert.ToSingle(resultParser.GetNstSearchDataOfScanNum(scannum, idx, PrecursorHeader));
            PrecursorMzError = Convert.ToSingle(resultParser.GetNstSearchDataOfScanNum(scannum, idx, PrecursorMzErrorHeader));

            if (IsotopeIdx < 0)
            {
                Precursor -= (float) Constants.C13MinusC12 * IsotopeIdx / Charge;
                IsotopeIdx = 0;
            }
        }


        /*
        private void CountNumofLabels()
        {
            NumOfBoundLabels = (sbyte)Regex.Matches(Peptide, LabelRegexStr).NumOfBoundLabels;
        }
        */

        public double GetUnlabeledMass()
        {
            var mass = new Sequence(UnmodifiedPeptide, Params.AminoAcidSet).Mass + Composition.H2O.Mass;
            foreach (var m in GetModStrings())
            {
                mass += double.Parse(m);
            }
            return mass;
        }

        public List<string> GetModStrings()
        {
            var mods = new List<string>();
            foreach (var m in ModRegex.Matches(UnlabeledPeptide))
            {
                mods.Add(m.ToString());    
            }
            return mods;
        }


        public float GetMassDifference(sbyte fromCn, sbyte toCn)
        {
            return LabelListOfId.GetMassDifference(fromCn, toCn);
        }

        public int GetDetCount(sbyte cn)
        {
            return LabelListOfId.DetCounts[cn];
        }

        public IsotopomerEnvelope GetImpurityIsotopomerEnvelope(sbyte cn)
        {
            /*------------------print values for debuging-----------------
            Console.WriteLine("For Peptide {0}, Channel {1}", Peptide, cn);
            var env = LabelListOfId.ImpurityIsotopomerEnvelopes[cn];

            foreach (var val in env.Envelope)
            {
                Console.Write(@"{0:00.000}   ", val * 100);
            }
            Console.WriteLine();
            ------------------print values for debuging-----------------*/

            return LabelListOfId.ImpurityIsotopomerEnvelopes[cn];
        }

        public void PrintValues()
        {
            Console.WriteLine(@"{0}: {1}", @"SpecFile", SpecFile);
            Console.WriteLine(@"{0}: {1}", @"ScanNum", ScanNum);
            Console.WriteLine(@"{0}: {1}", @"Peptide", Peptide);
            Console.WriteLine(@"{0}: {1}", @"UnmodPeptide", UnmodifiedPeptide);
            // Console.WriteLine(@"{0}: {1}", @"NumOfBoundLabels", NumOfBoundLabels);
            Console.WriteLine(@"{0}: {1}", @"Charge", Charge);
            Console.WriteLine(@"{0}: {1}", @"Isotope Idx", IsotopeIdx);
            Console.WriteLine(@"{0}: {1}", @"Channel#", LabelIndex);
            Console.WriteLine(@"{0}: {1}", @"Qvalue", Qvalue);
            Console.WriteLine(@"{0}: {1}", @"Precursor", Precursor);
        }

        public bool IsContam()
        {
            //var decoyContam = DecoyStartsWith + ContamStartsWith;
            foreach (var protein in Proteins)
                if (protein.StartsWith(ContamStartsWith)) return true;
            return false;
        }

        public bool IsDecoy()
        {
            foreach (var protein in Proteins)
                if (protein.StartsWith(DecoyStartsWith)) return true;
            return false;
        }

        private string GetUnmodifiedPeptide()
        {
            return Regex.Replace(UnlabeledPeptide, ModRegexStr, "");
        }

        public bool HaveSameChargeIsotopeChannelNum(Ms2Result other)
        {
            return (Charge == other.Charge) && (IsotopeIdx == other.IsotopeIdx) && (LabelIndex == other.LabelIndex);
        }

        /*public bool AreInTheSamePrecusor(Ms2Result other)
        {
            return (other.Charge == Charge) && (other.UnlabeledNoIlePeptide == UnlabeledNoIlePeptide);
        }*/

        public bool AreUnmodifiedPeptidesTheSameExceptIle(Ms2Result other)
        {
            //Isoleucine - Leucine difference is ignored
            return GetUnmodifiedPeptide().Replace('I', 'L') == other.GetUnmodifiedPeptide().Replace('I', 'L');
        }
        
        public bool ArePeptidesTheSameExceptIle(Ms2Result other)
        {
            //Isoleucine - Leucine difference is ignored
            return Peptide.Replace('I', 'L') == other.Peptide.Replace('I', 'L');
        }

        public Dictionary<char, int> GetLabeledAaCounts()
        {
            return LabelList.GetLabeledAaCounts(this);
        }

        public override int GetHashCode()
        {
            //Qvalue difference or Isoleucine - Leucine difference is ignored
            return ((Peptide.GetHashCode()*17 + Charge.GetHashCode())*17 + IsotopeIdx.GetHashCode())*
                   17 + LabelIndex.GetHashCode();
        }

        public override bool Equals(object other)
        {
            if ((other == null) || (other.GetType() != typeof(Ms2Result)))
                return false;
            return Equals((Ms2Result) other);
        }

        protected bool Equals(Ms2Result other)
        {
            //Qvalue difference or Isoleucine - Leucine difference is ignored
            var pep = ArePeptidesTheSameExceptIle(other);
            var charge = Charge == other.Charge;
            var isoidx = IsotopeIdx == other.IsotopeIdx;
            var channel = LabelIndex == other.LabelIndex;

            return pep & isoidx & charge & channel;
        }

        /*
        public const int ChannelMax = 5;
        public const int ChannelMin = 0;
        public const double MassH = 1.007825;
        public const double MassD = 2.01410178;
        public const double DiffDH = MassD - MassH;
        public const double DELightMass = 56.0626;
        public const double labelTolerance = 0.001;
         * 
        private void CalChannelNum()
        {
            Match match = NtermModRegex.Match(Peptide);
            if (match.Success)
            {
                double labelMass = Convert.ToDouble(match.Value);
                double labelDiff = (labelMass - DELightMass)/DiffDH/2;
                if (Math.Abs(Math.Round(labelDiff)-labelDiff) < labelTolerance)
                {
                    LabelIndex = (sbyte) Math.Round(labelDiff);
                }
            }
            if (LabelIndex < ChannelMin || LabelIndex > ChannelMax)
            {
                LabelIndex = -1;
            }
        }
         */
    }
}