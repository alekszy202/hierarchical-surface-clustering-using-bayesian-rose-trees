using Newtonsoft.Json;

namespace BRT
{
    [Serializable]
    public class BrtResultJsonAdapter
    {
        public Dictionary<int, Cluster> ClustersByKey { get; private set; }

        [JsonConstructor]
        public BrtResultJsonAdapter(Dictionary<int, Cluster> clustersByKey)
        {
            ClustersByKey = clustersByKey;
        }
    }

    public static class BrtResultSaver
    {
        public static string GetBrtFilePath(string inputPath, float alpha)
        {
            string directory = Path.GetDirectoryName(inputPath);
            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            string extension = Path.GetExtension(inputPath);

            string outputDirectory = Path.Combine(directory, $"{fileName}_output");
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            return Path.Combine(outputDirectory, $"{fileName}_BrtResult_alpha{alpha}.json");
        }

        public static void SerilizeBrtDataToJson(BrtResult brtResult, string filePath)
        {
            BrtResultJsonAdapter jsonAdapter = new BrtResultJsonAdapter(brtResult.ClustersByKey);
            string jsonContent = JsonConvert.SerializeObject(jsonAdapter, Formatting.Indented);
            File.WriteAllText(filePath, jsonContent);
            Console.WriteLine($"Serialized BRT result data to {filePath}");
        }

        public static BrtResult DeserializeFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            BrtResultJsonAdapter jsonAdapter = JsonConvert.DeserializeObject<BrtResultJsonAdapter>(json);

            if (BayesianRoseTree.ValidateTree(jsonAdapter.ClustersByKey))
            {
                List<ClusterResult> clustersResults = BayesianRoseTree.ConvertToClusterResult(jsonAdapter.ClustersByKey);
                Dictionary<int, List<ClusterResult>> treeLevels = BayesianRoseTree.DivideTreeIntoLevels(clustersResults);

                BrtResult brtResult = new BrtResult(jsonAdapter.ClustersByKey, null, clustersResults, treeLevels);
                Console.WriteLine($"Deserialized BRT result data from {filePath}");
                return brtResult;
            }

            return null;
        }
    }
}
