using System.Collections.Generic;
using Common;
using UnityEngine;

namespace AIComponent
{
    public class TickManager : MonoBehaviourSingleton<TickManager>
    {
        private List<Root> _behaviorRoots = new();
        
        public void AddBehaviorRoot(Root root)
        {
            _behaviorRoots.Add(root);
        }
        
        public void RemoveBehaviorRoot(Root root)
        {
            _behaviorRoots.Remove(root);
        }
        
        public void Clear()
        {
            _behaviorRoots.Clear();
        }
        
        private void Update()
        {
            Tick();
        }
        public void Tick()
        {
            _behaviorRoots.ForEach(root =>
            {
                root.Run(Time.deltaTime);
            });
        }
    }
}