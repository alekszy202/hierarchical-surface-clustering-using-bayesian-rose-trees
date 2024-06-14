namespace BRT
{
    public class MergeCandidate
    {
        public enum MergeType { Join, AbsorbLeft, AbsorbRight, Collapse, MergeTypeCount }

        public readonly int IndexFirst;
        public readonly int IndexSecond;
        public readonly double LikelihoodJoin;
        public readonly double LikelihoodAbsorbLeft;
        public readonly double LikelihoodAbsorbRight;
        public readonly double LikelihoodCollapse;

        public double this[MergeType mergeType]
        {
            get
            {
                switch (mergeType)
                {
                    case MergeType.Join:
                        return LikelihoodJoin;
                    case MergeType.AbsorbLeft:
                        return LikelihoodAbsorbLeft;
                    case MergeType.AbsorbRight:
                        return LikelihoodAbsorbRight;
                    case MergeType.Collapse:
                        return LikelihoodCollapse;
                    default:
                        throw new ArgumentOutOfRangeException("No such merge type");
                }
            }
        }

        public MergeCandidate(int indexFirst, int indexSecond, double likelihoodJoin,
            double likelihoodAbsorbLeft, double likelihoodAbsorbRight, double likelihoodCollapse)
        {
            IndexFirst = indexFirst;
            IndexSecond = indexSecond;
            LikelihoodJoin = likelihoodJoin;
            LikelihoodAbsorbLeft = likelihoodAbsorbLeft;
            LikelihoodAbsorbRight = likelihoodAbsorbRight;
            LikelihoodCollapse = likelihoodCollapse;

            if (double.IsNaN(LikelihoodJoin))
                LikelihoodJoin = double.Epsilon;
        }
    }
}
