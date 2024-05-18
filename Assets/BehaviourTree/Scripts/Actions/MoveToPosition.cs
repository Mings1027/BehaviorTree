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
            _aiPath.destination = destination;
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            return State.Success;
        }
    }
}