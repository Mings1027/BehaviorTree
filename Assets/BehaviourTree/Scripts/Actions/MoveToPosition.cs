using BehaviourTree.Scripts.Runtime;
using Pathfinding;
using UnityEngine;

namespace BehaviourTree.Scripts.Actions
{
    public class MoveToPosition : ActionNode
    {
        private AIPath _aiPath;
        [SerializeField] private Vector3 destination;

        public override void OnAwake()
        {
            _aiPath = nodeTransform.GetComponent<AIPath>();
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            _aiPath.destination = destination;
            return State.Success;
        }
    }
}