using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree.Scripts.Runtime;

public class Cooltime : ConditionNode
{
    private float _cooldownEndTime;
    [SerializeField] private float cooldownTime;

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (Time.time >= _cooldownEndTime)
        {
            _cooldownEndTime = Time.time + cooldownTime;
            return State.Failure;
        }

        return State.Success;
    }
}