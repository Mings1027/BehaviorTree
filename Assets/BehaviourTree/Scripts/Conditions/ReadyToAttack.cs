using UnityEngine;
using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using Utilities;

public class ReadyToAttack : ConditionNode
{
    public SharedCollider target;
    public SharedInt attackRange;

    private Cooldown _attackCooldown;
    [SerializeField] private float attackCooldownTime;

    public override void OnAwake()
    {
        _attackCooldown = new Cooldown();
        _attackCooldown.cooldownTime = attackCooldownTime;
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
        if (!target.Value) return State.Failure;

        // 타겟이 존재하고 활성화된 경우
        // 타겟과의 거리 계산
        var distanceToTarget = Vector3.Distance(nodeTransform.position, target.Value.transform.position);

        // 공격 범위 내에 있는 경우 성공 상태 반환
        if (distanceToTarget <= attackRange.Value)
        {
            return State.Success;
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