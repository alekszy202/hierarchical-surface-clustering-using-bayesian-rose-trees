namespace BRT
{
    public class ClusterResult
    {
        public int ID { get; set; }
        public List<ClusterResult> Children { get; set; }
        public ClusterResult Parent { get; set; }

        public ClusterResult(int id)
        {
            ID = id;
            Children = new List<ClusterResult>();
            Parent = null;
        }
    }

}
