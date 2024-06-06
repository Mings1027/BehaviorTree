using UnityEngine;

namespace BehaviorTreeTool.Scripts.Decorators
{
    public class Timeout : DecoratorNode
    {
        [SerializeField] private float duration = 1.0f;
        private float _startTime;

        protected override void OnStart() { }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            if (Time.time - _startTime > duration)
            {
                _startTime = Time.time;
                return TaskState.Failure;
            }

            return child.Update();
        }
    }
}