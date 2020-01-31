using System;
using System.Security.Cryptography.X509Certificates;

namespace Epiq
{

    public interface IQuantifiable
    {
        int LabelCount { get; }
        float SignalPower { get; }
        float NoisePower { get; }
        float[] Quantities { get; }
        float GetSnr();
        bool IsQuantified();
        bool IsFullyQuantified();

        float[] GetRatios();
        int GetQuantifiedLabelCount();

        /*
        public IQuantifiable(float[] ints)
        {
            Quantities = ints;
            LabelCount = Quantities.Length;
        }

        public float GetSnr()
        {
            return SignalPower / (NoisePower + 1e-6f);
        }*/

    }
}
