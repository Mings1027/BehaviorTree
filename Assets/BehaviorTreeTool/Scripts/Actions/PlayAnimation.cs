using UnityEngine;

namespace Tree
{
    [NodeCategory("Animation")]
    public class PlayAnimation : ActionNode
    {
        private Animator _animator;
        private int _animationHash;

        [SerializeField] private string animationName;

        protected override void OnAwake()
        {
            _animator = objectTransform.GetComponentInChildren<Animator>();
            _animationHash = Animator.StringToHash(animationName);
        }

        protected override void OnStart()
        {
            _animator.SetTrigger(_animationHash);
        }

        protected override TaskState OnUpdate()
        {
            return TaskState.Success;
        }
    }
}