
namespace Interfaces
{
    public interface IMatrix
    {
        int SampleDimensionality { get; }
        int SampleCount { get; }
        double Determinant { get; }
        IVector Mean { get; }
        IMatrix Transposed { get; }
        IMatrix CovarianceMatrix { get; }
        IMatrix SampleAsRowMatrix(int sampleIdx);
        IMatrix Sample(int sampleIdx1, int sampleIdx2);
        IMatrix Add(IMatrix addend1, IMatrix addend2);
        IMatrix Multiply(double factor);
        IMatrix Divide(double divisor);

    }
}
