using UnityEngine;

namespace Tree
{
    public class CheckInRange : ConditionNode
    {
        public SharedCollider target;
        public SharedInt checkRange;

        [SerializeField] private LayerMask targetLayer;

        private Collider[] _colliders;
        private float _nextSearchTime;
        private const float SearchInterval = 0.2f; // 초당 5번만 검색

        protected override void OnAwake()
        {
            base.OnAwake();
            _colliders = new Collider[10];
        }

        protected override TaskState OnUpdate()
        {
            // 일정 간격으로만 검색 수행
            if (Time.time < _nextSearchTime)
                return target.Value != null ? TaskState.Success : TaskState.Failure;

            _nextSearchTime = Time.time + SearchInterval;
            var (closestTarget, found) = FindClosestTarget();
            if (found)
            {
                target.Value = closestTarget;
                return TaskState.Success;
            }

            target.Value = null;
            return TaskState.Failure;
        }

        private (Collider, bool) FindClosestTarget()
        {
            var size = Physics.OverlapSphereNonAlloc(objectTransform.position, checkRange.Value, _colliders, targetLayer);
            if (size == 0) return (null, false);
            Collider closest = null;
            var minDistance = float.MaxValue;
            for (int i = 0; i < size; i++)
            {
                var distance = Vector3.Distance(_colliders[i].transform.position, objectTransform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = _colliders[i];
                }
            }

            return (closest, true);
        }
#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (objectTransform == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(objectTransform.position, checkRange.Value);
        }
#endif
    }
}