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

        protected override TaskState OnUpdate()
        {
            remainingDistance.Value = Vector3.Distance(curRandomPoint.Value, objectTransform.position);
            if (remainingDistance.Value < closeDistance.Value)
            {
                return TaskState.Success;
            }

            var direction = curRandomPoint.Value - objectTransform.position;
            var pos = Vector3.MoveTowards(objectTransform.position, curRandomPoint.Value, moveSpeed * Time.deltaTime);
            var lookDirection = new Vector3(direction.x, 0, direction.z);
            if (lookDirection != Vector3.zero)
            {
                var targetRotation = Quaternion.LookRotation(direction);
                var newRotation = Quaternion.RotateTowards(objectTransform.rotation, targetRotation,
                    rotationSpeed * Time.deltaTime);
                objectTransform.SetPositionAndRotation(pos, newRotation);
            }

            return TaskState.Running;
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (objectTransform == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(curRandomPoint.Value, 0.2f);
        }
#endif
    }
}