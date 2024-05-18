using BehaviourTree.Scripts.Runtime;
using UnityEngine;

namespace BehaviourTree.Scripts.Decorators
{
    public class Timeout : DecoratorNode
    {
        [SerializeField] private float duration = 1.0f;
        private float _startTime;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            if (Time.time - _startTime > duration)
            {
                _startTime = Time.time;
                return State.Failure;
            }

            return Child.Update();
        }
    }
}