using UnityEngine;

namespace Tree
{
    public class CheckInRange : ConditionNode
    {
        public SharedCollider target;
        public SharedInt checkRange;

        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private Collider[] targetColliders;

        protected override TaskState OnUpdate()
        {
            var targetCount = Physics.OverlapSphereNonAlloc(nodeTransform.position, checkRange.Value, targetColliders,
                targetLayer);
            if (targetCount > 0)
            {
                var closestDistance = Mathf.Infinity;
                Collider closestTarget = null;

                for (var i = 0; i < targetCount; i++)
                {
                    var distance = Vector3.SqrMagnitude(nodeTransform.position - targetColliders[i].transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = targetColliders[i];
                    }
                }

                target.Value = closestTarget;
                return TaskState.Success;
            }

            target.Value = null;
            return TaskState.Failure;
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(nodeTransform.position, checkRange.Value);
        }
#endif
    }
}