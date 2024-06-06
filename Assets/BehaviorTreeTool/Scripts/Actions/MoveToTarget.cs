namespace BehaviorTreeTool.Scripts.Actions
{
    public class MoveToTarget : ActionNode
    {
        public SharedCollider target;
        public SharedNavMeshAgent navMeshAgent;

        protected override void OnStart()
        {
            if (target.Value)
            {
                navMeshAgent.Value.destination = target.Value.transform.position;
            }
        }

        protected override TaskState OnUpdate()
        {
            if (target.Value)
            {
                navMeshAgent.Value.destination = target.Value.transform.position;
                return TaskState.Success;
            }

            return TaskState.Failure;
        }
    }
}