using BehaviourTree.Scripts.Runtime;
using UnityEngine;

namespace BehaviourTree.Scripts.Conditions
{
    public class CoolTime : ConditionNode
    {
        private float _cooldownEndTime;
        [SerializeField] private float cooldownTime;

        public override void OnAwake()
        {
            _cooldownEndTime = Time.time;
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            if (Time.time >= _cooldownEndTime)
            {
                _cooldownEndTime = Time.time + cooldownTime;
                return State.Failure;
            }

            return State.Success;
        }
    }
}