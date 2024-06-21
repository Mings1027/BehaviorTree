using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class ReadyToAttack : ConditionNode
    {
        public SharedCollider target;

        private NavMeshAgent agent;

        [SerializeField] private float attackRange;

        protected override void OnAwake()
        {
            agent = nodeTransform.GetComponent<NavMeshAgent>();
            agent.stoppingDistance = attackRange;
        }

        protected override TaskState OnUpdate()
        {
            if (target.Value && target.Value.enabled)
            {
                var distance = Vector3.Distance(target.Value.transform.position, nodeTransform.position);

                if (distance <= attackRange)
                {
                    return TaskState.Success;
                }
            }

            return TaskState.Failure;
        }
    }
}