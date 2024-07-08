using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class Wait : ConditionNode
    {
        private float _startTime;

        [SerializeField] private float duration = 1;
        [SerializeField] private float remaining;

        protected override void OnStart()
        {
            _startTime = Time.time;
            remaining = duration;
        }

        protected override TaskState OnUpdate()
        {
            remaining = duration - (Time.time - _startTime);
            if (remaining <= 0)
            {
                remaining = 0;
                return TaskState.Success;
            }
            return TaskState.Running;
        }
    }
}