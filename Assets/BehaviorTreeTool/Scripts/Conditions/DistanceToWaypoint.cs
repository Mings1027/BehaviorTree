using UnityEngine;

namespace Tree
{
    public class DistanceToWaypoint : ConditionNode
    {
        public SharedVector3 curWayPoint;

        [SerializeField] private int curIndex;
        [SerializeField] private Vector3[] waypoints;
        [SerializeField] private float remainingDistance = 1.0f;

        protected override void OnAwake()
        {
            curIndex = 0;
            curWayPoint.Value = waypoints[curIndex];
        }

        protected override TaskState OnUpdate()
        {
            if (Vector3.Distance(curWayPoint.Value, nodeTransform.position) <= remainingDistance)
            {
                curIndex = (curIndex + 1) % waypoints.Length;
                curWayPoint.Value = waypoints[curIndex];
                return TaskState.Success;
            }

            return TaskState.Failure;
        }
    }
}