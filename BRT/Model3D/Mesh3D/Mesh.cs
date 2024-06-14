using Model3D.Mesh3D;
using Model3D.ObjModifiers;

namespace Model3D.Mesh3D
{
    public class Mesh
    {
        private List<Vertex> _vertices;
        private List<uint> _indices;
        private List<Triangle> _triangles;
        private List<Neighbour> _neighbourhood;
        private List<Tuple<int, int>> _neighbourhoodMap;
        private bool _clockwise;

        #region GETTERS & SETTERS
        public List<Vertex> Vertices { get => _vertices; }
        public List<uint> Indices { get => _indices; }
        public List<Triangle> Triangles { get => _triangles; }
        public List<Neighbour> Neighbourhood { get => _neighbourhood; }
        public List<Tuple<int, int>> NeighbourhoodMap { get => _neighbourhoodMap; }

        public bool Clockwise { get => _clockwise; }
        #endregion

        #region CONSTRUCTORS
        public Mesh(IndexedObjData indexedObjData, bool clockwise)
        {
            _clockwise = clockwise;
            _vertices = new List<Vertex>();
            for (int i = 0; i < indexedObjData.Vertices.Count; i++)
            {
                _vertices.Add(new Vertex(
                    indexedObjData.Vertices[i],
                    indexedObjData.Normals[i],
                    indexedObjData.UVs[i]
                ));
            }

            _indices = new List<uint>(indexedObjData.Indices);

            int triangleID = 0;
            _triangles = new List<Triangle>();
            for (int i = 0; i < _indices.Count; i = i + 3)
            {
                _triangles.Add(new Triangle(
                    triangleID,
                    _clockwise,
                    _indices[i],
                    _indices[i + 1],
                    _indices[i + 2],
                    _vertices[(int)_indices[i]],
                    _vertices[(int)_indices[i + 1]],
                    _vertices[(int)_indices[i + 2]]
                ));
                triangleID++;
            }

            _neighbourhood = new List<Neighbour>();
            _neighbourhoodMap = new List<Tuple<int, int>>();
        }
        #endregion

        #region NEIGHBOURHOOD METHODS
        public void CreateNeighbourhood()
        {
            Dictionary<Tuple<uint, uint>, List<Triangle>> edgeMap = new Dictionary<Tuple<uint, uint>, List<Triangle>>();

            foreach (Triangle triangle in _triangles)
            {
                uint[] vertices = triangle.IndexVertices.ToArray();
                Array.Sort(vertices);

                List<Tuple<uint, uint>> edgeList = new List<Tuple<uint, uint>>
                {
                    new Tuple<uint, uint>(vertices[0], vertices[1]),
                    new Tuple<uint, uint>(vertices[1], vertices[2]),
                    new Tuple<uint, uint>(vertices[0], vertices[2])
                };

                foreach (Tuple<uint, uint> edge in edgeList)
                {
                    if (!edgeMap.ContainsKey(edge))
                    {
                        edgeMap[edge] = new List<Triangle>();
                    }
                    edgeMap[edge].Add(triangle);
                }
            }

            foreach (KeyValuePair<Tuple<uint, uint>, List<Triangle>> neighbour in edgeMap)
            {
                if (neighbour.Value.Count == 2)
                {
                    Tuple<uint, uint> key = neighbour.Key;
                    List<Triangle> value = neighbour.Value;
                    uint otherVertexA = FindOtherVertex(key.Item1, key.Item2, value[0].IndexVertices);
                    uint otherVertexB = FindOtherVertex(key.Item1, key.Item2, value[1].IndexVertices);
                    _neighbourhood.Add(new Neighbour(
                        key,
                        new Tuple<Vertex, Vertex>(_vertices[(int)key.Item1], _vertices[(int)key.Item2]),
                        value[0],
                        value[1],
                        otherVertexA,
                        _vertices[(int)otherVertexA],
                        otherVertexB,
                        _vertices[(int)otherVertexB]
                    ));
                }
            }

            CreateNeighbourhoodMap();
        }

        private void CreateNeighbourhoodMap()
        {
            foreach (Neighbour neighbour in _neighbourhood)
            {
                _neighbourhoodMap.Add(new Tuple<int, int>(neighbour.TriangleA.ID, neighbour.TriangleB.ID));
            }
        }

        public uint FindOtherVertex(uint vertexA, uint vertexB, List<uint> vertexList)
        {
            foreach (uint vertex in vertexList)
            {
                if (!(vertex == vertexA) && !(vertex == vertexB))
                {
                    return vertex;
                }
            }
            return 0;
        }
        #endregion
    }
}