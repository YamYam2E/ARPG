using AIComponent.BTTask;

namespace AIComponent
{
    public class Root
    {
        public Task MainTask { get; }

        public Root(Task mainTask)
        {
            MainTask = mainTask;
        }

        public void StartRoot()
        {
            TickManager.Instance.AddBehaviorRoot( this );   
        }
        
        public void StopRoot()
        {
            TickManager.Instance.RemoveBehaviorRoot( this );
        }

        public void Run(float deltaTime)
        {
            MainTask.Execute(deltaTime);
        }
    }
}