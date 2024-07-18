using System;

namespace AIComponent.BTTask
{
    public class Action : Task
    {
        private Func<TaskResult> _action;
        
        public Action( Action action )
        {
            
        }
        
        public Action(Func<TaskResult> action)
        {
            _action = action;
        }
        
        public override TaskResult Execute(float deltaTime)
        {
            return _action?.Invoke() ?? TaskResult.Failure;
        }
    }
}