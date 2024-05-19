using BehaviourTree.Scripts.Runtime;
using UnityEngine;

namespace BehaviourTree.Scripts.Actions
{
    public class Log : ActionNode
    {
        public string message;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            // Debug.Log($"{message}");
            return State.Success;
        }
    }
}