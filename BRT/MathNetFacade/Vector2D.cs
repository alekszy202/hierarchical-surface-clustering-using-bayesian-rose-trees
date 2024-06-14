using Interfaces;
using MathNet.Numerics.LinearAlgebra;

namespace MathNetFacade
{
    public class Vector2D : IVector
    {
        private Vector<double> _vectorData;

        public double X { get => _vectorData[0]; }
        public double Y { get => _vectorData[1]; }
        public Vector<double> VectorData { get => _vectorData; set => _vectorData = value; }
        public static Vector2D Zero { get => new Vector2D(0, 0); }
        public static Vector2D One { get => new Vector2D(1, 1); }

        public Vector2D()
        {
            _vectorData = Vector<double>.Build.Dense(new double[] { 0, 0 });
        }

        public Vector2D(double x, double y)
        {
            _vectorData = Vector<double>.Build.Dense(new double[] { x, y });
        }

        public Vector2D(Vector<double> other)
        {
            if (other.Count == 2)
                _vectorData = Vector<double>.Build.DenseOfVector(other);
            else
                throw new ArgumentException($"Vector2D class requires vector of 2 elements, got {other.Count}.");
        }

        public Vector2D IsSameType(IVector other)
        {
            Vector2D converted = other as Vector2D ?? throw new ArgumentException("Two vectors use different implementations");
            return converted;
        }

        public IVector Subtract(IVector subtrahend)
        {
            Vector2D subtrahendVector2D = IsSameType(subtrahend);
            return new Vector2D(VectorData.Subtract(subtrahendVector2D.VectorData));
        }

        public double Distance(Vector2D other)
        {
            return Math.Sqrt(DistanceSquared(other));
        }

        public double DistanceSquared(Vector2D other)
        {
            return Math.Pow(other.X - this.X, 2) +
                   Math.Pow(other.Y - this.Y, 2);
        }

        public double Dot(IVector other)
        {
            Vector2D validOther = IsSameType(other);
            return _vectorData.DotProduct(validOther.VectorData);
        }

        public IMatrix OuterProduct()
        {
            return new MNMatrix(VectorData.OuterProduct(VectorData));
        }

        public bool Equals(IVector other)
        {
            Vector2D validOther = IsSameType(other);
            return _vectorData.Equals(validOther.VectorData);
        }

        public static Vector2D operator +(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2D operator *(Vector2D v, double scalar)
        {
            return new Vector2D(v.X * scalar, v.Y * scalar);
        }

        public static Vector2D operator *(double scalar, Vector2D v)
        {
            return new Vector2D(v.X * scalar, v.Y * scalar);
        }

        public static Vector2D operator *(Vector2D v, float scalar)
        {
            return new Vector2D(v.X * scalar, v.Y * scalar);
        }

        public static Vector2D operator *(float scalar, Vector2D v)
        {
            return new Vector2D(v.X * scalar, v.Y * scalar);
        }

        public static Vector2D operator *(Vector2D v, int scalar)
        {
            return new Vector2D(v.X * scalar, v.Y * scalar);
        }

        public static Vector2D operator *(int scalar, Vector2D v)
        {
            return new Vector2D(v.X * scalar, v.Y * scalar);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}