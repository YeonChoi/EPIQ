using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using InformedProteomics.Backend.Data.Spectrometry;
using IntervalTreeLib;
using MathNet.Numerics.Statistics;
using Microsoft.Office.Interop.Excel;

namespace Epiq
{
    [Serializable]
    public class QuantifiedPsmList : List<QuantifiedPsm>
    {

        public QuantifiedPsmList()
        {
        }

        public QuantifiedPsmList(ICollection<QuantifiedPsm> psms) : base(psms)
        {
        }

        private QuantifiedPsmList(QuantifiedPsmList other)
        {
            foreach (var psm in other)
            {
                Add(psm.Clone());
            }
        } 

        public QuantifiedPsmList Clone()
        {
            return new QuantifiedPsmList(this);
        }

        /*public void SetConditionReplicateFraction(short c, short r, short f)
        {
            foreach (var psm in this)
                psm.SetConditionReplicateFraction(c, r, f);
        }*/

        public void TruncateQuantity(float minQuantity)
        {
            foreach (var qpsm in this)
            {
               qpsm.TruncateQuantities(minQuantity);
            }
        }

        public void AssignPsmIds(string prefix)
        {
            foreach (var qpsm in this)
            {
                qpsm.PsmId = String.Format("{0}_{1}", prefix, qpsm.Index);
            }
        }

        public void RemoveOverlappingXicClusters(float overlapEtThreshold, Tolerance tolerance)
        {
            Console.Write(@"Generating interval tree for {0} XicClusters ...", Count);
            for (var i = 0; i < Count; i++) this[i].SetIndex(i);

            var tree = new IntervalTree<int, float>();
            //Sort();

            foreach (var psm in this)
            {
                var mz = psm.Mz;
                var tol = (float)tolerance.GetToleranceAsTh(mz);
                tree.AddInterval(mz - tol / 2, mz + tol / 2, psm.Index); // same XIC comparison
            }
            tree.Build();
            Console.Write(@"Done. Now pruning ... ");

            var toRemoveIndexSet = new ConcurrentDictionary<int, byte>();

            Parallel.ForEach(this, new ParallelOptions { MaxDegreeOfParallelism = Params.MaxParallelThreads }, psm =>
            {
                if (toRemoveIndexSet.ContainsKey(psm.Index)) return;
                var mz = psm.Mz;
                foreach (var opsmIndex in tree.Get(mz))
                {
                    if (toRemoveIndexSet.ContainsKey(opsmIndex)) continue;
                    if (opsmIndex == psm.Index) continue;
                    var opsm = this[opsmIndex];

                    if (psm.Id.Charge != opsm.Id.Charge) continue;
                    if (psm.Id.UnlabeledPeptide != opsm.Id.UnlabeledPeptide && psm.Id.ArePeptidesTheSameExceptIle(opsm.Id))
                    {
                     //   Console.WriteLine(psm.Id.UnmodifiedPeptide + " " + opsm.Id.UnmodifiedPeptide);
                        continue;
                    }
                    
                    if (psm.Id.NumOfBoundLabels != opsm.Id.NumOfBoundLabels) continue;
                    if ((psm.RtStart > opsm.RtEnd) || (psm.RtEnd < opsm.RtStart))
                        continue;

                    if (Math.Min(psm.RtEnd - opsm.RtStart, opsm.RtEnd - psm.RtStart) < overlapEtThreshold *
                        Math.Min(psm.RtEnd - psm.RtStart, opsm.RtEnd - opsm.RtStart))
                        continue;
                    if (psm.SignalPower < opsm.SignalPower)
                    {
                        toRemoveIndexSet[psm.Index] = 1;
                    }
                    else if (psm.SignalPower > opsm.SignalPower)
                    {
                        toRemoveIndexSet[opsmIndex] = 1;
                    }
                    else
                    {
                        if (psm.Id.SpecEValue > opsm.Id.SpecEValue) toRemoveIndexSet[psm.Index] = 1;
                        else if(psm.Id.SpecEValue < opsm.Id.SpecEValue) toRemoveIndexSet[opsmIndex] = 1;
                        else if (psm.Id.ScanNum < opsm.Id.ScanNum) toRemoveIndexSet[psm.Index] = 1;
                        else if (psm.Id.ScanNum > opsm.Id.ScanNum) toRemoveIndexSet[opsmIndex] = 1;
                        else if (psm.Id.Idindex < opsm.Id.Idindex) toRemoveIndexSet[opsmIndex] = 1; // TODO check how many are same here.
                        else if (psm.Id.Idindex > opsm.Id.Idindex) toRemoveIndexSet[psm.Index] = 1; // TODO check how many are same here.
                    }
                }
            });

            var toRemoveListIndex = toRemoveIndexSet.Keys.ToList();
            toRemoveListIndex.Sort();
            var offset = 0;
            foreach (var i in toRemoveListIndex)
                RemoveAt(i + offset--);

            for (var i = 0; i < Count; i++) this[i].SetIndex(i+1);
            Console.WriteLine(@"Done - Remaining XicClusters : {0}", Count);
        }

        private void AssignQvalues(bool useXicClusterScore = true)
        {
            var psmScoreDictionary = new Dictionary<float, List<QuantifiedPsm>>();
            foreach (var psm in this)
            {
                var score = useXicClusterScore? psm.XicClusterScore : psm.Id.SpecEValue;
                List<QuantifiedPsm> psmList;
                if (psmScoreDictionary.TryGetValue(score, out psmList)) psmList.Add(psm);
                else psmScoreDictionary[score] = new List<QuantifiedPsm> { psm };
            }

            var scores = new List<float>();
            scores.AddRange(psmScoreDictionary.Keys);
            scores.Sort();
            //Console.WriteLine(scores.Count);
            /*
            var pepScoreDic = new Dictionary<string, float>();
            foreach (var psm in this)
            {
                var pep = psm.Id.UnlabeledNoIlePeptide + (psm.Id.IsDecoy() ? "XXX" : "");
                float score;
                if (!pepScoreDic.TryGetValue(pep, out score)) score = float.PositiveInfinity;
                score = Math.Min(score, useXicClusterScore ? psm.XicClusterScore : psm.Id.SpecEValue);
                pepScoreDic[pep] = score;
            }
            var pepDic = new Dictionary<float, List<string>>();
            foreach (var pep in pepScoreDic.Keys)
            {
                List<string> peps;
                var score = pepScoreDic[pep];
                if (pepDic.TryGetValue(score, out peps)) peps.Add(pep);
                else pepDic[score] = new List<string> { pep };
            }

            var scores = new List<float>();
            scores.AddRange(pepDic.Keys);
            scores.Sort();*/

            var numDecoy = 0f;
            var numTarget = 0f;
            var fdr = new float[scores.Count];
            for (var i = 0; i < scores.Count; i++)
            {
                var score = scores[i];
                foreach (var psm in psmScoreDictionary[score])
                {
                    if (psm.Id.IsDecoy()) numDecoy++;
                    else numTarget++;
                }
                fdr[i] = Math.Min(numDecoy / (numDecoy + numTarget), 1.0f);
            }

            var qValue = new float[fdr.Length];
            qValue[fdr.Length - 1] = fdr[fdr.Length - 1];
            for (var i = fdr.Length - 2; i >= 0; i--)
            {
                qValue[i] = Math.Min(qValue[i + 1], fdr[i]);
            }

            foreach (var psm in this)
            {
                var score = useXicClusterScore ? psm.XicClusterScore : psm.Id.SpecEValue;
                var index = scores.BinarySearch(score);
                if (index < 0) index = ~index;
                ////if(psm.Id.IsDecoy())
                    //Console.WriteLine(qValue[Math.Min(index, qValue.Length - 1)]);
                psm.SetQValue(qValue[Math.Min(index, qValue.Length - 1)]);
            }
        }

        public void FilterByPeptideSimilarity(float cosineThreshold)
        {
            var pepDictionary = new Dictionary<string, List<QuantifiedPsm>>();
            foreach (var psm in this)
            {
                var pep = psm.Id.UnlabeledPeptide;
                List<QuantifiedPsm> psms;
                if(!pepDictionary.TryGetValue(pep, out psms)) pepDictionary[pep] = psms = new List<QuantifiedPsm>();
                psms.Add(psm);
            }
            Clear();
            foreach (var pep in pepDictionary.Keys)
            {
                var psms = pepDictionary[pep];
                for (var i = 0; i < 5 && psms.Count > 0; i++) psms = FilterByPeptideSimilarity(psms, cosineThreshold);
                AddRange(psms);
            }
        }

        public void FilterBySnrAndMeanQuantity(float snrThreshold, float meanQuantityThreshold)
        {
            for (var i = 0; i < Count; i++)
            {  
                if (this[i].GetSnr() < snrThreshold)
                {
                    RemoveAt(i--);
                    continue;
                }
                var nonZeroIntensityAvg = .0;
                var nonZeroCntr = 0;
                foreach (var q in this[i].Quantities)
                {
                    if (q <= 0) continue;
                    nonZeroIntensityAvg += q;
                    nonZeroCntr++;
                }
                nonZeroIntensityAvg /= nonZeroCntr;
                if (nonZeroCntr == 0 || nonZeroIntensityAvg < meanQuantityThreshold) RemoveAt(i--);
            }
           // Console.WriteLine(@"Filtered using SNR threshold of {0} : {1} remaining", snrThreshold, Count);
        }


        public QuantifiedPsmList GetIntersectingPsms(List<QuantifiedPsm> other)
        {
            var otherSet = new HashSet<QuantifiedPsm>(other);
            var ret = Clone();
            for (var i = 0; i < ret.Count; i++)
            {
                if (otherSet.Contains(ret[i])) continue;
                ret.RemoveAt(i--);
            }
            return ret;
        }

        private List<QuantifiedPsm> FilterByPeptideSimilarity(List<QuantifiedPsm> psms, float cosineThreshold) 
        {
            var filteredPsms = new List<QuantifiedPsm>();
            
            var om = QuantifiedProtein.GetObservationMatrix(psms);
            if (om == null) return filteredPsms;
            List<int> validColumns;
            var rom = QuantifiedProtein.GetOutlierRemovedOm(om, cosineThreshold, out validColumns);
            if (rom == null) return filteredPsms;
            
            foreach (var i in validColumns)
            {
                filteredPsms.Add(psms[i]);
            }
            return filteredPsms;;
        }

        /* private Matrix<float> GetOutlierRemovedOm(Matrix<float> om, float cosineThrehsold, out List<int> validColumns)
        {
            validColumns = new List<int>();

            //   var w = (om).ColumnSums().PointwiseLog();
            //   var W = Matrix<float>.Build.DenseOfDiagonalVector(w);
            var normalizedOm = om.NormalizeColumns(1); // * W;
            var qv = normalizedOm.RowSums();

            for (var i = 0; i < normalizedOm.ColumnCount; i++)
            {
                var tqv = qv.Clone();
                if (normalizedOm.ColumnCount > 1) tqv -= normalizedOm.Column(i);
                var cosine = GetCosineBetween(normalizedOm.Column(i), tqv);
                    //(normalizedOm.Column(i) - qv).PointwisePower(2).Mean();  //;
                if (cosine < cosineThrehsold) continue;
                validColumns.Add(i);
            }
            if (validColumns.Count == 0) return null;
            var nom = Matrix<float>.Build.Dense(om.RowCount, validColumns.Count);

            var max = .0f;
            for (var i = 0; i < validColumns.Count; i++)
            {
                var col = om.Column(validColumns[i]);
                nom.SetColumn(i, col);
                max = Math.Max(max, col.Max());
            }

            return max > 0 ? nom : null;
        }*/


        public void FilterByQvalue(double qt)
        {
            var originalCount = Count;
            
            AssignQvalues(false);

            var tcntr1 = Count;
            for (var i = 0; i < Count; i++)
            {
                if (!this[i].Id.IsDecoy() && (this[i].Qvalue <= qt)) continue;
                tcntr1--;
            }

            AssignQvalues();
            var tcntr2 = Count;
            for (var i = 0; i < Count; i++)
            {
                if (!this[i].Id.IsDecoy() && (this[i].Qvalue <= qt)) continue;
                tcntr2--;
            }

            AssignQvalues(tcntr2 > tcntr1); // 
            var maxSpecEvalue = .0;
            for (var i = 0; i < Count; i++)
            {
                maxSpecEvalue = Math.Max(maxSpecEvalue, this[i].Id.SpecEValue);
                if (this[i].Qvalue <= qt) continue; //== 1 : 2Da
                RemoveAt(i--);
            }
            Console.WriteLine(
                @"Total {0}, FDR controled {1} ({2} - XicClusterScore used vs. {3} - SpecEvalue used) , Max SpecEvalue : {4}",
                originalCount, Count, tcntr2, tcntr1, maxSpecEvalue);
        }

        public void WriteMfile(string resultsFilePath)
        {
            var mfile = new StreamWriter(resultsFilePath);
            mfile.Write(@"psmq=[");

            foreach (var psm in this)
            {
               // if (!psm.IsQuantified()) continue;
                foreach (var pi in psm.Quantities)
                    mfile.Write(pi + ",");
                mfile.WriteLine();
            }

            mfile.WriteLine("];");

            mfile.Write(@"psmlabelcntr=[");

            foreach (var psm in this)
            {
               // if (!psm.IsQuantified()) continue;
                mfile.Write(psm.Id.NumOfBoundLabels);
                mfile.WriteLine();
            }

            mfile.WriteLine(@"];");

            mfile.Write(@"psmsnr=[");

            foreach (var psm in this)
            {
              //  if (!psm.IsQuantified()) continue;
                mfile.Write(psm.GetSnr());
                mfile.WriteLine();
            }

            mfile.WriteLine(@"];");

            mfile.Close();
        }

        public void WriteTsv(string resultFilePath, string msgfPlusFilePath, bool overwrite = true)
        {
            var msgfResults = new SearchResults(msgfPlusFilePath, false, Params.InitSpecEValueThreshold);
            var tsvWriter = new StreamWriter(resultFilePath);
         
            //Making header
            var intensityHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("Quantity_{0}", labelNum + 1);
            var ratioHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("Ratio_{0}", labelNum + 1);

            var msgfSuffix = "_MS-GF+";
            var msgfHeaders = from msgfField in msgfResults.Header select msgfField + msgfSuffix;

            var sheet2Header = new List<string> { "PSMID", "Peptide", "MatchedProteins", "MatchedProteinGroup", "QuantifiedLabelCount", };
            sheet2Header.AddRange(intensityHeaders);
            sheet2Header.AddRange(ratioHeaders);
            sheet2Header.AddRange(new List<string> { "QValue", "XicClusterScore", "Decoy", "SNR", "BoundLabels", "m/z_Positions", "RT_Positions", "RtStart", "RtEnd", "Cosines" });
            sheet2Header.AddRange(msgfHeaders);

         
            // Adding data to array

            for (var i = 0; i < sheet2Header.Count-1; i++)
            {
                tsvWriter.Write(sheet2Header[i] + "\t");
            }
            tsvWriter.WriteLine(sheet2Header[sheet2Header.Count-1]);

            
            for (var i = 0; i < Count; i++)
            {
                var qpsm = this[i];

                var msgfData = msgfResults.GetAllDataOfID(qpsm.Id);

                tsvWriter.Write(qpsm.PsmId); tsvWriter.Write("\t");
                tsvWriter.Write(qpsm.Id.Peptide); tsvWriter.Write("\t");
                tsvWriter.Write(String.Join(";", qpsm.Id.Proteins)); tsvWriter.Write("\t");
                if (qpsm.ProteinGroup != null)
                {
                    tsvWriter.Write(String.Join(";", from qProt in qpsm.ProteinGroup.QuantifiedProteinSet select qProt.Name));
                    tsvWriter.Write("\t");
                }
                else
                {
                    tsvWriter.Write("N/A"); tsvWriter.Write("\t");
                }
                tsvWriter.Write(qpsm.GetQuantifiedLabelCount()); tsvWriter.Write("\t");
                foreach (var intensity in qpsm.Quantities) { tsvWriter.Write(Math.Abs(intensity)); tsvWriter.Write("\t"); }
                foreach (var ratio in qpsm.GetRatios()) { tsvWriter.Write(Math.Abs(ratio)); tsvWriter.Write("\t"); }
                tsvWriter.Write(qpsm.Qvalue); tsvWriter.Write("\t");
                tsvWriter.Write(qpsm.XicClusterScore); tsvWriter.Write("\t");
                tsvWriter.Write(qpsm.Id.IsDecoy()); tsvWriter.Write("\t");
                tsvWriter.Write(qpsm.GetSnr()); tsvWriter.Write("\t");
                tsvWriter.Write(qpsm.Id.NumOfBoundLabels); tsvWriter.Write("\t");
                tsvWriter.Write(String.Join(";", qpsm.MzPositions)); tsvWriter.Write("\t");
                tsvWriter.Write(String.Join(";", qpsm.RtPositions)); tsvWriter.Write("\t");
                tsvWriter.Write(qpsm.RtStart); tsvWriter.Write("\t");
                tsvWriter.Write(qpsm.RtEnd); tsvWriter.Write("\t");
                tsvWriter.Write(String.Join(";", qpsm.Cosines.Select(x => x.ToString()).ToArray())); tsvWriter.Write("\t");
                foreach (var ms in msgfData)
                {
                    tsvWriter.Write(ms);
                    tsvWriter.Write("\t");
                }

                tsvWriter.WriteLine();
            }
              
            tsvWriter.Close();
        }

        public void WriteExcel(string resultFilePath, Application xlApp, string msgfPlusFilePath, bool overwrite=true)
        {
            var xlWorkBook = xlApp.Workbooks.Add();
            var msgfResults = new SearchResults(msgfPlusFilePath, false, Params.InitSpecEValueThreshold);

            var xlWorkSheet1 = (Worksheet)xlWorkBook.Worksheets.Add();
            xlWorkSheet1.Name = @"All Features";

            //Making header
            var intensityHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("'Quantity_{0}", labelNum + 1);
            var ratioHeaders = from labelNum in LabelList.LabelNumberArr select String.Format("'Ratio_{0}", labelNum + 1);

            var msgfSuffix = "_MS-GF+";
            var msgfHeaders = from msgfField in msgfResults.Header select "'" + msgfField + msgfSuffix;

            var sheet1Header = new List<string> {"'PSMID", "'Peptide", "'MatchedProteins", "'MatchedProteinGroup", "'QuantifiedLabelCount",};
            sheet1Header.AddRange(intensityHeaders);
            sheet1Header.AddRange(ratioHeaders);
            sheet1Header.AddRange(new List<string> { "'QValue", "'XicClusterScore", "'Decoy", "'SNR", "BoundLabels", "'m/z_Positions", "'RT_Positions", "'RtStart", "'RtEnd", "'Cosines" });
            sheet1Header.AddRange(msgfHeaders);

            // This fields should not be formatted automatically by Excel.
            var textFields = new List<string>
            {
                "'PSMID", "'Peptide", "'MatchedProteinGroup", "'Consines", "'#SpecFile" + msgfSuffix,
                "'SpecID" + msgfSuffix, "'FragMethod" + msgfSuffix, "'Peptide" + msgfSuffix, "'Protein" + msgfSuffix, 
                "'m/z_Positions", "'RT_Positions", "'MatchedProteins"
            };
            var sheetTextFiledIndices = new List<int>();
           
            for (var i = 0; i < sheet1Header.Count; i++)
            {
                if (textFields.Contains(sheet1Header[i]))
                {
                    sheetTextFiledIndices.Add(i);
                }
            }

            // Adding data to array
            
            var sheet1DataRange = xlWorkSheet1.Cells.Resize[Count+1, sheet1Header.Count];

            var sheet1Data = new object[1, sheet1Header.Count];
            for (var i = 0; i < sheet1Header.Count; i++)
            {
                sheet1Data[0, i] = sheet1Header[i];
            }

            var tmpRange = xlWorkSheet1.Range[xlWorkSheet1.Cells[1, 1], xlWorkSheet1.Cells[1, sheet1Header.Count]];
            tmpRange.Value2 = sheet1Data;

            var maxCntr = 500;

            for (var l = 0; l <= Count/maxCntr; l++)
            {
                var offset = l*maxCntr;
                sheet1Data = new object[maxCntr, sheet1Header.Count];
                for (var i = 0; i < maxCntr; i++)
                {
                    if (offset + i >= Count) break;
                    var qpsm = this[offset + i];

                    var msgfData = msgfResults.GetAllDataOfID(qpsm.Id);

                    var sheet1DataRow = new List<object>();
                    sheet1DataRow.Add(qpsm.PsmId);
                    sheet1DataRow.Add(qpsm.Id.Peptide);
                    sheet1DataRow.Add(String.Join(";", qpsm.Id.Proteins));
                    if (qpsm.ProteinGroup != null)
                    {
                        sheet1DataRow.Add(String.Join(";", from qProt in qpsm.ProteinGroup.QuantifiedProteinSet select qProt.Name));
                    }
                    else
                    {
                        sheet1DataRow.Add("N/A");
                    }
                    sheet1DataRow.Add(qpsm.GetQuantifiedLabelCount());
                    foreach (var intensity in qpsm.Quantities) { sheet1DataRow.Add(Math.Abs(intensity)); }
                    foreach (var ratio in qpsm.GetRatios()) { sheet1DataRow.Add(Math.Abs(ratio)); }
                    sheet1DataRow.Add(qpsm.Qvalue);
                    sheet1DataRow.Add(qpsm.XicClusterScore);
                    sheet1DataRow.Add(qpsm.Id.IsDecoy());
                    sheet1DataRow.Add(qpsm.GetSnr());
                    sheet1DataRow.Add(qpsm.Id.NumOfBoundLabels);
                    sheet1DataRow.Add(String.Join(";", qpsm.MzPositions));
                    sheet1DataRow.Add(String.Join(";", qpsm.RtPositions));
                    sheet1DataRow.Add(qpsm.RtStart);
                    sheet1DataRow.Add(qpsm.RtEnd);
                    sheet1DataRow.Add(String.Join(";", qpsm.Cosines.Select(x => x.ToString()).ToArray()));
                    sheet1DataRow.AddRange(msgfData);

                    if (sheet1DataRow.Count != sheet1Data.GetLength(1))
                    {
                        throw new Exception("Length of sheet1DataRow does not match with length of sheet1Header");
                    }

                    for (var j = 0; j < sheet1Header.Count; j++)
                    {
                        if (sheetTextFiledIndices.Contains(j))
                        {
                            sheet1Data[i, j] = "'" + sheet1DataRow[j];
                        }
                        else
                        {
                            sheet1Data[i, j] = sheet1DataRow[j];
                        }
                    }
                }
                tmpRange = xlWorkSheet1.Range[xlWorkSheet1.Cells[2 + offset, 1], xlWorkSheet1.Cells[1 + maxCntr + offset, sheet1Header.Count]];
                tmpRange.Value2 = sheet1Data;
            }
            xlWorkSheet1.Activate();
            xlWorkSheet1.Application.ActiveWindow.SplitRow = 1;
            xlWorkSheet1.Application.ActiveWindow.FreezePanes = true;
            Range firstRow = xlWorkSheet1.Rows[1];
            firstRow.AutoFilter(1, Type.Missing, XlAutoFilterOperator.xlAnd, Type.Missing, true);

            if (overwrite)
            {
                xlApp.DisplayAlerts = false;
                xlWorkBook.Close(SaveChanges:true, Filename:resultFilePath);
            }
            else
            {
                xlApp.DisplayAlerts = true;
                xlWorkBook.Close(SaveChanges:true, Filename:resultFilePath);
            }

            Marshal.ReleaseComObject(xlWorkBook);
        }

        public void WriteTrainingFile(string resultPath)
        {
            Console.WriteLine(@"Writing traing output files ...");
            /*
            using (var txtWriter = new StreamWriter(resultPath))
            {
                foreach (var psm in this)
                {
                    if (psm == null) throw new Exception(@"qfm is null");
                    if (psm.TrainingDictionary == null) throw new Exception(@"training dictionary is null");
                    if (psm.Id.IsDecoy()) continue;
                    if (psm.Id.IsContam()) continue;

                    foreach (var cn in LabelList.LabelNumberArr)
                    {
                        var trainingStr = psm.TrainingDictionary[cn];
                        if (trainingStr == null) continue;
                        var parsedStr = trainingStr.Split('\t');
                        var normedEtDiff = parsedStr[0];
                        var dCount = parsedStr[1];
                        var normedRepreEt = parsedStr[2];
                        var pepLen = parsedStr[3];
                        var aaRatio = parsedStr[4];

                        txtWriter.WriteLine("{0} 1:{1} 2:{2} 3:{3} 4:{4}", normedEtDiff, dCount, normedRepreEt, pepLen, aaRatio);
                    }
                }
            }
             */

            var pandasPath = Path.ChangeExtension(resultPath, @".tsv");
            using (var pandasWriter = new StreamWriter(pandasPath))
            {
                var index = 0;
                var dAaStrings = new List<string>();
                foreach (var aa in LabelList.DeuteratedLabelingSites)
                {
                    dAaStrings.Add("[" + aa +"]Ratio");
                }
                pandasWriter.WriteLine("DEFeatureIdx\tPeptide\tCharge\tSpecEValue\tChannel\tCosine" +
                                       "\tNormedApexEtDiff\tDnum\tNormedRepreEt\tPeptideLength\t" + 
                                       String.Join("\t", dAaStrings));
                foreach (var psm in this)
                {
                    if (psm == null) throw new Exception(@"qfm is null");
                    if (psm.TrainingDictionary == null) throw new Exception(@"training dictionary is null");
                    if (psm.Id.IsDecoy()) {continue;}
                    if (psm.Id.IsContam()) {continue;}

                    foreach (var cn in LabelList.LabelNumberArr)
                    {
                        var trainingStr = psm.TrainingDictionary[cn];
                        if (trainingStr == null) continue;
                        pandasWriter.WriteLine("{0}\t{1}", index, trainingStr);
                    }
                    index++;
                }
            }
        }

    }
}