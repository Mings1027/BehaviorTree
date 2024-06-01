using BehaviourTree.Scripts.Runtime;
using Pathfinding;
using UnityEngine;

namespace BehaviourTree.Scripts.Actions
{
    public class MoveToPosition : ActionNode
    {
        private AIPath _aiPath;
        
        [SerializeField] private Vector3 destination;

        protected override void OnAwake()
        {
            _aiPath = nodeTransform.GetComponent<AIPath>();
        }

        protected override void OnStart()
        {
        }

        protected override void OnEnd()
        {
        }

        protected override TaskState OnUpdate()
        {
            _aiPath.destination = destination;
            return TaskState.Success;
        }
    }
}