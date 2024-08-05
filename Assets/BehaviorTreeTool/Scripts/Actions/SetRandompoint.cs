using UnityEngine;
using Tree;

namespace Tree
{
    public class SetRandompoint : ActionNode
    {
        public SharedVector3 curRandomPoint;

        private Vector3 centerPosition;

        [SerializeField] private float radius;

        protected override void OnAwake()
        {
            centerPosition = nodeTransform.position;
        }

        protected override void OnStart()
        {
            SetRandompointInRange();
        }

        protected override TaskState OnUpdate()
        {
            return TaskState.Failure;
        }

        private void SetRandompointInRange()
        {
            var randomPoint = centerPosition + new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
            curRandomPoint.Value = randomPoint;
        }

        public override void OnDrawGizmos()
        {
            if (nodeTransform == null) return;
            Gizmos.DrawWireSphere(nodeTransform.position, radius);
        }
    }
}