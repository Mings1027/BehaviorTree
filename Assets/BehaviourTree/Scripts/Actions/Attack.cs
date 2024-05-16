using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using InterfaceFolder;

public class Attack : ActionNode
{
    [SerializeField] private SharedInt attackDamage;
    [SerializeField] private SharedCollider target;

    public override void OnAwake()
    {
        target = (SharedCollider)GetSharedVariable(target.variableName);
    }

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
            damageable.Damage(attackDamage.Value);
        }

        return State.Success;
    }
}