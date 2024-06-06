using UnityEngine;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class Wait : ConditionNode
    {
        private float _startTime;
        [SerializeField] private int duration = 1;

        protected override void OnStart()
        {
            _startTime = Time.time;
        }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            if (Time.time - _startTime > duration)
            {
                return TaskState.Success;
            }

            return TaskState.Running;
        }
    }
}