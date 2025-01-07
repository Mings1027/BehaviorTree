using UnityEngine;

namespace Tree
{
    public class CheckInRange : ConditionNode
    {
        public SharedCollider target;
        public SharedInt checkRange;

        [SerializeField] private LayerMask targetLayer;

        private float nextSearchTime;
        private const float SEARCH_INTERVAL = 0.2f; // 초당 5번만 검색

        protected override TaskState OnUpdate()
        {
            // 일정 간격으로만 검색 수행
            if (Time.time < nextSearchTime)
                return target.Value != null ? TaskState.Success : TaskState.Failure;

            nextSearchTime = Time.time + SEARCH_INTERVAL;
            var closestTarget = GridManager.FindNearestTarget(nodeTransform.position, targetLayer);
            if (closestTarget == null)
            {
                target.Value = null;
                return TaskState.Failure;
            }

            if (closestTarget.TryGetComponent(out Collider collider))
            {
                target.Value = collider;
            }

            return TaskState.Success;

        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (nodeTransform == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(nodeTransform.position, checkRange.Value);
        }
#endif
    }
}