﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using InformedProteomics.Backend.Data.Spectrometry;
using InformedProteomics.Backend.Utils;

namespace InformedProteomics.Backend.MassSpecData
{
    public class XCaliburReader: IMassSpecDataReader
    {
        // Parameters for centroiding spectra
        public const int PeakToBackgroundRatio = 0;

        public XCaliburReader(string filePath)
        {
            _msfileReader = (MSFileReaderLib.IXRawfile5) new MSFileReaderLib.MSFileReader_XRawfile();

            _msfileReader.Open(filePath);
            _msfileReader.SetCurrentController(0, 1);

            _minLcScan = 1;
            _maxLcScan = ReadNumSpectra();

            _msLevel = new Dictionary<int, int>();
        }

        /// <summary>
        ///  Count each MS level. Added by Yeon Choi
        /// </summary>
        /// <returns>Counts of each ms level</returns>
        public Dictionary<int, int> CountMsLevel()
        {
            var msLevelCounter = new Dictionary<int, int>();

            for (var scanNum = _minLcScan; scanNum <= _maxLcScan; scanNum++)
            {
                var msLevel = ReadMsLevel(scanNum);
                if (msLevelCounter.ContainsKey(msLevel))
                    msLevelCounter[msLevel] += 1;
                else
                    msLevelCounter[msLevel] = 1;
            }
            return msLevelCounter;
        }

        public int CountMsLevel(int mslevel, int charge = 0)
        {
            var msLevelCounter = 0;

            for (var scanNum = _minLcScan; scanNum <= _maxLcScan; scanNum++)
            {
                var msl = ReadMsLevel(scanNum);
                if (msl != mslevel) continue;
                if (charge > 0 && charge != ReadPrecursorInfo(scanNum).Charge) continue;
                msLevelCounter++;
            }
            return msLevelCounter;
        }

        /// <summary>
        ///  Read instrument model. Added by Yeon Choi
        /// </summary>
        /// <returns>instrument model name</returns>
        public string ReadInstModel()
        {
            string instModel = null;
            _msfileReader.GetInstModel(ref instModel);
            return instModel;
        }

        /// <summary>
        ///  Read instrument name. Added by Yeon Choi
        /// </summary>
        /// <returns>instrument name</returns>
        public string ReadInstName()
        {
            string instName = null;
            _msfileReader.GetInstName(ref instName);
            return instName;
        }

        /// <summary>
        /// Reads all spectra
        /// </summary>
        /// <returns>all spectra</returns>
        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        public IEnumerable<Spectrum> ReadAllSpectra()
        {
            for (var scanNum = _minLcScan; scanNum <= _maxLcScan; scanNum++)
            {
                Spectrum spec = null;
                try
                {
                    spec = ReadMassSpectrum(scanNum);
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    Console.WriteLine("[Warning] Ignore corrupted spectrum Scan={0}", scanNum);
                }

                if(spec != null) yield return spec;
            }
        }

        /// <summary>
        /// Reads the mass spectrum with the specified scanNum from the raw file
        /// </summary>
        /// <param name="scanNum">scan number</param>
        /// <returns>mass spectrum</returns>
        public Spectrum ReadMassSpectrum(int scanNum)
        {
            object peakArr = null;
            object peakFlags = null;
            var arraySize = 0;

            _msfileReader.GetMassListFromScanNum(
                ref scanNum,
                null, // optional scan scanFilterString
                0, // intensity cutoff type
                0, // intensity cutoff value
                0, // max number of peaks
                0, // return centroided spectrum if nonzero
                0, // centroid peak width
                ref peakArr,
                ref peakFlags,
                ref arraySize);

            var vals = peakArr as double[,];

            if (vals == null) return null;

            var mzArr = new double[vals.GetLength(1)];
            var intensityArr = new double[vals.GetLength(1)];

            var sortRequired = false;

            for (var i = 0; i < vals.GetLength(1); i++)
            {
                mzArr[i] = vals[0, i];
                intensityArr[i] = vals[1, i];

                if (i > 0 && mzArr[i] < mzArr[i - 1])
                    sortRequired = true;
            }

            if (sortRequired)
            {
                Array.Sort(mzArr, intensityArr);
            }

            // Centroid spectrum
            if (!ReadIsCentroidScan(scanNum))
            {
                // ProteoWizard
                var centroider = new Centroider(mzArr, intensityArr);
                double[] centroidedMzs, centroidedIntensities;
                centroider.GetCentroidedData(out centroidedMzs, out centroidedIntensities);
                mzArr = centroidedMzs;
                intensityArr = centroidedIntensities;
            }

            var elutionTime = RtFromScanNum(scanNum);
            var msLevel = ReadMsLevel(scanNum);
            if (msLevel == 1) return new Spectrum(mzArr, intensityArr, scanNum)
                {
                    ElutionTime = elutionTime
                };

            var isolationWindow = ReadPrecursorInfo(scanNum);

            var productSpec = new ProductSpectrum(mzArr, intensityArr, scanNum)
                {
                    MsLevel = msLevel,
                    ElutionTime = elutionTime,
                    ActivationMethod = GetActivationMethod(scanNum),
                    IsolationWindow = isolationWindow
                };
            return productSpec;
        }

        /// <summary>
        /// Reads the precursor information of the specified scan
        /// </summary>
        /// <param name="scanNum">scan number</param>
        /// <returns>precursor information</returns>
        public IsolationWindow ReadPrecursorInfo(int scanNum)
        {
            if (ReadMsLevel(scanNum) <= 1) return null;

            // Get Isolation Target m/z
            var isolationTargetMz = ReadIsolationWindowTargetMz(scanNum);

            // Get isolation window width
            //var isolationWindowWidth = ReadIsolationWidth(scanNum);
            var precursorInfo = ReadPrecursorInfoFromTrailerExtra(scanNum);
            
            var isolationWindowWidth = precursorInfo.IsolationWidth;
            var monoisotopicMz = precursorInfo.MonoisotopicMz;
            var charge = precursorInfo.Charge;

            return new IsolationWindow(
                isolationTargetMz, 
                isolationWindowWidth / 2, 
                isolationWindowWidth / 2,
                monoisotopicMz,
                charge
                );
        }

        public int GetMaxScanNum()
        {
            return _maxLcScan;
        }

        public int GetMinScanNum()
        {
            return _minLcScan;
        }

        public int ReadMsLevel(int scanNum)
        {
            int msLevel;
            if (_msLevel.TryGetValue(scanNum, out msLevel)) return msLevel;

            _msfileReader.GetMSOrderForScanNum(scanNum, ref msLevel);
            _msLevel[scanNum] = msLevel;

            return msLevel;
        }

        public double RtFromScanNum(int scanNum)
        {
            var rt = 0.0;
            _msfileReader.RTFromScanNum(scanNum, ref rt);
            return rt;
        }

        private readonly MSFileReaderLib.IXRawfile5 _msfileReader;
        private readonly int _minLcScan;
        private readonly int _maxLcScan;
        private readonly Dictionary<int, int> _msLevel;

        //private static DeconToolsPeakDetectorV2 _peakDetector;  // basic peak detector

        /// <summary>
        /// Returns whether the specified scan is a centroid scan
        /// </summary>
        /// <param name="scanNum">scan number</param>
        /// <returns>true if the specified scan is a centroid scan and false otherwise</returns>
        private bool ReadIsCentroidScan(int scanNum)
        {
            var isCentroid = 0;
            _msfileReader.IsCentroidScanForScanNum(scanNum, ref isCentroid);
            return isCentroid != 0;
        }

        /// <summary>
        /// Reads the isolation window target m/z
        /// </summary>
        /// <param name="scanNum">scan number</param>
        /// <returns>isolation window target m/z</returns>
        private double ReadIsolationWindowTargetMz(int scanNum)
        {
            string scanFilterString = null;
            _msfileReader.GetFilterForScanNum(scanNum, ref scanFilterString);

            var isolationTargetMz = -1.0;
            if (scanFilterString != null)
            {
                isolationTargetMz = ParseMzValueFromThermoScanInfo(scanFilterString);
            }
            return isolationTargetMz;
        }

        ///// <summary>
        ///// Reads the isolation window width in Th (e.g. 3 if +/- 1.5 Th)
        ///// </summary>
        ///// <param name="scanNum">scan number</param>
        ///// <returns>isolation width</returns>
        //private double ReadIsolationWidth(int scanNum)
        //{
        //    object value = null;
        //    _msfileReader.GetTrailerExtraValueForScanNum(scanNum, "MS2 Isolation Width:", ref value);

        //    return Convert.ToDouble(value);
        //}

        private PrecursorInfo ReadPrecursorInfoFromTrailerExtra(int scanNum)
        {
            if (ReadMsLevel(scanNum) == 1) return null;

            object objLabels = null;
            object objValues = null;
            var intArrayCount = 0;

            //if (scanNum == 18099)
            //{
            //    Console.WriteLine("***ScanNum: {0}", scanNum);
            //}

            _msfileReader.GetTrailerExtraForScanNum(scanNum, ref objLabels, ref objValues, ref intArrayCount);

            var labels = objLabels as string[];
            var values = objValues as string[];

            if (labels == null || values == null || intArrayCount <= 0) return null;

            var isolationWidth = 0.0;
            double? monoIsotopicMz = 0.0;
            int? charge = null;
            for (var i = 0; i < intArrayCount; i++)
            {
                //Console.WriteLine("{0}\t{1}", labels[i], values[i]);
                if (labels[i].Equals("Monoisotopic M/Z:"))
                {
                    monoIsotopicMz = Convert.ToDouble(values[i]);
                    if (monoIsotopicMz == 0.0) monoIsotopicMz = null;
                }
                else if (labels[i].Equals("Charge State:"))
                {
                    charge = Convert.ToInt32(values[i]);
                    if (charge == 0)
                    {
                        Console.WriteLine("charge is null");
                        charge = null;
                    }
                }
                else if (labels[i].Equals("MS2 Isolation Width:"))
                {
                    isolationWidth = Convert.ToDouble(values[i]);
                }
            }
            return new PrecursorInfo(isolationWidth, monoIsotopicMz, charge);
        }

        internal class PrecursorInfo
        {
            public PrecursorInfo(double isolationWidth, double? monoisotopicMz, int? charge)
            {
                MonoisotopicMz = monoisotopicMz;
                IsolationWidth = isolationWidth;
                Charge = charge;
            }

            internal double IsolationWidth { get; private set; }
            internal double? MonoisotopicMz { get; private set; }
            internal int? Charge { get; private set; }
        }

        private static double ParseMzValueFromThermoScanInfo(string scanFilterString)
        {
            const string pattern = @"(?<mz>[0-9.]+)@";

            var matchCid = Regex.Match(scanFilterString, pattern);

            if (matchCid.Success)
            {
                return Convert.ToDouble(matchCid.Groups["mz"].Value);
            }
            return -1;
        }

        private int ReadNumSpectra()
        {
            var numSpectra = 0;

            _msfileReader.GetNumSpectra(ref numSpectra);
            return numSpectra;
        }

        /// <summary>
        /// Gets the activation method
        /// </summary>
        /// <param name="scanNum">scan number</param>
        /// <returns>activation method</returns>
        private ActivationMethod GetActivationMethod(int scanNum)
        {
            var activationType = 0;
            _msfileReader.GetActivationTypeForScanNum(scanNum, ReadMsLevel(scanNum), ref activationType);
            switch (activationType)
            {
                case 0:
                    return ActivationMethod.CID;
                case 2:
                    return ActivationMethod.ECD;
                case 3:
                    return ActivationMethod.PQD;
                case 4:
                    return ActivationMethod.ETD;
                case 5:
                    return ActivationMethod.HCD;
                default:
                    return ActivationMethod.Unknown;
            }
        }

        public void Close()
        {
            if(_msfileReader != null) _msfileReader.Close();
        }
    }
}
