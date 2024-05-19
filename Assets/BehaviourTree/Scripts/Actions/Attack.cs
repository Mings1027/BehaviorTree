using UnityEngine;
using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using InterfaceFolder;

public class Attack : ActionNode
{
    public SharedCollider target;
    [SerializeField] private int attackDamage;

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (target.Value.TryGetComponent(out IDamageable damageable))
        {
            damageable.Damage(attackDamage);
            Debug.Log("Attack");
        }

        return State.Success;
    }
}