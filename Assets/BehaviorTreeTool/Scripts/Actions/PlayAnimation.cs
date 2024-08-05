using UnityEngine;

namespace Tree
{
    public class PlayAnimation : ActionNode
    {
        private Animator _animator;
        private int _animationHash;

        [SerializeField] private string animationName;

        protected override void OnAwake()
        {
            _animator = nodeTransform.GetComponentInChildren<Animator>();
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