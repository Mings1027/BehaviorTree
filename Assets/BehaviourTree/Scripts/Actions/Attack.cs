using UnityEngine;
using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeData;
using InterfaceFolder;

public class Attack : ActionNode
{
    public SharedCollider target;
    
    [SerializeField] private int attackDamage;

    protected override void OnStart()
    {
    }

    protected override void OnEnd()
    {
    }

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