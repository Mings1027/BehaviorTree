using UnityEngine.AI;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class CheckDistance : ConditionNode
    {
        public SharedNavMeshAgent agent;

        protected override TaskState OnUpdate()
        {
            if (agent.Value.pathPending) return TaskState.Running;
            if (agent.Value.remainingDistance <= agent.Value.stoppingDistance) return TaskState.Success;
            if (agent.Value.pathStatus == NavMeshPathStatus.PathInvalid) return TaskState.Failure;

            return TaskState.Running;
        }
    }
}