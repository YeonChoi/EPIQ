using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epiq
{
    public class QuantifiedPeptide : IQuantifiable
    {
        public List<QuantifiedPsm> MatchedPsms { get; private set; }
        public string Peptide { get; private set; }
        //IQuantifiable members
        public int LabelCount { get; private set; }
        public float SignalPower { get; private set; }
        public float NoisePower { get; private set; }
        public float[] Quantities { get; private set; }
        public float Qvalue { private set; get; }
        public float QvalueScore { private set; get; }
       

        public int GetQuantifiedLabelCount()
        {
            var ret = 0;
            foreach (var intensity in Quantities)
            {
                if (Math.Abs(intensity) > 0.1) ret++;
            }
            return ret;
        }

        public bool IsDecoy()
        {
            foreach (var p in MatchedPsms)
            {
                if (p.Id.IsDecoy()) return true;
            }
            return false;
        }

        public float[] GetRatios()
        {
            var sum = Quantities.Sum();
            if (sum <= 0) return Quantities;
            var ratio = new float[LabelCount];
            for (var i = 0; i < ratio.Length; i++)
            {
                ratio[i] = Quantities[i] / sum;
            }
            return ratio;
        }


        public float GetSnr()
        {
            return SignalPower / (NoisePower + 1e-6f);
        }


        public void SetQvalue(float q)
        {   
            Qvalue = q;
        }


        public bool IsFullyQuantified()
        {
            return Quantities != null && Quantities.Min() > 0;
        }

        public bool IsQuantified()
        {
            return Quantities != null && Quantities.Max() > 0;
        }

        public QuantifiedPeptide(string peptide, List<QuantifiedPsm> psms)
        {
            Peptide = peptide;
            if (psms == null || psms.Count < Params.NumMatchedPsmsPerPeptide) return;
            MatchedPsms = psms;
            QvalueScore = 100;
            Quantities = new float[psms[0].LabelCount];
            LabelCount = Quantities.Length;
            foreach (var psm in psms)
            {
                for (var l = 0; l < psm.LabelCount; l++)
                {
                    Quantities[l] += psm.Quantities[l];
                }
                SignalPower += (float)Math.Sqrt(psm.SignalPower);
                NoisePower += psm.NoisePower;
                QvalueScore = Math.Min(QvalueScore, psm.Qvalue);
            }
            SignalPower = SignalPower*SignalPower;
        }
    }
}
