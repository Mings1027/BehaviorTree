using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;
namespace BehaviorTreeTool.Scripts.Actions
{
    public class SetRandompoint : ActionNode
    {
        public SharedVector3 curWayPoint;

        [SerializeField] private Vector3 center;
        [SerializeField] private float radius;

        protected override void OnStart()
        {
            SetRandompointInRange();
        }

        protected override TaskState OnUpdate()
        {
            return TaskState.Success;
        }

        private void SetRandompointInRange()
        {
            var randomPoint = center + new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
            curWayPoint.Value = randomPoint;
        }

        public override void OnDrawGizmos()
        {
            if (nodeTransform == null) return;
            Gizmos.DrawWireSphere(center, radius);
        }
    }
}