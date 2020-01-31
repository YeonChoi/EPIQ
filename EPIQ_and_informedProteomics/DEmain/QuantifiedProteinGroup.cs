using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epiq
{
    [Serializable]
    public class QuantifiedProteinGroup : IQuantifiable
    {
        public HashSet<QuantifiedProtein> QuantifiedProteinSet { get; private set; }
        public List<QuantifiedPsm> MatchedPsms { get; private set; }
        public float Qvalue { private set; get; }
        public float QvalueScore { private set; get; }
        //IQuantifiable members
        public int LabelCount { get; private set; }
        public float SignalPower { get; private set; }
        public float NoisePower { get; private set; }
        public float[] Quantities { get; private set; }


        public int GetQuantifiedLabelCount()
        {
            var ret = 0;
            foreach (var intensity in Quantities)
            {
                if (Math.Abs(intensity) > 0.1) ret++;
            }
            return ret;
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

        public bool IsDecoy()
        {
            foreach (var p in QuantifiedProteinSet)
            {
                if (p.IsDecoy) return true;
            }
            return false;
        }

        public void SetQvalue(float q)
        {
            Qvalue = q;
        }


        public QuantifiedProteinGroup(List<QuantifiedPsm> psms, HashSet<int> indices, bool assignProteinToPsm)
        {
            QuantifiedProteinSet = new HashSet<QuantifiedProtein>();
            MatchedPsms = new List<QuantifiedPsm>();
            foreach (var i in indices)
            {
                MatchedPsms.Add(psms[i]);
                if(assignProteinToPsm) psms[i].SetQuantifiedProteinGroup(this);
            }
        }


        public bool IsFullyQuantified()
        {
            return Quantities != null && Quantities.Min() > 0;
        }

        public bool IsQuantified()
        {
            return Quantities != null && Quantities.Max() > 0;
        }

        public bool Add(QuantifiedProtein p)
        {
            Quantities = p.Quantities;
            SignalPower = p.SignalPower;
            NoisePower = p.NoisePower;
            LabelCount = p.LabelCount;
            QvalueScore = p.QvalueScore;
            return QuantifiedProteinSet.Add(p);
        }

        public string GetRepresentativeProtein() //TODO
        {
            foreach (var p in QuantifiedProteinSet)
            {
                return p.Name;
            }
            return null;
        }

    }
}
