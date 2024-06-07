using UnityEngine;
using UnityEngine.AI;

namespace BehaviorTreeTool.Scripts.Actions
{
    public class CheckDistance : ConditionNode
    {
        public SharedNavMeshAgent navMeshAgent;

        protected override void OnAwake()
        {
            navMeshAgent.Value.destination = nodeTransform.position + new Vector3(1, 0, 1);
        }

        protected override void OnStart() { }

        protected override TaskState OnUpdate()
        {
            if (navMeshAgent.Value.pathPending) return TaskState.Running;
            if (navMeshAgent.Value.remainingDistance <= navMeshAgent.Value.stoppingDistance) return TaskState.Success;
            if (navMeshAgent.Value.pathStatus == NavMeshPathStatus.PathInvalid) return TaskState.Failure;

            return TaskState.Running;
        }
    }
}