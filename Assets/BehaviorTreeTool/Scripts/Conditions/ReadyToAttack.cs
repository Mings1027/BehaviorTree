using UnityEngine;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class ReadyToAttack : ConditionNode
    {
        public SharedCollider target;
        public SharedInt viewAngle;
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
                var dirToTarget = (target.Value.transform.position - nodeTransform.position).normalized;
                var angleToTarget = Vector3.Angle(nodeTransform.forward, dirToTarget);
                if (distance <= attackRange && angleToTarget <= viewAngle.Value)
                {
                    return TaskState.Success;
                }
            }

            return TaskState.Failure;
        }
    }
}