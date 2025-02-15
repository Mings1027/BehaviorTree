using System.Threading;
using Global;
using UnityEngine;

namespace Tree
{
    public class Attack : ActionNode
    {
        public SharedCollider target;
        public SharedFloat attackRange;

        private static readonly int AttackID = Animator.StringToHash("Attack");
        private Animator _animator;
        private bool _isAttacking; // 공격 상태를 추적하는 플래그
        private CancellationTokenSource _atkCts;
        private DamageComponent _damageComponent;

        protected override void OnAwake()
        {
            base.OnAwake();
            _animator = objectTransform.GetComponent<Animator>();
            _damageComponent = objectTransform.GetComponent<DamageComponent>();
        }

        public override void OnTreeEnabled()
        {
            base.OnTreeEnabled();
            _atkCts = new CancellationTokenSource();
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (!target.Value || !target.Value.gameObject.activeSelf) return;
            var distance = Vector3.Distance(objectTransform.position, target.Value.transform.position);
            if (distance > attackRange.Value) return;

            TriggerAttack();
            if (target.Value.TryGetComponent(out IDamageable damageable))
            {
                damageable.Damage(_damageComponent.CurDamage);
            }
        }

        private async void TriggerAttack()
        {
            if (_isAttacking) return;

            try
            {
                _isAttacking = true;
                await AnimationManager.TriggerAnimation(_animator, AttackID, _atkCts.Token);
            }
            finally
            {
                _isAttacking = false;
            }
        }

        protected override TaskState OnUpdate()
        {
            if (!_isAttacking && !target.Value) return TaskState.Failure;
            return _isAttacking ? TaskState.Running : TaskState.Success;
        }

        public override void OnTreeDisabled()
        {
            base.OnTreeDisabled();
            _atkCts?.Cancel();
            _atkCts?.Dispose();
        }
    }
}