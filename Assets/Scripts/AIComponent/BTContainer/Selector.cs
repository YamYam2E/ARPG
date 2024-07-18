using AIComponent.BTTask;

namespace AIComponent.BTContainer
{
    /// <summary>
    /// 하나라도 성공하면 리턴
    /// </summary>
    public abstract class Selector : Composite
    {
        public Selector(params Task[] children) : base(children)
        {
        }
        
        public override TaskResult Execute(float deltaTime)
        {
            foreach (var child in Children)
            {
                var result = child.Execute(deltaTime);

                if (result == TaskResult.Success)
                    return TaskResult.Success;
            }

            return TaskResult.Failure;
        }
    }
}