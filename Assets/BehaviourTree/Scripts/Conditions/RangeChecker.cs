using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using UnityEditor;
using UnityEngine;

namespace BehaviourTree.Scripts.Conditions
{
    public class RangeChecker : ConditionNode
    {
        public SharedCollider target;
        [SerializeField] private int detectRange;
        [SerializeField] private int viewAngle = 45;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private Collider[] targetColliders;
        
        private float _curRange;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            if (target.Value && target.Value.enabled) return State.Success;

            _curRange = 0f;
            while (_curRange <= detectRange)
            {
                var size = Physics.OverlapSphereNonAlloc(nodeTransform.position, _curRange, targetColliders,
                    targetLayer);
                if (size > 0)
                {
                    var dirToTarget = (targetColliders[0].transform.position - nodeTransform.position).normalized;
                    var angleToTarget = Vector3.Angle(nodeTransform.forward, dirToTarget);
                    if (angleToTarget <= viewAngle)
                    {
                        target.Value = targetColliders[0];
                        return State.Success;
                    }
                }

                _curRange += Time.deltaTime;
            }

            target.Value = null;
            return State.Failure;
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (nodeTransform == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(nodeTransform.position, _curRange);
            Handles.color = new Color(0, 0, 1, 0.2f);
            var forward = nodeTransform.forward * detectRange;
            var leftBoundary = Quaternion.Euler(0, -viewAngle, 0) * forward;
            Handles.DrawSolidArc(nodeTransform.position, Vector3.up, leftBoundary, viewAngle * 2, detectRange);
        }
#endif
    }
}