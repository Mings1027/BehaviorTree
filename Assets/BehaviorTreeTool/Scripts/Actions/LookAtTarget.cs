using BehaviorTreeTool.Scripts.Runtime;
using Unity.Mathematics;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Actions
{
    public class LookAtTarget : ActionNode
    {
        public SharedCollider target;

        [SerializeField] private int rotationSpeed = 1;
        [SerializeField] private float rotationThreshold = 10f; // 회전 완료로 간주할 각도 차이

        protected override TaskState OnUpdate()
        {
            if (!target.Value) return TaskState.Failure;

            var direction = target.Value.transform.position - nodeTransform.position;
            var lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            nodeTransform.rotation = Quaternion.Slerp(nodeTransform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

            // 현재 회전과 목표 회전 사이의 각도 차이 계산
            float angle = Vector3.Angle(nodeTransform.forward, direction);
            if (angle > rotationThreshold)
            {
                return TaskState.Running;
            }

            return TaskState.Success;
        }


#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (nodeTransform == null || !target.Value) return;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(nodeTransform.position, target.Value.transform.position);
        }
#endif
    }
}