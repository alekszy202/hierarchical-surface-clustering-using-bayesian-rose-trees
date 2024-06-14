using Interfaces;
using MathNet.Numerics.LinearAlgebra;

namespace MathNetFacade
{
    public class Vector3D : IVector
    {
        private Vector<double> _vectorData;

        public double X { get => _vectorData[0]; }
        public double Y { get => _vectorData[1]; }
        public double Z { get => _vectorData[2]; }
        public Vector<double> VectorData { get => _vectorData; set => _vectorData = value; }
        public static Vector3D Zero { get => new Vector3D(0, 0, 0); }
        public static Vector3D One { get => new Vector3D(1, 1, 1); }

        public Vector3D()
        {
            _vectorData = Vector<double>.Build.Dense(new double[] { 0, 0, 0 });
        }

        public Vector3D(double x, double y, double z)
        {
            _vectorData = Vector<double>.Build.Dense(new double[] { x, y, z });
        }

        public Vector3D(Vector<double> other)
        {
            if (other.Count == 3)
                _vectorData = Vector<double>.Build.DenseOfVector(other);
            else
                throw new ArgumentException($"Vector3D class requires vector of 3 elements, got {other.Count}.");
        }

        public Vector3D IsSameType(IVector other)
        {
            Vector3D converted = other as Vector3D ?? throw new ArgumentException("Two vectors use different implementations");
            return converted;
        }

        public IVector Subtract(IVector subtrahend)
        {
            Vector3D subtrahendVector3D = IsSameType(subtrahend);
            return new Vector3D(VectorData.Subtract(subtrahendVector3D.VectorData));
        }

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public Vector3D Normalize()
        {
            double length = this.Length();
            return new Vector3D(X / length, Y / length, Z / length);
        }

        public double Distance(Vector3D other)
        {
            return Math.Sqrt(DistanceSquared(other));
        }

        public double DistanceSquared(Vector3D other)
        {
            return Math.Pow(other.X - this.X, 2) +
                   Math.Pow(other.Y - this.Y, 2) +
                   Math.Pow(other.Z - this.Z, 2);
        }

        public static double AngleRad(Vector3D v1, Vector3D v2)
        {
            double dotProduct = v1.Dot(v2);
            double normV1 = v1.Length();
            double normV2 = v2.Length();
            double angleRadians = Math.Acos(dotProduct / (normV1 * normV2));
            return angleRadians;
        }

        public double Dot(IVector other)
        {
            Vector3D validOther = IsSameType(other);
            return _vectorData.DotProduct(validOther.VectorData);
        }

        public IMatrix OuterProduct()
        {
            return new MNMatrix(VectorData.OuterProduct(VectorData));
        }

        public bool Equals(IVector other)
        {
            Vector3D validOther = IsSameType(other);
            return _vectorData.Equals(validOther.VectorData);
        }

        public Vector3D Cross(Vector3D other)
        {
            double A = this.Y * other.Z - (other.Y * this.Z);
            double B = -(this.X * other.Z - (other.X * this.Z));
            double C = this.X * other.Y - (other.X * this.Y);
            return new Vector3D(A, B, C);
        }

        public static Vector3D operator +(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3D operator *(Vector3D v, float scalar)
        {
            return new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        public static Vector3D operator *(float scalar, Vector3D v)
        {
            return new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        public static Vector3D operator *(Vector3D v, double scalar)
        {
            return new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        public static Vector3D operator *(double scalar, Vector3D v)
        {
            return new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        public static Vector3D operator *(Vector3D v, int scalar)
        {
            return new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        public static Vector3D operator *(int scalar, Vector3D v)
        {
            return new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }
}