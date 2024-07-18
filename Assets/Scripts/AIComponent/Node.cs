namespace AIComponent
{
    public abstract class Node
    {
        public enum NodeState
        {
            Active,
            Inactive,
            Running,
        }
        
        public NodeState State { get; protected set; }
    }
}