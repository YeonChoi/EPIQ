using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Epiq
{
    public class LabelingEfficiencyCorrection
    {
        private static Dictionary<char, float[]> SilacHardCodedLe = new Dictionary<char, float[]>
        {
            {'K', new []{1f, 0.995f, 0.997f, 0.997f, 0.995f, 0.994f}},
            {'R', new []{1f, 0.987f, 0.991f, 0.992f, 0.987f, 0.994f}},
        };


        private static Dictionary<char, float[]> _labelingEfficiencyDictionary;
        private static Dictionary<char, Matrix<float>> _labelingEfficiencyCorrectionMatrices = new Dictionary<char, Matrix<float>>();
        private static bool _useLabelingEfficiencyCorrection;

        public LabelingEfficiencyCorrection()
        {
        }

        public static bool ParseLabelingEfficiencyValues(bool useCorrection)
        {
            _useLabelingEfficiencyCorrection = useCorrection;
            Console.WriteLine("Use labeling efficiency correction: {0}", _useLabelingEfficiencyCorrection);
            if (!_useLabelingEfficiencyCorrection)
            {
                return false;
            }

            _labelingEfficiencyDictionary = SilacHardCodedLe; // TODO Read this from string, check whether 0 exists or not
            foreach (var aa in _labelingEfficiencyDictionary.Keys)
            {
                _labelingEfficiencyCorrectionMatrices[aa] = LabelingEffciencyToInverseMatrix(_labelingEfficiencyDictionary[aa]);
            }
            return true;
        }

        public static float[] CorrectQuantities(float[] quantities, Ms2Result id)
        {
            if (!_useLabelingEfficiencyCorrection) return quantities;

            var labeledAaCounts = LabelList.GetLabeledAaCounts(id);
            var correctedQuantities = new DenseVector(quantities);

            foreach (var aa in labeledAaCounts.Keys)
            {
                for (var i = 0; i < labeledAaCounts[aa]; i++)
                {
                    correctedQuantities = _labelingEfficiencyCorrectionMatrices[aa] * correctedQuantities as DenseVector;
                    if (correctedQuantities[0] < 0) correctedQuantities[0] = 0.0f;

                    //Console.WriteLine("{0} correction: {1}", aa, String.Join("\t", correctedQuantities));
                }
            }

            return correctedQuantities.ToArray();
        }


        private static Matrix<float> LabelingEffciencyToInverseMatrix(float[] leArr)
        {
            var leMat = new DenseMatrix(leArr.Length, leArr.Length);

            for (var j = 1; j < leArr.Length; j++)
            {
                leMat[0, j] = 1 - leArr[j];
            }
            for (var i = 0; i < leArr.Length; i++)
            {
                leMat[i, i] = leArr[i];
            }
            // TODO: Check invertibality.
            return leMat.Inverse();
        }
    }
}
