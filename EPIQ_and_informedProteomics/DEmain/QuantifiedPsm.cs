using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Epiq
{
    [Serializable]
    public class QuantifiedPsm : IQuantifiable
    {
        public float[] ChannelApexEts;
        public int Index { private set; get; }
        public string PsmId;
        public float XicClusterScore { private set; get; }
        public float Qvalue { private set; get; }
        public Ms2Result Id { private set; get; }
        public float[] Cosines { private set; get; }
        public float Mz { private set; get; }
        public float RtStart { private set; get; }
        public float RtEnd { private set; get; }
        public float[] MzPositions { private set; get; }
        public float[] RtPositions { private set; get; }
       // public float Cosine { private set; get; }
        //public short Condition { private set; get; }
        //public short Replicate { private set; get; }
        //public short Fraction { private set; get; }

        public string[] TrainingDictionary { private set; get; }
        public QuantifiedProteinGroup ProteinGroup { private set; get; }
        //IQuantifiable members
        public int LabelCount { get; private set; }
        public float SignalPower { get; private set; }
        public float NoisePower { get; private set; }
        public float[] Quantities { get; private set; }

        /*public void SetConditionReplicateFraction(short c, short r, short f)
        {
            Condition = c;
            Replicate = r;
            Fraction = f;
        }*/

        public void TruncateQuantities(float minQuantity)
        {
            for (var i = 0; i < Quantities.Length; i++)
                Quantities[i] = Quantities[i] < minQuantity ? 0 : Quantities[i];
        }

        public int GetQuantifiedLabelCount()
        {
            var ret = 0;
            foreach (var intensity in Quantities)
            {
                if (Math.Abs(intensity) > 1e-5) ret++;
            }
            return ret;
        }

        //one based
        public Tuple<int, int> GetInclusiveCoveringRange(string proteinSequence)
        {
            var s = proteinSequence.IndexOf(Id.UnmodifiedPeptide, StringComparison.Ordinal) + 1;
            var e = s + Id.UnmodifiedPeptide.Length;
            return new Tuple<int, int>(s, e);
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

        public bool IsFullyQuantified()
        {
            return Quantities != null && Quantities.Min() > 0;
        }

        public bool IsQuantified()
        {
            return Quantities != null && Quantities.Max() > 0;
        }

        public QuantifiedPsm Clone()
        {
            return new QuantifiedPsm(this);
        }

        private QuantifiedPsm(QuantifiedPsm other)
        {
            LabelCount = other.LabelCount;
            Quantities = other.Quantities;
            PsmId = other.PsmId;
            Index = other.Index;
            Id = other.Id;
            Mz = other.Mz;
            MzPositions = other.MzPositions;
            RtPositions = other.RtPositions;
            RtStart = other.RtStart;
            RtEnd = other.RtEnd;
            ChannelApexEts = other.ChannelApexEts;
            Cosines = other.Cosines;
            //Cosine = other.Cosine;
            XicClusterScore = other.XicClusterScore;
            Qvalue = other.Qvalue;
            SignalPower = other.SignalPower;
            NoisePower = other.NoisePower;
            TrainingDictionary = other.TrainingDictionary;
            

        }

        public QuantifiedPsm(XicCluster xicCluster)
        {
            LabelCount = xicCluster.LabelCount;
            Quantities = xicCluster.Quantities;
            Id = xicCluster.Id;
            Mz = xicCluster.Mzs[0];
            RtStart = xicCluster.Rts[0];
//            Cosine = xicCluster.Cosine;

            MzPositions = new float[LabelCount];
            RtPositions = new float[LabelCount];
            for (var c = 0; c < LabelCount; c++)
            {
                if (xicCluster.Templates[c] == null) continue;
                MzPositions[c] = xicCluster.Templates[c].MzPosition;
                RtPositions[c] = xicCluster.Templates[c].RtPosition;
            }

            RtEnd = xicCluster.Rts[xicCluster.Rts.Length - 1];
            ChannelApexEts = new float[xicCluster.Templates.Length];
            Cosines = new float[xicCluster.Templates.Length];
            for (var cn = 0; cn < ChannelApexEts.Length; cn++)
            {
                if (xicCluster.Templates[cn] == null)
                {
                    Cosines[cn] = -1;
                    continue;
                }
                ChannelApexEts[cn] = xicCluster.Templates[cn].RtPosition;
                Cosines[cn] = xicCluster.Templates[cn].Cosine;
            }

            XicClusterScore = xicCluster.XicClusterScore;
            SignalPower = xicCluster.SignalPower;
            NoisePower = xicCluster.NoisePower;
            TrainingDictionary = xicCluster.TrainingStringDictionary;
        }

        

        public void SetQuantifiedProteinGroup(QuantifiedProteinGroup pg)
        {
            ProteinGroup = pg;
        }
        
        public void SetXicClusterScore(float s)
        {
            XicClusterScore = s;
        }

        public void SetQValue(float p)
        {
            Qvalue = p;
        }

        public void SetIndex(int i)
        {
            Index = i;
        }


        public override bool Equals(object obj)
        {
            if (!(obj is QuantifiedPsm)) return false;

            var opsm = (QuantifiedPsm)obj;

            return opsm.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}