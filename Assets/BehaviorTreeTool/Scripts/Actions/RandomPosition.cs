using UnityEngine;
using UnityEngine.AI;

public class RandomPosition : ActionNode
{
    private NavMeshAgent _agent;

    protected override void OnAwake()
    {
        _agent = nodeTransform.GetComponent<NavMeshAgent>();
    }

    protected override void OnStart()
    {
        _agent.destination = new Vector3(Random.Range(-50, 50), 0, Random.Range(-50, 50));
    }

    protected override TaskState OnUpdate()
    {
        return TaskState.Success;
    }
}