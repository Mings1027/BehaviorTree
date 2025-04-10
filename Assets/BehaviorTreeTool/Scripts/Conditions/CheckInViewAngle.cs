#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace Tree
{
    public class CheckInViewAngle : ConditionNode
    {
        public SharedCollider checkTarget;
        public SharedInt checkRange;

        [SerializeField] private int viewAngle;

        protected override TaskState OnUpdate()
        {
            if (!checkTarget.Value) return TaskState.Failure;

            var directionToTarget = checkTarget.Value.transform.position - objectTransform.position;
            var angle = Vector3.Angle(objectTransform.forward, directionToTarget);

            // 시야각 내에 있으면 성공
            if (angle <= viewAngle / 2f)
            {
                return TaskState.Success;
            }

            // 시야각 밖에 있어도 범위 내에 있으면 성공
            var distanceToTarget = Vector3.SqrMagnitude(objectTransform.position - checkTarget.Value.transform.position);
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
            if (objectTransform == null) return;

            // 시야각을 그리기 위한 Handles 설정
            Handles.color = checkTarget.Value ? new Color(0, 1, 0, 0.2f) : new Color(1, 1, 0, 0.2f); // 노란색 반투명

            // 원호를 채워진 형태로 그리기
            Vector3 forward = objectTransform.forward * checkRange.Value;
            Vector3 startDirection = Quaternion.Euler(0, -viewAngle / 2f, 0) * forward;
            Handles.DrawSolidArc(objectTransform.position, Vector3.up, startDirection, viewAngle, checkRange.Value);

            // 중심선 그리기
            Handles.color = Color.red;
            Handles.DrawLine(objectTransform.position, objectTransform.position + objectTransform.forward * checkRange.Value);
        }
#endif
    }
}