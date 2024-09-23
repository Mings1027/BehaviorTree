using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Tree
{
    public class ReadyToAttack : ConditionNode
    {
        public SharedCollider enemy;

        private NavMeshAgent _agent;

        [SerializeField] private float attackRange;

        protected override void OnAwake()
        {
            _agent = nodeTransform.GetComponent<NavMeshAgent>();
            _agent.stoppingDistance = attackRange;
        }

        protected override TaskState OnUpdate()
        {
            if (enemy.Value && enemy.Value.enabled)
            {
                var distance = Vector3.Distance(nodeTransform.position, enemy.Value.transform.position);

                if (distance <= attackRange)
                {
                    return TaskState.Success;
                }
            }

            return TaskState.Failure;
        }

        public override void OnDrawGizmos()
        {
            if (nodeTransform == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(nodeTransform.position, attackRange);
        }
    }
}