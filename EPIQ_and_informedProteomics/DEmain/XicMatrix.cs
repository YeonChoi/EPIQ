using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;

namespace Epiq
{
    public class XicMatrix
    {
        protected XicMatrix()
        {
        }

        protected XicMatrix(float[,] intensities, float[] rts, float[] mzs)
        {
            Rts = rts;
            Mzs = mzs;
            M = Matrix<float>.Build.DenseOfArray(intensities);
        }

        public Matrix<float> M { get; protected set; }
        public float[] Rts { get; protected set; }
        public float[] Mzs { get; protected set; }

        public void AddLastNMzBinsToFirstNMzBins(int n)
        {
            for (var i = 0; i < n; i++)
                M.Row(i).Add(M.Row(M.RowCount - n + i));
        }

        public static XicMatrix GetXicMatrixFromVector(Vector<float> v, float[] rts, float[] mzs, List<int> activeRows)
        {
            var ret = new XicMatrix {Rts = rts, Mzs = mzs, M = Matrix<float>.Build.Dense(mzs.Length, rts.Length)};
            var i = 0;
            foreach (var r in activeRows)
                ret.M.SetRow(r, v.SubVector(i++*ret.M.ColumnCount, ret.M.ColumnCount));
            return ret;
        }


        public Vector<float> ToVector(List<int> activeRowIndices, Matrix<float> signalMatrix)
        {
            var vector = Vector<float>.Build.Dense(activeRowIndices.Count*M.ColumnCount);
            for (var i = 0; i < activeRowIndices.Count; i++)
            {
                var rv = M.Row(activeRowIndices[i]);
                // var refRv = referenceMatrix.M.Row(activeRowIndices[i]);
                // for(var j=0;j<rv.NumOfBoundLabels;j++) if (refRv[j] == 0) rv[j] = 0;
                var sig = signalMatrix.Row(activeRowIndices[i]);
                vector.SetSubVector(i*M.ColumnCount, M.ColumnCount, rv.PointwiseMultiply(sig)); //);
            }
            return vector;
        }

        public float GetArea(int mzBin)
        {
            var area = .0f;
            var r = M.Row(mzBin);
            var prev = r[0];
            for (var i = 1; i < r.Count; i++)
            {
                var curr = r[i];
                area += (prev + curr)/2*(Rts[i] - Rts[i - 1]);
                prev = curr;
            }
            return area;
        }

        protected void AddInterpolatable(Interpolatable it, int row, float dc = 0f, float max = 1f)
        {
            for (var col = 0; col < Rts.Length; col++)
            {
                M[row, col] += (it.InterpolateAt(Rts[col]) - dc) * max / (max - dc);
            }
        }
        protected void AddInterpolatable(Interpolatable it, int row, float[] sigRange)
        {
            var ints = new List<float>();
            var max = float.MinValue;

            for (var col = 0; col < Rts.Length; col++)
            {
                var v = M[row, col] = it.InterpolateAt(Rts[col]);
                if (Rts[col] > sigRange[0] && Rts[col] < sigRange[1])
                {
                    max = Math.Max(v, max);
                    // if (Rts[col] < sigRange[1]) 
                    //if(v> 0) 
                        ints.Add(v);
                }
            }
            if (max <= float.MinValue) return;

            //Console.WriteLine(max + " " + min);
            //min = it.InterpolateAt(sigRange[0]);
            //min /= 2;
            var min = ints.Min();
            if (min < 0) return;
            //if (min < ints.Median()/2) min = 0;

            for (var col = 0; col < Rts.Length; col++)
            {
                M[row, col] = 1 + (M[row, col] - min)*max/(max - min + 1);// + (max - min) / 100;
            }
        }

        public XicMatrix GetIntensityAdjustedXicMatrix(XicMatrix m)
        {
            var ret = new XicMatrix
            {
                Rts = Rts,
                Mzs = Mzs,
                M = Matrix<float>.Build.Dense(M.RowCount, M.ColumnCount)
            };
            var majorIsotopeRow = 0;
            var max = .0;
            for (var row = 0; row < M.RowCount; row++)
            {
                if (max > M.Row(row).Max()) continue;
                max = M.Row(row).Max();
                majorIsotopeRow = row;
            }

            var maxIndex = M.Row(majorIsotopeRow).MaximumIndex();
            var factor = m.M[majorIsotopeRow, maxIndex] - m.M.Row(majorIsotopeRow).Min() + 1;
            for (var row = 0; row < M.RowCount; row++)
            {
                var min = m.M.Row(row).Min();
                var mrow = M.Row(row);
                ret.M.SetRow(row, mrow*factor + min);
            }
            return ret;
        }

        public static XicMatrix GetDcComponentMatrix(int mzBin, float[] mzs, float[] ets, float value)
        {
            var intensities = new float[mzs.Length, ets.Length];
            for (var etBin = 0; etBin < ets.Length; etBin++)
                intensities[mzBin, etBin] = value;
            return new XicMatrix(intensities, ets, mzs);
        }


        public static XicMatrix GetDcComponentMatrixAlongMz(int rtBin, float[] mzs, float[] ets, float value)
        {
            var intensities = new float[mzs.Length, ets.Length];
            for (var mzBin = 0; mzBin < mzs.Length; mzBin++)
                intensities[mzBin, rtBin] = value;
            return new XicMatrix(intensities, ets, mzs);
        }

        public static XicMatrix GetDcComponentMatrix(int mzBin, float[] mzs, float[] ets, float[] values)
        {
            var intensities = new float[mzs.Length, ets.Length];
            for (var rtBin = 0; rtBin < ets.Length; rtBin++)
                intensities[mzBin, rtBin] = values[rtBin];
            return new XicMatrix(intensities, ets, mzs);
        }

        public static XicMatrix GetDcComponentMatrix(float[] mzs, float[] ets, float value)
        {
            var intensities = new float[mzs.Length, ets.Length];
            for (var mzBin = 0; mzBin < mzs.Length; mzBin++)
                for (var rtBin = 0; rtBin < ets.Length; rtBin++)
                    intensities[mzBin, rtBin] = value;
            return new XicMatrix(intensities, ets, mzs);
        }

        public static XicMatrix GetDcComponentMatrixOnSignal(Matrix<float> signalMatrix, float[] mzs, float[] ets)
        {
           
            return new XicMatrix(signalMatrix.ToArray(), ets, mzs);
        }

        public static XicMatrix GetDcComponentMatrixAroundSignal(int mzBin, Matrix<float> signalMatrix, float[] mzs,
            float[] rts, bool left)
        {
            var intensities = new float[mzs.Length, rts.Length];
            var sig = signalMatrix.Row(mzBin);
            var set = false;
            if (left)
                for (var rtBin = 0; rtBin < rts.Length; rtBin++)
                {
                    if (sig[rtBin] > 0) break;
                    intensities[mzBin, rtBin] = 1;
                    set = true;
                }
            else
                for (var rtBin = rts.Length - 1; rtBin >= 0; rtBin--)
                {
                    if (sig[rtBin] > 0) break;
                    intensities[mzBin, rtBin] = 1;
                    set = true;
                }
            return set ? new XicMatrix(intensities, rts, mzs) : null;
        }

        public static XicMatrix GetSignificantXicPeakMatrix(InterpolatedXic iXic, float apexRt, int mzBin, float[] mzs,
            float[] rts, float[] signalRange, float corrThreshold, out float templateApexRt)
        {
            var template = XicShape.GetXicShape(iXic, apexRt, signalRange);
            templateApexRt = .0f;
            if ((template == null) || (template.Cosine < corrThreshold)) return null;
            templateApexRt = template.Xapex;
            var intensities = new float[mzs.Length, rts.Length];
            var max = 0f;
            for (var rtBin = 0; rtBin < rts.Length; rtBin++)
            {
                intensities[mzBin, rtBin] = template.InterpolateAt(rts[rtBin]);
                max = Math.Max(max, intensities[mzBin, rtBin]);
            }
            if (max > 0)
                for (var rtBin = 0; rtBin < rts.Length; rtBin++)
                    intensities[mzBin, rtBin] /= max;
            else return null;
            return new XicMatrix(intensities, rts, mzs);
        }

        public void Write(StreamWriter sw, string var)
        {
            sw.WriteLine(@"clear " + var + @";");
            sw.Write(var + @"={");

            for (var row = 0; row < Mzs.Length; row++)
            {
                sw.Write(@"[");
                var min = M.Row(row).Min();
                for (var col = 0; col < Rts.Length; col++)
                {
                    if (M[row, col] <= min) continue;
                    sw.Write(Mzs[row] + @"," + Rts[col] + @"," + M[row, col] + @";");
                }

                sw.Write(@"],");
            }
            sw.WriteLine(@"};");

            // print xic ,

            //PrintMatrix(M, @"QF");
            //foreach (var cn in Params.LabelNumArr())
            //    if (_extendedTemplates[cn]!=null) PrintMatrix(_extendedTemplates[cn].M, @"T" + cn);
        }

        #region static functions

        public static void PrintMatrix(Matrix<float> m, string name)
        {
            Console.Write(name + @"=[");
            for (var r = 0; r < m.RowCount; r++)
            {
                for (var c = 0; c < m.ColumnCount; c++)
                    Console.Write(@"{0:e2},", m[r, c]);
                Console.Write(@";");
            }
            Console.WriteLine(@"];");
        }

        public static void PrintVector(Vector<float> m, string name)
        {
            Console.Write(name + @"=[");
            foreach (var t in m)
                Console.Write(@"{0:.00},", t);
            Console.WriteLine(@"];");
        }

        #endregion
    }
}