using System;
using AIComponent.BTTask;
using Action = AIComponent.BTTask.Action;

namespace AIComponent.CustomTask
{
    public class CheckTimer : Condition
    {
        public CheckTimer(Action action) : base(action)
        {
        }

        public CheckTimer(Func<TaskResult> action) : base(action)
        {
        }
    }
}