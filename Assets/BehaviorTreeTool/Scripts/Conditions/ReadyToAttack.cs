using UnityEngine;
using UnityEngine.AI;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class ReadyToAttack : ConditionNode
    {
        public SharedCollider target;

        [SerializeField] private float attackRange;

        protected override void OnAwake()
        {
            nodeTransform.GetComponent<NavMeshAgent>().stoppingDistance = attackRange;
        }

        protected override TaskState OnUpdate()
        {
            if (target.Value && target.Value.enabled)
            {
                return TaskState.Success;
            }

            return TaskState.Failure;
        }
    }
}