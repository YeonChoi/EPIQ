using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InformedProteomics.Backend.Data.Composition;

namespace Epiq
{
    public static class IsotopeImpurityValues
    {
        public static bool UseDefault;
        public static Dictionary<char, IsotopomerEnvelope[]> EnvlopesPerLabelSite { get; private set; }
        //public static IsotopomerEnvelope[] Envelopes { get; private set; }

        static IsotopeImpurityValues()
        {
            UseDefault = true;
        }

        public static void SetDefaultImpurityValues()
        {
            UseDefault = true;
        }


        public static void SetHardCodedImpurityValues(Dictionary<char, IsotopomerEnvelope[]> envPerLabel)
        {
            EnvlopesPerLabelSite = envPerLabel;
            UseDefault = false;
        }

        public static IsotopomerEnvelope[] GetEnv(char labelSite)
        {
            if (UseDefault)
            {
                return DefaultEnvs;
            }

            IsotopomerEnvelope[] retEnvs;
            if (EnvlopesPerLabelSite.TryGetValue(labelSite, out retEnvs))
            {
                Console.WriteLine("For label site {0} :", labelSite);
                PrintImpurityValues(retEnvs);
                return retEnvs;
            }
            else
            {
                Console.WriteLine("Warning: Cannot find isotope impurity values for label site {0}, using default values", labelSite);
                PrintImpurityValues(DefaultEnvs);
                return DefaultEnvs;
            }
        }


        public static void ParseImpurityValues()
        {
            //Enverlopes = blah blah
        }


        public static void PrintImpurityValues(IsotopomerEnvelope[] envArr)
        {
            foreach (var env in envArr)
            {
                foreach (var val in env.Envelope)
                {
                    Console.Write(@"{0:00.000}   ", val*100);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }


        public static void PrintImpurityValues()
        {
            Console.WriteLine();
            Console.WriteLine("Used Isotope impurity envelope:");
            if (EnvlopesPerLabelSite == null)
            {
                PrintImpurityValues(DefaultEnvs);
                return;
            }

            foreach (var labelSite in EnvlopesPerLabelSite.Keys)
            {
                var envArr = EnvlopesPerLabelSite[labelSite];
                Console.WriteLine("For label site {0} :", labelSite);
                foreach (var env in envArr)
                {
                    foreach (var val in env.Envelope)
                    {
                        Console.Write(@"{0:00.000}   ", val*100);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }


        private static readonly IsotopomerEnvelope[] DefaultEnvs = new[]
        {
            new IsotopomerEnvelope(new [] {0.0, 0.0, 1, 0.0, 0.0}, 2, 2), 
            new IsotopomerEnvelope(new [] {0.0, 0.0, 1, 0.0, 0.0}, 2, 2), 
            new IsotopomerEnvelope(new [] {0.0, 0.0, 1, 0.0, 0.0}, 2, 2), 
            new IsotopomerEnvelope(new [] {0.0, 0.0, 1, 0.0, 0.0}, 2, 2), 
            new IsotopomerEnvelope(new [] {0.0, 0.0, 1, 0.0, 0.0}, 2, 2), 
            new IsotopomerEnvelope(new [] {0.0, 0.0, 1, 0.0, 0.0}, 2, 2), 
        };


        private static readonly IsotopomerEnvelope[] DE6LumosEnvs = new[]
        {
            new IsotopomerEnvelope(new [] {0.0, 1}, 1, 1), 
            new IsotopomerEnvelope(new [] {0.017303694821478, 1}, 1, 1), 
            new IsotopomerEnvelope(new [] {0.0461961839817114, 1}, 1, 1), 
            new IsotopomerEnvelope(new [] {0.0841117473065305, 1}, 1, 1), 
            new IsotopomerEnvelope(new [] {0.068782976189056, 1}, 1, 1), 
            new IsotopomerEnvelope(new [] {0.0966287643528354, 1}, 1, 1), 
        };


        private static readonly IsotopomerEnvelope[] DE6QeEnvs = new[]
        {
            new IsotopomerEnvelope(new [] {0.0, 1}, 1, 1), 
            new IsotopomerEnvelope(new [] {0.0177174855757846, 1}, 1, 1), 
            new IsotopomerEnvelope(new [] {0.0430347276547101, 1}, 1, 1), 
            new IsotopomerEnvelope(new [] {0.084367846266165, 1}, 1, 1), 
            new IsotopomerEnvelope(new [] {0.0529372829367461, 1}, 1, 1), 
            new IsotopomerEnvelope(new [] {0.0793023439214469, 1}, 1, 1), 
        };


        public static readonly Dictionary<char, IsotopomerEnvelope[]> DE6LumosEnvsPerLabelSite = new Dictionary<char, IsotopomerEnvelope[]>()
        {
            {'^', DE6LumosEnvs}, {'K', DE6LumosEnvs},
        };


        public static readonly Dictionary<char, IsotopomerEnvelope[]> DE6QeEnvsPerLabelSite = new Dictionary<char, IsotopomerEnvelope[]>()
        {
            {'^', DE6QeEnvs}, {'K', DE6QeEnvs},
        };

        public static readonly Dictionary<char, IsotopomerEnvelope[]> SILAC_K14Envs = new Dictionary<char, IsotopomerEnvelope[]>()
        {
            {'K', new[] {
                         new IsotopomerEnvelope(new[] {0.0, 1}, 1, 1),
                         new IsotopomerEnvelope(new[] {0.663471859468834, 1}, 1, 1),
            }},
        };


        public static readonly Dictionary<char, IsotopomerEnvelope[]> SILAC6EnvsPerLabelSite = new Dictionary<char, IsotopomerEnvelope[]>()
        {
            {'K', new[] {
                         new IsotopomerEnvelope(new[] {0.0, 1}, 1, 1),
                         new IsotopomerEnvelope(new[] {0.028211974, 1}, 1, 1),
                         new IsotopomerEnvelope(new[] {0.069539922, 1}, 1, 1),
                         new IsotopomerEnvelope(new[] {0.045912387, 1}, 1, 1),
                         new IsotopomerEnvelope(new[] {0.034372652, 1}, 1, 1),
                         new IsotopomerEnvelope(new[] {0.12170358, 1}, 1, 1),
            }},

            {'R', new[] {
                         new IsotopomerEnvelope(new[] {0.0, 1}, 1, 1),
                         new IsotopomerEnvelope(new[] {0.041680606, 1}, 1, 1),
                         new IsotopomerEnvelope(new[] {0.046845137, 1}, 1, 1),
                         new IsotopomerEnvelope(new[] {0.127001092, 1}, 1, 1),
                         new IsotopomerEnvelope(new[] {0.091464455, 1}, 1, 1),
                         new IsotopomerEnvelope(new[] {0.104461568, 1}, 1, 1),
            }},
        };


        public static readonly IsotopomerEnvelope[] DE6envsLumosManual = new[]
        {
            new IsotopomerEnvelope(new [] {0.0, 3.38E-03, 1, 0.0, 0.0}, 2, 2), 
            new IsotopomerEnvelope(new [] {0.0, 1.05E-02, 1, 0.0, 0.0}, 2, 2), 
            new IsotopomerEnvelope(new [] {0.0, 4.29E-02, 1, 0.0, 0.0}, 2, 2), 
            new IsotopomerEnvelope(new [] {0.0, 7.75E-02, 1, 0.0, 0.0}, 2, 2), 
            new IsotopomerEnvelope(new [] {0.0, 6.40E-02, 1, 0.0, 0.0}, 2, 2), 
            new IsotopomerEnvelope(new [] {0.0, 9.73E-02, 1, 0.0, 0.0}, 2, 2), 
        };

    }
}
