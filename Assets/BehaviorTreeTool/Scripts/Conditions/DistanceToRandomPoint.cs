using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Conditions
{
    public class DistanceToRandomPoint : ConditionNode
    {
        public SharedVector3 curWayPoint;

        [SerializeField] private float remainingDistance = 1.0f;

        protected override TaskState OnUpdate()
        {
            if (Vector3.Distance(curWayPoint.Value, nodeTransform.position) <= remainingDistance)
            {
                return TaskState.Success;
            }

            return TaskState.Failure;
        }
    }
}