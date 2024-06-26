﻿using MathNetFacade;
using Model3D.Mesh3D;
using System.Globalization;

namespace Model3D.ObjModifiers
{
    public static class ObjSaver
    {
        public static string PrepareFileCatalogAndName(string inputPath, int retopologyLevel, double alpha)
        {
            string directory = Path.GetDirectoryName(inputPath);
            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            string extension = Path.GetExtension(inputPath);

            string outputDirectory = Path.Combine(directory, $"{fileName}_output");
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            return Path.Combine(outputDirectory, $"{fileName}_level{retopologyLevel}_alpha{alpha}.obj");
        }

        public static void SaveObj(Mesh mesh, string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("# OBJ file generated by Bayesian Rose Tree System");

                List<Vector3D> positions = mesh.Vertices.Select(vertex => vertex.Position).ToList();
                List<Vector2D> uvs = mesh.Vertices.Select(vertex => vertex.Uv).ToList();
                List<Vector3D> normals = mesh.Vertices.Select(vertex => vertex.Normals).ToList();

                bool saveUV = uvs.Any(uv => !uv.Equals(Vector2D.Zero));
                bool saveNormals = normals.Any(normal => !normal.Equals(Vector3D.Zero));

                foreach (Vector3D position in positions)
                {
                    writer.WriteLine($"v {position.X.ToString(CultureInfo.InvariantCulture)} {position.Y.ToString(CultureInfo.InvariantCulture)} {position.Z.ToString(CultureInfo.InvariantCulture)}");
                }

                if (saveUV)
                {
                    foreach (Vector2D uv in uvs)
                    {
                        writer.WriteLine($"vt {uv.X.ToString(CultureInfo.InvariantCulture)} {uv.Y.ToString(CultureInfo.InvariantCulture)}");
                    }
                }

                if (saveNormals)
                {
                    foreach (Vector3D normal in normals)
                    {
                        writer.WriteLine($"vn {normal.X.ToString(CultureInfo.InvariantCulture)} {normal.Y.ToString(CultureInfo.InvariantCulture)} {normal.Z.ToString(CultureInfo.InvariantCulture)}");
                    }
                }

                foreach (Triangle triangle in mesh.Triangles)
                {
                    string vertexA = (triangle.IndexVertexA + 1).ToString();
                    string vertexB = (triangle.IndexVertexB + 1).ToString();
                    string vertexC = (triangle.IndexVertexC + 1).ToString();

                    string uvA = saveUV ? (triangle.IndexVertexA + 1).ToString() : "";
                    string uvB = saveUV ? (triangle.IndexVertexB + 1).ToString() : "";
                    string uvC = saveUV ? (triangle.IndexVertexC + 1).ToString() : "";

                    string normalA = saveNormals ? (triangle.IndexVertexA + 1).ToString() : "";
                    string normalB = saveNormals ? (triangle.IndexVertexB + 1).ToString() : "";
                    string normalC = saveNormals ? (triangle.IndexVertexC + 1).ToString() : "";

                    if (saveUV && saveNormals)
                    {
                        writer.WriteLine($"f {vertexA}/{uvA}/{normalA} {vertexB}/{uvB}/{normalB} {vertexC}/{uvC}/{normalC}");
                    }
                    else if (saveUV)
                    {
                        writer.WriteLine($"f {vertexA}/{uvA} {vertexB}/{uvB} {vertexC}/{uvC}");
                    }
                    else if (saveNormals)
                    {
                        writer.WriteLine($"f {vertexA}//{normalA} {vertexB}//{normalB} {vertexC}//{normalC}");
                    }
                    else
                    {
                        writer.WriteLine($"f {vertexA} {vertexB} {vertexC}");
                    }
                }
            }
            Console.WriteLine($"\nSuccessfully saved model to path: {path}");
        }
    }
}
