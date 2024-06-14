using System.Globalization;
using MathNetFacade;

namespace Model3D.ObjModifiers
{
    public class ObjData
    {
        public List<Vector3D> Position { get; set; } = new List<Vector3D>();
        public List<Vector2D> UVs { get; set; } = new List<Vector2D>();
        public List<Vector3D> Normals { get; set; } = new List<Vector3D>();
    }

    public static class ObjReader
    {
        public static ObjData ReadObj(string objPathName)
        {
            List<int> vertexIndices = new List<int>();
            List<int> uvIndices = new List<int>();
            List<int> normalIndices = new List<int>();

            List<Vector3D> tempVertices = new List<Vector3D>();
            List<Vector2D> tempUVs = new List<Vector2D>();
            List<Vector3D> tempNormals = new List<Vector3D>();

            try
            {
                using (var reader = new StreamReader(objPathName))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (tokens.Length == 0)
                            continue;

                        switch (tokens[0])
                        {
                            case "v":
                                Vector3D vertex = new Vector3D(
                                    double.Parse(tokens[1], CultureInfo.InvariantCulture),
                                    double.Parse(tokens[2], CultureInfo.InvariantCulture),
                                    double.Parse(tokens[3], CultureInfo.InvariantCulture)
                                );
                                tempVertices.Add(vertex);
                                break;

                            case "vt":
                                Vector2D uv = new Vector2D(
                                    double.Parse(tokens[1], CultureInfo.InvariantCulture),
                                    double.Parse(tokens[2], CultureInfo.InvariantCulture)
                                );
                                tempUVs.Add(uv);
                                break;

                            case "vn":
                                Vector3D normals = new Vector3D(
                                    double.Parse(tokens[1], CultureInfo.InvariantCulture),
                                    double.Parse(tokens[2], CultureInfo.InvariantCulture),
                                    double.Parse(tokens[3], CultureInfo.InvariantCulture)
                                );
                                tempNormals.Add(normals);
                                break;

                            case "f":
                                if ((tokens.Length - 1) == 3)
                                {
                                    for (int i = 1; i < tokens.Length; i++)
                                    {
                                        string[] parts = tokens[i].Split('/');
                                        vertexIndices.Add(int.Parse(parts[0]) - 1);

                                        if (parts.Length > 1 && parts[1] != "")
                                            uvIndices.Add(int.Parse(parts[1]) - 1);

                                        if (parts.Length > 2 && parts[2] != "")
                                            normalIndices.Add(int.Parse(parts[2]) - 1);
                                    }
                                }
                                else
                                    throw new NotSupportedException($"ObjReader supports only triangular meshes, got {tokens.Length - 1}-quad!");
                                break;
                        }
                    }
                }

            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"File {objPathName} not found!");
                return null;
            }
            catch (NotSupportedException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            ObjData objData = new ObjData();
            for (int i = 0; i < vertexIndices.Count; i++)
            {
                // Vertices 
                objData.Position.Add(tempVertices[vertexIndices[i]]);

                // UVs
                if (i < uvIndices.Count)
                    objData.UVs.Add(tempUVs[uvIndices[i]]);
                else
                    objData.UVs.Add(new Vector2D());

                // Normals
                if (i < normalIndices.Count)
                    objData.Normals.Add(tempNormals[normalIndices[i]]);
                else
                    objData.Normals.Add(new Vector3D());
            }

            return objData;
        }
    }

}
