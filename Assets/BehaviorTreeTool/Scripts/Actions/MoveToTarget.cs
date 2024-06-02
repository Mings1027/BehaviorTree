using Pathfinding;
using UnityEngine.AI;

public class MoveToTarget : ActionNode
{
    public SharedCollider target;

    private NavMeshAgent _navMeshAgent;

    protected override void OnAwake()
    {
        _navMeshAgent = nodeTransform.GetComponent<NavMeshAgent>();
    }

    protected override void OnStart()
    {
        _navMeshAgent.destination = target.Value.transform.position;
    }

    protected override TaskState OnUpdate()
    {
        return TaskState.Success;
    }
}