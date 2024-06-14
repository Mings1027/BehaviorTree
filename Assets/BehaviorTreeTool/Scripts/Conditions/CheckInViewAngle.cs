#if UNITY_EDITOR
using UnityEditor;
#endif
using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class CheckInViewAngle : ConditionNode
    {
        public SharedCollider target;
        public SharedInt checkRange;

        [SerializeField] private int viewAngle;

        protected override TaskState OnUpdate()
        {
            if (!target.Value) return TaskState.Failure;

            var directionToTarget = target.Value.transform.position - nodeTransform.position;
            var angle = Vector3.Angle(nodeTransform.forward, directionToTarget);

            // 시야각 내에 있으면 성공
            if (angle <= viewAngle / 2f)
            {
                return TaskState.Success;
            }

            // 시야각 밖에 있어도 범위 내에 있으면 성공
            var distanceToTarget = Vector3.SqrMagnitude(nodeTransform.position - target.Value.transform.position);
            if (distanceToTarget <= checkRange.Value * checkRange.Value)
            {
                return TaskState.Success;
            }

            // 시야각 밖이면서 범위 밖이면 실패
            return TaskState.Failure;
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (nodeTransform == null) return;

            // 시야각을 그리기 위한 Handles 설정
            Handles.color = new Color(1, 1, 0, 0.2f); // 노란색 반투명

            // 원호를 채워진 형태로 그리기
            Vector3 forward = nodeTransform.forward * checkRange.Value;
            Vector3 startDirection = Quaternion.Euler(0, -viewAngle / 2f, 0) * forward;
            Handles.DrawSolidArc(nodeTransform.position, Vector3.up, startDirection, viewAngle, checkRange.Value);

            // 중심선 그리기
            Handles.color = Color.red;
            Handles.DrawLine(nodeTransform.position, nodeTransform.position + nodeTransform.forward * checkRange.Value);
        }
#endif
    }
}