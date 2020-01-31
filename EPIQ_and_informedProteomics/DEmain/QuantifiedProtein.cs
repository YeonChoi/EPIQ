using System;
using System.Collections.Generic;
using System.Linq;
using InformedProteomics.Backend.Utils;
using MathNet.Numerics.LinearAlgebra;

namespace Epiq
{
    [Serializable]
    public class QuantifiedProtein : IQuantifiable
    {
        private static float _cosineThreshold;
        public float QvalueScore { private set; get; }
        public bool IsDecoy { private set; get; }
        public int MatchedPsmCount { get; private set; }
        public int MatchedPeptideCount { get; private set; }
        public List<QuantifiedPsm> MatchedPsms { get; private set; }
        
        public string Name { get; private set; }
        //IQuantifiable members
        public int LabelCount { get; private set; }
        public float SignalPower { get; private set; }
        public float NoisePower { get; private set; }
        public float[] Quantities { get; private set; }

        public int GetQuantifiedLabelCount()
        {
            var ret = 0;
            foreach (var intensity in Quantities)
            {
                if (Math.Abs(intensity) > 0.1) ret++;
            }
            return ret;
        }

    

        public float[] GetRatios()
        {
            var sum = Quantities.Sum();
            if (sum <= 0) return Quantities;
            var ratio = new float[LabelCount];
            for (var i = 0; i < ratio.Length; i++)
            {
                ratio[i] = Quantities[i] / sum;
            }
            return ratio;
        }

      
        public float GetSnr()
        {
            return SignalPower / (NoisePower + 1e-6f);
        }

        public QuantifiedProtein(string name, float cosineThreshold)
        {
            Name = name;
            _cosineThreshold = cosineThreshold;
        }

        public bool SetMatchedPsmsAndQvalueScore(List<QuantifiedPsm> psmList, bool[] adjMatrix, out List<int> usedIndices)
        {
            var connected = new List<QuantifiedPsm>();
            var connectedIndices = new List<int>();
            for (var i = 0; i < adjMatrix.Length; i++)
            {
                if (!adjMatrix[i]) continue;
                connected.Add(psmList[i]);
                connectedIndices.Add(i);
            }
            List<int> validColumns;
            Quantities = GetProteinIntensities(connected, out validColumns);
            usedIndices = new List<int>();
            MatchedPsms = new List<QuantifiedPsm>();

            foreach (var i in validColumns)
            {
                usedIndices.Add(connectedIndices[i]);
                MatchedPsms.Add(psmList[connectedIndices[i]]);
            }
            SignalPower = NoisePower = 0;
            if (!MatchedPsms.Any()) return Quantities != null;

            SetProteinQvalueScore(MatchedPsms);
            foreach (var psm in MatchedPsms)
            {
                SignalPower += (float)Math.Sqrt(psm.SignalPower);
                NoisePower += psm.NoisePower;
                LabelCount = psm.LabelCount;
            }
            SignalPower = SignalPower*SignalPower;

            return Quantities != null;
        }

        public bool IsFullyQuantified()
        {
            return Quantities != null && Quantities.Min() > 0;
        }

        public bool IsQuantified()
        {
            return Quantities!=null && Quantities.Max() > 0;
        }



        private void SetProteinQvalueScore(List<QuantifiedPsm> qfs)
        {
            QuantifiedPsm bestQf = null;
            QvalueScore = 100;
            foreach (var qf in qfs)
            {
                QvalueScore = Math.Min(QvalueScore, qf.Qvalue);
                if (bestQf != null && bestQf.Qvalue < qf.Qvalue) continue;
                bestQf = qf;
            }
            if (bestQf == null) return;
            
            IsDecoy = bestQf.Id.IsDecoy();
        }


        public float GetCosineWithPsmQuantity(QuantifiedPsm psm)
        {
            if (MatchedPsmCount == 0) return 0;
            if (MatchedPsmCount == 1) return _cosineThreshold + 1e-6f;
            var pepQ = Vector<float>.Build.DenseOfArray(psm.Quantities);
            return GetCosineBetween(pepQ, Vector<float>.Build.DenseOfArray(Quantities));
                // corr between sum and psm
        }

        public static Matrix<float> GetObservationMatrix(List<QuantifiedPsm> psms)
        {
            if (psms.Count == 0) return null;
            var builder = Matrix<float>.Build;
            var om = builder.Dense(LabelList.LabelNumberArr.Length, psms.Count);

            for (var c = 0; c < om.ColumnCount; c++)
            {
                var qs = psms[c];
                for (var r = 0; r < om.RowCount; r++)
                {
                    var q = qs.Quantities[r];
                    //if (q > 0) 
                    om[r, c] = q; // > 0? q : q/2;
                   
                }
            }
            //om = om + 1e5f;
            return om;
        }

        private Vector<float> GetSv(Matrix<float> om, Vector<float> qv)
        {
            qv.Normalize(1);
            var sv = Vector<float>.Build.Dense(om.ColumnCount);
            for (var c = 0; c < sv.Count; c++)
            {
                var ratioSum = 0f;
                var sum = 0f;
                for (var r = 0; r < om.RowCount; r++)
                {
                    if (Math.Abs(om[r, c]) < 1e-10) continue;
                    sum += Math.Abs(om[r, c]);
                    ratioSum += Math.Abs(qv[r]);
                }
                sv[c] = ratioSum <= 0 ? 0 : sum/ratioSum;
            }
            return sv;
        }

        private Vector<float> GetQv(Matrix<float> om)
        {
            var qv = om.RowSums();
            return qv.Normalize(1);
        }



        private static float GetCosineBetween(Vector<float> a, Vector<float> b)
        {
            
            var an = a.Clone();
            var bn = b.Clone();
            for (var i = 0; i < a.Count; i++)
            {
               an[i] = Math.Abs(a[i]);
               bn[i] = Math.Abs(b[i]);
            }

            return FitScoreCalculator.GetCosine(an.ToArray(), bn.ToArray());
        }

  

        public static Matrix<float> GetOutlierRemovedOm(Matrix<float> om, float cosineThrehsold, out List<int> validColumns)
        {
            validColumns = new List<int>();

            //   var w = (om).ColumnSums().PointwiseLog();
            //   var W = Matrix<float>.Build.DenseOfDiagonalVector(w);
            var normalizedOm = om.Clone();//.NormalizeColumns(1); // * W;
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
        }



        private float[] GetProteinIntensities(List<QuantifiedPsm> qfList, out List<int> validColumns)
        {
            validColumns = new List<int>();
            if (qfList == null) return null;

            var om = GetObservationMatrix(qfList);
            if (om == null) return null; // om.RowAbsoluteSums().AbsoluteMinimum() == 0

            var qv = GetQv(om); //om.RowSums().Normalize(1);
            // om = GetObservationMatrix(qfList);
            //GetQv(om);

            var prevValidColumns = validColumns;

            var nom = om;
            var prevNom = nom;
            for (var i = 0; i < 100; i++)
            {
                //float meanCos;
                nom = GetOutlierRemovedOm(nom, _cosineThreshold, out validColumns);
                //if (meanCos < _cosineThreshold) return null;
                if (nom == null)
                {
                    nom = prevNom;
                    validColumns = prevValidColumns;
                    qv = GetQv(nom);
                    break;
                }
                if (nom.Equals(prevNom)) break;
                qv = GetQv(nom);
                prevValidColumns = validColumns;
                prevNom = nom;
            }
            if (validColumns.Count < Params.NumMatchedPsmsPerProtein) return null;

            var pepSet = new HashSet<string>();
            var qfSet = new HashSet<QuantifiedPsm>();

            foreach (var vc in validColumns)
            {
                var qf = qfList[vc];
                 qfSet.Add(qf);
                 pepSet.Add(qf.Id.UnlabeledPeptide);
            }
            MatchedPsmCount = qfSet.Count;
            MatchedPeptideCount = pepSet.Count;
            var scale = GetSv(nom, qv).Sum();
            return (scale*qv).ToArray();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }


        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var p = obj as QuantifiedProtein;
            return (p != null) && Name.Equals(p.Name);
        }


    }
}