using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Biology;
using InformedProteomics.Backend.Data.Sequence;
using InformedProteomics.Backend.Database;
using IntervalTreeLib;
using Microsoft.Office.Interop.Excel;

namespace Epiq
{
    public class QuantifiedProteinGroupDictionary : Dictionary<string, Dictionary<Tuple<short, short>, QuantifiedProteinGroup>> // rep pro seq, cond, qpep 
    {
        private void AssignQvalues(List<QuantifiedProteinGroup> pgs)
        {
            if (pgs.Count == 0) return;
            var proteinGroupDictionary = new Dictionary<float, List<QuantifiedProteinGroup>>();
            foreach (var pg in pgs)
            {
                var score = pg.QvalueScore;
                List<QuantifiedProteinGroup> pgList;
                if (proteinGroupDictionary.TryGetValue(score, out pgList)) pgList.Add(pg);
                else proteinGroupDictionary[score] = new List<QuantifiedProteinGroup> { pg };
            }
            var scores = new List<float>();
            scores.AddRange(proteinGroupDictionary.Keys);
            scores.Sort();

            var numDecoy = 0;
            var numTarget = 0;
            var fdr = new float[scores.Count];
            for (var i = 0; i < scores.Count; i++)
            {
                var score = scores[i];
                foreach (var pg in proteinGroupDictionary[score])
                {
                    if (pg.IsDecoy()) numDecoy++;
                    else numTarget++;
                }
                fdr[i] = Math.Min((float)numDecoy / (numDecoy + numTarget), 1.0f);
            }
           // Console.Write(pgs.Count);
            var qValue = new float[fdr.Length];
            qValue[fdr.Length - 1] = fdr[fdr.Length - 1];
            for (var i = fdr.Length - 2; i >= 0; i--)
            {
                qValue[i] = Math.Min(qValue[i + 1], fdr[i]);
            }
           // Console.WriteLine(2);
            foreach (var pg in pgs)
            {
                var score = pg.QvalueScore;
                var index = scores.BinarySearch(score);
                if (index < 0) index = ~index;
                pg.SetQvalue(qValue[Math.Min(index, qValue.Length - 1)]);
            }
        }


        public void AddAndAssignMatchedProteinsToPsm(QuantifiedPsmList psmList, short condition, short replicate, List<QuantifiedProtein> proteinList, bool[][] adjMatrix,
            int minMatchedPsmCountPerProtein, int minMatchedPepCountPerProtein, float qvalueThreshold,float snrThreshold, float singleHitSnrThreshold, bool assignProteinToPsm)
        {
             var proteinGroupDictionary =
                new Dictionary<HashSet<int>, QuantifiedProteinGroup>(HashSet<int>.CreateSetComparer());
            var concurrentProteinDictionary = new ConcurrentDictionary<QuantifiedProtein, bool[]>();
            Parallel.ForEach(proteinList, new ParallelOptions { MaxDegreeOfParallelism = Params.MaxParallelThreads },
                protein =>
                {
                    var i = proteinList.IndexOf(protein);
                    List<int> usedIndices;
                    var set = protein.SetMatchedPsmsAndQvalueScore(psmList, adjMatrix[i], out usedIndices);
                    adjMatrix[i] = new bool[psmList.Count];
                    if (!set) return;
                    foreach (var u in usedIndices) adjMatrix[i][u] = true;
                    concurrentProteinDictionary[protein] = adjMatrix[i];
                });

            //var proteinMatchedIndices = new HashSet<int>();
            foreach (var protein in concurrentProteinDictionary.Keys)
            {
                if (protein.MatchedPsmCount < minMatchedPsmCountPerProtein) continue;
                if (protein.MatchedPeptideCount < minMatchedPepCountPerProtein) continue;
                if (!protein.IsQuantified()) continue;
               
                var matchedPsmIndices = new HashSet<int>();
                var tadjMatrix = concurrentProteinDictionary[protein];
                for (var j = 0; j < tadjMatrix.Length; j++)
                {
                    if (!tadjMatrix[j]) continue;
                    matchedPsmIndices.Add(j);
                //    proteinMatchedIndices.Add(j);
                }

                QuantifiedProteinGroup proteinsInGroup;
                if (!proteinGroupDictionary.TryGetValue(matchedPsmIndices, out proteinsInGroup))
                {
                    proteinGroupDictionary[matchedPsmIndices] = proteinsInGroup = new QuantifiedProteinGroup(psmList, matchedPsmIndices, assignProteinToPsm);
                }
                proteinsInGroup.Add(protein);
            }

            var proteinGroupList = proteinGroupDictionary.Values.ToList();
           // Console.WriteLine(proteinGroupList.Count);
            AssignQvalues(proteinGroupList);
            for (var i=0;i<proteinGroupList.Count;i++)
            {
                var pg = proteinGroupList[i];
                if (pg.Qvalue <= qvalueThreshold){
                    if(pg.MatchedPsms.Count > 1 && pg.GetSnr() >= snrThreshold)
                        continue; //!this[i].Id.IsDecoy() && 
                    if (pg.MatchedPsms.Count <= 1 && pg.GetSnr() >= singleHitSnrThreshold)
                        continue;
                }
                proteinGroupList.RemoveAt(i--);
            }
           // Console.WriteLine(proteinGroupList.Count);
            foreach (var proteinsInGroup in proteinGroupList)
            {
                Dictionary<Tuple<short, short>, QuantifiedProteinGroup> v;
                var repProtein = proteinsInGroup.GetRepresentativeProtein();
                if (!TryGetValue(repProtein, out v)) this[repProtein] = v = new Dictionary<Tuple<short, short>, QuantifiedProteinGroup>();
                var t = new Tuple<short, short>(condition, replicate);
                v[t] = proteinsInGroup;
            }

            //Console.WriteLine(@"Protein Group : {0}   Protein : {1}", proteinGroupList.Count, GetProteinCount(proteinGroupList));
            
        }

        private int GetProteinCount(List<QuantifiedProteinGroup> proteinGroupList)
        {
            var ps = new HashSet<QuantifiedProtein>();
            foreach (var pg in proteinGroupList)
            {
                foreach (var p in pg.QuantifiedProteinSet)
                    ps.Add(p);
            }
            return ps.Count;
        }

        private short[,] GetMinMaxConditionReplicate(out HashSet<Tuple<short, short>> keys)
        {
            var ret = new short[2, 2];

            ret[0, 0] = ret[0, 1] = short.MaxValue;
            ret[1, 0] = ret[1, 1] = short.MinValue;

            keys = new HashSet<Tuple<short, short>>();
            foreach (var t in Values)
            {
                foreach (var s in t.Keys)
                {
                    keys.Add(s);
                    ret[0, 0] = Math.Min(ret[0, 0], s.Item1);
                    ret[0, 1] = Math.Min(ret[0, 1], s.Item2);
                   
                    ret[1, 0] = Math.Max(ret[1, 0], s.Item1);
                    ret[1, 1] = Math.Max(ret[1, 1], s.Item2);
                }
            }
            return ret;
        }


        public void WriteMfile(string resultsFilPath)
        {
            var mfile = new StreamWriter(resultsFilPath);
            HashSet<Tuple<short, short>> keys;
            var cr = GetMinMaxConditionReplicate(out keys);
               
            for (var c = cr[0, 0]; c <= cr[1, 0]; c++)
            {
                for (var r = cr[0, 1]; r <= cr[1, 1]; r++)
                {  
                    var k = new Tuple<short, short>(c, r);
                    if (!keys.Contains(k)) continue;
                    mfile.Write(@"Pq_{0}_{1}=[", c, r);
                    foreach (var v in Values)
                    {
                        QuantifiedProteinGroup pg;
                        if (!v.TryGetValue(k, out pg))
                        {
                            for (var i = 0; i < LabelList.LabelCount; i++)
                                mfile.Write(0 + ",");
                        }
                        else
                        {
                            foreach (var pi in pg.Quantities)
                                mfile.Write(pi + ",");
                        }
                        mfile.WriteLine();
                    }
                    mfile.WriteLine("];");
                 
                }
            }
            mfile.Close();
        }



        private void GetProteinInfo(QuantifiedProteinGroup pg, FastaDatabase fastaDb, out string massStr, out string lengthStr, out string covLengthStr, out string coverageStr,
            out string covStartStr, out string covEndStr, out List<int> numPossiblePeptides)
        {

            var masses = new List<double>();
            var lengths = new List<int>();
            var covLengths = new List<int>();
            var coverages = new List<string>();
            var covStarts = new List<string>();
            var covEnds = new List<string>();
            numPossiblePeptides = new List<int>();

            foreach (var p in pg.QuantifiedProteinSet)
            {
                var seq = fastaDb.GetProteinSequence(p.Name);
                var mass = .0;
                foreach (var residue in seq)
                {
                    var aa = Params.AminoAcidSet.GetAminoAcid(residue);
                    if (aa == null) continue;
                    mass += aa.Mass;
                }
                masses.Add(mass);
                lengths.Add(seq.Length);
               
                var pts = new List<int>();
                var starts = new HashSet<int>();
                var ends = new HashSet<int>();
                foreach (var psm in p.MatchedPsms)
                {
                    var se = psm.GetInclusiveCoveringRange(seq);
                    if (se.Item1 < 0 || se.Item2 < 0) continue;
                    pts.Add(se.Item1);
                    pts.Add(se.Item2);
                    starts.Add(se.Item1);
                    ends.Add(se.Item2);
                }
                pts.Sort();
                var cntr = 0;
                var cstart = 0;
                var covLength = 0;
                var covStart = new List<int>();
                var covEnd = new List<int>();
                
                foreach (var pt in pts)
                {
                    var prevCntr = cntr;
                    if (starts.Contains(pt)) cntr++;
                    if (ends.Contains(pt)) cntr--;
                    if (prevCntr == 0 && cntr == 1) cstart = pt;
                    if (cntr != 0 || pt - cstart <= 0) continue;
                    covStart.Add(cstart);
                    covEnd.Add(pt);
                    covLength += pt - cstart;
                }
                covLengths.Add(covLength);
                covStarts.Add(String.Join(",", covStart));
                covEnds.Add(String.Join(",", covEnd));
                
                coverages.Add(String.Format("{0:F2}", (double)covLength/seq.Length*100));
                var numPossiblePeptide = 0;
                for (var i = 0; i < seq.Length; i++)
                {
                    var j = seq.IndexOfAny(Params.Enzyme.Residues, i + 1);
                    if(j>=0 && j-i+1 >= 6 && j-i+1<=30) numPossiblePeptide++;
                    i = Math.Max(i, j);
                }
                numPossiblePeptides.Add(numPossiblePeptide);
            }

            massStr = String.Join(";", masses);
            lengthStr = String.Join(";", lengths);
            covLengthStr = String.Join(";", covLengths);
            coverageStr = String.Join(";", coverages);
            covStartStr = String.Join(";", covStarts);
            covEndStr = String.Join(";", covEnds);
        }


        public void WriteTsv(string resultFilePath, FastaDatabase fastaDb, bool overwrite = true)
        {
            var tsvWriter = new StreamWriter(resultFilePath);
         
            //Making header
            var intensityHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("Quantity_{0}", labelNum + 1);
            var ratioHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("Ratio_{0}", labelNum + 1);
            var iBaqHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("iBAQ_{0}", labelNum + 1);

            var header = new List<string> { "ProteinGroup", "RepresentativeProtein", "Mass", "Length", "CoveredAminoAcidCount", "Coverage(%)", "CoveredPortionStarts", "CoveredPortionEnds", "Decoy" };
            var perSetHeader = new List<string> { "PSMIDs", "QuantifiedLabelCount", };
            perSetHeader.AddRange(intensityHeaders);
            perSetHeader.AddRange(ratioHeaders);
            perSetHeader.AddRange(iBaqHeaders);
            perSetHeader.AddRange(new List<string> { "QValue", "SNR", });

            HashSet<Tuple<short, short>> keys;
            var cr = GetMinMaxConditionReplicate(out keys);
            for (var c = cr[0, 0]; c <= cr[1, 0]; c++)
            {
                for (var r = cr[0, 1]; r <= cr[1, 1]; r++)
                {
                    var k = new Tuple<short, short>(c, r);
                    if (!keys.Contains(k)) continue;
                    var headerSuffix = @"";
                    if (cr[1, 0] - cr[0, 0] > 0) headerSuffix += string.Format(@"cond:{0}", c);
                    if (cr[1, 1] - cr[0, 1] > 0 && r > 0) headerSuffix += string.Format(@"rep:{0}", r);
                    foreach (var ph in perSetHeader)
                    {
                        header.Add(ph + (headerSuffix.Length > 0 ? @" (" + headerSuffix + @")" : @""));
                    }
                }
            }
            
            for (var i = 0; i < header.Count-1; i++)
            {
                tsvWriter.Write(header[i] + "\t");
            }
            tsvWriter.WriteLine(header[header.Count-1]);

            var pgs = Keys.ToList();
               
            for (var i = 0; i < Count; i++)
            {
                var repProtein = pgs[i];

                var dataRow = new List<string>();
                var commonDataRow = new List<string>();
                var isCommonDataRowFilled = false;
                var numPossiblePeptides = new List<int>();

                var subd = this[repProtein];
                bool write = false;

                for (var c = cr[0, 0]; c <= cr[1, 0]; c++)
                {
                    for (var r = cr[0, 1]; r <= cr[1, 1]; r++)
                    {
                        var k = new Tuple<short, short>(c, r);
                        if (!keys.Contains(k)) continue;
                        QuantifiedProteinGroup qPg;
                        var qs = new float[LabelList.LabelCount];
                        if (subd.TryGetValue(k, out qPg) && qPg.IsQuantified())
                        {
                            qs = qPg.Quantities;
                            if (!isCommonDataRowFilled)
                            {
                                commonDataRow.Add(String.Join(";", from qProt in qPg.QuantifiedProteinSet select qProt.Name));
                                commonDataRow.Add(repProtein);
                                string masses, lengths, covLengths, coverages, covStarts, covEnds;
                                GetProteinInfo(qPg, fastaDb, out masses, out lengths,
                                    out covLengths, out coverages, out covStarts, out covEnds, out numPossiblePeptides);
                                commonDataRow.Add(masses);
                                commonDataRow.Add(lengths);
                                commonDataRow.Add(covLengths);
                                commonDataRow.Add(coverages);
                                commonDataRow.Add(covStarts);
                                commonDataRow.Add(covEnds);
                                commonDataRow.Add(qPg.IsDecoy().ToString());
                                isCommonDataRowFilled = true;
                            }
                        }
                        if (qPg != null)
                        {
                            if (r == cr[0, 1]) write = true;

                            //psm location, num quantified, intensity ,  qval, snr
                            dataRow.Add(String.Join(";", from qpsm in qPg.MatchedPsms select qpsm.PsmId));
                            dataRow.Add(qPg.GetQuantifiedLabelCount().ToString());
                            foreach (var q in qs)
                            {
                                dataRow.Add(string.Format("{0}", Math.Abs(q)));
                            }
                            foreach (var q in qPg.GetRatios())
                            {
                                dataRow.Add(string.Format("{0}", Math.Abs(q)));
                            }

                            if (numPossiblePeptides == null)
                            {
                                foreach (var q in qs)
                                {
                                    dataRow.Add(Math.Abs(0).ToString());
                                }
                            }
                            else
                            {
                                foreach (var q in qs)
                                {
                                    var ibaqs = new List<string>();
                                    foreach (var n in numPossiblePeptides)
                                    {
                                        ibaqs.Add(String.Format("{0:f2}", q / n));
                                    }

                                    dataRow.Add(String.Join(";", ibaqs));
                                }
                            }

                            dataRow.Add(string.Format("{0}", Math.Abs(qPg.Qvalue)));
                            dataRow.Add(string.Format("{0}", Math.Abs(qPg.GetSnr())));
                        }
                        else
                        {
                            dataRow.Add(@"N/A");
                            dataRow.Add(0.ToString());
                            foreach (var q in qs)
                            {
                                dataRow.Add(string.Format("{0}", Math.Abs(q)));
                            }
                            foreach (var q in qs)
                            {
                                dataRow.Add(string.Format("{0}", Math.Abs(q)));
                            }
                            foreach (var q in qs)
                            {
                                dataRow.Add(0.ToString());
                            }

                            dataRow.Add(@"NaN");
                            dataRow.Add(@"NaN");
                        }
                    }
                }

                if (!write)
                {
                    continue;
                }

                foreach(var s in commonDataRow)
                    tsvWriter.Write(s + "\t");
                for (var j=0;j<dataRow.Count-1;j++)
                    tsvWriter.Write(dataRow[j] + "\t");
                tsvWriter.WriteLine(dataRow[dataRow.Count-1]);
            }
            tsvWriter.Close();

        }


        public void WriteExcel(string resultFilePath, Application xlApp, FastaDatabase fastaDb, bool overwrite = true)
        {
            var xlWorkBook = xlApp.Workbooks.Add();

            var xlWorkSheet = (Worksheet)xlWorkBook.Worksheets.Item[1];
            xlWorkSheet.Name = @"ProteinGroups";

            //Making header
            var intensityHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("'Quantity_{0}", labelNum+1);
            var ratioHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("'Ratio_{0}", labelNum + 1);
            var iBaqHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("'iBAQ_{0}", labelNum + 1);

            var header = new List<string> { "'ProteinGroup", "'RepresentativeProtein", "'Mass", "'Length", "'CoveredAminoAcidCount", "'Coverage(%)", 
                "'CoveredPortionStarts", "'CoveredPortionEnds", "'Decoy" };
            var perSetHeader = new List<string> { "'PSMIDs", "'QuantifiedLabelCount", };
            perSetHeader.AddRange(intensityHeaders);
            perSetHeader.AddRange(ratioHeaders);
            perSetHeader.AddRange(iBaqHeaders);
            perSetHeader.AddRange(new List<string> {"'QValue", "'SNR",  });

            HashSet<Tuple<short, short>> keys;
            var cr = GetMinMaxConditionReplicate(out keys);
            for (var c = cr[0, 0]; c <= cr[1, 0]; c++)
            {
                for (var r = cr[0, 1]; r <= cr[1, 1]; r++)
                {
                    var k = new Tuple<short, short>(c, r);
                    if (!keys.Contains(k)) continue;
                    var headerSuffix = @"";
                    if (cr[1, 0] - cr[0, 0] > 0) headerSuffix += string.Format(@"cond:{0}", c);
                    if (cr[1, 1] - cr[0, 1] > 0 && r > 0) headerSuffix += string.Format(@"rep:{0}", r);
                    foreach (var ph in perSetHeader)
                    {
                        header.Add(ph + (headerSuffix.Length > 0 ? @" (" + headerSuffix + @")" : @""));
                    }
                }
            }

            // This fields should not be formatted automatically by Excel.
            var textFields = new List<string>
            {
                "'ProteinGroup", "'PSMIDs", "'RepresentativeProtein", "'CoveredPortion"
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

            var dataArray = new object[1, header.Count];
            for (var i = 0; i < header.Count; i++)
            {
                dataArray[0, i] = header[i];
            }

            var tmpRange = xlWorkSheet.Range[xlWorkSheet.Cells[1, 1], xlWorkSheet.Cells[1, header.Count]];
            tmpRange.Value2 = dataArray;

            const int maxCntr = 500;
            var skipCntr = 0;
            var prevSkipCntr = 0;
           
            var pgs = Keys.ToList();

            for (var l = 0; l <= Count/maxCntr; l++)
            {
                var offset = l*maxCntr;
                dataArray = new object[maxCntr, header.Count];
                var index = 0;

                for (var i = 0; i < maxCntr; i++)
                {
                    if (offset + i >= Count) break;
                    var repProtein = pgs[offset + i];
                    
                    var dataRow = new List<object>();
                    var commonDataRow = new List<object>();
                    var isCommonDataRowFilled = false;
                    var numPossiblePeptides = new List<int>();
                            
                    var subd = this[repProtein];
                    bool write = false;

                    for (var c = cr[0, 0]; c <= cr[1, 0]; c++)
                    {
                        for (var r = cr[0, 1]; r <= cr[1, 1]; r++)
                        {
                            var k = new Tuple<short, short>(c, r);
                            if (!keys.Contains(k)) continue;
                            QuantifiedProteinGroup qPg;
                            var qs = new float[LabelList.LabelCount];
                            if (subd.TryGetValue(k, out qPg) && qPg.IsQuantified())
                            {
                                qs = qPg.Quantities;
                                if (!isCommonDataRowFilled)
                                {
                                    commonDataRow.Add(String.Join(";", from qProt in qPg.QuantifiedProteinSet select qProt.Name));
                                    commonDataRow.Add(repProtein);
                                    string masses, lengths, covLengths, coverages, covStarts, covEnds;
                                    GetProteinInfo(qPg, fastaDb, out masses, out lengths,
                                        out covLengths, out coverages, out covStarts, out covEnds, out numPossiblePeptides);
                                    commonDataRow.Add(masses);
                                    commonDataRow.Add(lengths);
                                    commonDataRow.Add(covLengths);
                                    commonDataRow.Add(coverages);
                                    commonDataRow.Add(covStarts);
                                    commonDataRow.Add(covEnds);
                                    commonDataRow.Add(qPg.IsDecoy());
                                    isCommonDataRowFilled = true;
                                }
                            }
                            if (qPg != null)
                            {
                                if (r == cr[0, 1]) write = true;
                                   
                                //psm location, num quantified, intensity ,  qval, snr
                                dataRow.Add(String.Join(";", from qpsm in qPg.MatchedPsms select qpsm.PsmId));
                                dataRow.Add(qPg.GetQuantifiedLabelCount());
                                foreach (var q in qs)
                                {
                                    dataRow.Add(Math.Abs(q));
                                }
                                foreach (var q in qPg.GetRatios())
                                {
                                    dataRow.Add(Math.Abs(q));
                                }

                                if (numPossiblePeptides == null)
                                {
                                    foreach (var q in qs)
                                    {
                                        dataRow.Add(Math.Abs(0));
                                    }
                                }
                                else
                                {
                                    foreach (var q in qs)
                                    {
                                        var ibaqs = new List<string>();
                                        foreach (var n in numPossiblePeptides)
                                        {
                                            ibaqs.Add(String.Format("{0:f2}", q/n));
                                           // break;//TODO
                                        }
                                        dataRow.Add(String.Join(";", ibaqs));
                                    }
                                }
                                
                                dataRow.Add(qPg.Qvalue);
                                dataRow.Add(qPg.GetSnr());
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
                                foreach (var q in qs)
                                {
                                    dataRow.Add(Math.Abs(0));
                                }

                                dataRow.Add(@"NaN");
                                dataRow.Add(@"NaN");
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