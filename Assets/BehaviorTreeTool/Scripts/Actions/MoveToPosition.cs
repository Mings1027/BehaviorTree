namespace BehaviorTreeTool.Scripts.Actions
{
    public class MoveToPosition : ActionNode
    {
        public SharedTransform target;
        public SharedNavMeshAgent agent;

        protected override void OnStart()
        {
            agent.Value.destination = target.Value.position;
        }
    }
}