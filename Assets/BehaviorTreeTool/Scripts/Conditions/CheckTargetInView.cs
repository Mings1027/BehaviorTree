using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tree
{
    public class CheckTargetInView : ConditionNode
    {
        public SharedCollider target;
        
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private int checkRange;
        [SerializeField] private int viewAngle;

        private Collider[] _colliders;
        private float _nextSearchTime;
        private const float SearchInterval = 0.2f;

        protected override void OnAwake()
        {
            base.OnAwake();
            _colliders = new Collider[10];
        }

        protected override TaskState OnUpdate()
        {
            if (Time.time < _nextSearchTime)
                return target.Value != null ? TaskState.Success : TaskState.Failure;

            _nextSearchTime = Time.time + SearchInterval;
            var (closestTarget, found) = FindClosestTargetInView();
            if (found)
            {
                target.Value = closestTarget;
                return TaskState.Success;
            }

            target.Value = null;
            return TaskState.Failure;
        }

        private (Collider, bool) FindClosestTargetInView()
        {
            var size = Physics.OverlapSphereNonAlloc(objectTransform.position, checkRange, _colliders, targetLayer);
            if (size == 0) return (null, false);

            Collider closest = null;
            var minDistance = float.MaxValue;

            for (int i = 0; i < size; i++)
            {
                var directionToTarget = _colliders[i].transform.position - objectTransform.position;
                var angle = Vector3.Angle(objectTransform.forward, directionToTarget);
                
                // 시야각 안에 있는 대상만 고려
                if (angle <= viewAngle / 2f)
                {
                    var distance = directionToTarget.sqrMagnitude;
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closest = _colliders[i];
                    }
                }
            }

            return (closest, closest != null);
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            // 범위 표시
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(objectTransform.position, checkRange);

            // 시야각 표시
            Handles.color = target.Value ? new Color(0, 1, 0, 0.2f) : new Color(1, 1, 0, 0.2f);
            Vector3 forward = objectTransform.forward * checkRange;
            Vector3 startDirection = Quaternion.Euler(0, -viewAngle / 2f, 0) * forward;
            Handles.DrawSolidArc(objectTransform.position, Vector3.up, startDirection, viewAngle, checkRange);
        }
#endif
    }
}