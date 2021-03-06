using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Composition;

namespace InformedProteomics.Backend.Data.Sequence
{
    public class AminoAcid : IMolecule
    {
        #region Constructors

        public AminoAcid(char residue, string name, Composition.Composition comp)
        {
            Residue = residue;
            Name = name;
            Composition = comp;
            Mass = Composition.Mass;
            _nominalMass = Composition.NominalMass;
        }

        #endregion

        #region Properties

        public char Residue { get; private set; }
        public string Name { get; private set; }
        public Composition.Composition Composition { get; private set; }
        public double Mass { get; private set; }

        #endregion

        #region Getters

        //public IsotopomerEnvelope GetIsotopomerEnvelopeRelativeIntensities()
        //{
        //    return Composition.GetIsotopomerEnvelopeRelativeIntensities();
        //}

        public int GetNominalMass()
        {
            return _nominalMass;
        }

        #endregion

        #region Private Members

        private readonly int _nominalMass;

        #endregion

        //public static AminoAcid GetStandardAminoAcid(char residue)
        //{
        //    return StandardAaSet.GetAminoAcid(residue);
        //}

        # region Public Static Members

        public static readonly AminoAcid ProteinNTerm = new AminoAcid('[', "Protein-N-terminus", Data.Composition.Composition.Zero);
        public static readonly AminoAcid ProteinCTerm = new AminoAcid(']', "Protein-C-terminus", Data.Composition.Composition.Zero);

        public static readonly AminoAcid PeptideNTerm = new AminoAcid('(', "Peptide-N-terminus", Data.Composition.Composition.Zero);
        public static readonly AminoAcid PeptideCTerm = new AminoAcid(')', "Peptide-C-terminus", Data.Composition.Composition.Zero);

        public static readonly AminoAcid Empty = new AminoAcid('\0', "Empty", Data.Composition.Composition.Zero);

        public static readonly AminoAcid[] StandardAminoAcidArr =
        {
            new AminoAcid('G', "Glycine", new Composition.Composition(2, 3, 1, 1, 0)),
            new AminoAcid('A', "Alanine", new Composition.Composition(3, 5, 1, 1, 0)),
            new AminoAcid('S', "Serine", new Composition.Composition(3, 5, 1, 2, 0)),
            new AminoAcid('P', "Proline", new Composition.Composition(5, 7, 1, 1, 0)),
            new AminoAcid('V', "Valine", new Composition.Composition(5, 9, 1, 1, 0)),
            new AminoAcid('T', "Threonine", new Composition.Composition(4, 7, 1, 2, 0)),
            new AminoAcid('C', "Cystine", new Composition.Composition(3, 5, 1, 1, 1)),
            new AminoAcid('L', "Leucine", new Composition.Composition(6, 11, 1, 1, 0)),
            new AminoAcid('I', "Isoleucine", new Composition.Composition(6, 11, 1, 1, 0)),
            new AminoAcid('N', "Asparagine", new Composition.Composition(4, 6, 2, 2, 0)),
            new AminoAcid('D', "Aspartate", new Composition.Composition(4, 5, 1, 3, 0)),
            new AminoAcid('Q', "Glutamine", new Composition.Composition(5, 8, 2, 2, 0)),
            new AminoAcid('K', "Lysine", new Composition.Composition(6, 12, 2, 1, 0)),
            new AminoAcid('E', "Glutamate", new Composition.Composition(5, 7, 1, 3, 0)),
            new AminoAcid('M', "Methionine", new Composition.Composition(5, 9, 1, 1, 1)),
            new AminoAcid('H', "Histidine", new Composition.Composition(6, 7, 3, 1, 0)),
            new AminoAcid('F', "Phenylalanine", new Composition.Composition(9, 9, 1, 1, 0)),
            new AminoAcid('R', "Arginine", new Composition.Composition(6, 12, 4, 1, 0)),
            new AminoAcid('Y', "Tyrosine", new Composition.Composition(9, 9, 1, 2, 0)),
            new AminoAcid('W', "Tryptophan", new Composition.Composition(11, 10, 2, 1, 0))
        };

        public const string StandardAminoAcidCharacters = "ACDEFGHIKLMNPQRSTVWY";

        public static bool IsStandardAminoAcidResidue(char residue)
        {
            return StandardAminoAcidCharacters.IndexOf(residue) >= 0;
        }

        #endregion

        # region Private Static Members

//        private static readonly AminoAcidSet StandardAaSet = new AminoAcidSet();

        #endregion
    }
}
