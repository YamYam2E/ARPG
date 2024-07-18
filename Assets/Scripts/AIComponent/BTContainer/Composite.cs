using AIComponent.BTTask;

namespace AIComponent.BTContainer
{
    public abstract class Composite : Task
    {
        protected  Task[] Children;
        
        protected Composite(Task[] children)
        {
            Children = children;
        }
    }
}