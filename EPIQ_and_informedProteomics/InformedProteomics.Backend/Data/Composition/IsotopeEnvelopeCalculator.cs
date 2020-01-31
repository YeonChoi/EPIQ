using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Spectrometry;
using MathNet.Numerics;

namespace InformedProteomics.Backend.Data.Composition
{
    public class IsotopeEnvelopeCalculator
    {
        private static int _maxNumIsotopes = 20;
        public static readonly Dictionary<string, double[]> Probs = new Dictionary<string, double[]>
        {
            {"C",  new [] { .9893, 0.0107 , 0 ,0 }},
            {"H",  new [] { .999885, .000115 , 0, 0}},
            {"N", new []{ .99632, .00368,0 ,0 }},
            {"O",  new [] { 0.99757, 0.00038, 0.00205, 0}},
            {"S",  new [] { 0.9493, 0.0076, 0.0429, 0.0002}},
            {"P",  new [] { 1.0,0,0,0}},
            {"2H",  new [] { 1.0, 0, 0, 0}},
            {"13C",  new [] { 1.0, 0, 0, 0 }}
        }; 

        private static readonly Tuple<Composition, float>[][] ConsideredIsotopeCompositions =
        {
            new [] {
                new Tuple<Composition, float>(new Composition(-1, 0, 0, 0, 0, 0, new Tuple<Atom, short>(Atom.Get("13C"), 1)), (float)Probs["C"][1]),
                new Tuple<Composition, float>(new Composition(0, -1, 0, 0, 0, 0, new Tuple<Atom, short>(Atom.Get("2H"), 1)), (float)Probs["H"][1]),
                new Tuple<Composition, float>(new Composition(0, 0, -1, 0, 0, 0, new Tuple<Atom, short>(Atom.Get("15N"), 1)), (float)Probs["N"][1]),
                new Tuple<Composition, float>(new Composition(0, 0, 0, -1, 0, 0, new Tuple<Atom, short>(Atom.Get("17O"), 1)), (float)Probs["O"][1]),
                new Tuple<Composition, float>(new Composition(0, 0, 0, 0, -1, 0, new Tuple<Atom, short>(Atom.Get("33S"), 1)), (float)Probs["S"][1]),
            },
            new [] {
                new Tuple<Composition, float>(new Composition(0, 0, 0, -1, 0, 0, new Tuple<Atom, short>(Atom.Get("18O"), 1)), (float)Probs["O"][2]),
                new Tuple<Composition, float>(new Composition(0, 0, 0, 0, -1, 0, new Tuple<Atom, short>(Atom.Get("34S"), 1)), (float)Probs["S"][2]),
            }
        };

        public const double IntensityThreshold = .01;
        public static void SetMaxNumIsotopes(int n)
        {
            _maxNumIsotopes = n;
            ComputePossibleIsotopeCombinations(_maxNumIsotopes);
        }

        public static Dictionary<Composition, float>[] CalculateIsotopeCompositions(Composition comp)
        {
            var iprobs = new Dictionary<Composition, float>[ConsideredIsotopeCompositions.Length];
            for (var i = 0; i < iprobs.Length; i++)
            {
                iprobs[i] = new Dictionary<Composition, float>();
                foreach (var c in ConsideredIsotopeCompositions[i])
                {
                    var n = -(comp.C * c.Item1.C + comp.H * c.Item1.H + comp.N * c.Item1.N + comp.O * c.Item1.O + comp.S * c.Item1.S);
                    var p = c.Item2;
                    iprobs[i][c.Item1] = (float)(n * p * Math.Pow(1 - p, n - 1));
                }
            }
            var possibleIsotopeCompositions = new Dictionary<Composition, float>[_maxNumIsotopes];
            for (var i = 0; i < possibleIsotopeCompositions.Length; i++)
            {
                possibleIsotopeCompositions[i] = new Dictionary<Composition, float>();
                if (i == 0)
                {
                    possibleIsotopeCompositions[i][new Composition(0, 0, 0, 0, 0)] = 1;
                    continue;
                }
                for (var j = 0; j < ConsideredIsotopeCompositions.Length; j++)
                {
                    var k = i - j - 1;
                    if (k < 0) break;
                    foreach (var c in ConsideredIsotopeCompositions[j])
                    {
                        var comp1 = c.Item1;
                        var edgeProb = iprobs[j][comp1];
                        foreach (var comp2 in possibleIsotopeCompositions[k].Keys)
                        {
                            var nodeProb = possibleIsotopeCompositions[k][comp2];
                            float p;
                            possibleIsotopeCompositions[i].TryGetValue(comp1 + comp2, out p);
                            possibleIsotopeCompositions[i][comp1 + comp2] = p + nodeProb * edgeProb;
                        }
                    }
                }

                foreach (var c in possibleIsotopeCompositions[i].Keys.ToList())
                {
                    if (possibleIsotopeCompositions[i][c] > 0f) continue;
                    possibleIsotopeCompositions[i].Remove(c);
                }

            }

            foreach (var pi in possibleIsotopeCompositions)
            {
                var max = pi.Values.Max();
                {
                    foreach (var mass in pi.Keys.ToList())
                    {
                        if (pi[mass] < max * .001f) pi.Remove(mass);
                    }
                }
            }

            return possibleIsotopeCompositions;
        }

        public static IsotopomerEnvelope GetIsotopomerEnvelop(Composition c, int numIsotopes = 100)
        {
            var dist = new double[Math.Min(numIsotopes,_maxNumIsotopes)];
            var lambdas = new double[_possibleIsotopeCombinations[0][0].Length + 1];
            //var lnLambdas = new double[lambdas.Length];
            //var lnComplementaryLambdas = new double[lambdas.Length];

            var dc = c as CompositionWithDeltaMass;
            if (dc != null) c = c + Averagine.GetAverageComposition(dc.DeltaMass);
            var additionalElements = c.GetAdditionalElements();
            var total = 0;
            for (var i = 0; i < lambdas.Length; i++) // precalculate means and thier exps
            {
                lambdas[i] = c.C * Probs["C"][i] + c.H * Probs["H"][i] + c.N * Probs["N"][i] + c.O * Probs["O"][i] + c.S * Probs["S"][i] + c.P * Probs["P"][i];
                if (i == 0) total += c.C + c.H + c.N + c.O + c.S + c.P;
                if (additionalElements != null)
                {
                    foreach (var n in additionalElements.Keys)
                    {
                        lambdas[i] += additionalElements[n] * Probs[n.Code][i];
                        if (i == 0) total += additionalElements[n];
                    }
                }
               // lnLambdas[i] = Math.Log(lambdas[i]);
            }
          //  for (var i = 0; i < lambdas.Length; i++)
          //  {
          //      lnComplementaryLambdas[i] = Math.Log(total-lambdas[i]);
           // }


            var maxHeight = 0.0;
            var mostIntenseIsotopomerIndex = -1;
            for (var isotopeIndex = 0; isotopeIndex < dist.Length; isotopeIndex++)
            {
                foreach (var isopeCombinations in _possibleIsotopeCombinations[isotopeIndex])
                {
                    dist[isotopeIndex] += GetIsotopeProbability(isopeCombinations, lambdas, total);
                }
                if (Double.IsInfinity(dist[isotopeIndex]))
                {
                    throw new NotFiniteNumberException();
                }
                if (dist[isotopeIndex] <= maxHeight) continue;
                maxHeight = dist[isotopeIndex];
                mostIntenseIsotopomerIndex = isotopeIndex;
            }

            var truncationIndex = dist.Length - 1;
            for (; truncationIndex >= 0; truncationIndex--)
            {
                if (dist[truncationIndex] / maxHeight > IntensityThreshold)
                {
                    break;
                }
            }

            var truncatedDist = new double[truncationIndex + 1];
            for (var i = 0; i < truncatedDist.Length; i++)
            {
                truncatedDist[i] = dist[i] / maxHeight;
            }

            return new IsotopomerEnvelope(truncatedDist, mostIntenseIsotopomerIndex);
        }

        static IsotopeEnvelopeCalculator()
        {
            ComputePossibleIsotopeCombinations(_maxNumIsotopes);
        }

       

        private static int[][][] _possibleIsotopeCombinations;

        private static void ComputePossibleIsotopeCombinations(int max) // called just once. 
        {
            var comb = new List<int[]>[max + 1];
            var maxIsotopeNumberInElement = Probs["C"].Length-1;
            comb[0] = new List<int[]> { (new int[maxIsotopeNumberInElement]) };

            for (var n = 1; n <= max; n++)
            {
                comb[n] = new List<int[]>();
                for (var j = 1; j <= maxIsotopeNumberInElement; j++)
                {
                    var index = n - j;
                    if (index < 0) continue;
                    foreach (var t in comb[index])
                    {
                        var add = new int[maxIsotopeNumberInElement];
                        add[j - 1]++;
                        for (var k = 0; k < t.Length; k++)
                            add[k] += t[k];
                        var toAdd = comb[n].Select(v => !v.Where((t1, p) => t1 != add[p]).Any()).All(equal => !equal);
                        if (toAdd) comb[n].Add(add);
                    }
                }
            }
            var possibleIsotopeCombinations = new int[max][][];
            for (var i = 0; i < possibleIsotopeCombinations.Length; i++)
            {
                possibleIsotopeCombinations[i] = new int[comb[i].Count][];
                var j = 0;
                foreach (var t in comb[i])
                {
                    possibleIsotopeCombinations[i][j++] = t;
                }
                
            }
            _possibleIsotopeCombinations = possibleIsotopeCombinations;
        }

        private static double GetIsotopeProbability(int[] ks, double[] lambdas, int total)
        {
            var prob = SpecialFunctions.GammaLn(total + 1);
            for (var i = 0; i < ks.Length + 1; i++)
            {
                var p = lambdas[i]/total;
                var x = i == 0 ? total - ks.Sum() : ks[i - 1];
                if (x <= 0) continue;
                prob += -SpecialFunctions.GammaLn(x + 1) + x * Math.Log(p);
            }
            return Math.Pow(Math.E, prob);
        }
    }
}
