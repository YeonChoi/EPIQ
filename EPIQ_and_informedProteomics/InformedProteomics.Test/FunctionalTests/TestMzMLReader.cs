﻿using System;
using System.Diagnostics;
using System.Linq;
using InformedProteomics.Backend.MassSpecData;
using NUnit.Framework;

namespace InformedProteomics.Test.FunctionalTests
{
	[TestFixture]
	internal class TestMzMLReader
	{
		[Test]
		[TestCase(@"\\protoapps\UserData\Sangtae\TestData\SpecFiles\Online_Dig_v17_QC_Shew_Stab-02_c0-5_01_04Aug14_Alder_14-06-11.mzML", 17288)] // Centroid, Thermo/XCaliber
		[TestCase(@"\\protoapps\UserData\Sangtae\TestData\SpecFiles\QC_Shew_12_02_2_1Aug12_Cougar_12-06-11.mzML", 31163)] // Profile, Thermo/XCaliber
		[TestCase(@"\\protoapps\UserData\Sangtae\TestData\SpecFiles\QC_Shew_13_06_500ng_CID_2_9Aug14_Lynx_14-04-08.mzML", 5649)] // Centroid, Thermo/XCaliber
		[TestCase(@"\\protoapps\UserData\Sangtae\TestData\SpecFiles\CTRL_Dam_17022011_1.mzML", 18696)] // Centroid, Waters/MassLynx, mzML 1.0.0, requires the referenceable Param Group
		[TestCase(@"\\protoapps\UserData\Sangtae\TestData\SpecFiles\napedro_L120224_005_SW-400AQUA no background 2ul dilution 6.mzML", 78012)] // Centroid, ABI SCIex WIFF files
		[TestCase(@"\\protoapps\UserData\Sangtae\TestData\SpecFiles\VA139IMSMS.mzML", 3145)] // Centroid, Agilent QTOF
		[TestCase(@"\\protoapps\UserData\Sangtae\TestData\SpecFiles\VA139IMSMS.mzML.gz", 3145)] // Centroid, Agilent QTOF, gzipped
		[TestCase(@"\\protoapps\UserData\Sangtae\TestData\SpecFiles\VA139IMSMS_compressed.mzML", 3145)] // Centroid, Agilent QTOF, compressed binary data
		public void TestReadMzML(string filePath, int expectedSpectra)
		{
			Stopwatch timer = new Stopwatch();
			timer.Start();
			var reader = new MzMLReader(filePath);
			var spectra = reader.ReadAllSpectra();
			timer.Stop();
			Console.WriteLine("Time: " + timer.Elapsed);
			Assert.AreEqual(expectedSpectra, spectra.Count());
		}
	}
}
