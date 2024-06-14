using Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace BRT
{
    public class BayesianRoseTree
    {
        protected readonly IFacade _facade;
        protected readonly IMatrix _data;
        protected readonly IModel _model;
        protected double _alpha = 0.5f;

        protected State _brtState;
        protected Dictionary<int, Cluster> _clustersByKey;
        protected List<MergeCandidate> _mergeCandidates;
        protected Dictionary<int, List<MergeCandidate>> _mergeCandidatesMap;

        public BayesianRoseTree(IFacade facade, IMatrix data, IModel model)
        {
            _facade = facade;
            _data = data;
            _model = model;
        }

        public BrtResult Build(float alpha)
        {
            _alpha = Math.Clamp(alpha, 0.0f, 1.0f);
            _brtState = new State(_data.SampleCount);
            _clustersByKey = new Dictionary<int, Cluster>();
            _mergeCandidates = new List<MergeCandidate>();
            _mergeCandidatesMap = new Dictionary<int, List<MergeCandidate>>();
            Console.WriteLine($"\nStarting Bayesian Rose Tree procedure\nAlpha: {alpha}");

            InitNodes();
            InitPairs();

            int newClusterKey = _data.SampleCount;
            int nextClusterKey = newClusterKey + 1;
            Stopwatch stopwatch = Stopwatch.StartNew();
            Console.WriteLine($"Starting clusters: {newClusterKey}");

            while (_brtState.IsActive)
            {
                (MergeCandidate bestCandidate, MergeCandidate.MergeType operationOfMaxLikelihood) = FindRangesMax();

                BuildInterval(bestCandidate, operationOfMaxLikelihood, newClusterKey, ref nextClusterKey);
                newClusterKey = nextClusterKey;
                nextClusterKey = newClusterKey + 1;

                if (newClusterKey % 1000 == 0)
                    Console.WriteLine($"Processing cluster: {newClusterKey}");
            }

            Console.WriteLine($"\nFinished BRT procedure");
            Console.WriteLine($"Final clusters count: {newClusterKey}\nBRT total time: {stopwatch.ElapsedMilliseconds}ms");
            Prune(_clustersByKey);

            List<ClusterResult> clusterResults = new List<ClusterResult>();
            Dictionary<int, List<ClusterResult>> treeLevels = null;
            if (ValidateTree(_clustersByKey))
                clusterResults = ConvertToClusterResult(_clustersByKey);
                treeLevels = DivideTreeIntoLevels(clusterResults);
                Console.WriteLine($"Tree levels: {treeLevels.Count}");

            return new BrtResult(_clustersByKey, _brtState, clusterResults, treeLevels);
        }

        private void InitNodes()
        {
            for (int idx = 0; idx < _data.SampleCount; ++idx)
            {
                double marginalLikelihood = _model.CalculateMarginalLikelihood(_data.SampleAsRowMatrix(idx));
                double lnMarginalLikelihood = _model.CalculateLnMarginalLikelihood(_data.SampleAsRowMatrix(idx));
                _clustersByKey[idx] = new Cluster(idx, marginalLikelihood, lnMarginalLikelihood);
            }
        }

        protected virtual void InitPairs()
        {
            for (int idxFirst = 0; idxFirst < _data.SampleCount; ++idxFirst)
            {
                for (int idxSecond = idxFirst + 1; idxSecond < _data.SampleCount; ++idxSecond)
                {
                    IMatrix samplesMerged = _data.Sample(idxFirst, idxSecond);
                    double dataMarginalLikelihood = _model.CalculateMarginalLikelihood(samplesMerged);
                    // double dataLnMarginalLikelihood = _model.CalculateLnMarginalLikelihood(samplesMerged);

                    double mixtureLikelihood = _clustersByKey[idxFirst].MarginalLikelihood * _clustersByKey[idxSecond].MarginalLikelihood;
                    // double mixtureLnLikelihood = _clustersByKey[idxFirst].LnMarginalLikelihood + _clustersByKey[idxSecond].LnMarginalLikelihood;

                    double likelihood = CalculateLikelihood(dataMarginalLikelihood, mixtureLikelihood, 2);
                    // double lnLikelihood = CalculateLnLikelihood(dataLnMarginalLikelihood, mixtureLnLikelihood, 2);

                    double likelihoodJoin = likelihood / mixtureLikelihood;
                    MergeCandidate mergeCandidate = new MergeCandidate(idxFirst, idxSecond, likelihoodJoin, 0, 0, 0);
                    AddMergeCandidates(mergeCandidate);
                }
            }
        }

        protected void AddMergeCandidates(MergeCandidate candidate)
        {
            _mergeCandidates.Add(candidate);

            if (!_mergeCandidatesMap.ContainsKey(candidate.IndexFirst))
            {
                _mergeCandidatesMap[candidate.IndexFirst] = new List<MergeCandidate>();
            }

            if (!_mergeCandidatesMap.ContainsKey(candidate.IndexSecond))
            {
                _mergeCandidatesMap[candidate.IndexSecond] = new List<MergeCandidate>();
            }

            _mergeCandidatesMap[candidate.IndexFirst].Add(candidate);
            _mergeCandidatesMap[candidate.IndexSecond].Add(candidate);
        }

        protected double CalculateLikelihood(double dataMarginalLikelihood, double mixtureLikelihood, uint sampleCount)
        {
            return Math.Pow((1 - _alpha), (sampleCount - 1)) * (mixtureLikelihood - dataMarginalLikelihood) + dataMarginalLikelihood;
        }

        private void CalculateLnLikelihood(double dataLnMarginalLikelihood, double mixtureLnLikelihood, uint sampleCount)
        {
        }

        private (MergeCandidate, MergeCandidate.MergeType) FindRangesMax()
        {
            // Main variables
            int indexOfMaxLikelihood = -1;
            MergeCandidate.MergeType operationOfMaxLikelihood = (MergeCandidate.MergeType)(-1);
            double maxLikelihood = double.NegativeInfinity;

            for (int testedIdx = 0; testedIdx < _mergeCandidates.Count; ++testedIdx)
            {
                MergeCandidate mergeCandidate = _mergeCandidates[testedIdx];
                for (MergeCandidate.MergeType mergeType = MergeCandidate.MergeType.Join; mergeType < MergeCandidate.MergeType.MergeTypeCount; ++mergeType)
                {
                    double mergeLikelihood = mergeCandidate[mergeType];

                    // Change for better candidate
                    if (mergeLikelihood > maxLikelihood)
                    {
                        indexOfMaxLikelihood = testedIdx;
                        operationOfMaxLikelihood = mergeType;
                        maxLikelihood = mergeLikelihood;
                    }
                }
            }

            MergeCandidate bestCandidate = _mergeCandidates[indexOfMaxLikelihood];
            return (bestCandidate, operationOfMaxLikelihood);
        }

        private void BuildInterval(MergeCandidate mergedCandidate, MergeCandidate.MergeType mergeType, int newClusterKey, ref int nextClusterKey)
        {
            List<List<MergeCandidate>> candidates = new List<List<MergeCandidate>>();
            if (_mergeCandidatesMap.ContainsKey(mergedCandidate.IndexFirst))
            {
                candidates.Add(new List<MergeCandidate>(_mergeCandidatesMap[mergedCandidate.IndexFirst]));
            }

            if (_mergeCandidatesMap.ContainsKey(mergedCandidate.IndexSecond))
            {
                candidates.Add(new List<MergeCandidate>(_mergeCandidatesMap[mergedCandidate.IndexSecond]));
            }

            foreach (List<MergeCandidate> candidateList in candidates)
            {
                foreach (MergeCandidate candidate in candidateList)
                {
                    _mergeCandidates.Remove(candidate);
                    _mergeCandidatesMap[candidate.IndexFirst].Remove(candidate);
                    _mergeCandidatesMap[candidate.IndexSecond].Remove(candidate);
                }
            }

            double likelihoodRatio = mergedCandidate[mergeType];
            double likelihoodProduct = _clustersByKey[mergedCandidate.IndexFirst].MarginalLikelihood *
                                       _clustersByKey[mergedCandidate.IndexSecond].MarginalLikelihood;

            // TODO: To mnożenie tu jest tak naprawdę niepotrzebne - mnożę ratio, a ratio to jest mixture / product, więc mógłbym zapisywać mixture po prostu obok ratia.
            Cluster newCluster = new Cluster(newClusterKey, likelihoodRatio * likelihoodProduct);
            _clustersByKey.Add(newClusterKey, newCluster);

            switch (mergeType)
            {
                case MergeCandidate.MergeType.Join:
                    _clustersByKey[newClusterKey].AddChildren(_clustersByKey, mergedCandidate.IndexFirst, mergedCandidate.IndexSecond);
                    break;

                case MergeCandidate.MergeType.AbsorbLeft:
                    _clustersByKey[newClusterKey].AddChildren(_clustersByKey, mergedCandidate.IndexFirst);
                    _clustersByKey[newClusterKey].AddChildren(_clustersByKey, _clustersByKey[mergedCandidate.IndexSecond].Children);
                    _clustersByKey.Remove(mergedCandidate.IndexSecond);
                    break;

                case MergeCandidate.MergeType.AbsorbRight:
                    _clustersByKey[newClusterKey].AddChildren(_clustersByKey, _clustersByKey[mergedCandidate.IndexFirst].Children);
                    _clustersByKey[newClusterKey].AddChildren(_clustersByKey, mergedCandidate.IndexSecond);
                    _clustersByKey.Remove(mergedCandidate.IndexFirst);
                    break;

                case MergeCandidate.MergeType.Collapse:
                    _clustersByKey[newClusterKey].AddChildren(_clustersByKey, _clustersByKey[mergedCandidate.IndexFirst].Children,
                    _clustersByKey[mergedCandidate.IndexSecond].Children);
                    _clustersByKey.Remove(mergedCandidate.IndexSecond);
                    _clustersByKey.Remove(mergedCandidate.IndexFirst);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            BuildStates(mergedCandidate, newClusterKey);
        }

        protected virtual void BuildStates(MergeCandidate mergedCandidate, int newClusterKey)
        {
            // Indeks nowego klastra przypisywany jest do elementów, które zostały połączone jako ten klaster
            List<IMatrix> newClusterData = new List<IMatrix>();
            _brtState.ForAllMatchingAssignments((assignmentIdx) =>
            {
                _brtState.Assign(assignmentIdx, newClusterKey);
                newClusterData.Add(_data.SampleAsRowMatrix(assignmentIdx)); // Pobierz wartości, które zostały przypisane do nowego klastra
                // TODO: W Pythonie robiłem tutaj transpozycję, ale nie wydaje się sensowna

            }, mergedCandidate.IndexFirst, mergedCandidate.IndexSecond);

            _brtState.ActiveNodes.RemoveAll(
                activeNode => activeNode == mergedCandidate.IndexFirst ||
                activeNode == mergedCandidate.IndexSecond
            );

            // Dla każdego z nieprzeanalizowanych jeszcze node'ów
            IMatrix newClusterMatrix = _facade.VStack(newClusterData);
            foreach (int nodeKey in _brtState.ActiveNodes)
            {
                List<int> newClusterChildren = _clustersByKey[newClusterKey].Children;
                List<int> nodeChildren = _clustersByKey[nodeKey].Children;

                // Weź ten node i utworzony właśnie node
                List<IMatrix> nodeClusterData = new List<IMatrix>();

                // Bierze wszystko co jest przypisane do tego węzła, tu są indeksy węzłów na wyjściu
                _brtState.ForAllMatchingAssignments((assignmentIdx) => nodeClusterData.Add(_data.SampleAsRowMatrix(assignmentIdx)), nodeKey);
                // TODO: Wydaje się, że można byłoby to zrobić jakimś słownikiem zamiast iteracją

                IMatrix nodeClusterMatrix = _facade.VStack(nodeClusterData);
                IMatrix dataMerged = _facade.VStack(newClusterMatrix, nodeClusterMatrix);

                double dataLikelihood = _model.CalculateMarginalLikelihood(dataMerged);
                dataLikelihood = double.IsInfinity(dataLikelihood) || double.IsNaN(dataLikelihood) ? double.Epsilon : dataLikelihood;
                double productOfClustersLikelihoods = _clustersByKey[nodeKey].MarginalLikelihood * _clustersByKey[newClusterKey].MarginalLikelihood;

                // Wylicza prawdopodobieństwa dla różnych klastrów powstałych przy użyciu analizowanego klastra i nowego
                double likelihoodFactorJoin = ComputeJoin(dataLikelihood, productOfClustersLikelihoods);
                double likelihoodFactorAbsorbLeft = ComputeAbsorbLeft(dataLikelihood, nodeChildren, newClusterKey, productOfClustersLikelihoods);
                double likelihoodFactorAbsorbRight = ComputeAbsorbRight(dataLikelihood, newClusterChildren, nodeKey, productOfClustersLikelihoods);
                double likelihoodFactorCollapse = ComputeCollapse(dataLikelihood, nodeChildren, newClusterChildren, productOfClustersLikelihoods);

                MergeCandidate newMergeCandidate = new MergeCandidate(
                                                        newClusterKey, nodeKey,
                                                        likelihoodFactorJoin,
                                                        likelihoodFactorAbsorbLeft,
                                                        likelihoodFactorAbsorbRight,
                                                        likelihoodFactorCollapse);
                AddMergeCandidates(newMergeCandidate);
            }
            _brtState.ActiveNodes.Add(newClusterKey);
        }

        protected double ComputeJoin(double dataLikelihood, double productOfClustersLikelihoods)
        {
            double joinLikelihood = CalculateLikelihood(dataLikelihood, productOfClustersLikelihoods, 2);
            return joinLikelihood / productOfClustersLikelihoods;
        }

        protected double ComputeAbsorbLeft(double dataLikelihood, List<int> nodeChildren, int newClusterIdx, double productOfClustersLikelihoods)
        {
            double absorbLeftLikelihoodFactor = 0;
            if (nodeChildren.Count > 0)
            {
                double productOfChildrenLikelihoods = _clustersByKey[newClusterIdx].MarginalLikelihood *
                    nodeChildren.Select(nc => _clustersByKey[nc].MarginalLikelihood).Aggregate((a, b) => a * b);

                double absorbLikelihood = CalculateLikelihood(dataLikelihood, productOfChildrenLikelihoods, (uint)(nodeChildren.Count + 1));
                absorbLeftLikelihoodFactor = absorbLikelihood / productOfClustersLikelihoods;
            }

            return absorbLeftLikelihoodFactor;
        }

        protected double ComputeAbsorbRight(double dataLikelihood, List<int> newClusterChildren, int nodeKey, double productOfClustersLikelihoods)
        {
            double absorbRightLikelihoodFactor = 0;
            if (newClusterChildren.Count > 0)
            {
                double productOfChildrenLikelihoods = _clustersByKey[nodeKey].MarginalLikelihood *
                    newClusterChildren.Select(nc => _clustersByKey[nc].MarginalLikelihood).Aggregate((a, b) => a * b);

                double absorbLikelihood = CalculateLikelihood(dataLikelihood, productOfChildrenLikelihoods, (uint)(newClusterChildren.Count + 1));
                absorbRightLikelihoodFactor = absorbLikelihood / productOfClustersLikelihoods;
            }

            return absorbRightLikelihoodFactor;
        }

        protected double ComputeCollapse(double dataLikelihood, List<int> nodeChildren, List<int> newClusterChildren, double productOfClustersLikelihoods)
        {
            double collapseLikelihoodFactor = 0;
            if (newClusterChildren.Count > 0 && nodeChildren.Count > 0)
            {
                double productOfChildrenLikelihoods = nodeChildren.Select(nc => _clustersByKey[nc].MarginalLikelihood).Aggregate((a, b) => a * b) *
                                                    newClusterChildren.Select(nc => _clustersByKey[nc].MarginalLikelihood).Aggregate((a, b) => a * b);

                double collapseLikelihood = CalculateLikelihood(dataLikelihood, productOfChildrenLikelihoods,
                    (uint)(newClusterChildren.Count + nodeChildren.Count));
                collapseLikelihoodFactor = collapseLikelihood / productOfClustersLikelihoods;
            }
            return collapseLikelihoodFactor;
        }


        private static void Prune(Dictionary<int, BRT.Cluster> clustersByKey)
        {
            int[] keys = clustersByKey.Keys.ToArray();
            for (int i = 0; i < keys.Length; ++i)
            {
                int key = keys[i];
                if (!clustersByKey.TryGetValue(key, out Cluster cluster))
                    continue;

                if (cluster.Children.Count == 0)
                    continue;

                foreach (KeyValuePair<int, Cluster> kvp in clustersByKey)
                {
                    if (kvp.Key == key)
                        continue;

                    if (kvp.Value.Children.Count != cluster.Children.Count)
                        continue;
                }
            }
        }

        public static bool ValidateTree(Dictionary<int, Cluster> clusters)
        {
            // Find root
            Cluster root = clusters.Values.FirstOrDefault(c => !c.Parents.Any());

            if (root == null)
            {
                Console.WriteLine("BRT root not found!");
                return false;
            }

            // Check all elements connected to root
            var visited = new HashSet<int>();
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(root.Key);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                visited.Add(current);

                foreach (int child in clusters[current].Children)
                {
                    if (!visited.Contains(child))
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            return visited.Count == clusters.Count;
        }

        public static List<ClusterResult> ConvertToClusterResult(Dictionary<int, Cluster> clusters)
        {
            Dictionary<int, ClusterResult> resultMap = new Dictionary<int, ClusterResult>();

            // Initiating all clusters
            foreach (Cluster cluster in clusters.Values)
            {
                resultMap[cluster.Key] = new ClusterResult(cluster.Key);
            }

            // Set references to children
            foreach (Cluster cluster in clusters.Values)
            {
                ClusterResult resultCluster = resultMap[cluster.Key];
                foreach (int childKey in cluster.Children)
                {
                    ClusterResult childCluster = resultMap[childKey];
                    resultCluster.Children.Add(childCluster);
                    childCluster.Parent = resultCluster;
                }
            }

            return resultMap.Values.ToList();
        }

        public static Dictionary<int, List<ClusterResult>> DivideTreeIntoLevels(List<ClusterResult> clusterResults)
        {
            Dictionary<int, List<ClusterResult>> levels = new Dictionary<int, List<ClusterResult>>();

            // Find root
            ClusterResult root = clusterResults.FirstOrDefault(cr => cr.Parent == null);

            if (root == null)
            {
                throw new InvalidOperationException("Root not found!");
            }

            Queue<Tuple<ClusterResult, int>> queue = new Queue<Tuple<ClusterResult, int>>();
            queue.Enqueue(new Tuple<ClusterResult, int>(root, 0));

            while (queue.Count > 0)
            {
                (ClusterResult current, int level) = queue.Dequeue();

                if (!levels.ContainsKey(level))
                {
                    levels[level] = new List<ClusterResult>();
                }

                levels[level].Add(current);

                foreach (ClusterResult child in current.Children)
                {
                    queue.Enqueue(new Tuple<ClusterResult, int>(child, level + 1));
                }
            }

            return levels;
        }

    }
}
