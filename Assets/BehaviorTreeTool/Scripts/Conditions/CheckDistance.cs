using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine.AI;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class CheckDistance : ConditionNode
    {
        private NavMeshAgent agent;

        protected override void OnAwake()
        {
            agent = nodeTransform.GetComponent<NavMeshAgent>();
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