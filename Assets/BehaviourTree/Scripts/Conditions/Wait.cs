using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using UnityEngine;

namespace BehaviourTree.Scripts.Conditions
{
    public class Wait : ConditionNode
    {
        [SerializeField] private SharedInt duration;
        private float _startTime;

        public override void OnAwake()
        {
        }

        protected override void OnStart()
        {
            _startTime = Time.time;

            if (duration == null)
            {
                Debug.LogError("SharedFloat 'Duration' not found.");
            }
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            if (duration == null)
            {
                return State.Failure;
            }

            if (Time.time - _startTime > duration.Value)
            {
                return State.Success;
            }

            return State.Running;
        }
    }
}