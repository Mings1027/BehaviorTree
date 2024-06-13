using UnityEditor;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class RangeChecker : ConditionNode
    {
        public SharedCollider target;
        public SharedInt viewAngle;

        [SerializeField] private int detectRange;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private Collider[] targetColliders;

        private const float CheckInterval = 0.5f; // Check every 0.5 seconds
        private float _nextCheckTime;

        protected override void OnAwake()
        {
            _nextCheckTime = Time.time;
        }

        protected override TaskState OnUpdate()
        {
            if (Time.time < _nextCheckTime)
                return TaskState.Running;

            _nextCheckTime = Time.time + CheckInterval;

            // OverlapSphereNonAlloc 사용하여 콜라이더 찾기
            var size = Physics.OverlapSphereNonAlloc(nodeTransform.position, detectRange, targetColliders, targetLayer);
            if (size > 0)
            {
                Collider closestCollider = null;
                var closestDistance = float.MaxValue;

                for (var i = 0; i < size; i++)
                {
                    var collider = targetColliders[i];
                    var directionToTarget = (collider.transform.position - nodeTransform.position).normalized;
                    var angleToTarget = Vector3.Angle(nodeTransform.forward, directionToTarget);

                    if (angleToTarget <= viewAngle.Value)
                    {
                        var distance = Vector3.SqrMagnitude(nodeTransform.position - collider.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestCollider = collider;
                        }
                    }
                }

                if (closestCollider != null)
                {
                    target.Value = closestCollider;
                    return TaskState.Success;
                }
            }

            target.Value = null;
            return TaskState.Failure;
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (nodeTransform == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(nodeTransform.position, detectRange);
            Handles.color = new Color(0, 0, 1, 0.2f);
            var forward = nodeTransform.forward * detectRange;
            var leftBoundary = Quaternion.Euler(0, -viewAngle.Value, 0) * forward;
            Handles.DrawSolidArc(nodeTransform.position, Vector3.up, leftBoundary, viewAngle.Value * 2, detectRange);
        }
#endif
    }
}