using System;
using System.Text.RegularExpressions;
using InformedProteomics.Backend.Data.Composition;

namespace Epiq
{
    [Serializable]
    public class Label : IComparable<Label>
    {
        private readonly Regex _regexForBound;
        private readonly Regex _regexForUnbound;
        public IsotopomerEnvelope ImpurityIsotopeEnvelope { set;  get; }
        public const char LabelFreeAa = '.';

        public Label(char aa, float massShift, sbyte detCount, sbyte channel, IsotopomerEnvelope impurityIsotopomerEnv)
        {
            AminoAcid = aa;
            MassShift = massShift;
            DetCount = detCount;
            Channel = channel;
            if (aa == LabelFreeAa)
            {
                _regexForBound = null;
                _regexForUnbound = null;
            }
            else if (aa != '$')
            {
                _regexForBound = new Regex(aa + @"\+" + MassShift.ToString("F3"));
                _regexForUnbound = new Regex(aa + @"[ACDEFGHIKLMNPQRSTUVWY]");
            }
            else
            {
                _regexForBound = new Regex(@"\+" + MassShift.ToString("F3") + aa);
                _regexForUnbound = new Regex(@"[ACDEFGHIKLMNPQRSTUVWY]" + aa);
            }
            //Console.WriteLine(_regexForBound + " " + _regexForUnbound);
            ImpurityIsotopeEnvelope = impurityIsotopomerEnv;
            // ImpurityIsotopeEnvelope = new IsotopomerEnvelope(new[] { 1.0 }, 0, 0);
        }

        public char AminoAcid { get; private set; }

        public float MassShift { get; private set; }

        public sbyte DetCount { get; private set; }

        public sbyte Channel { get; private set; }

        int IComparable<Label>.CompareTo(Label other)
        {
            var c = MassShift.CompareTo(other.MassShift);
            if (c == 0) c = DetCount.CompareTo(other.DetCount);
            return c;
        }

        public override string ToString()
        {
            return string.Format(@"{0}+{1},{2}D", AminoAcid, MassShift, DetCount);
            //  return base.ToString();
        }

        public int NumBound(string peptide)
        {
            if (AminoAcid == LabelFreeAa)
                return 1;
            return _regexForBound.Matches(peptide).Count;
        }

        public bool HasUnbound(string peptide)
        {
            if (AminoAcid == LabelFreeAa)
                return false;
            return _regexForUnbound.IsMatch(peptide);
        }

        public string GetUnlabeledPeptide(string peptide)
        {
            return NumBound(peptide) == 0 ? peptide : new Regex(@"\+" + MassShift.ToString("F3")).Replace(peptide, "");
            //return _regexForBound.Replace(peptide, @"$1");
        }
    }
}