namespace BRT
{
    public class BrtResult
    {
        public Dictionary<int, Cluster> ClustersByKey { get; private set; }
        public State BrtState { get; private set; }
        public List<ClusterResult> ClusterResults { get; private set; }
        public Dictionary<int, List<ClusterResult>> TreeLevels { get; private set; }

        public BrtResult(Dictionary<int, Cluster> clustersByKey, State brtState, List<ClusterResult> clusterResults, Dictionary<int, List<ClusterResult>> treeLevels)
        {
            ClustersByKey = clustersByKey;
            BrtState = brtState;
            ClusterResults = clusterResults;
            TreeLevels = treeLevels;
        }
    }
}
