namespace BRT
{
    public class State
    {
        private readonly List<int> _assignments;

        internal bool IsActive => ActiveNodes.Count > 1;
        internal List<int> ActiveNodes { get; }

        internal State(int sampleCount)
        {
            IEnumerable<int> enumeration = Enumerable.Range(0, sampleCount);
            // ReSharper disable once PossibleMultipleEnumeration
            _assignments = enumeration.ToList();
            // ReSharper disable once PossibleMultipleEnumeration
            ActiveNodes = enumeration.ToList();
        }

        internal State(State intervalState)
        {
            _assignments = new List<int>(intervalState._assignments);
            ActiveNodes = new List<int>(intervalState.ActiveNodes);
        }

        internal void ForAllMatchingAssignments(Action<int> action, params int[] keysToMatch)
        {
            for (int assignmentIdx = 0; assignmentIdx < _assignments.Count; ++assignmentIdx)
            {
                int key = _assignments[assignmentIdx];
                if (keysToMatch.Any(keyToMatch => keyToMatch == key))
                    action(assignmentIdx);
            }
        }

        internal void Assign(int assignmentIdx, int clusterKey)
        {
            _assignments[assignmentIdx] = clusterKey;
        }
    }
}
