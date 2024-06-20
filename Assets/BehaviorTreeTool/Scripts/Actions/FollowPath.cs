using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Actions
{
    public class FollowPath : ActionNode
    {
        public SharedVector3 curWayPoint;

        [SerializeField] private float moveSpeed;

        protected override TaskState OnUpdate()
        {
            nodeTransform.position = Vector3.MoveTowards(nodeTransform.position, curWayPoint.Value, moveSpeed * Time.deltaTime);
            return TaskState.Success;
        }
    }
}
