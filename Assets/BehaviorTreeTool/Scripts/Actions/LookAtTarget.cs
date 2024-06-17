using BehaviorTreeTool.Scripts.Runtime;
using Unity.Mathematics;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Actions
{
    public class LookAtTarget : ActionNode
    {
        public SharedCollider target;
        public SharedNavMeshAgent agent;

        [SerializeField] private int rotationSpeed;
        [SerializeField] private float rotationThreshold = 0.1f; // 회전 완료로 간주할 각도 차이

        protected override TaskState OnUpdate()
        {
            var directionToTarget = target.Value.transform.position - nodeTransform.position;
            var targetRotation = Quaternion.LookRotation(directionToTarget);
            nodeTransform.rotation = Quaternion.Slerp(nodeTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // 현재 회전과 목표 회전 사이의 각도 차이 계산
            float angleDifference = Quaternion.Angle(nodeTransform.rotation, targetRotation);
            if (angleDifference < rotationThreshold)
            {
                agent.Value.isStopped = false;
                return TaskState.Success;
            }

            return TaskState.Running;
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