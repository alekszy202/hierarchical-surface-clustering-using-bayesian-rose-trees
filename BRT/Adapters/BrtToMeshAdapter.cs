using BRT;
using MathNetFacade;
using Model3D.Mesh3D;
using Model3D.ObjModifiers;
using TraingleNetFacade;

namespace Adapters
{
    [Serializable]
    public class InvalidRetopologyException : InvalidOperationException
    {
        public InvalidRetopologyException() : base() { }
        public InvalidRetopologyException(string message) : base(message) { }
        public InvalidRetopologyException(string message, InvalidOperationException inner) : base(message, inner) { }
    }

    public static class BrtToMeshAdapter
    {
        private enum FlatteningAxis
        {
            X = 0,
            Y = 1,
            Z = 2
        }

        private class ConvertCluster
        {
            public bool Clockwise { get; private set; }
            public List<Triangle> Triangles { get; private set; }
            public Dictionary<uint, Vertex> Vertices { get; private set; }
            public Dictionary<uint, Vertex> FrameVertices { get; private set; }
            public Dictionary<Vertex, uint> FrameVerticesIndices { get; private set; }
            public FlatteningAxis FltAxis { get; private set; }
            public Dictionary<Tuple<double, double>, Vertex> FlattenVertexMap { get; private set; }

            public ConvertCluster(List<Triangle> originalTriangles, bool clockwise)
            {
                Clockwise = clockwise;
                Triangles = originalTriangles;
                Vertices = new Dictionary<uint, Vertex>();

                foreach (Triangle triangle in Triangles)
                {
                    for (int i = 0; i < triangle.Vertices.Count; i++)
                    {
                        if (!Vertices.ContainsKey(triangle.IndexVertices[i]))
                        {
                            Vertices[triangle.IndexVertices[i]] = triangle.Vertices[i];
                        }
                    }
                }
            }

            public void FindFrameVertices()
            {
                Dictionary<Tuple<uint, uint>, List<Triangle>> edgeMap = new Dictionary<Tuple<uint, uint>, List<Triangle>>();

                foreach (Triangle triangle in Triangles)
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

                List<Tuple<uint, uint>> frameEdgeMap = new List<Tuple<uint, uint>>();
                foreach (KeyValuePair<Tuple<uint, uint>, List<Triangle>> neighbour in edgeMap)
                {
                    if (neighbour.Value.Count == 1)
                    {
                        frameEdgeMap.Add(neighbour.Key);
                    }
                }

                if (frameEdgeMap.Count == 0)
                    throw new InvalidRetopologyException("Edge of polygon not found!");

                List<uint> orderedFrameVertices = new List<uint>();
                HashSet<Tuple<uint, uint>> visitedEdges = new HashSet<Tuple<uint, uint>>();

                // Default start vertex
                uint startVertex = frameEdgeMap[0].Item1;
                orderedFrameVertices.Add(startVertex);

                uint currentVertex = startVertex;
                uint previousVertex = uint.MaxValue;

                while (true)
                {
                    bool foundNext = false;
                    foreach (Tuple<uint, uint> edge in frameEdgeMap)
                    {
                        if ((edge.Item1 == currentVertex || edge.Item2 == currentVertex) && !visitedEdges.Contains(edge))
                        {
                            uint nextVertex = edge.Item1 == currentVertex ? edge.Item2 : edge.Item1;
                            if (nextVertex != previousVertex)
                            {
                                orderedFrameVertices.Add(nextVertex);
                                visitedEdges.Add(edge);
                                previousVertex = currentVertex;
                                currentVertex = nextVertex;
                                foundNext = true;
                                break;
                            }
                        }
                    }

                    if (!foundNext || currentVertex == startVertex)
                        break;
                }

                if (currentVertex != startVertex)
                    throw new InvalidRetopologyException("Polygon is not closed!");
                else
                    orderedFrameVertices.Remove(currentVertex);

                FrameVertices = new Dictionary<uint, Vertex>();
                FrameVerticesIndices = new Dictionary<Vertex, uint>();
                foreach (uint index in orderedFrameVertices)
                {
                    FrameVertices[index] = Vertices[index];
                    FrameVerticesIndices[Vertices[index]] = index;
                }
            }

            public void FindFlatteningAxis()
            {
                try
                {
                    double[] xPosition = FrameVertices.Values.Select(x => x.Position.X).ToArray();
                    double[] yPosition = FrameVertices.Values.Select(x => x.Position.Y).ToArray();
                    double[] zPosition = FrameVertices.Values.Select(x => x.Position.Z).ToArray();

                    double xStd = Facade.CalculateStandardDeviation(xPosition);
                    double yStd = Facade.CalculateStandardDeviation(yPosition);
                    double zStd = Facade.CalculateStandardDeviation(zPosition);

                    double minStd = Math.Min(xStd, Math.Min(yStd, zStd));

                    if (minStd == xStd)
                        FltAxis = FlatteningAxis.X;
                    else if (minStd == yStd)
                        FltAxis = FlatteningAxis.Y;
                    else
                        FltAxis = FlatteningAxis.Z;
                }
                catch (NullReferenceException)
                {
                    throw new InvalidRetopologyException("Frame vertices dictionary can not be empty for finding flattening axis operation!");
                }
            }

            public List<Tuple<double, double>> FlattenVertices()
            {
                FlattenVertexMap = new Dictionary<Tuple<double, double>, Vertex>();
                List<Tuple<double, double>> flattenedOuterVertices = new List<Tuple<double, double>>();

                switch (FltAxis)
                {
                    case FlatteningAxis.X:
                        foreach (Vertex frameVertex in FrameVertices.Values)
                        {
                            Tuple<double, double> flattened = new Tuple<double, double>(frameVertex.Position.Y, frameVertex.Position.Z);
                            FlattenVertexMap[flattened] = frameVertex;
                            flattenedOuterVertices.Add(flattened);
                        }
                        break;

                    case FlatteningAxis.Y:
                        foreach (Vertex frameVertex in FrameVertices.Values)
                        {
                            Tuple<double, double> flattened = new Tuple<double, double>(frameVertex.Position.X, frameVertex.Position.Z);
                            FlattenVertexMap[flattened] = frameVertex;
                            flattenedOuterVertices.Add(flattened);
                        }
                        break;

                    case FlatteningAxis.Z:
                        foreach (Vertex frameVertex in FrameVertices.Values)
                        {
                            Tuple<double, double> flattened = new Tuple<double, double>(frameVertex.Position.X, frameVertex.Position.Y);
                            FlattenVertexMap[flattened] = frameVertex;
                            flattenedOuterVertices.Add(flattened);
                        }
                        break;
                }
                return flattenedOuterVertices;
            }

            public (List<Vector3D> position, List<Vector2D> uvs, List<Vector3D> normals) UnflattenVertices(Dictionary<int, Tuple<double, double>> newVertexMap, List<List<int>> newTriangles)
            {
                Dictionary<int, Vertex> newIndicesOldVertices = new Dictionary<int, Vertex>();
                List<int> newVertexIndices = new List<int>(newVertexMap.Keys);

                List<Vector3D> outputPosition = new List<Vector3D>();
                List<Vector2D> outputUVs = new List<Vector2D>();
                List<Vector3D> outputNormals = new List<Vector3D>();

                // Transform new vertices indices to old ones
                foreach (KeyValuePair<int, Tuple<double, double>> newVertexInfo in newVertexMap)
                {
                    int newIndex = newVertexInfo.Key;
                    Tuple<double, double> newVertex = newVertexInfo.Value;

                    if (FlattenVertexMap.ContainsKey(newVertex))
                    {
                        Vertex oldVertex = FlattenVertexMap[newVertex];
                        newIndicesOldVertices[newIndex] = oldVertex;

                        newVertexIndices.Remove(newIndex);
                    }
                }

                // Generating new vertices
                if (newVertexIndices.Count > 0)
                {
                    List<Vector2D> frameVertices2DPositions = new List<Vector2D>();
                    List<double> frameCalcParams = new List<double>();
                    List<Vector3D> frameCalcNormals = new List<Vector3D>();
                    List<Vector2D> frameCalcUVs = new List<Vector2D>();

                    if (FltAxis == FlatteningAxis.X)
                    {
                        foreach (Vertex v in FrameVertices.Values)
                        {
                            frameVertices2DPositions.Add(new Vector2D(v.Position.Y, v.Position.Z));
                            frameCalcParams.Add(v.Position.X);
                            frameCalcNormals.Add(v.Normals);
                            frameCalcUVs.Add(v.Uv);
                        }
                    }
                    else if (FltAxis == FlatteningAxis.Y)
                    {
                        foreach (Vertex v in FrameVertices.Values)
                        {
                            frameVertices2DPositions.Add(new Vector2D(v.Position.X, v.Position.Z));
                            frameCalcParams.Add(v.Position.Y);
                            frameCalcNormals.Add(v.Normals);
                            frameCalcUVs.Add(v.Uv);
                        }
                    }
                    else
                    {
                        foreach (Vertex v in FrameVertices.Values)
                        {
                            frameVertices2DPositions.Add(new Vector2D(v.Position.X, v.Position.Y));
                            frameCalcParams.Add(v.Position.Z);
                            frameCalcNormals.Add(v.Normals);
                            frameCalcUVs.Add(v.Uv);
                        }
                    }

                    foreach (int index in newVertexIndices)
                    {
                        Vertex newVetex = CalculateNewVertex(newVertexMap[index], frameVertices2DPositions, frameCalcParams, frameCalcNormals, frameCalcUVs);
                        newIndicesOldVertices[index] = newVetex;
                    }
                }

                // Save triangles
                foreach (List<int> triangle in newTriangles)
                {
                    Vertex v0 = newIndicesOldVertices[triangle[0]];
                    Vertex v1 = newIndicesOldVertices[triangle[1]];
                    Vertex v2 = newIndicesOldVertices[triangle[2]];

                    Vector3D u = (Vector3D)v1.Position.Subtract(v0.Position);
                    Vector3D v = (Vector3D)v2.Position.Subtract(v0.Position);
                    Vector3D tNormal = (Clockwise ? -1 : 1) * u.Cross(v).Normalize();

                    Vector3D meanNormal = v0.Normals;
                    meanNormal = meanNormal + v1.Normals;
                    meanNormal = meanNormal + v2.Normals;
                    meanNormal = meanNormal * (1d / 3d);

                    double angle = Vector3D.AngleRad(tNormal, meanNormal);

                    if (angle > (Math.PI / 2))
                    {
                        outputPosition.Add(v2.Position);
                        outputUVs.Add(v2.Uv);
                        outputNormals.Add(v2.Normals);

                        outputPosition.Add(v1.Position);
                        outputUVs.Add(v1.Uv);
                        outputNormals.Add(v1.Normals);

                        outputPosition.Add(v0.Position);
                        outputUVs.Add(v0.Uv);
                        outputNormals.Add(v0.Normals);
                    }
                    else
                    {
                        outputPosition.Add(v0.Position);
                        outputUVs.Add(v0.Uv);
                        outputNormals.Add(v0.Normals);

                        outputPosition.Add(v1.Position);
                        outputUVs.Add(v1.Uv);
                        outputNormals.Add(v1.Normals);

                        outputPosition.Add(v2.Position);
                        outputUVs.Add(v2.Uv);
                        outputNormals.Add(v2.Normals);
                    }
                }

                return (outputPosition, outputUVs, outputNormals);
            }

            private Vertex CalculateNewVertex(
                Tuple<double, double> vertex2D,
                List<Vector2D> frameVertices2DPositions,
                List<double> frameCalcParams,
                List<Vector3D> frameCalcNormals,
                List<Vector2D> framCalcUVs
            )
            {
                Vector2D input = new Vector2D(vertex2D.Item1, vertex2D.Item2);

                // Harmonic mean of square distances between each frame point and new point
                List<double> wages = frameVertices2DPositions.Select(p => 1 / (p.DistanceSquared(input))).ToList();
                double sumOfWages = wages.Sum();

                // Position
                double newParam = 0;
                Vector3D newNormal = Vector3D.Zero;
                Vector2D newUv = Vector2D.Zero;

                for (int i = 0; i < frameCalcParams.Count; i++)
                {
                    newParam += wages[i] * frameCalcParams[i];
                    newNormal = newNormal + (frameCalcNormals[i] * wages[i]);
                    newUv = newUv + (framCalcUVs[i] * wages[i]);
                }

                newParam = newParam / sumOfWages;
                newNormal = newNormal * (1 / sumOfWages);
                newUv = newUv * (1 / sumOfWages);

                Vector3D newPosition;
                if (FltAxis == FlatteningAxis.X)
                    newPosition = new Vector3D(newParam, input.X, input.Y);
                else if (FltAxis == FlatteningAxis.Y)
                    newPosition = new Vector3D(input.X, newParam, input.Y);
                else
                    newPosition = new Vector3D(input.X, input.Y, newParam);

                return new Vertex(newPosition, newNormal, newUv);
            }

            public (List<Vector3D> position, List<Vector2D> uvs, List<Vector3D> normals) GetOriginalObjData()
            {
                List<Vector3D> outputPosition = new List<Vector3D>();
                List<Vector2D> outputUVs = new List<Vector2D>();
                List<Vector3D> outputNormals = new List<Vector3D>();

                foreach (Triangle triangle in Triangles)
                {
                    foreach (Vertex vertex in triangle.Vertices)
                    {
                        outputPosition.Add(vertex.Position);
                        outputUVs.Add(vertex.Uv);
                        outputNormals.Add(vertex.Normals);
                    }
                }

                return (outputPosition, outputUVs, outputNormals);
            }
        }

        public static Mesh Convert(Mesh originalMesh, List<ClusterResult> treeLevel, bool clockwise)
        {
            // 1. Create convert clusters based on tree level info
            // 2. Create single clusters of what has left
            List<ConvertCluster> convertClusters = InitConvertClusters(originalMesh, treeLevel, clockwise);

            // 3. Remove inner vertices
            // 4. Check std for flattening procedure
            // 5. Flatten verticies
            // 6. Calculate new triangles with Ear Clipping Triangulation algorithm
            // 7. Unflatten new vertices
            ObjData objData = RetopologyProcess(convertClusters);

            // 8. Index new data
            IndexedObjData indexedObjData = ObjIndexer.IndexObj(objData, true, false);

            // 9. Recreate mesh
            Mesh outputMesh = new Mesh(indexedObjData, clockwise);
            Console.WriteLine($"> Old Mesh:\nTriangles: {originalMesh.Triangles.Count}\nVertices: {originalMesh.Vertices.Count}");
            Console.WriteLine($"\n> New Mesh:\nTriangles: {outputMesh.Triangles.Count}\nVertices: {outputMesh.Vertices.Count}");
            Console.WriteLine($"\nTriangle loss: {((float)outputMesh.Triangles.Count * 100) / (float)originalMesh.Triangles.Count}%");
            Console.WriteLine($"Vertex loss: {((float)outputMesh.Vertices.Count * 100) / (float)originalMesh.Vertices.Count}%");
            return outputMesh;
        }

        public static int GetRetopologyLevelFromUser(int levelCount)
        {
            int userInput;
            int min = 1;
            int max = levelCount;

            Console.Write($"\nChoose retopology level in range from {min} to {max}: ");

            while (!int.TryParse(Console.ReadLine(), out userInput) || userInput < min || userInput > max)
            {
                Console.Write($"Incorrect value. Please enter a valid integer in the range from {min} to {max}: ");
            }

            return userInput - 1;
        }

        private static List<ConvertCluster> InitConvertClusters(Mesh mesh, List<ClusterResult> clusters, bool clockwise)
        {
            Dictionary<int, Triangle> originalTriangles = new Dictionary<int, Triangle>();
            List<int> originalTriangleIds = new List<int>();
            foreach (Triangle triangle in mesh.Triangles)
            {
                originalTriangles[triangle.ID] = triangle;
                originalTriangleIds.Add(triangle.ID);
            }

            List<ConvertCluster> convertClusters = new List<ConvertCluster>();
            foreach (ClusterResult cluster in clusters)
            {
                List<int> clusterTrisIds = RetreiveTrianglesFromCluster(cluster, originalTriangleIds);
                List<Triangle> clusterTriangles = clusterTrisIds.Select(id => originalTriangles[id]).ToList();
                ConvertCluster convertCluster = new ConvertCluster(clusterTriangles, clockwise);
                convertClusters.Add(convertCluster);
            }

            foreach (int leftTriangleID in originalTriangleIds)
            {
                List<Triangle> clusterTriangles = new List<Triangle>() { originalTriangles[leftTriangleID] };
                ConvertCluster convertCluster = new ConvertCluster(clusterTriangles, clockwise);
                convertClusters.Add(convertCluster);
            }

            return convertClusters;
        }

        private static List<int> RetreiveTrianglesFromCluster(ClusterResult root, List<int> originalTriangleIds)
        {
            List<int> ids = new List<int>();

            if (originalTriangleIds.Contains(root.ID))
            {
                ids.Add(root.ID);
                originalTriangleIds.Remove(root.ID);
            }

            foreach (ClusterResult child in root.Children)
            {
                ids.AddRange(RetreiveTrianglesFromCluster(child, originalTriangleIds));
            }

            return ids;
        }

        private static ObjData RetopologyProcess(List<ConvertCluster> convertClusters)
        {
            ObjData simplifiedObjData = new ObjData();

            int index = 0;
            foreach (ConvertCluster convertCluster in convertClusters)
            {
                if (convertCluster.Triangles.Count > 1)
                {
                    try
                    {
                        // 3. Remove inner vertices
                        convertCluster.FindFrameVertices();

                        // 4. Check std for flattening procedure
                        convertCluster.FindFlatteningAxis();

                        // 5. Flatten vertices
                        List<Tuple<double, double>> flattenedVertices = convertCluster.FlattenVertices();

                        // 6. Calculate new triangles with Ear Clipping Triangulation algorithm
                        (Dictionary<int, Tuple<double, double>> newVertexMap, List<List<int>> newTriangles) = EarClippingTriangulation.Triangulate(flattenedVertices);

                        // 7. Unflatten new vertices
                        (List<Vector3D> position, List<Vector2D> uvs, List<Vector3D> normals) = convertCluster.UnflattenVertices(newVertexMap, newTriangles);

                        simplifiedObjData.Position.AddRange(position);
                        simplifiedObjData.UVs.AddRange(uvs);
                        simplifiedObjData.Normals.AddRange(normals);
                    }
                    catch (InvalidRetopologyException ex)
                    {
                        Console.WriteLine($"Managed to save retopology cluster[{index}] error: {ex.Message}");
                        (List<Vector3D> position, List<Vector2D> uvs, List<Vector3D> normals) = convertCluster.GetOriginalObjData();

                        simplifiedObjData.Position.AddRange(position);
                        simplifiedObjData.UVs.AddRange(uvs);
                        simplifiedObjData.Normals.AddRange(normals);
                    }
                }
                // Single triangle cluster
                else
                {
                    Triangle singleTriangle = convertCluster.Triangles[0];
                    foreach (Vertex vertex in singleTriangle.Vertices)
                    {
                        simplifiedObjData.Position.Add(vertex.Position);
                        simplifiedObjData.UVs.Add(vertex.Uv);
                        simplifiedObjData.Normals.Add(vertex.Normals);
                    }
                }
                index++;
            }
            return simplifiedObjData;
        }
    }
}
