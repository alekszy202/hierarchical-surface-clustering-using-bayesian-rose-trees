using MathNetFacade;

namespace Model3D.Mesh3D
{
    public class Neighbour
    {
        public Tuple<uint, uint> EdgeIndices { get; private set; }
        public Tuple<Vertex, Vertex> EdgeVertices { get; private set; }
        public Triangle TriangleA { get; private set; }
        public Triangle TriangleB { get; private set; }
        public uint OtherVertexIndexA { get; private set; }
        public Vertex OtherVertexA { get; private set; }
        public uint OtherVertexIndexB { get; private set; }
        public Vertex OtherVertexB { get; private set; }

        private double _clusterizationParameter = double.NaN;
        public double ClusterizationParameter
        {
            get
            {
                if (_clusterizationParameter == double.NaN)
                    _clusterizationParameter = CountClusterizationParamter();
                return _clusterizationParameter;
            }
        }

        #region CONSTRUCTORS
        public Neighbour(
            Tuple<uint, uint> indices,
            Tuple<Vertex, Vertex> vertices,
            Triangle triangleA,
            Triangle triangleB,
            uint otherIndexA,
            Vertex otherVertexA,
            uint otherIndexB,
            Vertex otherVertexB)
        {
            EdgeIndices = indices;
            EdgeVertices = vertices;
            TriangleA = triangleA;
            TriangleB = triangleB;
            OtherVertexIndexA = otherIndexA;
            OtherVertexA = otherVertexA;
            OtherVertexIndexB = otherIndexB;
            OtherVertexB = otherVertexB;
        }
        #endregion

        #region CLUSTERIZATION PARAMETR METHODS
        private double CountClusterizationParamter()
        {
            double angle = CountAngle();
            double epsilon = double.Epsilon;
            double distance = OtherVertexA.Position.Distance(OtherVertexB.Position);

            return (epsilon + angle + 1) * distance;
        }

        private double CountAngle()
        {
            Vertex vertexA = EdgeVertices.Item1;
            Vertex vertexB = EdgeVertices.Item2;

            Vector3D edgeVector = (Vector3D)vertexB.Position.Subtract(vertexA.Position);
            Vector3D otherVectorA = (Vector3D)OtherVertexA.Position.Subtract(vertexA.Position);
            Vector3D otherVectorB = (Vector3D)OtherVertexB.Position.Subtract(vertexA.Position);

            Vector3D crossProductA = edgeVector.Cross(otherVectorA);
            Vector3D crossProductB = edgeVector.Cross(otherVectorB);

            Vector3D normalA = crossProductA.Normalize();
            Vector3D normalB = crossProductB.Normalize();

            double dotProduct = normalA.Dot(normalB);
            return dotProduct;
        }
        #endregion
    }
}