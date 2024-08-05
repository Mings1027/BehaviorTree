using UnityEngine;

namespace Tree
{
    public class MoveToRandomPoint : ActionNode
    {
        public SharedVector3 curRandomPoint;
        public SharedFloat remainingDistance;
        public SharedFloat closeDistance;

        [SerializeField] private float moveSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float rotationThreshold;

        protected override TaskState OnUpdate()
        {
            remainingDistance.Value = Vector3.Distance(curRandomPoint.Value, nodeTransform.position);
            if (remainingDistance.Value < closeDistance.Value)
            {
                return TaskState.Success;
            }

            var direction = curRandomPoint.Value - nodeTransform.position;
            var pos = Vector3.MoveTowards(nodeTransform.position, curRandomPoint.Value, moveSpeed * Time.deltaTime);
            var lookDirection = new Vector3(direction.x, 0, direction.z);
            if (lookDirection != Vector3.zero)
            {
                var rot = Quaternion.LookRotation(direction);
                nodeTransform.SetPositionAndRotation(pos, rot);
            }

            return TaskState.Running;
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (nodeTransform == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(curRandomPoint.Value, 0.2f);
        }
#endif
    }
}