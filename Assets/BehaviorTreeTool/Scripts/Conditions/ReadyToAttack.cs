using UnityEngine;
using UnityEngine.AI;

namespace Tree
{
    public class ReadyToAttack : ConditionNode
    {
        public SharedCollider target;

        private NavMeshAgent _agent;

        [SerializeField] private float attackRange;

        protected override void OnAwake()
        {
            _agent = nodeTransform.GetComponent<NavMeshAgent>();
            _agent.stoppingDistance = attackRange;
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