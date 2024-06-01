using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeData;
using Pathfinding;

namespace BehaviourTree.Scripts.Actions
{
    public class MoveToTarget : ActionNode
    {
        public SharedCollider target;

        private AIPath _aiPath;

        protected override void OnAwake()
        {
            _aiPath = nodeTransform.GetComponent<AIPath>();
        }

        protected override void OnStart()
        {
            _aiPath.destination = target.Value.transform.position;
        }

        protected override TaskState OnUpdate()
        {
            return TaskState.Success;
        }
    }
}