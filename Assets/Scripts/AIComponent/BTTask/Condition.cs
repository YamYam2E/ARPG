using System;

namespace AIComponent.BTTask
{
    public class Condition : Task
    {
        private Func<TaskResult> _action;
        
        public Condition( Action action )
        {
            
        }
        
        public Condition(Func<TaskResult> action)
        {
            _action = action;
        }
        
        public override TaskResult Execute(float deltaTime)
        {
            return _action?.Invoke() ?? TaskResult.Failure;
        }
    }
}