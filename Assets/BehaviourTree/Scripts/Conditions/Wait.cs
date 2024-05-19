using BehaviourTree.Scripts.Runtime;
using UnityEngine;

namespace BehaviourTree.Scripts.Conditions
{
    public class Wait : ConditionNode
    {
        [SerializeField] private int duration = 1;
        private float _startTime;

        protected override void OnStart()
        {
            _startTime = Time.time;
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            if (Time.time - _startTime > duration)
            {
                return State.Success;
            }

            return State.Running;
        }
    }
}