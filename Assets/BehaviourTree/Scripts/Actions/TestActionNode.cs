using UnityEngine;
using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;

public class TestActionNode : ActionNode
{
    public SharedQuaternion sharedQuaternion;
    
    protected override void OnStart()
    {
        nodeTransform.Rotate(sharedQuaternion.Value.eulerAngles);
    }

    protected override void OnStop() 
    {
    }

    protected override State OnUpdate() 
    {
        return State.Success;
    }
}
