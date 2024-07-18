using AIComponent.BTTask;

namespace AIComponent.BTContainer
{
    /// <summary>
    /// 하나라도 실패하면 즉시 리턴
    /// </summary>
    public class Sequence : Composite
    {
        public Sequence(params Task[] children) : base(children)
        {
        }

        public override TaskResult Execute(float deltaTime)
        {
            foreach (var child in Children)
            {
                var result = child.Execute(deltaTime);
             
                if (result == TaskResult.Failure)
                    return TaskResult.Failure;
            }

            return TaskResult.Success;
        }
    }
}