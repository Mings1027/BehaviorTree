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

        private float _curRange;

        protected override TaskState OnUpdate()
        {
            if (target.Value && target.Value.enabled)
            {
                var distance = Vector3.Distance(target.Value.transform.position, nodeTransform.position);
                var dirToTarget = (target.Value.transform.position - nodeTransform.position).normalized;
                var angleToTarget = Vector3.Angle(nodeTransform.forward, dirToTarget);

                if (distance <= detectRange && angleToTarget <= viewAngle.Value)
                {
                    return TaskState.Success;
                }
            }

            _curRange = 0f;
            while (_curRange <= detectRange)
            {
                var size = Physics.OverlapSphereNonAlloc(nodeTransform.position, _curRange, targetColliders,
                    targetLayer);
                if (size > 0)
                {
                    var dirToTarget = (targetColliders[0].transform.position - nodeTransform.position).normalized;
                    var angleToTarget = Vector3.Angle(nodeTransform.forward, dirToTarget);
                    if (angleToTarget <= viewAngle.Value)
                    {
                        target.Value = targetColliders[0];
                        return TaskState.Success;
                    }
                }

                _curRange += Time.deltaTime;
            }

            target.Value = null;
            return TaskState.Failure;
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (nodeTransform == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(nodeTransform.position, _curRange);
            Handles.color = new Color(0, 0, 1, 0.2f);
            var forward = nodeTransform.forward * detectRange;
            var leftBoundary = Quaternion.Euler(0, -viewAngle.Value, 0) * forward;
            Handles.DrawSolidArc(nodeTransform.position, Vector3.up, leftBoundary, viewAngle.Value * 2, detectRange);
        }
#endif
    }
}