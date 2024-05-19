using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourTree.Scripts.Conditions
{
    /// <summary>
    /// Use this node class when dealing with the field of view.
    /// </summary>
    public class WithInSight : ConditionNode
    {
        public SharedCollider target;

        [SerializeField] private int attackRange;
        [SerializeField] private float viewAngle = 45f; // 시야 각도 설정

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            if (!target.Value) return State.Failure;
Debug.Log("target");
            var distanceToTargetSq = (target.Value.transform.position - nodeTransform.position).sqrMagnitude;
            float attackRangeSq = attackRange * attackRange;

            // 타겟이 공격 범위 내에 있는지 확인
            if (distanceToTargetSq <= attackRangeSq)
            {
                Debug.Log("공격 범위안");
                var directionToTarget = (target.Value.transform.position - nodeTransform.position).normalized;
                var angleToTarget = Vector3.Angle(nodeTransform.forward, directionToTarget);
                Debug.Log(angleToTarget);
                // 타겟이 시야각 내에 있는지 확인
                if (angleToTarget <= viewAngle)
                {
                    Debug.Log("시야각 안");
                    // Raycast로 타겟과의 라인에 장애물이 없는지 확인
                    if (Physics.Raycast(nodeTransform.position, directionToTarget, out var hit, attackRange))
                    {
                        if (hit.collider == target.Value)
                        {
                            return State.Success;
                        }
                    }
                }
            }

            return State.Failure;
        }

        public override void OnDrawGizmos()
        {
            if (nodeTransform == null) return;

            // 시야각 표시
            Handles.color = new Color(0, 0, 1, 0.2f); // 파란색, 투명도 0.2
            Vector3 forward = nodeTransform.forward * attackRange;
            Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle, 0) * forward;

            Handles.DrawSolidArc(nodeTransform.position, Vector3.up, leftBoundary, viewAngle * 2, attackRange);

            // 타겟과의 거리 표시
            Handles.color = Color.red;
            Handles.DrawLine(nodeTransform.position, nodeTransform.position + forward);
        }
    }
}