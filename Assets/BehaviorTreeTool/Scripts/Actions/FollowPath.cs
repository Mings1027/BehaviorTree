using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Actions
{
    public class FollowPath : ActionNode
    {
        public SharedVector3 curWayPoint;

        [SerializeField] private float moveSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float rotationThreshold;

        protected override TaskState OnUpdate()
        {
            nodeTransform.position = Vector3.MoveTowards(nodeTransform.position, curWayPoint.Value, moveSpeed * Time.deltaTime);

            var direction = curWayPoint.Value - nodeTransform.position;
            var lookDirection = new Vector3(direction.x, 0, direction.z);

            // lookDirection이 Zero가 아닐 때만 회전
            if (lookDirection != Vector3.zero)
            {
                var lookRotation = Quaternion.LookRotation(lookDirection);
                nodeTransform.rotation = Quaternion.Slerp(nodeTransform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }

            // 현재 회전과 목표 회전 사이의 각도 차이 계산
            float angle = Vector3.Angle(nodeTransform.forward, lookDirection);
            if (angle > rotationThreshold)
            {
                return TaskState.Running;
            }

            return TaskState.Success;
        }

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (nodeTransform == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(curWayPoint.Value, 0.2f);
        }
#endif
    }
}
