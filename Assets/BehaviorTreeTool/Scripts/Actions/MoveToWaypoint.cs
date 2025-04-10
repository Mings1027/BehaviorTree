using UnityEngine.AI;

namespace Tree
{
    [NodeCategory("NavMeshAgent")]
    public class MoveToWaypoint : ActionNode
    {
        public SharedTransformArray wayPoints;

        private NavMeshAgent _agent;
        private int _wayIndex;

        protected override void OnAwake()
        {
            _agent = objectTransform.GetComponent<NavMeshAgent>();
            _wayIndex = 0;
        }

        protected override void OnStart()
        {
            _agent.destination = wayPoints.Value[_wayIndex].position;
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