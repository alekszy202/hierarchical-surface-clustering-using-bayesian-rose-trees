using MathNetFacade;

namespace Model3D.ObjModifiers
{
    public class IndexedObjData
    {
        public List<Vector3D> Vertices { get; set; } = new List<Vector3D>();
        public List<Vector2D> UVs { get; set; } = new List<Vector2D>();
        public List<Vector3D> Normals { get; set; } = new List<Vector3D>();
        public List<uint> Indices { get; set; } = new List<uint>();
    }

    public struct PackedVertex
    {
        public Vector3D Position;
        public Vector2D UV;
        public Vector3D Normal;

        public override bool Equals(object obj)
        {
            if (!(obj is PackedVertex)) throw new ArgumentException("Given object is not the type of PackedVertex!");
            PackedVertex other = (PackedVertex)obj;
            return Position.Equals(other.Position) && UV.Equals(other.UV) && Normal.Equals(other.Normal);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Position.VectorData.GetHashCode();
                hash = hash * 23 + UV.VectorData.GetHashCode();
                hash = hash * 23 + Normal.VectorData.GetHashCode();
                return hash;
            }
        }
    }

    public static class ObjIndexer
    {
        public static IndexedObjData IndexObj(ObjData obj, bool includeNormals, bool includeUVs)
        {
            Dictionary<PackedVertex, uint> vertexToOutIndex = new Dictionary<PackedVertex, uint>();
            IndexedObjData indexedDataObj = new IndexedObjData();

            for (int i = 0; i < obj.Position.Count; i++)
            {
                PackedVertex packed = new PackedVertex
                {
                    Position = obj.Position[i],
                    UV = includeUVs ? obj.UVs[i] : Vector2D.Zero,
                    Normal = includeNormals ? obj.Normals[i] : Vector3D.Zero
                };

                if (vertexToOutIndex.TryGetValue(packed, out uint index))
                {
                    indexedDataObj.Indices.Add(index);
                }
                else
                {
                    indexedDataObj.Vertices.Add(packed.Position);
                    indexedDataObj.UVs.Add(packed.UV);
                    indexedDataObj.Normals.Add(packed.Normal);

                    uint newIndex = (uint)(indexedDataObj.Vertices.Count - 1);
                    indexedDataObj.Indices.Add(newIndex);
                    vertexToOutIndex.Add(packed, newIndex);
                }
            }

            return indexedDataObj;
        }

    }
}
