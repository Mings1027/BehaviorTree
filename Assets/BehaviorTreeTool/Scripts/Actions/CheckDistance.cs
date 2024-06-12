using UnityEngine;
using UnityEngine.AI;

namespace BehaviorTreeTool.Scripts.Actions
{
    public class CheckDistance : ConditionNode
    {
        public SharedNavMeshAgent navMeshAgent;

        protected override TaskState OnUpdate()
        {
            if (navMeshAgent.Value.pathPending) return TaskState.Running;
            if (navMeshAgent.Value.remainingDistance <= navMeshAgent.Value.stoppingDistance) return TaskState.Success;
            if (navMeshAgent.Value.pathStatus == NavMeshPathStatus.PathInvalid) return TaskState.Failure;

            return TaskState.Running;
        }
    }
}