using MathNetFacade;

namespace Model3D.Mesh3D
{
    public class Vertex
    {
        public Vector3D Position { get; private set; }
        public Vector3D Normals { get; private set; }
        public Vector2D Uv { get; private set; }

        #region CONSTRUCTORS
        public Vertex()
        {
            Position = new Vector3D();
            Normals = new Vector3D();
            Uv = new Vector2D();
        }

        public Vertex(Vector3D position, Vector3D normals, Vector2D uv)
        {
            Position = position;
            Normals = normals;
            Uv = uv;
        }
        #endregion
    }
}