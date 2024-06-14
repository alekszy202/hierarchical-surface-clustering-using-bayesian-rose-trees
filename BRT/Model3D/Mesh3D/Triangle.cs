using MathNetFacade;

namespace Model3D.Mesh3D
{
    public class Triangle
    {
        public int ID { get; private set; }
        public bool Clockwise { get; private set; }

        public uint IndexVertexA { get; private set; }
        public uint IndexVertexB { get; private set; }
        public uint IndexVertexC { get; private set; }
        public Vertex VertexA { get; private set; }
        public Vertex VertexB { get; private set; }
        public Vertex VertexC { get; private set; }

        public Vector3D Normal { get; private set; }
        public double Perimeter { get; private set; }
        public double[] BrtParameters { get; private set; }

        public List<uint> IndexVertices { get; private set; }
        public List<Vertex> Vertices { get; private set; }

        #region CONSTRUCTORS
        public Triangle()
        {
            ID = 0;
            Clockwise = false;
            IndexVertexA = 0;
            IndexVertexB = 0;
            IndexVertexC = 0;
            VertexA = new Vertex();
            VertexB = new Vertex();
            VertexC = new Vertex();
            IndexVertices = new List<uint>() { IndexVertexA, IndexVertexB, IndexVertexC };
            Vertices = new List<Vertex>() { VertexA, VertexB, VertexC };

            Normal = CalculateNormal();
            Perimeter = CalculatePerimeter();
        }

        public Triangle(int id, bool clockwise, uint indexVertexA, uint indexVertexB, uint indexVertexC, Vertex vertexA, Vertex vertexB, Vertex vertexC)
        {
            ID = id;
            Clockwise = clockwise;
            IndexVertexA = indexVertexA;
            IndexVertexB = indexVertexB;
            IndexVertexC = indexVertexC;
            VertexA = vertexA;
            VertexB = vertexB;
            VertexC = vertexC;
            IndexVertices = new List<uint>() { IndexVertexA, IndexVertexB, IndexVertexC };
            Vertices = new List<Vertex> { VertexA, VertexB, VertexC };

            Normal = CalculateNormal();
            Perimeter = CalculatePerimeter();
            BrtParameters = new double[4] { Normal.X, Normal.Y, Normal.Z, Perimeter };
        }
        #endregion

        #region CALCULATIONS
        private Vector3D CalculateNormal()
        {
            Vector3D u = (Vector3D)Vertices[1].Position.Subtract(Vertices[0].Position);
            Vector3D v = (Vector3D)Vertices[2].Position.Subtract(Vertices[0].Position);

            // Coordinate system of the program is right handed, Y-up!
            return (Clockwise ? -1 : 1) * u.Cross(v).Normalize();
        }

        private double CalculatePerimeter()
        {
            double d1 = Vertices[0].Position.Distance(Vertices[1].Position);
            double d2 = Vertices[1].Position.Distance(Vertices[2].Position);
            double d3 = Vertices[2].Position.Distance(Vertices[0].Position);
            return d1 + d2 + d3;
        }
        #endregion
    }
}