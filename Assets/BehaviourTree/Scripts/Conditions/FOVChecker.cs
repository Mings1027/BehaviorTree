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
    public class FOVChecker : ConditionNode
    {
        public SharedCollider target;
        public SharedFloat attackRange;

        [SerializeField] private float viewAngle = 45f; // 시야 각도 설정

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            if (!target.Value || !target.Value.enabled) return State.Failure;

            if (Vector3.Distance(nodeTransform.position, target.Value.transform.position) <= attackRange.Value)
            {
                var directionToTarget = (target.Value.transform.position - nodeTransform.position).normalized;
                var angleToTarget = Vector3.Angle(nodeTransform.forward, directionToTarget);
                if (angleToTarget <= viewAngle)
                {
                    return State.Success;
                }
            }

            return State.Failure;
        }

        public override void OnDrawGizmos()
        {
            if (nodeTransform == null) return;

            // 시야각 표시
            Handles.color = new Color(0, 0, 1, 0.2f); // 파란색, 투명도 0.2
            Vector3 forward = nodeTransform.forward * attackRange.Value;
            Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle, 0) * forward;

            Handles.DrawSolidArc(nodeTransform.position, Vector3.up, leftBoundary, viewAngle * 2, attackRange.Value);
        }
    }
}