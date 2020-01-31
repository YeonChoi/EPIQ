using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Epiq
{
    public static class BuiltInRtModels
    {
        private static readonly string[][] RtModels = new string[][]
        {
            new string[]
            {
                "D-shifts_libSVMData_UniquePepSampled_ForPaperSCXLumos_ThermoUltimate3000_130m.standardized.s4t2c0.1e0.00005.model",
                "D-shifts_libSVMData_UniquePepSampled_ForPaperSILAC6plexLumos_ThermoUltimate3000_130m.standardized.s4t2c0.1e0.00005.model",
                "D-shifts_libSVMData_UniquePepSampled_ForPaperSILAC6plexLumos_nanoACQUITY_120m.standardized.s4t2c0.1e0.00005.model",
                "D-shifts_libSVMData_UniquePepSampled_ForPaperDM5_QE.standardized.s4t2c0.1e0.00005.model",
            },

            new string[]
            {
                "D-shifts_libSVMData_UniquePepSampled_ForPaperSCXLumos_ThermoUltimate3000_130m.standard",
                "D-shifts_libSVMData_UniquePepSampled_ForPaperSILAC6plexLumos_ThermoUltimate3000_130m.standard",
                "D-shifts_libSVMData_UniquePepSampled_ForPaperSILAC6plexLumos_nanoACQUITY_120m.standard",
                "D-shifts_libSVMData_UniquePepSampled_ForPaperDM5_QE.standard",
            }
        };

        public static readonly object[] RtModelLcTypes = new string[]
        {
            "DE-6plex, Thermo ultimate 3000 RSLC nano-system, nonlinear gradient, 40% sol B",
            "SILAC-6plex, Thermo ultimate 3000 RSLC nano-system, nonlinear gradient, 40% sol B",
            "SILAC-6plex, Waters nanoACQUITY, nonlinear gradient, 40% sol B",
            "DM-5plex, Waters nanoACQUITY, nonlinear gradient, 40% sol B",
        };
            /*@"Thermo ultimate 3000 RSLC nano-system, nonlinear gradient, 130m, 40% sol B, 20171218Data",
            @"Thermo ultimate 3000 RSLC nano-system, nonlinear gradient, 40% sol B, 08250913 algorithm",
            @"Thermo ultimate 3000 RSLC nano-system, nonlinear gradient, 40% sol B, old algorithm",
            @"Waters 3000 RSLC nano-system, nonlinear gradient, 40% sol B",*/
            //"Thermo 3000 RSLC nano-system, 350 nL/min, 2 hrs, linear gradient, 95% sol A, 40% sol B",
            //"Waters 3000 RSLC nano-system, 350 nL/min, 4 hrs, linear gradient, 95% sol A, 40% sol B",

        private static readonly string builtInRtDir = "RTmodels";


        private static string SelectedRtModelPath(int idx)
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                Path.DirectorySeparatorChar + builtInRtDir + Path.DirectorySeparatorChar + RtModels[0][idx];
        }

        private static string SelectedRtStandardPath(int idx)
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                Path.DirectorySeparatorChar + builtInRtDir + Path.DirectorySeparatorChar + RtModels[1][idx];
        }

        public static string[] GetRtBuiltInRtModelPaths(string lcType)
        {
            var idx = Array.IndexOf(RtModelLcTypes, lcType);
            return new[] {SelectedRtModelPath(idx), SelectedRtStandardPath(idx)};
        }


    }
}
