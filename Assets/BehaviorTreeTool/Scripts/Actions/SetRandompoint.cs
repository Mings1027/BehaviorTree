using UnityEngine;
using Tree;

namespace Tree
{
    public class SetRandomPoint : ActionNode
    {
        public SharedVector3 curRandomPoint;

        private Vector3 _centerPosition;

        [SerializeField] private float radius;

        protected override void OnAwake()
        {
            _centerPosition = nodeTransform.position;
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
            var randomPoint = _centerPosition +
                              new Vector3(Random.Range(-radius, radius), 0, Random.Range(-radius, radius));
            curRandomPoint.Value = randomPoint;
        }
#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            if (nodeTransform == null) return;
            Gizmos.DrawWireSphere(nodeTransform.position, radius);
        }
#endif
    }
}