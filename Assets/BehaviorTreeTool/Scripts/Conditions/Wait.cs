using UnityEditor;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class Wait : ConditionNode
    {
        private float _startTime;
        [SerializeField] private int duration = 1;

        private string _remainingTimeText;

        protected override void OnStart()
        {
            _startTime = Time.time;
        }
        protected override TaskState OnUpdate()
        {
            float elapsedTime = Time.time - _startTime;
            float remainingTime = duration - elapsedTime;
#if UNITY_EDITOR
            _remainingTimeText = $"Remaining Time: {remainingTime:0.00} seconds";
#endif
            if (remainingTime <= 0)
            {
                _startTime = Time.time;
                return TaskState.Success;
            }

            return TaskState.Running;
        }
#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(nodeTransform.position, 0.5f);

                Handles.Label(nodeTransform.position, _remainingTimeText);
            }
        }
#endif
    }
}