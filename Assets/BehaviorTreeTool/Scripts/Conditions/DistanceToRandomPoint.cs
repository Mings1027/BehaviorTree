using UnityEngine;

namespace Tree
{
    public class DistanceToRandomPoint : ConditionNode
    {
        public SharedVector3 curRandomPoint;
        public SharedFloat remainingDistance;
        public SharedFloat stoppingDistance;

        protected override void OnAwake()
        {
            curRandomPoint.Value = objectTransform.position;
        }

        protected override TaskState OnUpdate()
        {
            remainingDistance.Value = Vector3.Distance(curRandomPoint.Value, objectTransform.position);

            if (remainingDistance.Value <= stoppingDistance.Value)
            {
                return TaskState.Success;
            }

            return TaskState.Failure;
        }
    }
}