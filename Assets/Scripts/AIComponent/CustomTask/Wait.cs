using System;
using AIComponent.BTTask;
using Action = AIComponent.BTTask.Action;

namespace AIComponent.CustomTask
{
    public class Wait : Action
    {
        public Wait(Action action) : base(action)
        {
        }

        public Wait(Func<TaskResult> action) : base(action)
        {
        }
    }
}