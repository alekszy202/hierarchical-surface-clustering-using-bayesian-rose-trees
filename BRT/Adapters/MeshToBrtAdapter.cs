using MathNetFacade;
using Model3D.Mesh3D;

namespace Adapters
{
    public static class MeshToBrtDataAdapter
    {
        public static MNMatrix Convert(Mesh mesh)
        {
            double[] brtData = GenerateBRTData(mesh);
            MNMatrix brtMatrix = new MNMatrix(brtData, 4);
            return brtMatrix;
        }

        private static double[] GenerateBRTData(Mesh mesh)
        {
            List<double> data = new List<double>();
            int numerOfBrtParameters = 4;

            for (int i = 0; i < numerOfBrtParameters; i++)
            {
                foreach (Triangle t in mesh.Triangles)
                {
                    data.Add(t.BrtParameters[i]);
                }
            }

            return data.ToArray();
        }
    }
}
