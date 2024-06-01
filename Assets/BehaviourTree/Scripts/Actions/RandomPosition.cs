using BehaviourTree.Scripts.Runtime;
using UnityEngine;

namespace BehaviourTree.Scripts.Actions
{
    public class RandomPosition : ActionNode
    {
        protected override void OnStart()
        {
        }

        protected override void OnEnd()
        {
        }

        protected override TaskState OnUpdate()
        {
            return TaskState.Success;
        }
    }
}