using InformedProteomics.Backend.Data.Enum;

namespace InformedProteomics.Backend.Data.Sequence
{
    /// <summary>
    /// A modification specified in the search
    /// </summary>
    public class SearchModification
    {
        public SearchModification(Modification mod, char targetResidue, SequenceLocation loc, bool isFixedModification)
        {
            Modification = mod;
            TargetResidue = targetResidue;
            Location = loc;
            IsFixedModification = isFixedModification;
        }

        public Modification Modification { get; private set; }
        public char TargetResidue { get; private set; }
        public SequenceLocation Location { get; private set; }
        public bool IsFixedModification { get; private set; }
        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4}", 
                Modification.Composition, 
                TargetResidue, 
                (IsFixedModification ? "fix" : "opt"), 
                Location, 
                Modification.Name
                );
        }
    }
}
