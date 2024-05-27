using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using Pathfinding;
using UnityEngine;

namespace BehaviourTree.Scripts.Actions
{
    public class MoveToTarget : ActionNode
    {
        public SharedCollider target;
        public SharedFloat attackRange;

        private AIPath _aiPath;

        public override void OnAwake()
        {
            _aiPath = nodeTransform.GetComponent<AIPath>();
            _aiPath.endReachedDistance = attackRange.Value;
        }

        protected override void OnStart()
        {
            _aiPath.destination = target.Value.transform.position;

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