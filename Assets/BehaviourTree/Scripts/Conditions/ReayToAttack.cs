using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeData;
using Pathfinding;
using UnityEngine;

public class ReayToAttack : ConditionNode
{
    public SharedCollider target;
    
    [SerializeField] private float attackRange;

    protected override void OnAwake()
    {
        nodeTransform.GetComponent<AIPath>().endReachedDistance = attackRange;
    }

    protected override TaskState OnUpdate()
    {
        if (target.Value && target.Value.enabled)
        {
            return TaskState.Success;
        }

        return TaskState.Failure;
    }
}