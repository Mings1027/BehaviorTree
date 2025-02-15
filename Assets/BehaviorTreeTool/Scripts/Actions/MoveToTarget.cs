using UnityEngine.AI;

namespace Tree
{
    [NodeCategory("NavMeshAgent")]
    public class MoveToTarget : ActionNode
    {
        public SharedCollider target;

        private NavMeshAgent _agent;

        protected override void OnAwake()
        {
            _agent = objectTransform.GetComponent<NavMeshAgent>();
        }

        protected override void OnStart()
        {
            if (target.Value)
            {
                _agent.destination = target.Value.transform.position;
            }
        }

        protected override TaskState OnUpdate()
        {
            if (target.Value)
            {
                _agent.destination = target.Value.transform.position;
                return TaskState.Success;
            }

            return TaskState.Failure;
        }
    }
}