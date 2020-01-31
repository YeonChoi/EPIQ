using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using InformedProteomics.Backend.Database;
using Microsoft.Office.Interop.Excel;

namespace Epiq
{
    public class QuantifiedPeptideDictionary : Dictionary<string, Dictionary<Tuple<short, short, short>, QuantifiedPeptide>> // pep seq, cond, rep, frac, qpep 
    {

        public void Add(QuantifiedPeptide peptide, short c, short r, short f)
        {
            Dictionary<Tuple<short, short, short>, QuantifiedPeptide> v;
            if(!TryGetValue(peptide.Peptide, out v)) this[peptide.Peptide] = v = new Dictionary<Tuple<short, short, short>, QuantifiedPeptide>();
            var t = new Tuple<short, short, short>(c, r, f);
            v[t] = peptide;
        }

        public void Filter(float qvalueThrehsold, float snrThreshold)
        {
            var cs = new HashSet<short>();
            var rs = new HashSet<short>();
            foreach (var v in Values)
            {
                foreach (var k in v.Keys)
                {
                    cs.Add(k.Item1);
                    rs.Add(k.Item2);
                }
            }

            foreach (var c in cs)
            {
                foreach (var r in rs) Filter(c, r, qvalueThrehsold, snrThreshold);
            }
        }

        private void Filter(short c, short r, float qvalueThreshold, float snrThreshold)
        {
            var pepList = new List<QuantifiedPeptide>();
            var key = new Tuple<short, short, short>(c, r, 0);
            foreach (var v in Values)
            {
                QuantifiedPeptide pep;
                if(v.TryGetValue(key, out pep)) pepList.Add(pep);
            }
            if (pepList.Count == 0) return;
            AssignQvalues(pepList);
            foreach (var pep in pepList)
            {
               
                if (pep.Qvalue <= qvalueThreshold && pep.GetSnr() >= snrThreshold){
                   // this[pep.Peptide][key].SetQvalue(1); // TODO
                    continue; //!this[i].Id.IsDecoy() && 
                }               
                // pep seq, cond, rep, frac, qpep
                this[pep.Peptide].Remove(key);
                if (this[pep.Peptide].Count == 0) Remove(pep.Peptide);
            }

        }


        private void AssignQvalues(List<QuantifiedPeptide> peps)
        {
            var peptideDictionary = new Dictionary<float, List<QuantifiedPeptide>>();
            foreach (var pep in peps)
            {
                var score = pep.QvalueScore;
                List<QuantifiedPeptide> pepList;
                if (peptideDictionary.TryGetValue(score, out pepList)) pepList.Add(pep);
                else peptideDictionary[score] = new List<QuantifiedPeptide> { pep };
            }

            var scores = new List<float>();
            scores.AddRange(peptideDictionary.Keys);
            scores.Sort();

            var numDecoy = 0f;
            var numTarget = 0f;
            var fdr = new float[scores.Count];
            for (var i = 0; i < scores.Count; i++)
            {
                var score = scores[i];
                foreach (var pep in peptideDictionary[score])
                {
                    if (pep.IsDecoy()) numDecoy++;
                    else numTarget++;
                }
                fdr[i] = Math.Min(numDecoy / (numDecoy + numTarget), 1.0f);
            }
            //Console.WriteLine(numDecoy + " " + numTarget + " " + scores.Max());
            var qValue = new float[fdr.Length];
            qValue[fdr.Length - 1] = fdr[fdr.Length - 1];
            for (var i = fdr.Length - 2; i >= 0; i--)
                qValue[i] = Math.Min(qValue[i + 1], fdr[i]);

            foreach (var pep in peps)
            {
                var score = pep.QvalueScore;
                var index = scores.BinarySearch(score);
                if (index < 0) index = ~index;
                pep.SetQvalue(qValue[Math.Min(index, qValue.Length - 1)]);
             //   Console.WriteLine(pep.Qvalue);
            }
        }


        private short[,] GetMinMaxConditionReplicateFraction(out HashSet<Tuple<short, short, short>> keys)
        {
            var ret = new short[2,3];

            ret[0, 0] = ret[0, 1] = ret[0, 2] = short.MaxValue;
            ret[1, 0] = ret[1, 1] = ret[1, 2] = short.MinValue;
            keys = new HashSet<Tuple<short, short, short>>();

            foreach (var t in Values)
            {
                foreach (var s in t.Keys)
                {
                    keys.Add(s);
                    ret[0, 0] = Math.Min(ret[0, 0], s.Item1);
                    ret[0, 1] = Math.Min(ret[0, 1], s.Item2);
                    ret[0, 2] = Math.Min(ret[0, 2], s.Item3);

                    ret[1, 0] = Math.Max(ret[1, 0], s.Item1);
                    ret[1, 1] = Math.Max(ret[1, 1], s.Item2);
                    ret[1, 2] = Math.Max(ret[1, 2], s.Item3);
                }
            }
            return ret;
        }


        public void WriteMfile(string resultsFilPath)
        {
            var mfile = new StreamWriter(resultsFilPath);
            HashSet<Tuple<short, short, short>> keys;
            var crf = GetMinMaxConditionReplicateFraction(out keys);
            for (var c = crf[0, 0]; c <= crf[1, 0]; c++)
            {
                for (var r = crf[0, 1]; r <= crf[1, 1]; r++)
                {
                    for (var f = crf[0, 2]; f <= crf[1, 2]; f++)
                    {
                        var k = new Tuple<short, short, short>(c, r, f);
                        if (!keys.Contains(k)) continue;
                        mfile.Write(@"pepq_{0}_{1}_{2}=[", c,r,f);
                        foreach (var v in Values)
                        {
                            QuantifiedPeptide pep;
                            if (!v.TryGetValue(k, out pep))
                            {
                                for(var i=0;i<LabelList.LabelCount;i++)
                                    mfile.Write(0 + ",");
                            }
                            else
                            {
                                foreach (var pi in pep.Quantities)
                                    mfile.Write(pi + ",");
                            }
                            mfile.WriteLine();
                        }
                        mfile.WriteLine("];");
                    }
                }
            }
            mfile.Close();
        }

        public void WriteTsv(string resultFilePath, FastaDatabase fastaDb, bool overwrite = false)
        {
            var tsvWriter = new StreamWriter(resultFilePath);
            //Making header
            var intensityHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("Quantity_{0}", labelNum + 1);
            var ratioHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("Ratio_{0}", labelNum + 1);

            //var header = new List<string> { "'ProteinGroup", "'RepresentativeProtein", "'Mass", "'Length", "'CoveredAminoAcidCount", "'Coverage(%)", "'CoveredPortionStarts", "'CoveredPortionEnds" };

            var header = new List<string> { "Peptide", "MatchingProteins", "Mass(ExcludingLabelMass)", "AminoAcidCount", "Modifications", "StartInProteins", "EndInProteins" , "Decoy"};
            var perSetHeader = new List<string> {"PSMIDs", "QuantifiedLabelCount",};
            perSetHeader.AddRange(intensityHeaders);
            perSetHeader.AddRange(ratioHeaders);
            perSetHeader.AddRange(new List<string> { "QValue", "SNR", });

            HashSet<Tuple<short, short, short>> keys;
            var crf = GetMinMaxConditionReplicateFraction(out keys);
            for (var c = crf[0, 0]; c <= crf[1, 0]; c++)
            {
                for (var r = crf[0, 1]; r <= crf[1, 1]; r++)
                {
                    for (var f = crf[0, 2]; f <= crf[1, 2]; f++)
                    {
                        var k = new Tuple<short, short, short>(c, r, f);
                        if (!keys.Contains(k)) continue;
                        var headerSuffix = @"";
                        if (crf[1, 0] - crf[0, 0] > 0) headerSuffix += string.Format(@"cond:{0}", c);
                        if (crf[1, 1] - crf[0, 1] > 0 && r > 0) headerSuffix += string.Format(@"rep:{0}", r);
                        if (crf[1, 2] - crf[0, 2] > 0 && f > 0) headerSuffix += string.Format(@"frac:{0}", f);
                        foreach (var ph in perSetHeader)
                        {
                            header.Add(ph + (headerSuffix.Length>0? @" ("+headerSuffix+@")":@""));
                        }

                    }
                }
            }



            for (var i = 0; i < header.Count - 1; i++)
            {
                tsvWriter.Write(header[i] + "\t");
            }
            tsvWriter.WriteLine(header[header.Count - 1]);
          

            var peps = Keys.ToList();
            
           for (var i = 0; i < Count; i++)
            {
                var pep = peps[i];

                var dataRow = new List<string>();
                var commonDataRow = new List<string> { pep };
                var isCommonDataRowFilled = false;

                var subd = this[pep];
                bool write = false;

                for (var c = crf[0, 0]; c <= crf[1, 0]; c++)
                {
                    for (var r = crf[0, 1]; r <= crf[1, 1]; r++)
                    {
                        for (var f = crf[0, 2]; f <= crf[1, 2]; f++)
                        {
                            var k = new Tuple<short, short, short>(c, r, f);
                            if (!keys.Contains(k)) continue;
                            QuantifiedPeptide qPep;
                            var qs = new float[LabelList.LabelCount];
                            if (subd.TryGetValue(k, out qPep) && qPep.IsQuantified())
                            {
                                qs = qPep.Quantities;
                                if (!isCommonDataRowFilled)
                                {
                                    var proteins = qPep.MatchedPsms[0].Id.Proteins; 
                                    commonDataRow.Add(String.Join(";", proteins));
                                    commonDataRow.Add(String.Format("{0}", qPep.MatchedPsms[0].Id.GetUnlabeledMass()));
                                    commonDataRow.Add(String.Format("{0}", qPep.MatchedPsms[0].Id.UnmodifiedPeptide.Length));
                                    commonDataRow.Add(String.Join(",",qPep.MatchedPsms[0].Id.GetModStrings()));
                                    var starts = new int[proteins.Length];
                                    var ends = new int[proteins.Length];
                                    for (var m=0;m<proteins.Length;m++)
                                    {
                                        var seq = fastaDb.GetProteinSequence(proteins[m]);
                                      //  if (seq == null) Console.WriteLine(proteins[m]);
                                        var crange = qPep.MatchedPsms[0].GetInclusiveCoveringRange(seq);
                                        starts[m] = crange.Item1;
                                        ends[m] = crange.Item2;
                                    }
                                    commonDataRow.Add(String.Join(";", starts));
                                    commonDataRow.Add(String.Join(";", ends));
                                    commonDataRow.Add(qPep.IsDecoy().ToString());
                                    isCommonDataRowFilled = true;
                                }
                            }

                            if (qPep != null)
                            {
                                if (r == crf[0, 1] && f == crf[0, 2]) write = true;
                                //psm location, num quantified, intensity , qvalue, snr
                                dataRow.Add(String.Join(";", from qpsm in qPep.MatchedPsms select qpsm.PsmId));
                                dataRow.Add(qPep.GetQuantifiedLabelCount().ToString());
                                foreach (var q in qs)
                                {
                                    dataRow.Add(String.Format("{0}", Math.Abs(q)));
                                }
                                foreach (var q in qPep.GetRatios())
                                {
                                    dataRow.Add(String.Format("{0}", Math.Abs(q)));
                                }
                                dataRow.Add(String.Format("{0}", qPep.Qvalue));
                                dataRow.Add(String.Format("{0}", qPep.GetSnr()));
                            }
                            else
                            {
                                dataRow.Add(@"N/A");
                                dataRow.Add(0.ToString());
                                foreach (var q in qs)
                                {
                                    dataRow.Add(String.Format("{0}", Math.Abs(q)));
                                }
                                foreach (var q in qs)
                                {
                                    dataRow.Add(String.Format("{0}", Math.Abs(q)));
                                }
                                dataRow.Add(@"NaN");
                                dataRow.Add(@"NaN");
                            }
                        }
                    }
                }

                if (!write)
                {
                    continue;
                }

                foreach (var s in commonDataRow)
                    tsvWriter.Write(s + "\t");
                for (var j = 0; j < dataRow.Count - 1; j++)
                    tsvWriter.Write(dataRow[j] + "\t");
                tsvWriter.WriteLine(dataRow[dataRow.Count - 1]);
            }

            tsvWriter.Close();
          
        }
    

        public void WriteExcel(string resultFilePath, Application xlApp, FastaDatabase fastaDb, bool overwrite = false)
        {
            var xlWorkBook = xlApp.Workbooks.Add();

            var xlWorkSheet = (Worksheet)xlWorkBook.Worksheets.Item[1];
            xlWorkSheet.Name = @"Peptides";

            //Making header
            var intensityHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("'Quantity_{0}", labelNum + 1);
            var ratioHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("'Ratio_{0}", labelNum + 1);

            //var header = new List<string> { "'ProteinGroup", "'RepresentativeProtein", "'Mass", "'Length", "'CoveredAminoAcidCount", "'Coverage(%)", "'CoveredPortionStarts", "'CoveredPortionEnds" };

            var header = new List<string> { "'Peptide", "'MatchingProteins", "'Mass(ExcludingLabelMass)", "'AminoAcidCount", "'Modifications", "'StartInProteins", "'EndInProteins" , "'Decoy"};
            var perSetHeader = new List<string> {"'PSMIDs", "'QuantifiedLabelCount",};
            perSetHeader.AddRange(intensityHeaders);
            perSetHeader.AddRange(ratioHeaders);
            perSetHeader.AddRange(new List<string> { "'QValue", "'SNR", });

            HashSet<Tuple<short, short, short>> keys;
            var crf = GetMinMaxConditionReplicateFraction(out keys);
            for (var c = crf[0, 0]; c <= crf[1, 0]; c++)
            {
                for (var r = crf[0, 1]; r <= crf[1, 1]; r++)
                {
                    for (var f = crf[0, 2]; f <= crf[1, 2]; f++)
                    {
                        var k = new Tuple<short, short, short>(c, r, f);
                        if (!keys.Contains(k)) continue;
                        var headerSuffix = @"";
                        if (crf[1, 0] - crf[0, 0] > 0) headerSuffix += string.Format(@"cond:{0}", c);
                        if (crf[1, 1] - crf[0, 1] > 0 && r > 0) headerSuffix += string.Format(@"rep:{0}", r);
                        if (crf[1, 2] - crf[0, 2] > 0 && f > 0) headerSuffix += string.Format(@"frac:{0}", f);
                        foreach (var ph in perSetHeader)
                        {
                            header.Add(ph + (headerSuffix.Length>0? @" ("+headerSuffix+@")":@""));
                        }

                    }
                }
            }


            // This fields should not be formatted automatically by Excel.
            var textFields = new List<string>
            {
                "'Peptide", "'PSMIDs", "'MatchingProteins", "'Modifications"
            };
            var textFieldIndices = new List<int>();
            for (var i = 0; i < header.Count; i++)
            {
                foreach (var tf in textFields)
                {
                    if (header[i].StartsWith(tf))
                    {
                        textFieldIndices.Add(i);
                        break;
                    }
                }
            }


            // Adding data to array

            var range = xlWorkSheet.Cells.Resize[Count + 1, header.Count];

            var dataArray = new object[1, header.Count];
            for (var i = 0; i < header.Count; i++)
            {
                dataArray[0, i] = header[i];
            }

            var tmpRange = xlWorkSheet.Range[xlWorkSheet.Cells[1, 1], xlWorkSheet.Cells[1, header.Count]];
            tmpRange.Value2 = dataArray;

            const int maxCntr = 500;

            var peps = Keys.ToList();
            var skipCntr = 0;
            var prevSkipCntr = 0;
            
            for (var l = 0; l <= Count / maxCntr; l++)
            {
                var offset = l * maxCntr;
                dataArray = new object[maxCntr, header.Count];
                var index = 0;

                for (var i = 0; i < maxCntr; i++)
                {
                    if (offset + i >= Count) break;
                    var pep = peps[offset + i];

                    var dataRow = new List<object>();
                    var commonDataRow = new List<object>{pep};
                    var isCommonDataRowFilled = false;

                    var subd = this[pep];
                    bool write = false;

                    for (var c = crf[0, 0]; c <= crf[1, 0]; c++)
                    {
                        for (var r = crf[0, 1]; r <= crf[1, 1]; r++)
                        {
                            for (var f = crf[0, 2]; f <= crf[1, 2]; f++)
                            {
                                var k = new Tuple<short, short, short>(c, r, f);
                                if (!keys.Contains(k)) continue;
                                QuantifiedPeptide qPep;
                                var qs = new float[LabelList.LabelCount];
                                if (subd.TryGetValue(k, out qPep) && qPep.IsQuantified())
                                {
                                    qs = qPep.Quantities;
                                    if (!isCommonDataRowFilled)
                                    {
                                        var proteins = qPep.MatchedPsms[0].Id.Proteins; 
                                        commonDataRow.Add(String.Join(";", proteins));
                                        commonDataRow.Add(qPep.MatchedPsms[0].Id.GetUnlabeledMass());
                                        commonDataRow.Add(qPep.MatchedPsms[0].Id.UnmodifiedPeptide.Length);
                                        commonDataRow.Add(String.Join(",",qPep.MatchedPsms[0].Id.GetModStrings()));
                                        var starts = new int[proteins.Length];
                                        var ends = new int[proteins.Length];
                                        for (var m=0;m<proteins.Length;m++)
                                        {
                                            var seq = fastaDb.GetProteinSequence(proteins[m]);
                                            var crange = qPep.MatchedPsms[0].GetInclusiveCoveringRange(seq);
                                            starts[m] = crange.Item1;
                                            ends[m] = crange.Item2;
                                        }
                                        commonDataRow.Add(String.Join(";", starts));
                                        commonDataRow.Add(String.Join(";", ends));
                                        commonDataRow.Add(qPep.IsDecoy());
                                        isCommonDataRowFilled = true;
                                    }
                                }

                                if (qPep != null)
                                {
                                    if (r == crf[0, 1] && f == crf[0, 2]) write = true;
                                    //psm location, num quantified, intensity , qvalue, snr
                                    dataRow.Add(String.Join(";", from qpsm in qPep.MatchedPsms select qpsm.PsmId));
                                    dataRow.Add(qPep.GetQuantifiedLabelCount());
                                    foreach (var q in qs)
                                    {
                                        dataRow.Add(Math.Abs(q));
                                    }
                                    foreach (var q in qPep.GetRatios())
                                    {
                                        dataRow.Add(Math.Abs(q));
                                    }
                                    dataRow.Add(qPep.Qvalue);
                                    dataRow.Add(qPep.GetSnr());
                                }
                                else
                                {
                                    dataRow.Add(@"N/A");
                                    dataRow.Add(0);
                                    foreach (var q in qs)
                                    {
                                        dataRow.Add(Math.Abs(q));
                                    }
                                    foreach (var q in qs)
                                    {
                                        dataRow.Add(Math.Abs(q));
                                    }
                                    dataRow.Add(@"NaN");
                                    dataRow.Add(@"NaN");
                                }
                            }
                        }
                    }

                    if (!write)
                    {
                        skipCntr++;
                        continue;
                    }

                    dataRow.InsertRange(0, commonDataRow);

                    if (dataRow.Count != dataArray.GetLength(1))
                    {
                        throw new Exception("Length of sheet2DataRow does not match with length of sheet2Header");
                    }

                    for (var j = 0; j < header.Count; j++)
                    {
                        if (textFieldIndices.Contains(j))
                        {
                            dataArray[index, j] = "'" + dataRow[j];
                        }
                        else
                        {
                            dataArray[index, j] = dataRow[j];
                        }
                    }
                    index++;
                }
                tmpRange = xlWorkSheet.Range[xlWorkSheet.Cells[2 - prevSkipCntr + offset, 1], xlWorkSheet.Cells[1 - prevSkipCntr + offset + index, header.Count]];
                prevSkipCntr = skipCntr;
                var dataArrayTruncated = new object[index, header.Count];
                for (var j = 0; j < header.Count; j++)
                {
                    for (var i = 0; i < index; i++)
                        dataArrayTruncated[i, j] = dataArray[i, j];
                }

                tmpRange.Value2 = dataArrayTruncated;
            }
            
            xlWorkSheet.Activate();
            xlWorkSheet.Application.ActiveWindow.SplitRow = 1;
            xlWorkSheet.Application.ActiveWindow.FreezePanes = true;

            Range firstRow = xlWorkSheet.Rows[1];
            firstRow.AutoFilter(1, Type.Missing, XlAutoFilterOperator.xlAnd, Type.Missing, true);
            //Console.WriteLine(resultFilePath.Replace(".xlsx", ".txt"));
            if (overwrite)
            {
                xlApp.DisplayAlerts = false;
                xlWorkBook.Close(SaveChanges:true, Filename:resultFilePath);
            }
            else
            {
                xlApp.DisplayAlerts = false;
                xlWorkBook.Close(SaveChanges:true, Filename:resultFilePath);
            }

            Marshal.ReleaseComObject(xlWorkBook);
        }
    }
}
