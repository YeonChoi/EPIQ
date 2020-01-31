using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math.Comparers;
using InformedProteomics.Backend.Data.Spectrometry;
using Epiq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using NUnit.Framework;



namespace InformedProteomics.Test.TestEpiq
{
    [TestFixture]
    class TestEpiqforPaper
    {

        [Test]
        public void TestQuantifyDE6_20_30_10_1_5_10()
        {
            var tolerance = new Tolerance(5);
            var overwrite = true;
            var merge = false;

            Params.InitParams(@"DE",
             @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
             @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard");
            var basePath = @"E:\MassSpec\PaperData\DE6_New_ratio_QE\";
            var rawFilePath = basePath + @"RawFiles\";
            var searchFilePath = basePath + @"SearchResults\";
            var resultPath = basePath + @"Results\";
           // QuantifiedProteinGroupDictionary.GetQuantifiedProteins(rawFilePath, searchFilePath, resultPath, tolerance, overwrite, merge);
        }

        [Test]
        public void TestQuantifyDE3()
        {
            var tolerance = new Tolerance(5);
            var overwrite = true;
            var merge = true;

            Params.InitParams(@"DE3",
             @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standardized.s4t2g1c1.model",
             @"E:\MassSpec\libSVMModel\D-shifts_libSVMData_UniquePepSampled_QE_ThermoLC_2hrG.standard");

            var baseDir = @"E:\MassSpec\PaperData\DE3_LysC_QE\";

            foreach (var basePath in Directory.EnumerateDirectories(baseDir))
            {
                var rawFilePath = basePath + @"\RawFiles\";
                var searchFilePath = basePath + @"\SearchResults\";
                var resultPath = basePath + @"\Results\";
             //   QuantifiedProteinGroupDictionary.GetQuantifiedProteins(rawFilePath, searchFilePath, resultPath, tolerance, overwrite, merge, false, @"");
            }
        }


        [Test]
        public void DE3OverlapPeptidesBetweenEpiqAndMaxQuantPeptides()
        {
            for (var cond = 1; cond <= 3; cond++)
            {

                var eqMedianRatio = new[] {1 ,   1.0000 ,   1}; // 0.9400    1.0000    0.9909
                var mqMedianRatio = new[] { 1,    1.0000,   1}; // 0.9678    1.0000    0.9753
                var medianNormalizeFactor = 1; //median([mqall(:,1);mqall(:,2);mqall(:,3)])./median(epiqall(:))

                int[] inputratio;
                if (cond == 1) inputratio = new[] {1, 1, 1};
                else if (cond == 2) inputratio = new[] {1, 5, 10};
                else if (cond == 3) inputratio = new[] {1, 10, 20};
                else inputratio = new[] {1, 20, 50};

                var epiqFile = @"E:\MassSpec\PaperData\DE3_LysC_QE\quantFiles\peptides.tsv";
                var mqFile = String.Format(@"E:\MassSpec\PaperData\DE3_LysC_QE\MaxQuantResults\txt_DE_{0}_{1}_{2}\peptides.txt",
                    inputratio[0], inputratio[1], inputratio[2]);
                var outmFile = @"E:\MassSpec\PaperData\DE3_LysC_QE\mFiles\mqepiq" + cond + ".m";

                var epiq = new Dictionary<string, double[]>();
                var mq = new Dictionary<string, double[]>();

                var mqKey = @"Sequence";
                //var mqPep = @"PEP";
                var mqValues = new[] {@"Intensity L", @"Intensity M", @"Intensity H", @"Ratio H/M", @"Ratio H/L" };
                var mqRevs = new[] {@"Reverse",	@"Potential contaminant"};
                var epiqKey = @"Peptide";
                var epiqValues = new[]
                {
                    @"Quantity_1 (cond:" + cond + ")", @"Quantity_2 (cond:" + cond + ")",
                    @"Quantity_3 (cond:" + cond + ")",
                };

                var mqKeyIndex = 0;
                // var mqPepIndex = 0;
                var mqValueIndices = new int[mqValues.Length];
                var mqRevIndices = new int[mqRevs.Length];

                var epiqKeyIndex = 0;
                var epiqValueIndices = new int[epiqValues.Length];

                var start = true;
                foreach (var line in File.ReadLines(mqFile))
                {
                    var tokens = line.Split('\t');
                    if (start)
                    {
                        for (var i = 0; i < tokens.Length; i++)
                        {
                            if (tokens[i] == mqKey)
                            {
                                mqKeyIndex = i;
                                continue;
                            }
                            /*if (tokens[i] == mqPep)
                            {
                                mqPepIndex = i;
                                continue;
                            }*/

                            for (var j = 0; j < mqValues.Length; j++)
                            {
                                if (tokens[i] != mqValues[j]) continue;
                                mqValueIndices[j] = i;
                                break;
                            }

                            for (var j = 0; j < mqRevs.Length; j++)
                            {
                                if (tokens[i] != mqRevs[j]) continue;
                                mqRevIndices[j] = i;
                                break;
                            }
                        }

                        Console.Write(mqKeyIndex);
                        foreach (var j in mqValueIndices) Console.Write(@" {0}", j);
                        Console.WriteLine();
                        start = false;
                        continue;
                    }

                    var seq = tokens[mqKeyIndex]; //.Replace('I', 'L');
                    //var pep = double.Parse(tokens[mqPepIndex]);
                    //if (pep > .01) continue;
                    var skip = false;
                    foreach (var ri in mqRevIndices)
                    {
                        if (tokens[ri] == "+") skip = true;
                    }
                    if (skip) continue;

                    var values = new double[mqValueIndices.Length];

                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = double.Parse(tokens[mqValueIndices[i]]);
                    }
                    if (values.Max() <= 0) continue;
                    
                    mq[seq] = values;
                }

                start = true;
                foreach (var line in File.ReadLines(epiqFile))
                {
                    var tokens = line.Split('\t');
                    if (tokens[7].StartsWith("TRUE")) continue;
                    if (start)
                    {
                        for (var i = 0; i < tokens.Length; i++)
                        {
                            if (tokens[i] == epiqKey)
                            {
                                epiqKeyIndex = i;
                                continue;
                            }

                            for (var j = 0; j < epiqValues.Length; j++)
                            {
                                if (tokens[i] != epiqValues[j]) continue;
                                epiqValueIndices[j] = i;
                                break;
                            }
                        }

                        Console.Write(epiqKeyIndex);
                        foreach (var j in epiqValueIndices) Console.Write(@" {0}", j);
                        Console.WriteLine();
                        start = false;
                        continue;
                    }

                    var seq = tokens[epiqKeyIndex].Replace("+57.021", "");//.Replace('I', 'L');
                    var values = new double[epiqValueIndices.Length];

                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = double.Parse(tokens[epiqValueIndices[i]]);
                    }
                    if (values.Max() <= 0) continue;
                    epiq[seq] = values;
                }

                var overlappingPeptides = mq.Keys.Intersect(epiq.Keys).ToList();

                var outm = new StreamWriter(outmFile);
                outm.Write(@"clear;mqoverlap=[");
                foreach (var pep in overlappingPeptides)
                {
                    var qs = mq[pep];
                    foreach (var q in qs) outm.Write(@"{0},", q);
                    outm.Write(@";");
                }
                outm.WriteLine(@"];");

                outm.Write(@"epiqoverlap=[");
                foreach (var pep in overlappingPeptides)
                {
                    var qs = epiq[pep];
                    foreach (var q in qs) outm.Write(@"{0},", q);
                    outm.Write(@";");
                }
                outm.WriteLine(@"];");

                outm.Write(@"mqall=[");
                foreach (var pep in mq.Keys)
                {
                    var qs = mq[pep];
                    foreach (var q in qs) outm.Write(@"{0},", q);
                    outm.Write(@";");
                }
                outm.WriteLine(@"];");

                outm.Write(@"epiqall=[");
                foreach (var pep in epiq.Keys)
                {
                    var qs = epiq[pep];
                    foreach (var q in qs) outm.Write(@"{0},", q);
                    outm.Write(@";");
                }
                outm.WriteLine(@"];");

                outm.WriteLine(@"mqall(:,1) = mqall(:,1)./ {0};", mqMedianRatio[0]);
                outm.WriteLine(@"mqall(:,3) = mqall(:,3)./  {0};", mqMedianRatio[2]);
                //outm.WriteLine(@"mqall(:,4) = mqall(:,4)./  {0};", mqMedianRatio[0]);
                //outm.WriteLine(@"mqall(:,5) = mqall(:,5).*  {0};", mqMedianRatio[2]);

                outm.WriteLine(@"mqoverlap(:,1) = mqoverlap(:,1)./ {0};", mqMedianRatio[0]);
                outm.WriteLine(@"mqoverlap(:,3) = mqoverlap(:,3)./  {0};", mqMedianRatio[2]);
                //outm.WriteLine(@"mqoverlap(:,4) = mqoverlap(:,4)./  {0};", mqMedianRatio[0]);
                //outm.WriteLine(@"mqoverlap(:,5) = mqoverlap(:,5).*  {0};", mqMedianRatio[2]);

                outm.WriteLine(@"epiqall(:,1) = epiqall(:,1)./ {0};", eqMedianRatio[0]);
                outm.WriteLine(@"epiqall(:,3) = epiqall(:,3)./  {0};", eqMedianRatio[2]);

                outm.WriteLine(@"epiqoverlap(:,1) = epiqoverlap(:,1)./ {0};", eqMedianRatio[0]);
                outm.WriteLine(@"epiqoverlap(:,3) = epiqoverlap(:,3)./  {0};", eqMedianRatio[2]);


                outm.WriteLine(@"mqall(:,1:3) = mqall(:,1:3)./{0};", medianNormalizeFactor);//
                outm.WriteLine(@"mqoverlap(:,1:3) = mqoverlap(:,1:3)./{0};", medianNormalizeFactor);
                //outm.WriteLine(@"mqoverlap(:,1:3) = mqoverlap(:,1:3)./median([mqall(:,1);mqall(:,2);mqall(:,3)]).*median(epiqall(:));");

                outm.WriteLine(@"mqAllratio = [1./mqall(:,5) 1./mqall(:,4) ones(size(mqall(:,4))) ];");
                outm.WriteLine(@"mqOverlapratio = [1./mqoverlap(:,5) 1./mqoverlap(:,4) ones(size(mqoverlap(:,4)))];");

                outm.WriteLine(@"mqAllratio(:,1) = mqAllratio(:,1)./ {0};", mqMedianRatio[0]);
                outm.WriteLine(@"mqAllratio(:,3) = mqAllratio(:,3)./  {0};", mqMedianRatio[2]);
                outm.WriteLine(@"mqOverlapratio(:,1) = mqOverlapratio(:,1)./ {0};", mqMedianRatio[0]);
                outm.WriteLine(@"mqOverlapratio(:,3) = mqOverlapratio(:,3)./  {0};", mqMedianRatio[2]);
                outm.WriteLine(@"mqAllratio(isnan(mqAllratio)) = 0;");
                outm.WriteLine(@"mqOverlapratio(isnan(mqOverlapratio)) = 0;");
               

                outm.WriteLine(@"mqepiqoverlapL3QuantifiedIndices = epiqoverlap(:,3)~=0 & mqoverlap(:,3)~=0;");
                outm.WriteLine(@"epiqoverlapfqIndices = epiqoverlap(mqepiqoverlapL3QuantifiedIndices,:)~=0;");
                outm.WriteLine(@"mqoverlapfqIndices = mqoverlap(mqepiqoverlapL3QuantifiedIndices,1:3)~=0;");
                outm.WriteLine(@"mqall = mqall(:,1:3);mqoverlap = mqoverlap(:,1:3);");
                outm.WriteLine(
                    @"epiqoverlapInferredIntensityFromL3 = [ epiqoverlap(mqepiqoverlapL3QuantifiedIndices,3)/{0} epiqoverlap(mqepiqoverlapL3QuantifiedIndices,3)*{1}/{0} epiqoverlap(mqepiqoverlapL3QuantifiedIndices,3)];",
                    inputratio[2], inputratio[1]);
                outm.WriteLine(
                    @"mqoverlapInferredIntensityFromL3 = [ mqoverlap(mqepiqoverlapL3QuantifiedIndices,3)/{0} mqoverlap(mqepiqoverlapL3QuantifiedIndices,3)*{1}/{0} mqoverlap(mqepiqoverlapL3QuantifiedIndices,3)];",
                    inputratio[2], inputratio[1]);
                outm.WriteLine(
                    @"epiqUnquantifiedByEpiqInferred = epiqoverlapInferredIntensityFromL3(~(epiqoverlapfqIndices));");
                outm.WriteLine(
                    @"mqUnquantifiedByEpiqInferred = epiqoverlapInferredIntensityFromL3(~(mqoverlapfqIndices));");
                outm.WriteLine(
                    @"epiqUnquantifiedByMqInferred = mqoverlapInferredIntensityFromL3(~(epiqoverlapfqIndices));");
                outm.WriteLine(@"mqUnquantifiedByMqInferred = mqoverlapInferredIntensityFromL3(~(mqoverlapfqIndices));");
                outm.WriteLine(@"[epx, epy, mqx, mqy] = drawUnQuantifiedQuantityDistribution(epiqUnquantifiedByMqInferred, mqUnquantifiedByMqInferred);");
                outm.WriteLine(@"csvwrite('epiq_unqunatified_quantity_{0}_{1}_{2}_by_mq.csv',[epx;epy])", inputratio[0],inputratio[1],inputratio[2]);
                outm.WriteLine(@"csvwrite('mq_unqunatified_quantity_{0}_{1}_{2}_by_mq.csv',[mqx;mqy])", inputratio[0], inputratio[1], inputratio[2]);

                outm.WriteLine(@"[epx, epy, mqx, mqy] = drawUnQuantifiedQuantityDistribution(epiqUnquantifiedByEpiqInferred, mqUnquantifiedByEpiqInferred);");
                outm.WriteLine(@"csvwrite('epiq_unqunatified_quantity_{0}_{1}_{2}_by_epiq.csv',[epx;epy])", inputratio[0], inputratio[1], inputratio[2]);
                outm.WriteLine(@"csvwrite('mq_unqunatified_quantity_{0}_{1}_{2}_by_epiq.csv',[mqx;mqy])", inputratio[0], inputratio[1], inputratio[2]);

                outm.WriteLine(@"csvwrite('mq_quantity_{0}_{1}_{2}.csv', mqoverlap)", inputratio[0], inputratio[1], inputratio[2]);
                outm.WriteLine(@"csvwrite('mq_ratio_{0}_{1}_{2}.csv', mqOverlapratio)", inputratio[0], inputratio[1], inputratio[2]);
                outm.WriteLine(@"csvwrite('epiq_quantity_{0}_{1}_{2}.csv', epiqoverlap)", inputratio[0], inputratio[1], inputratio[2]);
                
                outm.WriteLine(@"inputratio=[{0} {1} {2}];", inputratio[0], inputratio[1], inputratio[2]);
                 outm.Close();
            }
        }

        [Test]
        public void DM3OverlapPeptidesBetweenEpiqAndMaxQuantPeptides()
        {
            for (var cond = 1; cond <= 4; cond++)
            {

                var eqMedianRatio = new[] { 1, 1.0000, 1 }; // 0.9400    1.0000    0.9909
                var mqMedianRatio = new[] { 1, 1.0000, 1 }; // 0.9678    1.0000    0.9753
                var medianNormalizeFactor = 1; //median([mqall(:,1);mqall(:,2);mqall(:,3)])./median(epiqall(:))

                int[] inputratio;
                if (cond == 1) inputratio = new[] { 1, 1, 1 };
                else if (cond == 2) inputratio = new[] { 1, 5, 10 };
                else if (cond == 3) inputratio = new[] { 1, 10, 20 };
                else inputratio = new[] { 1, 20, 50 };

                var epiqFile = @"E:\MassSpec\PaperData\DM3_LysC_QE\quantFiles\peptides.tsv";
                var mqFile = String.Format(@"E:\MassSpec\PaperData\DM3_LysC_QE\MaxQuantResults\txt_DM_{0}_{1}_{2}\peptides.txt",
                    inputratio[0], inputratio[1], inputratio[2]);
                var outmFile = @"E:\MassSpec\PaperData\DM3_LysC_QE\mFiles\mqepiq" + cond + ".m";

                var epiq = new Dictionary<string, double[]>();
                var mq = new Dictionary<string, double[]>();

                var mqKey = @"Sequence";
                //var mqPep = @"PEP";
                var mqValues = new[] { @"Intensity L", @"Intensity M", @"Intensity H", @"Ratio H/M", @"Ratio H/L" };
                var mqRevs = new[] { @"Reverse", @"Potential contaminant" };
                var epiqKey = @"Peptide";
                var epiqValues = new[]
                {
                    @"Quantity_1 (cond:" + cond + ")", @"Quantity_2 (cond:" + cond + ")",
                    @"Quantity_3 (cond:" + cond + ")",
                };

                var mqKeyIndex = 0;
                // var mqPepIndex = 0;
                var mqValueIndices = new int[mqValues.Length];
                var mqRevIndices = new int[mqRevs.Length];

                var epiqKeyIndex = 0;
                var epiqValueIndices = new int[epiqValues.Length];

                var start = true;
                foreach (var line in File.ReadLines(mqFile))
                {
                    var tokens = line.Split('\t');
                    if (start)
                    {
                        for (var i = 0; i < tokens.Length; i++)
                        {
                            if (tokens[i] == mqKey)
                            {
                                mqKeyIndex = i;
                                continue;
                            }
                            /*if (tokens[i] == mqPep)
                            {
                                mqPepIndex = i;
                                continue;
                            }*/

                            for (var j = 0; j < mqValues.Length; j++)
                            {
                                if (tokens[i] != mqValues[j]) continue;
                                mqValueIndices[j] = i;
                                break;
                            }

                            for (var j = 0; j < mqRevs.Length; j++)
                            {
                                if (tokens[i] != mqRevs[j]) continue;
                                mqRevIndices[j] = i;
                                break;
                            }
                        }

                        Console.Write(mqKeyIndex);
                        foreach (var j in mqValueIndices) Console.Write(@" {0}", j);
                        Console.WriteLine();
                        start = false;
                        continue;
                    }

                    var seq = tokens[mqKeyIndex]; //.Replace('I', 'L');
                    //var pep = double.Parse(tokens[mqPepIndex]);
                    //if (pep > .01) continue;
                    var skip = false;
                    foreach (var ri in mqRevIndices)
                    {
                        if (tokens[ri] == "+") skip = true;
                    }
                    if (skip) continue;

                    var values = new double[mqValueIndices.Length];

                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = double.Parse(tokens[mqValueIndices[i]]);
                    }
                    if (values.Max() <= 0) continue;

                    mq[seq] = values;
                }

                start = true;
                foreach (var line in File.ReadLines(epiqFile))
                {
                    var tokens = line.Split('\t');
                    if (tokens[7].StartsWith("TRUE")) continue;
                    if (start)
                    {
                        for (var i = 0; i < tokens.Length; i++)
                        {
                            if (tokens[i] == epiqKey)
                            {
                                epiqKeyIndex = i;
                                continue;
                            }

                            for (var j = 0; j < epiqValues.Length; j++)
                            {
                                if (tokens[i] != epiqValues[j]) continue;
                                epiqValueIndices[j] = i;
                                break;
                            }
                        }

                        Console.Write(epiqKeyIndex);
                        foreach (var j in epiqValueIndices) Console.Write(@" {0}", j);
                        Console.WriteLine();
                        start = false;
                        continue;
                    }

                    var seq = tokens[epiqKeyIndex].Replace("+57.021", "");//.Replace('I', 'L');
                    var values = new double[epiqValueIndices.Length];

                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = double.Parse(tokens[epiqValueIndices[i]]);
                    }
                    if (values.Max() <= 0) continue;
                    epiq[seq] = values;
                }

                var overlappingPeptides = mq.Keys.Intersect(epiq.Keys).ToList();

                var outm = new StreamWriter(outmFile);
                outm.Write(@"clear;mqoverlap=[");
                foreach (var pep in overlappingPeptides)
                {
                    var qs = mq[pep];
                    foreach (var q in qs) outm.Write(@"{0},", q);
                    outm.Write(@";");
                }
                outm.WriteLine(@"];");

                outm.Write(@"epiqoverlap=[");
                foreach (var pep in overlappingPeptides)
                {
                    var qs = epiq[pep];
                    foreach (var q in qs) outm.Write(@"{0},", q);
                    outm.Write(@";");
                }
                outm.WriteLine(@"];");

                outm.Write(@"mqall=[");
                foreach (var pep in mq.Keys)
                {
                    var qs = mq[pep];
                    foreach (var q in qs) outm.Write(@"{0},", q);
                    outm.Write(@";");
                }
                outm.WriteLine(@"];");

                outm.Write(@"epiqall=[");
                foreach (var pep in epiq.Keys)
                {
                    var qs = epiq[pep];
                    foreach (var q in qs) outm.Write(@"{0},", q);
                    outm.Write(@";");
                }
                outm.WriteLine(@"];");

                outm.WriteLine(@"mqall(:,1) = mqall(:,1)./ {0};", mqMedianRatio[0]);
                outm.WriteLine(@"mqall(:,3) = mqall(:,3)./  {0};", mqMedianRatio[2]);
                //outm.WriteLine(@"mqall(:,4) = mqall(:,4)./  {0};", mqMedianRatio[0]);
                //outm.WriteLine(@"mqall(:,5) = mqall(:,5).*  {0};", mqMedianRatio[2]);

                outm.WriteLine(@"mqoverlap(:,1) = mqoverlap(:,1)./ {0};", mqMedianRatio[0]);
                outm.WriteLine(@"mqoverlap(:,3) = mqoverlap(:,3)./  {0};", mqMedianRatio[2]);
                //outm.WriteLine(@"mqoverlap(:,4) = mqoverlap(:,4)./  {0};", mqMedianRatio[0]);
                //outm.WriteLine(@"mqoverlap(:,5) = mqoverlap(:,5).*  {0};", mqMedianRatio[2]);

                outm.WriteLine(@"epiqall(:,1) = epiqall(:,1)./ {0};", eqMedianRatio[0]);
                outm.WriteLine(@"epiqall(:,3) = epiqall(:,3)./  {0};", eqMedianRatio[2]);

                outm.WriteLine(@"epiqoverlap(:,1) = epiqoverlap(:,1)./ {0};", eqMedianRatio[0]);
                outm.WriteLine(@"epiqoverlap(:,3) = epiqoverlap(:,3)./  {0};", eqMedianRatio[2]);


                outm.WriteLine(@"mqall(:,1:3) = mqall(:,1:3)./{0};", medianNormalizeFactor);//
                outm.WriteLine(@"mqoverlap(:,1:3) = mqoverlap(:,1:3)./{0};", medianNormalizeFactor);
                //outm.WriteLine(@"mqoverlap(:,1:3) = mqoverlap(:,1:3)./median([mqall(:,1);mqall(:,2);mqall(:,3)]).*median(epiqall(:));");

                outm.WriteLine(@"mqAllratio = [1./mqall(:,5) 1./mqall(:,4) ones(size(mqall(:,4))) ];");
                outm.WriteLine(@"mqOverlapratio = [1./mqoverlap(:,5) 1./mqoverlap(:,4) ones(size(mqoverlap(:,4)))];");

                outm.WriteLine(@"mqAllratio(:,1) = mqAllratio(:,1)./ {0};", mqMedianRatio[0]);
                outm.WriteLine(@"mqAllratio(:,3) = mqAllratio(:,3)./  {0};", mqMedianRatio[2]);
                outm.WriteLine(@"mqOverlapratio(:,1) = mqOverlapratio(:,1)./ {0};", mqMedianRatio[0]);
                outm.WriteLine(@"mqOverlapratio(:,3) = mqOverlapratio(:,3)./  {0};", mqMedianRatio[2]);
                outm.WriteLine(@"mqAllratio(isnan(mqAllratio)) = 0;");
                outm.WriteLine(@"mqOverlapratio(isnan(mqOverlapratio)) = 0;");


                outm.WriteLine(@"mqepiqoverlapL3QuantifiedIndices = epiqoverlap(:,3)~=0 & mqoverlap(:,3)~=0;");
                outm.WriteLine(@"epiqoverlapfqIndices = epiqoverlap(mqepiqoverlapL3QuantifiedIndices,:)~=0;");
                outm.WriteLine(@"mqoverlapfqIndices = mqoverlap(mqepiqoverlapL3QuantifiedIndices,1:3)~=0;");
                outm.WriteLine(@"mqall = mqall(:,1:3);mqoverlap = mqoverlap(:,1:3);");
                outm.WriteLine(
                    @"epiqoverlapInferredIntensityFromL3 = [ epiqoverlap(mqepiqoverlapL3QuantifiedIndices,3)/{0} epiqoverlap(mqepiqoverlapL3QuantifiedIndices,3)*{1}/{0} epiqoverlap(mqepiqoverlapL3QuantifiedIndices,3)];",
                    inputratio[2], inputratio[1]);
                outm.WriteLine(
                    @"mqoverlapInferredIntensityFromL3 = [ mqoverlap(mqepiqoverlapL3QuantifiedIndices,3)/{0} mqoverlap(mqepiqoverlapL3QuantifiedIndices,3)*{1}/{0} mqoverlap(mqepiqoverlapL3QuantifiedIndices,3)];",
                    inputratio[2], inputratio[1]);
                outm.WriteLine(
                    @"epiqUnquantifiedByEpiqInferred = epiqoverlapInferredIntensityFromL3(~(epiqoverlapfqIndices));");
                outm.WriteLine(
                    @"mqUnquantifiedByEpiqInferred = epiqoverlapInferredIntensityFromL3(~(mqoverlapfqIndices));");
                outm.WriteLine(
                    @"epiqUnquantifiedByMqInferred = mqoverlapInferredIntensityFromL3(~(epiqoverlapfqIndices));");
                outm.WriteLine(@"mqUnquantifiedByMqInferred = mqoverlapInferredIntensityFromL3(~(mqoverlapfqIndices));");
                outm.WriteLine(@"[epx, epy, mqx, mqy] = drawUnQuantifiedQuantityDistribution(epiqUnquantifiedByMqInferred, mqUnquantifiedByMqInferred);");
                outm.WriteLine(@"csvwrite('epiq_unqunatified_quantity_{0}_{1}_{2}_by_mq.csv',[epx;epy])", inputratio[0], inputratio[1], inputratio[2]);
                outm.WriteLine(@"csvwrite('mq_unqunatified_quantity_{0}_{1}_{2}_by_mq.csv',[mqx;mqy])", inputratio[0], inputratio[1], inputratio[2]);

                outm.WriteLine(@"[epx, epy, mqx, mqy] = drawUnQuantifiedQuantityDistribution(epiqUnquantifiedByEpiqInferred, mqUnquantifiedByEpiqInferred);");
                outm.WriteLine(@"csvwrite('epiq_unqunatified_quantity_{0}_{1}_{2}_by_epiq.csv',[epx;epy])", inputratio[0], inputratio[1], inputratio[2]);
                outm.WriteLine(@"csvwrite('mq_unqunatified_quantity_{0}_{1}_{2}_by_epiq.csv',[mqx;mqy])", inputratio[0], inputratio[1], inputratio[2]);

                outm.WriteLine(@"csvwrite('mq_quantity_{0}_{1}_{2}.csv', mqoverlap)", inputratio[0], inputratio[1], inputratio[2]);
                outm.WriteLine(@"csvwrite('mq_ratio_{0}_{1}_{2}.csv', mqOverlapratio)", inputratio[0], inputratio[1], inputratio[2]);
                outm.WriteLine(@"csvwrite('epiq_quantity_{0}_{1}_{2}.csv', epiqoverlap)", inputratio[0], inputratio[1], inputratio[2]);

                outm.WriteLine(@"inputratio=[{0} {1} {2}];", inputratio[0], inputratio[1], inputratio[2]);
                outm.Close();
            }
        }

        [Test]
        public void OverlapPeptidesBetweenEpiqAndMaxQuantPsms()
        {
            for (var cond = 1; cond <= 4; cond++)
            {

                var eqMedianRatio = new[] { 1.0 / 1.0687, 1, 1.0 / 1.006 };
                var mqMedianRatio = new[] { 1.0 / 1.0333, 1, 1.0 / 1.0252 };
                var medianNormalizeFactor = 2.9171;//3.0728;

                int[] inputratio;
                if (cond == 1) inputratio = new[] { 1, 1, 1 };
                else if (cond == 2) inputratio = new[] { 1, 5, 10 };
                else if (cond == 3) inputratio = new[] { 1, 10, 20 };
                else inputratio = new[] { 1, 20, 50 };

                var epiqFiles = new []
                {
                    string.Format(@"E:\MassSpec\PaperData\DE3_LysC_QE\quantFiles\psms_c{0}r1f1.txt", cond),
                     string.Format(@"E:\MassSpec\PaperData\DE3_LysC_QE\quantFiles\psms_c{0}r2f1.txt", cond),
                      string.Format(@"E:\MassSpec\PaperData\DE3_LysC_QE\quantFiles\psms_c{0}r3f1.txt", cond),
                };
                var mqFile = String.Format(@"E:\MassSpec\PaperData\MaxQuantResults\txt_DE_{0}_{1}_{2}\evidence.txt",
                    inputratio[0], inputratio[1], inputratio[2]);
                var outmFile = @"E:\MassSpec\PaperData\DE3_LysC_QE\mFiles\mqepiqpsm" + cond + ".m";

                var epiq = new Dictionary<string, double[]>();
                var mq = new Dictionary<string, double[]>();

                var mqKeys = new List<string> { @"Raw file", @"MS/MS Scan Number", @"Sequence" };
                //var mqPep = @"PEP";
                var mqValues = new[] { @"Intensity L", @"Intensity M", @"Intensity H", @"Ratio H/M", @"Ratio M/L" };
                var epiqKeys = new List<string> { @"#SpecFile_MS-GF+", @"ScanNum_MS-GF+", @"Peptide" };
                var epiqValues = new[]
                {
                    @"Quantity_1", @"Quantity_2",
                    @"Quantity_3",
                };

                var mqKeyIndices = new List<int>();
                // var mqPepIndex = 0;
                var mqValueIndices = new int[mqValues.Length];

                var epiqKeyIndices = new List<int>();
                var epiqValueIndices = new int[epiqValues.Length];

                var start = true;
                foreach (var line in File.ReadLines(mqFile))
                {
                    var tokens = line.Split('\t');
                    if (start)
                    {
                        for (var i = 0; i < tokens.Length; i++)
                        {
                            if (mqKeys.Contains(tokens[i]))
                            {
                                mqKeyIndices.Add(i);
                                continue;
                            }
                            /*if (tokens[i] == mqPep)
                            {
                                mqPepIndex = i;
                                continue;
                            }*/

                            for (var j = 0; j < mqValues.Length; j++)
                            {
                                if (tokens[i] != mqValues[j]) continue;
                                mqValueIndices[j] = i;
                                break;
                            }
                        }

                        foreach (var j in mqKeyIndices) Console.Write(@" {0}", j); Console.Write(@" - ");
                        foreach (var j in mqValueIndices) Console.Write(@" {0}", j);
                        Console.WriteLine();
                        start = false;
                        continue;
                    }

                    var key = @"";//tokens[mqKeyIndex]; //.Replace('I', 'L');
                    foreach (var j in mqKeyIndices)
                    {
                        key += tokens[j];
                    }
                   
                    
                    //var pep = double.Parse(tokens[mqPepIndex]);
                    //if (pep > .01) continue;
                    var values = new double[mqValueIndices.Length];

                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = tokens[mqValueIndices[i]].Length == 0? 0 : double.Parse(tokens[mqValueIndices[i]]);
                    }
                    if (values.Max() <= 0) continue;

                    mq[key] = values;
                }
                foreach (var epiqFile in epiqFiles)
                {

                    start = true;
                    foreach (var line in File.ReadLines(epiqFile))
                    {
                        var tokens = line.Split('\t');
                        if (start)
                        {
                            for (var i = 0; i < tokens.Length; i++)
                            {
                                if (epiqKeys.Contains(tokens[i]))
                                {
                                    epiqKeyIndices.Add(i);
                                    continue;
                                }

                                for (var j = 0; j < epiqValues.Length; j++)
                                {
                                    if (tokens[i] != epiqValues[j]) continue;
                                    epiqValueIndices[j] = i;
                                    break;
                                }
                            }
                            foreach (var j in epiqKeyIndices) Console.Write(@" {0}", j);
                            Console.Write(@" - ");
                            foreach (var j in epiqValueIndices) Console.Write(@" {0}", j);
                            Console.WriteLine();
                            start = false;
                            continue;
                        }

                        var key = @""; //tokens[mqKeyIndex]; //.Replace('I', 'L');
                        string ModRegexStr = @"\+\d*\.\d*";

                        foreach (var j in epiqKeyIndices)
                        {
                            var k = tokens[j];
                            if (k.Contains(@"+"))
                            {
                                k = Regex.Replace(k, ModRegexStr, "");
                            }
                            
                            key += k.Replace(".mzML", "");
                        }
                        //Console.WriteLine(key);//AAAAAAALQAKDE_1_1_1_NCP1C2_J100_0519_029682
                        //var key = tokens[epiqKeyIndex].Replace("+57.021", "");//.Replace('I', 'L');
                        var values = new double[epiqValueIndices.Length];

                        for (var i = 0; i < values.Length; i++)
                        {
                            values[i] = double.Parse(tokens[epiqValueIndices[i]]);
                        }
                        if (values.Max() <= 0) continue;
                        epiq[key] = values;
                    }
                }
                var overlappingPeptides = mq.Keys.Intersect(epiq.Keys).ToList();

                var outm = new StreamWriter(outmFile);
                outm.Write(@"clear;mqoverlap=[");
                foreach (var pep in overlappingPeptides)
                {
                    var qs = mq[pep];
                    foreach (var q in qs) outm.Write(@"{0},", q);
                    outm.Write(@";");
                }
                outm.WriteLine(@"];");

                outm.Write(@"epiqoverlap=[");
                foreach (var pep in overlappingPeptides)
                {
                    var qs = epiq[pep];
                    foreach (var q in qs) outm.Write(@"{0},", q);
                    outm.Write(@";");
                }
                outm.WriteLine(@"];");

                outm.Write(@"mqall=[");
                foreach (var pep in mq.Keys)
                {
                    var qs = mq[pep];
                    foreach (var q in qs) outm.Write(@"{0},", q);
                    outm.Write(@";");
                }
                outm.WriteLine(@"];");

                outm.Write(@"epiqall=[");
                foreach (var pep in epiq.Keys)
                {
                    var qs = epiq[pep];
                    foreach (var q in qs) outm.Write(@"{0},", q);
                    outm.Write(@";");
                }
                outm.WriteLine(@"];");

                outm.WriteLine(@"mqall(:,1) = mqall(:,1)./ {0};", mqMedianRatio[0]);
                outm.WriteLine(@"mqall(:,3) = mqall(:,3)./  {0};", mqMedianRatio[2]);
                outm.WriteLine(@"mqall(:,4) = mqall(:,4)./  {0};", mqMedianRatio[0]);
                outm.WriteLine(@"mqall(:,5) = mqall(:,5).*  {0};", mqMedianRatio[2]);

                outm.WriteLine(@"mqoverlap(:,1) = mqoverlap(:,1)./ {0};", mqMedianRatio[0]);
                outm.WriteLine(@"mqoverlap(:,3) = mqoverlap(:,3)./  {0};", mqMedianRatio[2]);
                outm.WriteLine(@"mqoverlap(:,4) = mqoverlap(:,4)./  {0};", mqMedianRatio[0]);
                outm.WriteLine(@"mqoverlap(:,5) = mqoverlap(:,5).*  {0};", mqMedianRatio[2]);

                outm.WriteLine(@"epiqall(:,1) = epiqall(:,1)./ {0};", eqMedianRatio[0]);
                outm.WriteLine(@"epiqall(:,3) = epiqall(:,3)./  {0};", eqMedianRatio[2]);

                outm.WriteLine(@"epiqoverlap(:,1) = epiqoverlap(:,1)./ {0};", eqMedianRatio[0]);
                outm.WriteLine(@"epiqoverlap(:,3) = epiqoverlap(:,3)./  {0};", eqMedianRatio[2]);


                outm.WriteLine(@"mqall(:,1:3) = mqall(:,1:3)./{0};", medianNormalizeFactor);//
                outm.WriteLine(@"mqoverlap(:,1:3) = mqoverlap(:,1:3)./{0};", medianNormalizeFactor);
                //outm.WriteLine(@"mqoverlap(:,1:3) = mqoverlap(:,1:3)./median([mqall(:,1);mqall(:,2);mqall(:,3)]).*median(epiqall(:));");

                outm.WriteLine(@"mqAllratio = [1./mqall(:,5) ones(size(mqall(:,4))) mqall(:,4) ];");
                outm.WriteLine(@"mqOverlapratio = [1./mqoverlap(:,5) ones(size(mqoverlap(:,4))) mqoverlap(:,4) ];");
                outm.WriteLine(@"mqepiqoverlapL3QuantifiedIndices = epiqoverlap(:,3)~=0 & mqoverlap(:,3)~=0;");
                outm.WriteLine(@"epiqoverlapfqIndices = epiqoverlap(mqepiqoverlapL3QuantifiedIndices,:)~=0;");
                outm.WriteLine(@"mqoverlapfqIndices = mqoverlap(mqepiqoverlapL3QuantifiedIndices,1:3)~=0;");
                outm.WriteLine(@"mqall = mqall(:,1:3);mqoverlap = mqoverlap(:,1:3);");
                outm.WriteLine(
                    @"epiqoverlapInferredIntensityFromL3 = [ epiqoverlap(mqepiqoverlapL3QuantifiedIndices,3)/{0} epiqoverlap(mqepiqoverlapL3QuantifiedIndices,3)*{1}/{0} epiqoverlap(mqepiqoverlapL3QuantifiedIndices,3)];",
                    inputratio[2], inputratio[1]);
                outm.WriteLine(
                    @"mqoverlapInferredIntensityFromL3 = [ mqoverlap(mqepiqoverlapL3QuantifiedIndices,3)/{0} mqoverlap(mqepiqoverlapL3QuantifiedIndices,3)*{1}/{0} mqoverlap(mqepiqoverlapL3QuantifiedIndices,3)];",
                    inputratio[2], inputratio[1]);
                outm.WriteLine(
                    @"epiqUnquantifiedByEpiqInferred = epiqoverlapInferredIntensityFromL3(~(epiqoverlapfqIndices));");
                outm.WriteLine(
                    @"mqUnquantifiedByEpiqInferred = epiqoverlapInferredIntensityFromL3(~(mqoverlapfqIndices));");
                outm.WriteLine(
                    @"epiqUnquantifiedByMqInferred = mqoverlapInferredIntensityFromL3(~(epiqoverlapfqIndices));");
                outm.WriteLine(@"mqUnquantifiedByMqInferred = mqoverlapInferredIntensityFromL3(~(mqoverlapfqIndices));");
                outm.WriteLine(@"[epx, epy, mqx, mqy] = drawUnQuantifiedQuantityDistribution(epiqUnquantifiedByMqInferred, mqUnquantifiedByMqInferred);");
                outm.WriteLine(@"csvwrite('epiq_unqunatified_quantity_{0}_{1}_{2}_by_mq.csv',[epx;epy])", inputratio[0], inputratio[1], inputratio[2]);
                outm.WriteLine(@"csvwrite('mq_unqunatified_quantity_{0}_{1}_{2}_by_mq.csv',[mqx;mqy])", inputratio[0], inputratio[1], inputratio[2]);

                outm.WriteLine(@"[epx, epy, mqx, mqy] = drawUnQuantifiedQuantityDistribution(epiqUnquantifiedByEpiqInferred, mqUnquantifiedByEpiqInferred);");
                outm.WriteLine(@"csvwrite('epiq_unqunatified_quantity_{0}_{1}_{2}_by_epiq.csv',[epx;epy])", inputratio[0], inputratio[1], inputratio[2]);
                outm.WriteLine(@"csvwrite('mq_unqunatified_quantity_{0}_{1}_{2}_by_epiq.csv',[mqx;mqy])", inputratio[0], inputratio[1], inputratio[2]);

                outm.WriteLine(@"csvwrite('mq_quantity_{0}_{1}_{2}.csv', mqoverlap)", inputratio[0], inputratio[1], inputratio[2]);
                outm.WriteLine(@"csvwrite('mq_ratio_{0}_{1}_{2}.csv', mqOverlapratio)", inputratio[0], inputratio[1], inputratio[2]);
                outm.WriteLine(@"csvwrite('epiq_quantity_{0}_{1}_{2}.csv', epiqoverlap)", inputratio[0], inputratio[1], inputratio[2]);

                outm.Close();
            }
        }

        private double GetCv(double[] vs)
        {
            if (vs.Length < 3) return double.NaN;
            var mu = vs.Mean();
            var sum = .0;
            foreach (var v in vs)
            {
                sum += (v - mu) * (v - mu);
            }

            return 100.0 * Math.Sqrt(sum/(vs.Length-1)) / mu;
        }

        [Test]
        public void GetPeptideCVsTMTvsDEMQ()
        {
            // foreach (var line in File.ReadLines(mqFile))
            // tmt first
            var tmtFile = @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\TMT_MS2_Embryo_results.txt";
            var dePepFile = @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\MQ_combined_SET1\txt\peptides_De_01.txt";
            var deProFile = @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\MQ_combined_SET1\txt\proteinGroups_DE_SET01.txt";
            
            var tmtDictionary = new Dictionary<string, Dictionary<string,double[]>>(); // protein - peptide -  ratios
            var deDictionary = new Dictionary<string, Dictionary<string, double[]>>();
            var proPepSet = new HashSet<string>();

            var currentProteinRatios = new double[3];
            foreach (var line in File.ReadLines(tmtFile))
            {

                if (line.StartsWith(@"Checked") || line.StartsWith("\tChecked") || line.StartsWith("\t\tChecked") || line.StartsWith("\t\t")) continue;
                var tokens = line.Split('\t');
                if (line.StartsWith("FALSE")) // protein 22 23 24
                {
                  //  Console.WriteLine(line);

                    if (tokens[22].Length == 0 || tokens[23].Length == 0 || tokens[24].Length == 0  || int.Parse(tokens[10]) < 3)
                        currentProteinRatios = new double[2];
                    else
                    {
                        currentProteinRatios[0] = double.Parse(tokens[22]);
                        currentProteinRatios[1] = double.Parse(tokens[23]);
                        currentProteinRatios[1] = double.Parse(tokens[24]);
                    }
                    continue;
                }

                // peptide
                if (int.Parse(tokens[9]) > 1) continue; // single protein i.e., unique peptide
                if (tokens[21].Length == 0 || tokens[22].Length == 0 || tokens[23].Length == 0) continue;

                var pep = tokens[3].Split('.')[1];
                var pro = tokens[11];
                proPepSet.Add(pep + ";" + pro);

                var ratio = new[] { double.Parse(tokens[21]), double.Parse(tokens[22]), double.Parse(tokens[23]) };

                Dictionary<string, double[]> val;
                if (!tmtDictionary.TryGetValue(pro, out val)) tmtDictionary[pro] = val = new Dictionary<string, double[]>();
                val[pep] = ratio;
            }

            var deProteins = new Dictionary<string, double[]>();

            foreach (var line in File.ReadLines(deProFile))
            {
                if (line.StartsWith(@"Protein IDs")) continue;
                var tokens = line.Split('\t');
                if (tokens[0].Contains(";") || int.Parse(tokens[9]) < 3) continue;
                var ratio = new[] { double.Parse(tokens[46]), double.Parse(tokens[52]), double.Parse(tokens[58]) };
                deProteins[tokens[0]] = ratio;
            }


            foreach (var line in File.ReadLines(dePepFile)) // 33 proteins 69 75
            {
                if (line.StartsWith(@"Sequence")) continue;
                var tokens = line.Split('\t');
                if (tokens[33].Contains(";") || !deProteins.ContainsKey(tokens[33])) continue;
                if (tokens[38] == @"no") continue; // only unique peps

                var pep = tokens[0];
                var pro = tokens[33];
                if (!proPepSet.Contains(pep + ";" + pro)) continue;

                var ratio = new[] { double.Parse(tokens[69]), double.Parse(tokens[75]), double.Parse(tokens[81]) };
                //var pRatio = deProteins[tokens[33]];

                Dictionary<string, double[]> val;
                if (!deDictionary.TryGetValue(pro, out val)) deDictionary[pro] = val = new Dictionary<string, double[]>();
                val[pep] = ratio;
                //{Math.Abs(Math.Log10(ratio[0] / pRatio[0])/Math.Log10(pRatio[0])), Math.Abs(Math.Log10(ratio[1] / pRatio[1])/Math.Log10(pRatio[1])) };

            }

            Console.Write(@"cvs=[");
            foreach (var pro in deDictionary.Keys)
            {
                var tmt = tmtDictionary[pro];
                var de = deDictionary[pro];

                var tmtRs1 = new List<double>();
                var deRs1 = new List<double>();

                var tmtRs2 = new List<double>();
                var deRs2 = new List<double>();

                var tmtRs3 = new List<double>();
                var deRs3 = new List<double>();

                foreach (var pep in de.Keys)
                {
                    var tmtRs = tmt[pep];
                    var deRs = de[pep];

                    tmtRs1.Add(tmtRs[0]);
                    tmtRs2.Add(tmtRs[1]);
                    tmtRs3.Add(tmtRs[2]);

                    deRs1.Add(deRs[0]);
                    deRs2.Add(deRs[1]);
                    deRs3.Add(deRs[2]);
                }

                if(tmtRs1.Contains(0)) continue;
                if (tmtRs2.Contains(0)) continue;
                if (tmtRs3.Contains(0)) continue;
                if (deRs1.Contains(0)) continue;
                if (deRs2.Contains(0)) continue;
                if (deRs3.Contains(0)) continue;
                
                var tmtCv1 = GetCv(tmtRs1.ToArray());
                var tmtCv2 = GetCv(tmtRs2.ToArray());
                var tmtCv3 = GetCv(tmtRs3.ToArray());
                var deCv1 = GetCv(deRs1.ToArray());
                var deCv2 = GetCv(deRs2.ToArray());
                var deCv3 = GetCv(deRs3.ToArray());

                if (double.IsNaN(tmtCv1) || tmtCv1 <= 0) continue;
                if (double.IsNaN(tmtCv2) || tmtCv2 <= 0) continue;
                if (double.IsNaN(tmtCv3) || tmtCv3 <= 0) continue;
                if (double.IsNaN(deCv1) || deCv1 <= 0) continue;
                if (double.IsNaN(deCv2) || deCv2 <= 0) continue;
                if (double.IsNaN(deCv3) || deCv3 <= 0) continue;

                Console.Write(@"{0},{1},{2},{3},{4},{5};", deCv1, tmtCv1, deCv2, tmtCv2, deCv3, tmtCv3);

            }
            Console.WriteLine(@"];");

        }


        [Test]
        public void GetPeptideCVsTMTvsDEEPIQ()
        {
            // foreach (var line in File.ReadLines(mqFile))
            // tmt first
            var tmtFile = @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\TMT_MS2_Embryo_results.txt";
            var dePepFile = @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\quantFiles\PeptidesPerConditionReplicate\peptides_c1r1_Embryo_1_8_13_0814_J100_NCP1_FN.tsv";
            var deProFile = @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\quantFiles\ProteinGroupsPerConditionReplicate\proteinGroups_c1r1_Embryo_1_8_13_0814_J100_NCP1_FN.tsv";

            var tmtDictionary = new Dictionary<string, Dictionary<string, double[]>>(); // protein - peptide -  ratios
            var deDictionary = new Dictionary<string, Dictionary<string, double[]>>();
            var proPepSet = new HashSet<string>();

            var currentProteinRatios = new double[3];
            foreach (var line in File.ReadLines(tmtFile))
            {

                if (line.StartsWith(@"Checked") || line.StartsWith("\tChecked") || line.StartsWith("\t\tChecked") || line.StartsWith("\t\t")) continue;
                var tokens = line.Split('\t');
                if (line.StartsWith("FALSE")) // protein 22 23 24
                {
                    //  Console.WriteLine(line);

                    if (tokens[22].Length == 0 || tokens[23].Length == 0 || tokens[24].Length == 0 || int.Parse(tokens[10]) < 3)
                        currentProteinRatios = new double[2];
                    else
                    {
                        currentProteinRatios[0] = double.Parse(tokens[22]);
                        currentProteinRatios[1] = double.Parse(tokens[23]);
                        currentProteinRatios[1] = double.Parse(tokens[24]);
                    }
                    continue;
                }

                // peptide
                if (int.Parse(tokens[9]) > 1) continue; // single protein i.e., unique peptide
                if (tokens[21].Length == 0 || tokens[22].Length == 0 || tokens[23].Length == 0) continue;

                var pep = tokens[3].Split('.')[1];
                var pro = tokens[11];
                proPepSet.Add(pep + ";" + pro);

                var ratio = new[] { double.Parse(tokens[21]), double.Parse(tokens[22]), double.Parse(tokens[23]) };

                Dictionary<string, double[]> val;
                if (!tmtDictionary.TryGetValue(pro, out val)) tmtDictionary[pro] = val = new Dictionary<string, double[]>();
                val[pep] = ratio;
            }

            var deProteins = new HashSet<string>();

            foreach (var line in File.ReadLines(deProFile))
            {
                if (line.StartsWith(@"ProteinGroup")) continue;
                var tokens = line.Split('\t');
                if (tokens[0].Contains(";")) continue;
                deProteins.Add(tokens[0]);
            }

            foreach (var line in File.ReadLines(dePepFile)) // 33 proteins 69 75
            {
                if (line.StartsWith(@"Peptide")) continue;
                var tokens = line.Split('\t');
                if (tokens[1].Contains(";") || !deProteins.Contains(tokens[1])) continue;

                var pep = tokens[0].Replace("C+57.021", "C").Replace("M+15.995", "M");
                var pro = tokens[1];
                if (!proPepSet.Contains(pep + ";" + pro)) continue;
                //13 14 15
                if (double.Parse(tokens[17]) < 2) continue;
                var aratio = new[] { double.Parse(tokens[13]), double.Parse(tokens[14]), double.Parse(tokens[15]) };
                //var pRatio = deProteins[tokens[33]];
                if (aratio.Contains(0)) continue;
                var ratio = new[] {aratio[1]/aratio[0], aratio[2]/aratio[0], aratio[2]/aratio[1]};
                Dictionary<string, double[]> val;
                if (!deDictionary.TryGetValue(pro, out val)) deDictionary[pro] = val = new Dictionary<string, double[]>();
                val[pep] = ratio;
                //{Math.Abs(Math.Log10(ratio[0] / pRatio[0])/Math.Log10(pRatio[0])), Math.Abs(Math.Log10(ratio[1] / pRatio[1])/Math.Log10(pRatio[1])) };

            }

            Console.Write(@"cvseqiq=[");
            foreach (var pro in deDictionary.Keys)
            {
                var tmt = tmtDictionary[pro];
                var de = deDictionary[pro];

                var tmtRs1 = new List<double>();
                var deRs1 = new List<double>();

                var tmtRs2 = new List<double>();
                var deRs2 = new List<double>();

                var tmtRs3 = new List<double>();
                var deRs3 = new List<double>();

                foreach (var pep in de.Keys)
                {
                    var tmtRs = tmt[pep];
                    var deRs = de[pep];

                    tmtRs1.Add(tmtRs[0]);
                    tmtRs2.Add(tmtRs[1]);
                    tmtRs3.Add(tmtRs[2]);

                    deRs1.Add(deRs[0]);
                    deRs2.Add(deRs[1]);
                    deRs3.Add(deRs[2]);
                }

                if (tmtRs1.Contains(0)) continue;
                if (tmtRs2.Contains(0)) continue;
                if (tmtRs3.Contains(0)) continue;
                if (deRs1.Contains(0)) continue;
                if (deRs2.Contains(0)) continue;
                if (deRs3.Contains(0)) continue;


                var tmtCv1 = GetCv(tmtRs1.ToArray());
                var tmtCv2 = GetCv(tmtRs2.ToArray());
                var tmtCv3 = GetCv(tmtRs3.ToArray());
                var deCv1 = GetCv(deRs1.ToArray());
                var deCv2 = GetCv(deRs2.ToArray());
                var deCv3 = GetCv(deRs3.ToArray());

                if (double.IsNaN(tmtCv1) || tmtCv1 <= 0) continue;
                if (double.IsNaN(tmtCv2) || tmtCv2 <= 0) continue;
                if (double.IsNaN(tmtCv3) || tmtCv3 <= 0) continue;
                if (double.IsNaN(deCv1) || deCv1 <= 0) continue;
                if (double.IsNaN(deCv2) || deCv2 <= 0) continue;
                if (double.IsNaN(deCv3) || deCv3 <= 0) continue;

                Console.Write(@"{0},{1},{2},{3},{4},{5};", deCv1, tmtCv1, deCv2, tmtCv2, deCv3, tmtCv3);

            }
            Console.WriteLine(@"];");

        }

        [Test]
        public void GetCorrelationBetweenTwoTsvs() // tsv 1: DE intensities, tsv 2: MQ TMT ratios
        {
            var tsv1 =
                @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\quantFiles\ProteinGroupsPerConditionReplicate\proteinGroups_c1r1_Embryo_1_8_13_0814_J100_NCP1_FN.tsv";
            //@"E:\MassSpec\PaperData\DE3_LysC_XenopusEmbryo_QE\Results\Embryo_1_8_13_0814_J100_NCP1_FN01/ProteinList.tsv";
            var tsv2 = @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\MQ_combined_SET1\txt\proteinGroups_DE_SET01.txt";
            // @"E:\MassSpec\PaperData\DE3_LysC_XenopusEmbryo_QE\Results\Embryo_1_8_13_0814_J100_NCP1_FN01/proteinGroups_DE_MQ_Original file.txt";     

            var outDir = @"E:\MassSpec\PaperData\DE3_LysC_QE\DE3_Xenopus\quantFiles\";
            var outm = outDir + @"corr.m";
            //@"E:\MassSpec\PaperData\DE3_LysC_XenopusEmbryo_QE\Results\Embryo_1_8_13_0814_J100_NCP1_FN01/corr.m";

            var outBothTsv = outDir + @"both.tsv";

            var dic1 = new Dictionary<HashSet<string>, List<double>>(HashSet<string>.CreateSetComparer());
            var dic2 = new Dictionary<HashSet<string>, List<double>>(HashSet<string>.CreateSetComparer());

            var pro1 = 1;
            var pro2 = 1;

            var qs1 = new[] { 14, 16 }; // ratios ... // 1 vs 13 
            var qs2 = new[] { 52 };//

            foreach (var line in File.ReadLines(tsv1))
            {
                var tokens = line.Split('\t');

                if (line.StartsWith(@"Protein"))
                {
                    foreach (var i in qs1) Console.Write(tokens[i] + @" ");
                    Console.WriteLine();
                    continue;
                }
                var ratios = new List<double>();
                var nan = false;
                foreach (var i in qs1)
                {
                    if (double.IsNaN(double.Parse(tokens[i]))) nan = true;
                    ratios.Add(double.Parse(tokens[i]));
                }
                if (nan) continue;
                var k = new HashSet<string>();
                foreach (var tt in tokens[pro1].Split(';'))
                {
                    if (tt.Length == 0) continue;
                    k.Add(tt);
                }
                dic1[k] = ratios;
            }

            foreach (var line in File.ReadLines(tsv2))
            {
                var tokens = line.Split('\t');
                if (line.StartsWith(@"Protein"))
                {
                    foreach (var i in qs2) Console.Write(tokens[i] + @" ");
                    Console.WriteLine();
                    continue;
                }
                var intensities = new List<double>();
                var nan = false;
                foreach (var i in qs2)
                {
                    if (double.IsNaN(double.Parse(tokens[i]))) nan = true;
                    intensities.Add(double.Parse(tokens[i]));
                }
                if (nan) continue;
                var k = new HashSet<string>();
                foreach (var tt in tokens[pro2].Split(';'))
                {
                    if (tt.Length == 0) continue;
                    k.Add(tt);
                }
                dic2[k] = intensities;
            }

            var btsv = new StreamWriter(outBothTsv);
          
            btsv.WriteLine("ProteinGroup1\tProteinGroup2\tRatio DE\tRatio MQ");
          
            var overlaps = new HashSet<HashSet<string>>();
            foreach (var pr in dic1.Keys)
            {
                var overlap = new HashSet<string>();
                foreach (var pr2 in dic2.Keys)
                {
                    if (pr.Intersect(pr2).Any())
                    {
                        overlap = pr2;
                        break;
                    }
                }
                if (overlap.Any()) // both
                {
                    overlaps.Add(overlap);
                    foreach (var p in pr) btsv.Write(p + @";");
                    btsv.Write("\t");
                    foreach (var p in overlap) btsv.Write(p + @";");
                    btsv.Write("\t");
                    btsv.Write("{0}\t", dic1[pr][1]/dic1[pr][0]);
                    btsv.Write("{0}", dic2[overlap][0]);
                    btsv.WriteLine();
                }
               
            }
           
            btsv.Close();
           

            var m = new StreamWriter(outm);

            m.WriteLine(@"Ratio1=[");
            foreach (var pr in dic1.Keys)
            {
                if (!dic2.ContainsKey(pr)) continue;
                if (dic1[pr].Min() == 0 || dic2[pr].Min() == 0) continue;
                //foreach (var i in dic1[pr]) m.Write(@"{0},", i);
                m.Write("{0},", dic1[pr][1] / dic1[pr][0]);

                m.Write(@" %");
                foreach (var p in pr) m.Write(p + ";");
                m.WriteLine();
            }
            m.WriteLine(@"];");

            m.WriteLine(@"Ratio2=[");
            foreach (var pr in dic1.Keys)
            {
                if (!dic2.ContainsKey(pr)) continue;
                if (dic1[pr].Min() == 0 || dic2[pr].Min() == 0) continue;
                m.Write("{0}", dic2[pr][0]);
                m.Write(@" %");
                foreach (var p in pr) m.Write(p + ";");
                m.WriteLine();
            }
            m.WriteLine(@"];");


            m.Close();




        }

    }
}
