using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using Utilities;

public class ReadyToAttack : ConditionNode
{
    private Cooldown _attackCooldown;
    [SerializeField] private SharedFloat attackCooldownTime;
    [SerializeField] private SharedCollider target;
    [SerializeField] private SharedInt attackRange;

    public override void OnAwake()
    {
        _attackCooldown = new Cooldown();
        _attackCooldown.cooldownTime = attackCooldownTime.Value;
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
        // 공격 쿨다운 중이면 실패 상태 반환
        if (_attackCooldown.isCoolingDown) return State.Failure;

        // 쿨다운 시작
        _attackCooldown.StartCooldown();

        // 타겟이 존재하고 활성화된 경우
        var targetTransform = target.Value?.transform;
        if (targetTransform)
        {
            // 타겟과의 거리 계산
            var distanceToTarget = Vector3.Distance(nodeTransform.position, targetTransform.position);

            // 공격 범위 내에 있는 경우 성공 상태 반환
            if (distanceToTarget <= attackRange.Value)
            {
                return State.Success;
            }
        }

        // 타겟이 활성화된 경우 성공 상태 반환
        if (target.Value && target.Value.enabled)
        {
            return State.Success;
        }

        // 나머지 경우 실패 상태 반환
        return State.Failure;
    }
}