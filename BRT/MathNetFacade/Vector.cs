using Interfaces;
using MathNet.Numerics.LinearAlgebra;

namespace MathNetFacade
{
    public class Vector : IVector
    {
        public Vector<double> VectorData;

        public Vector(Vector<double> vectorData)
        {
            VectorData = Vector<double>.Build.DenseOfVector(vectorData);
        }

        public Vector IsSameType(IVector other)
        {
            Vector converted = other as Vector ?? throw new ArgumentException("Two vectors use different implementations");
            return converted;
        }

        public IVector Subtract(IVector subtrahend)
        {
            Vector subtrahendVector = IsSameType(subtrahend);
            return new Vector(VectorData.Subtract(subtrahendVector.VectorData));
        }

        public double Dot(IVector other)
        {
            Vector validOther = IsSameType(other);
            return VectorData.DotProduct(validOther.VectorData);
        }

        public IMatrix OuterProduct()
        {
            return new MNMatrix(VectorData.OuterProduct(VectorData));
        }

        public bool Equals(IVector other)
        {
            Vector validOther = IsSameType(other);
            return VectorData.Equals(validOther.VectorData);
        }
    }
}