using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class ReadyToAttack : ConditionNode
    {
        public SharedCollider target;
        public SharedNavMeshAgent agent;

        [SerializeField] private float attackRange;

        protected override void OnAwake()
        {
            agent.Value.stoppingDistance = attackRange;
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