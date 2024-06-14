using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class CoolTime : ConditionNode
    {
        private float _cooldownEndTime;
        [SerializeField] private float cooldownTime;

        protected override void OnAwake()
        {
            _cooldownEndTime = Time.time;
        }

        protected override void OnStart() { }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            if (Time.time >= _cooldownEndTime)
            {
                _cooldownEndTime = Time.time + cooldownTime;
                return TaskState.Failure;
            }

            return TaskState.Success;
        }
    }
}