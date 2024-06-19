using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;
namespace BehaviorTreeTool.Scripts.Conditions
{
    public class WaitForAnimation : ConditionNode
    {
        private Animator _animator;

        protected override void OnAwake()
        {
            _animator = nodeTransform.GetComponentInChildren<Animator>();
        }

        protected override TaskState OnUpdate()
        {
            var currentAnimatorStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (currentAnimatorStateInfo.normalizedTime < 1.0f && currentAnimatorStateInfo.length > 0)
            {
                return TaskState.Running;
            }

            return TaskState.Success;
        }
    }
}