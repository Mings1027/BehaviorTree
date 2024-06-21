using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine.AI;

namespace BehaviorTreeTool.Scripts.Actions
{
    public class MoveToTarget : ActionNode
    {
        public SharedCollider target;

        private NavMeshAgent agent;

        protected override void OnAwake()
        {
            agent = nodeTransform.GetComponent<NavMeshAgent>();
        }

        protected override void OnStart()
        {
            if (target.Value)
            {
                agent.destination = target.Value.transform.position;
            }
        }

        protected override TaskState OnUpdate()
        {
            if (target.Value)
            {
                agent.destination = target.Value.transform.position;
                return TaskState.Success;
            }

            return TaskState.Failure;
        }
    }
}