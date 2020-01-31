using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Epiq
{
    public class MatrixCalculation
    {
        private static bool IsAnyElementTrue(bool[] bs)
        {
            foreach (var b in bs)
                if (b) return true;
            return false;
        }

        private static bool IsAnyElementNonPositive(Vector<float> z, bool[] positives)
        {
            for (var i = 0; i < positives.Length; i++)
            {
                if (!positives[i]) continue;
                if (z[i] <= 0) return true;
            }
            return false;
        }

        private static bool IsAnyElementWithoutTolerance(Vector<float> w, bool[] z, float tol)
        {
            for (var i = 0; i < w.Count; i++)
            {
                if (!z[i]) continue;
                if (w[i] > tol) return true;
            }
            return false;
            //any(w(Z) > tol)
        }

        private static void SetVector(Vector<float> v, bool[] b, Vector<float> d)
        {
            for (var i = 0; i < v.Count; i++)
            {
                if (!b[i]) continue;
                v[i] = d[i];
            }
        }

        private static void SetVector(Vector<float> v, bool[] b, float d)
        {
            for (var i = 0; i < v.Count; i++)
            {
                if (!b[i]) continue;
                v[i] = d;
            }
        }

        private static bool Solve(Vector<float> z, bool[] positives, Matrix<float> cMatrix, Vector<float> dVector)
        {
            var setBit = new List<int>();
            for (var i = 0; i < positives.Length; i++) if (positives[i]) setBit.Add(i);
            if (setBit.Count <= 0) return false;
            var tC = Matrix<float>.Build.Dense(cMatrix.RowCount, setBit.Count);
            for (var i = 0; i < setBit.Count; i++)
                tC.SetColumn(i, cMatrix.Column(setBit[i]));
            var tCInv = MoorePenrosePsuedoinverse(tC);
            var sol = tCInv*dVector;
            for (var i = 0; i < setBit.Count; i++)
                z[setBit[i]] = sol[i];
            return true;
        }

        public static Matrix<float> MoorePenrosePsuedoinverse(Matrix<float> x)
        {
            const float macheps = 1e-6f;
            if (x.ColumnCount > x.RowCount)
            {
                var t = MoorePenrosePsuedoinverse(x.Transpose());
                return t == null ? null : t.Transpose();
            }
            
            return x.QR().Solve(DenseMatrix.CreateIdentity(x.RowCount));
            //Console.WriteLine(x);


            var svdX = x.Svd();
            if (svdX.Rank < 1)
                return null;
            var w = svdX.W;

            var tol = Math.Max(x.ColumnCount, x.RowCount)*w[0, 0]*macheps;

            var uColumnVectors = new List<Vector<float>>();
            var vRowVectors = new List<Vector<float>>();
            for (var i = 0; i < Math.Min(w.RowCount, w.ColumnCount); i++)
            {
                if (Math.Abs(w[i, i]) < tol) continue;
                var t = 1.0f/w[i, i];

                uColumnVectors.Add(svdX.U.Column(i)*t);
                vRowVectors.Add(svdX.VT.Row(i));
            }
            var mb = Matrix<float>.Build;
            return (mb.DenseOfColumnVectors(uColumnVectors)*mb.DenseOfRowVectors(vRowVectors)).Transpose();
                //svdX.VT.Transpose() * w.Transpose() * svdX.U.ConjugateTranspose();
        }


        public static Vector<float> NonNegativeLeastSquare(Matrix<float> cMatrix, Vector<float> dVector)
        {
            const float tol = 1e-5f; // set later
            var vb = Vector<float>.Build;
            var n = cMatrix.ColumnCount;
            var wz = vb.Dense(n);
            var positives = new bool[n];
            var zeros = new bool[n];
            for (var i = 0; i < zeros.Length; i++) zeros[i] = true;
            var x = vb.Dense(n);
            var resid = dVector - cMatrix*x;
            var w = cMatrix.Transpose()*resid;
            var iter = 0;
            var itmax = 3*n;
            var run = true;
            while (run)
            {
                var z = vb.Dense(n);
                SetVector(wz, positives, float.NegativeInfinity);
                SetVector(wz, zeros, w);
                var t = wz.MaximumIndex();
                positives[t] = true;
                zeros[t] = false;
                if (!Solve(z, positives, cMatrix, dVector)) break;
                //iter = 0;
                while (IsAnyElementNonPositive(z, positives))
                {
                    if (iter++ > itmax) return z;
                    var xQa = new List<float>();
                    var zQa = new List<float>();
                    for (var i = 0; i < z.Count; i++)
                    {
                        if (positives[i] & (z[i] > 0)) continue;
                        xQa.Add(x[i]);
                        zQa.Add(z[i]);
                    }
                    var xQ = Vector<float>.Build.DenseOfEnumerable(xQa);
                    var zQ = Vector<float>.Build.DenseOfEnumerable(zQa);
                    var alpha = xQ.PointwiseDivide(xQ - zQ).Minimum();
                    x = x + alpha*(z - x);
                    for (var i = 0; i < zeros.Length; i++)
                        zeros[i] = zeros[i] || ((Math.Abs(x[i]) < tol) && positives[i]);

                    for (var i = 0; i < zeros.Length; i++)
                        positives[i] = !zeros[i];
                    z = vb.Dense(n);
                    if (!Solve(z, positives, cMatrix, dVector)) break;
                }
                x = z;
                resid = dVector - cMatrix*x;
                w = cMatrix.Transpose()*resid;
                run = IsAnyElementTrue(zeros) && IsAnyElementWithoutTolerance(w, zeros, tol);
            }
            return x;
        }
    }
}