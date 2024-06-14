using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;

namespace TraingleNetFacade
{
    public static class EarClippingTriangulation
    {
        public static (Dictionary<int, Tuple<double, double>> vertexMap, List<List<int>> triangles) Triangulate(List<Tuple<double, double>> inputPosition)
        {
            // Prepare data and options
            List<Vertex> positions = inputPosition.Select(v => new Vertex(v.Item1, v.Item2)).ToList();

            Polygon polygon = new Polygon();
            polygon.Add(new Contour(positions));

            QualityOptions quality = new QualityOptions() { };
            ConstraintOptions options = new ConstraintOptions() { ConformingDelaunay = false, SegmentSplitting = 0 };

            // Triangulate
            Mesh mesh = (Mesh)polygon.Triangulate(options, quality);

            Dictionary<Vertex, int> vertexMap = new Dictionary<Vertex, int>();
            List<List<int>> triangles = new List<List<int>>();

            // Save vertices
            int vertexIndex = 0;
            foreach (Triangle triangle in mesh.Triangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vertex vertex = triangle.GetVertex(i);
                    if (!vertexMap.ContainsKey(vertex))
                    {
                        vertexMap[vertex] = vertexIndex;
                        vertexIndex++;
                    }
                }
            }

            // Save triangles
            foreach (Triangle triangle in mesh.Triangles)
            {
                int a = vertexMap[triangle.GetVertex(0)];
                int b = vertexMap[triangle.GetVertex(1)];
                int c = vertexMap[triangle.GetVertex(2)];

                triangles.Add(new List<int>() { a, b, c });
            }

            // Invert vertex map
            Dictionary<int, Tuple<double, double>> resultVertexMap = new Dictionary<int, Tuple<double, double>>();
            foreach (KeyValuePair<Vertex, int> mapElement in vertexMap)
            {
                resultVertexMap[mapElement.Value] = new Tuple<double, double>(mapElement.Key.X, mapElement.Key.Y);
            }


            return (resultVertexMap, triangles);
        }
    }
}
