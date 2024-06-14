using BehaviorTreeTool.Scripts.Runtime;
using Unity.Mathematics;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Actions
{
    public class LookAtTarget : ActionNode
    {
        public SharedCollider target;
        [SerializeField] private int rotationSpeed;

        protected override TaskState OnUpdate()
        {
            var directionToTarget = target.Value.transform.position - nodeTransform.position;
            var targetRotation = Quaternion.LookRotation(directionToTarget);
            nodeTransform.rotation =
                Quaternion.Slerp(nodeTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
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