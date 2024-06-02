using UnityEngine.AI;

public class CheckDistance : ConditionNode
{
    private NavMeshAgent _agent;

    protected override void OnAwake()
    {
        _agent = nodeTransform.GetComponent<NavMeshAgent>();
    }

    protected override TaskState OnUpdate()
    {
        if (_agent.pathPending) return TaskState.Running;
        if (_agent.remainingDistance < _agent.stoppingDistance) return TaskState.Success;
        if (_agent.pathStatus == NavMeshPathStatus.PathInvalid) return TaskState.Failure;

        return TaskState.Running;
    }
}