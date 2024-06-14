using Newtonsoft.Json;

namespace BRT
{
    public class Cluster
    {
        internal double MarginalLikelihood { get; }
        internal double LnMarginalLikelihood { get; }
        public List<int> Children { get; }
        public HashSet<int> Parents { get; }
        public int Key { get; private set; }

        internal Cluster(int key, double marginalLikelihood)
        {
            MarginalLikelihood = marginalLikelihood;
            Children = new List<int>();
            Parents = new HashSet<int>();
            Key = key;
        }

        internal Cluster(int key, double marginalLikelihood, double lnMarginalLikelihood)
        {
            MarginalLikelihood = marginalLikelihood;
            LnMarginalLikelihood = lnMarginalLikelihood;
            Children = new List<int>();
            Parents = new HashSet<int>();
            Key = key;
        }

        internal Cluster(int key, Cluster source)
        {
            MarginalLikelihood = source.MarginalLikelihood;
            Children = new List<int>(source.Children);
            Parents = new HashSet<int>(source.Parents);
            Key = key;
        }

        [JsonConstructor]
        internal Cluster(int key, List<int> children, HashSet<int> parents)
        {
            Parents = parents;
            Children = children;
            Key = key;
        }

        internal void AddChildren(Dictionary<int, Cluster> clustersByKey, params int[] children)
        {
            // TODO: Przejść na Sety?
            Children.AddRange(children.Except(Children));
            foreach (int child in children)
            {
                clustersByKey[child].Parents.Add(Key);
            }
        }

        internal void AddChildren(Dictionary<int, Cluster> clustersByKey, params List<int>[] children)
        {
            for (int i = 0; i < children.Length; ++i)
            {
                Children.AddRange(children[i].Except(Children));
                foreach (int child in children[i])
                {
                    clustersByKey[child].Parents.Add(Key);
                }
            }
        }

        internal void AddParent(Dictionary<int, Cluster> clustersByKey, int parent)
        {
            Parents.Add(parent);
            if (!clustersByKey[parent].Children.Contains(Key))
                clustersByKey[parent].Children.Add(Key);
        }

        internal void RemoveChild(Dictionary<int, Cluster> clustersByKey, int child)
        {
            Children.Remove(child);
            clustersByKey[child].Parents.Remove(Key);
        }

        internal void RemoveParent(Dictionary<int, Cluster> clustersByKey, int parent)
        {
            Parents.Remove(parent);
            clustersByKey[parent].Children.Remove(Key);
        }

        internal void Replace(Dictionary<int, Cluster> clustersByKey, int newCluster)
        {
            foreach (int parent in Parents)
            {
                clustersByKey[parent].Children.Remove(Key);
                clustersByKey[parent].AddChildren(clustersByKey, newCluster);
                clustersByKey[newCluster].AddParent(clustersByKey, parent);
            }

            foreach (int child in Children)
            {
                clustersByKey[child].Parents.Remove(Key);
            }

            clustersByKey.Remove(Key);
        }
    }
}
