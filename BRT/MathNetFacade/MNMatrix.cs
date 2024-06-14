using Interfaces;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;

namespace MathNetFacade
{
    public class MNMatrix : IMatrix
    {
        public Matrix<double> MatrixData { get; set; }

        public int SampleDimensionality => MatrixData.ColumnCount;

        public int SampleCount => MatrixData.RowCount;

        public double Determinant => MatrixData.Determinant();

        public IVector Mean
        {
            get
            {
                return new Vector(MatrixData.ReduceRows((a, b) => a + b).Divide(MatrixData.RowCount));
            }
        }

        public IMatrix Transposed => new MNMatrix(MatrixData.Transpose());

        public IMatrix CovarianceMatrix
        {
            get
            {
                int featureCount = MatrixData.RowCount;
                Matrix<double> cov = Matrix<double>.Build.Dense(featureCount, featureCount);
                for (int i = 0; i < featureCount; ++i)
                {
                    for (int j = 0; j <= i; ++j)
                    {
                        cov[j, i] = cov[i, j] = MatrixData.Row(i).Covariance(MatrixData.Row(j));
                    }
                }

                return new MNMatrix(cov);
            }
        }

        public MNMatrix(double[] data, int collumns)
        {
            MatrixData = Matrix<double>.Build.Dense(data.Length / collumns, collumns, data);
        }

        public MNMatrix(Matrix<double> matrixData) => MatrixData = matrixData;

        public IMatrix SampleAsRowMatrix(int sampleIdx)
        {
            return new MNMatrix(MatrixData.Row(sampleIdx).ToRowMatrix());
        }

        public IMatrix Sample(int sampleIdx1, int sampleIdx2)
        {
            return new MNMatrix(MatrixData.Row(sampleIdx1).ToRowMatrix().InsertRow(1, MatrixData.Row(sampleIdx2)));
        }

        public IMatrix Add(IMatrix addend1, IMatrix addend2)
        {
            MNMatrix addend1Matrix = addend1 as MNMatrix ?? throw new ArgumentException("Matrices use different implementations");
            MNMatrix addend2Matrix = addend2 as MNMatrix ?? throw new ArgumentException("Matrices use different implementations");
            return new MNMatrix(MatrixData.Add(addend1Matrix.MatrixData).Add(addend2Matrix.MatrixData));
        }

        public IMatrix Multiply(double factor)
        {
            return new MNMatrix(MatrixData.Multiply(factor));
        }

        public IMatrix Divide(double divisor)
        {
            return new MNMatrix(MatrixData.Divide(divisor));
        }
    }
}
