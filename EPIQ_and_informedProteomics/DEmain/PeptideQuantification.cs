using System.Collections.Generic;

namespace Epiq
{
    public class PeptideQuantification
    {
        public static void UpdateDictionary(QuantifiedPeptideDictionary peptideDictionary, List<QuantifiedPsm> psms, short condition, short replicate = 0, short fraction = 0)
        {
            var peptidePsmDictionary = new Dictionary<string, List<QuantifiedPsm>>();
            foreach (var psm in psms)
            {
                //if (psm.Id.IsDecoy()) continue;
                var peptide = psm.Id.UnlabeledPeptide;
                List<QuantifiedPsm> matchingPsms;
                if (!peptidePsmDictionary.TryGetValue(peptide, out matchingPsms))
                {
                    peptidePsmDictionary[peptide] = matchingPsms = new List<QuantifiedPsm>();
                }
                matchingPsms.Add(psm);
            }

            //var peptideDictionary = new QuantifiedPeptideDictionary();

            foreach (var peptide in peptidePsmDictionary.Keys)
            {
                var p = new QuantifiedPeptide(peptide, peptidePsmDictionary[peptide]);
                if(p.IsQuantified()) peptideDictionary.Add(p, condition, replicate, fraction);
            }

           // return peptideDictionary;
        }
    }
}
