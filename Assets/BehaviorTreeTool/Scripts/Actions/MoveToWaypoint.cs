using BehaviorTreeTool.Scripts.Runtime;

namespace BehaviorTreeTool.Scripts.Actions
{
    public class MoveToWaypoint : ActionNode
    {
        public SharedNavMeshAgent agent;
        public SharedTransformArray wayPoints;

        private int _wayIndex;

        protected override void OnAwake()
        {
            _wayIndex = 0;
        }

        protected override void OnStart()
        {
            agent.Value.destination = wayPoints.Value[_wayIndex].position;
            _wayIndex++;
            if (wayPoints.Value.Length <= _wayIndex)
            {
                _wayIndex = 0;
            }
        }

        protected override TaskState OnUpdate()
        {
            return TaskState.Success;
        }
    }
}