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
                var distance = Vector3.Distance(enemy.Value.transform.position, nodeTransform.position);

                if (distance <= attackRange)
                {
                    return TaskState.Success;
                }
            }

            return TaskState.Failure;
        }
    }
}