using UnityEngine;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class CheckInRange : ConditionNode
    {
        public SharedCollider target;
        public SharedInt checkRange;

        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private Collider[] targetColliders;

        protected override TaskState OnUpdate()
        {
            int targetCount = Physics.OverlapSphereNonAlloc(nodeTransform.position, checkRange.Value, targetColliders, targetLayer);
            if (targetCount > 0)
            {

                float closestDistance = Mathf.Infinity;
                Collider closestTarget = null;

                for (int i = 0; i < targetCount; i++)
                {
                    float distance = Vector3.SqrMagnitude(nodeTransform.position - targetColliders[i].transform.position);
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
            if (nodeTransform == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(nodeTransform.position, checkRange.Value);
        }
#endif
    }
}