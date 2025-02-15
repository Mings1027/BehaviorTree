using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Tree
{
    [NodeCategory("NavMeshAgent")]
    public class ReadyToAttack : ConditionNode
    {
        public SharedCollider enemy;
        public SharedNavMeshAgent agent;
        public SharedFloat attackRange;

        protected override void OnAwake()
        {
            agent.Value.stoppingDistance = attackRange.Value;
        }

        protected override TaskState OnUpdate()
        {
            if (enemy.Value && enemy.Value.enabled)
            {
                var distance = Vector3.Distance(objectTransform.position, enemy.Value.transform.position);

                if (distance <= attackRange.Value)
                {
                    return TaskState.Success;
                }
            }

            return TaskState.Failure;
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (objectTransform == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(objectTransform.position, attackRange.Value);
        }
#endif
    }
}