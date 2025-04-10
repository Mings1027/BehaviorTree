using UnityEngine.AI;

namespace Tree
{
    [NodeCategory("NavMeshAgent")]
    public class CheckDistance : ConditionNode
    {
        private NavMeshAgent agent;

        protected override void OnAwake()
        {
            agent = objectTransform.GetComponent<NavMeshAgent>();
        }

        protected override TaskState OnUpdate()
        {
            if (agent.pathPending) return TaskState.Running;
            if (agent.remainingDistance <= agent.stoppingDistance) return TaskState.Success;
            if (agent.pathStatus == NavMeshPathStatus.PathInvalid) return TaskState.Failure;

            return TaskState.Running;
        }
    }
}