using BRT;
using Interfaces;
using MathNetFacade;
using Model3D.Mesh3D;
using Model3D.ObjModifiers;
using Adapters;

namespace BRTConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string inputObjPath = @"..\..\..\..\Resources\teapot.obj";
            bool useSavedBrtData = true;
            bool exportAllLevels = true;

            // Read obj file
            ObjData objData = ObjReader.ReadObj(inputObjPath);
            IndexedObjData indexedObjData = ObjIndexer.IndexObj(objData, true, false);
            Console.WriteLine("Indexing done");

            // Create mesh and triangles neighbourhood
            bool clockwise = false;
            Mesh mesh = new Mesh(indexedObjData, clockwise);
            mesh.CreateNeighbourhood();
            Console.WriteLine("Mesh done");

            // BRT input parameters
            float alpha = 0.5f;
            double varianceRatio = 10;
            double scaleFactor = 0.001;

            BrtResult brtResult = null;
            string brtResultPath = BrtResultSaver.GetBrtFilePath(inputObjPath, alpha);

            if (!useSavedBrtData)
            {
                // Convert mesh data to BRT data
                MNMatrix brtMatrix = MeshToBrtDataAdapter.Convert(mesh);
                List<Tuple<int, int>> neighbourhoodMap = mesh.NeighbourhoodMap;

                // Create calculate model
                IFacade facade = Facade.Instance;
                NormalInverseWishart niwModel = new NormalInverseWishart(facade, brtMatrix, varianceRatio, scaleFactor);

                // Calculate retopology model using BRT
                BayesianRoseTreeTriangleProcessor brt = new BayesianRoseTreeTriangleProcessor(facade, brtMatrix, neighbourhoodMap, niwModel);
                brtResult = brt.Build(alpha);
                BrtResultSaver.SerilizeBrtDataToJson(brtResult, brtResultPath);
            }
            else
            {
                brtResult = BrtResultSaver.DeserializeFromJson(brtResultPath);
            }

            // Perform retopology process
            List<int> levelForRetopology = new List<int>();
            if (exportAllLevels)
                levelForRetopology = Enumerable.Range(0, brtResult.TreeLevels.Count - 1).ToList();
            else
                levelForRetopology.Add(BrtToMeshAdapter.GetRetopologyLevelFromUser(brtResult.TreeLevels.Count));

            foreach (int retopologyLevel in levelForRetopology)
            {
                Console.WriteLine($"\n>>> Level {retopologyLevel + 1} <<<");
                Mesh outputMesh = BrtToMeshAdapter.Convert(mesh, brtResult.TreeLevels[retopologyLevel], clockwise);

                // Save output mesh back to obj
                string outputPath = ObjSaver.PrepareFileCatalogAndName(inputObjPath, retopologyLevel + 1, alpha);
                ObjSaver.SaveObj(outputMesh, outputPath);
            }
            
            Console.WriteLine("Done");
        }
    }
}