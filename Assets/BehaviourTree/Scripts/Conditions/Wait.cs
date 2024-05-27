using BehaviourTree.Scripts.Runtime;
using UnityEngine;

namespace BehaviourTree.Scripts.Conditions
{
    public class Wait : ConditionNode
    {
        private float _startTime;
        [SerializeField] private int duration = 1;

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