namespace AIComponent.BTTask
{
    public enum TaskResult
    {
        Success,
        Failure,
        Running,
    }
    
    public abstract class Task : Node
    {
        public float LastTickTime { get; set; }
        
        public abstract TaskResult Execute(float deltaTime);
    }
}