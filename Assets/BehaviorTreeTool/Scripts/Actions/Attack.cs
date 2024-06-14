using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Actions
{
    public class Attack : ActionNode
    {
        public SharedCollider target;

        [SerializeField] private int attackDamage;

        protected override TaskState OnUpdate()
        {
            if (!target.Value) return TaskState.Failure;
            if (target.Value.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(attackDamage);
            }

            return TaskState.Success;
        }
    }
}