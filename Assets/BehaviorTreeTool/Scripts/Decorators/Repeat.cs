using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Decorators
{
    public class Repeat : DecoratorNode
    {
        [SerializeField] private bool repeatForever = true;
        [SerializeField] private int repeatCount;

        private int currentCount;

        protected override void OnStart()
        {
            currentCount = 0;
        }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            if (!repeatForever && currentCount >= repeatCount)
            {
                return TaskState.Success;
            }

            var childState = child.Update();

            if (childState != TaskState.Running)
            {
                currentCount++;
                if (!repeatForever && currentCount >= repeatCount)
                {
                    return TaskState.Success;
                }
            }

            return TaskState.Running;
        }
    }
}
