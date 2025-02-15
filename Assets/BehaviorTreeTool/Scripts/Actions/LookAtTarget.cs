using UnityEngine;

namespace Tree
{
    public class LookAtTarget : ActionNode
    {
        public SharedCollider target;

        [SerializeField] private int rotationSpeed = 1;
        [SerializeField] private float rotationThreshold = 10f; // 회전 완료로 간주할 각도 차이

        protected override TaskState OnUpdate()
        {
            if (!target.Value) return TaskState.Failure;


            var direction = target.Value.transform.position - objectTransform.position;
            var lookDirection = new Vector3(direction.x, 0, direction.z);

            // lookDirection이 Zero가 아닐 때만 회전
            if (lookDirection != Vector3.zero)
            {
                var lookRotation = Quaternion.LookRotation(lookDirection);
                objectTransform.rotation =
                    Quaternion.Slerp(objectTransform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }

            // 현재 회전과 목표 회전 사이의 각도 차이 계산
            var angle = Vector3.Angle(new Vector3(objectTransform.forward.x, 0, objectTransform.forward.z),
                lookDirection);
            if (angle > rotationThreshold)
            {
                return TaskState.Running;
            }

            return TaskState.Success;
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (objectTransform == null || !target.Value) return;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(objectTransform.position, target.Value.transform.position);
        }
#endif
    }
}