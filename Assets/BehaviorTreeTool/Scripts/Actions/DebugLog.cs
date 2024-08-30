using UnityEngine;

namespace Tree
{
    public class DebugLog : ActionNode
    {
        public string message;

        protected override void OnStart() { }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            Debug.Log($"{message}");
            return TaskState.Success;
        }
    }
}