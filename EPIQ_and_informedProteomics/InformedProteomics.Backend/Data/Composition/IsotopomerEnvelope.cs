using System;

namespace InformedProteomics.Backend.Data.Composition
{
    [Serializable]
    public class IsotopomerEnvelope
    {
        public IsotopomerEnvelope(double[] envelope, int mostAbundantIsotopeIndex, sbyte monoIsitopeIndex = 0)
        {
            Envelope = envelope;
            MostAbundantIsotopeIndex = mostAbundantIsotopeIndex;
            MonoIsotopeIndex = monoIsitopeIndex;
        }

        public double[] Envelope { get; private set; }
        public int MostAbundantIsotopeIndex { get; private set; }
        public sbyte MonoIsotopeIndex { get; private set; }

        public IsotopomerEnvelope GetConvolutedEnvelope(IsotopomerEnvelope other, double minIntensity = .005)
        {
            var env = new double[Envelope.Length + other.Envelope.Length];
            var mostAbundantIndex = 0;
            var monoIndex = MonoIsotopeIndex + other.MonoIsotopeIndex;
            for (var n = 0; n < env.Length; n++)
            {
                env[n] = 0;
                for (var i = 0; i < Envelope.Length; i++)
                {
                    if (n - i >= other.Envelope.Length || n - i <0) continue;
                    env[n] += Envelope[i]*other.Envelope[n - i];
                }
                if (env[mostAbundantIndex] < env[n]) mostAbundantIndex = n;
            }
            var max = env[mostAbundantIndex];
            if (max <= 0)
            {
                return new IsotopomerEnvelope(env, 0, (sbyte)monoIndex);
            }
            /*
            if (Envelope.Length > 3)
            {
                foreach (var e in Envelope)
                {
                    Console.Write(e + " ");
                }
                Console.WriteLine();
                foreach (var e in other.Envelope)
                {
                    Console.Write(e + " ");
                }
                Console.WriteLine();

                foreach (var e in env)
                {
                    Console.Write(e + " ");
                }
                Console.WriteLine();
                Console.WriteLine();
            }
            */
            var truncateLeft = -1;
            var truncateRight = env.Length;
            for (var n = 0; n < env.Length; n++)
            {
                env[n] /= max;
                if (env[n] <= minIntensity) continue;
                if (truncateLeft < 0) truncateLeft = n;
                truncateRight = n;
            }
            if (truncateLeft <= 0 && truncateRight >= env.Length)
                return new IsotopomerEnvelope(env, (sbyte)mostAbundantIndex,(sbyte) monoIndex);
            
            var truncated = new double[truncateRight - truncateLeft + 1];
            var m = 0;
            for (var n = truncateLeft; n <= truncateRight; n++)
            {
                truncated[m++] = env[n];
            }
            env = truncated;
            mostAbundantIndex -= truncateLeft;
            monoIndex -= truncateLeft;

            /*if (Envelope.Length > 3)
            {
                foreach (var e in Envelope)
                {
                    Console.Write(e + " ");
                }
                Console.WriteLine();
                foreach (var e in other.Envelope)
                {
                    Console.Write(e + " ");
                }
                Console.WriteLine();

                foreach (var e in env)
                {
                    Console.Write(e + " ");
                }
                Console.WriteLine("\n_____");

                Console.WriteLine();
            }*/
            return new IsotopomerEnvelope(env, (sbyte)mostAbundantIndex, (sbyte)monoIndex);
            

        }

    }
}
