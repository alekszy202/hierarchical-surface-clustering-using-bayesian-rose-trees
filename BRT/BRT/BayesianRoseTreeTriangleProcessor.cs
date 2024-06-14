using Interfaces;

namespace BRT
{
    public class BayesianRoseTreeTriangleProcessor : BayesianRoseTree
    {
        private List<Tuple<int, int>> _triangleNeighbourhood;
        private Dictionary<int, List<Tuple<int, int>>> _triangleNeighbourhoodMap;
        private bool _endOfNeighbours = false;

        public BayesianRoseTreeTriangleProcessor(IFacade facade, IMatrix data, List<Tuple<int, int>> triangleNeighbourhood, IModel model) : base(facade, data, model)
        {
            _triangleNeighbourhood = triangleNeighbourhood;
        }

        protected override void InitPairs()
        {
            _triangleNeighbourhoodMap = new Dictionary<int, List<Tuple<int, int>>>();
            foreach (Tuple<int, int> tPair in _triangleNeighbourhood)
            {
                int idxFirst = tPair.Item1;
                int idxSecond = tPair.Item2;

                IMatrix samplesMerged = _data.Sample(idxFirst, idxSecond);
                double dataMarginalLikelihood = _model.CalculateMarginalLikelihood(samplesMerged);
                // double dataLnMarginalLikelihood = _model.CalculateLnMarginalLikelihood(samplesMerged);

                double mixtureLikelihood = _clustersByKey[idxFirst].MarginalLikelihood * _clustersByKey[idxSecond].MarginalLikelihood;
                // double mixtureLnLikelihood = _clustersByKey[idxFirst].LnMarginalLikelihood + _clustersByKey[idxSecond].LnMarginalLikelihood;

                double likelihood = CalculateLikelihood(dataMarginalLikelihood, mixtureLikelihood, 2);
                //double lnLikelihood = CalculateLnLikelihood(dataLnMarginalLikelihood, mixtureLnLikelihood, 2);

                double likelihoodJoin = likelihood / mixtureLikelihood;
                MergeCandidate mergeCandidate = new MergeCandidate(idxFirst, idxSecond, likelihoodJoin, 0, 0, 0);

                AddMergeCandidates(mergeCandidate);
                if (!_triangleNeighbourhoodMap.ContainsKey(idxFirst))
                {
                    _triangleNeighbourhoodMap[idxFirst] = new List<Tuple<int, int>>();
                }

                if (!_triangleNeighbourhoodMap.ContainsKey(idxSecond))
                {
                    _triangleNeighbourhoodMap[idxSecond] = new List<Tuple<int, int>>();
                }

                _triangleNeighbourhoodMap[idxFirst].Add(tPair);
                _triangleNeighbourhoodMap[idxSecond].Add(tPair);
            }
        }
        protected void AddTriangleNeighbourhood(Tuple<int, int> neighbourhood)
        {
            _triangleNeighbourhood.Add(neighbourhood);

            if (!_triangleNeighbourhoodMap.ContainsKey(neighbourhood.Item1))
            {
                _triangleNeighbourhoodMap[neighbourhood.Item1] = new List<Tuple<int, int>>();
            }

            if (!_triangleNeighbourhoodMap.ContainsKey(neighbourhood.Item2))
            {
                _triangleNeighbourhoodMap[neighbourhood.Item2] = new List<Tuple<int, int>>();
            }

            if (!_triangleNeighbourhoodMap[neighbourhood.Item1].Contains(neighbourhood))
            {
                _triangleNeighbourhoodMap[neighbourhood.Item1].Add(neighbourhood);
            }

            if (!_triangleNeighbourhoodMap[neighbourhood.Item2].Contains(neighbourhood))
            {
                _triangleNeighbourhoodMap[neighbourhood.Item2].Add(neighbourhood);
            }
        }

        protected override void BuildStates(MergeCandidate mergedCandidate, int newClusterKey)
        {
            if (_mergeCandidates.Count == 0)
                _endOfNeighbours = true;

            if (_endOfNeighbours)
            {
                base.BuildStates(mergedCandidate, newClusterKey);
                return;
            }

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

            List<List<Tuple<int, int>>> allTargetNeighbours = new List<List<Tuple<int, int>>>();
            if (_triangleNeighbourhoodMap.ContainsKey(mergedCandidate.IndexFirst))
            {
                allTargetNeighbours.Add(new List<Tuple<int, int>>(_triangleNeighbourhoodMap[mergedCandidate.IndexFirst]));
            }

            if (_triangleNeighbourhoodMap.ContainsKey(mergedCandidate.IndexSecond))
            {
                allTargetNeighbours.Add(new List<Tuple<int, int>>(_triangleNeighbourhoodMap[mergedCandidate.IndexSecond]));
            }

            List<int> newNeighbours = new List<int>();
            foreach (List<Tuple<int, int>> targetNeighbours in allTargetNeighbours)
            {
                foreach (Tuple<int, int> neighbours in targetNeighbours)
                {
                    if (neighbours.Item1 == mergedCandidate.IndexFirst || neighbours.Item1 == mergedCandidate.IndexSecond)
                    {
                        newNeighbours.Add(neighbours.Item2);
                    }

                    if (neighbours.Item2 == mergedCandidate.IndexFirst || neighbours.Item2 == mergedCandidate.IndexSecond)
                    {
                        newNeighbours.Add(neighbours.Item1);
                    }

                    _triangleNeighbourhood.Remove(neighbours);
                    _triangleNeighbourhoodMap[neighbours.Item1].Remove(neighbours);
                    _triangleNeighbourhoodMap[neighbours.Item2].Remove(neighbours);
                }
            }

            newNeighbours.RemoveAll(value => value == mergedCandidate.IndexFirst);
            newNeighbours.RemoveAll(value => value == mergedCandidate.IndexSecond);

            IMatrix newClusterMatrix = _facade.VStack(newClusterData);
            foreach (int nodeKey in newNeighbours) // Dla każdego z sąsiadujących nodów
            {
                List<int> newClusterChildren = _clustersByKey[newClusterKey].Children;
                List<int> nodeChildren = _clustersByKey[nodeKey].Children;

                // Weź ten node i utworzony właśnie node
                List<IMatrix> nodeClusterData = new List<IMatrix>();

                // Bierze wszystko co jest przypisane do tego węzła, tu są indeksy węzłów na wyjściu
                // TODO: Wydaje się, że można byłoby to zrobić jakimś słownikiem zamiast iteracją
                _brtState.ForAllMatchingAssignments((assignmentIdx) => nodeClusterData.Add(_data.SampleAsRowMatrix(assignmentIdx)), nodeKey);

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
                AddTriangleNeighbourhood(new Tuple<int, int>(newClusterKey, nodeKey));
            }
            _brtState.ActiveNodes.Add(newClusterKey);
        }
    }
}
