using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class DistanceToWaypoint : ConditionNode
    {
        public SharedVector3 curWayPoint;

        private int curIndex;

        [SerializeField] private Vector3[] waypoints;
        [SerializeField] private float remainingDistance;

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
                return TaskState.Success;
            }

            return TaskState.Failure;
        }
    }
}