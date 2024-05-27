using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;

public class ReayToAttack : ConditionNode
{
    public SharedCollider target;

    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (target.Value && target.Value.enabled)
        {
            return State.Success;
        }

        return State.Failure;
    }
}