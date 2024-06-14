using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Actions
{
    public class Breakpoint : ActionNode
    {
        protected override void OnStart()
        {
            Debug.Log("Triggered Breakpoint");
            Debug.Break();
        }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            return TaskState.Success;
        }
    }
}