using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BehaviorTreeManager : MonoBehaviour
{
    private static BehaviorTreeManager _instance;
    private List<IBehaviorTree> _behaviorTree;

    private void Awake()
    {
        _instance = this;
        _behaviorTree = new List<IBehaviorTree>();
    }

    private void Update()
    {
        for (int i = 0; i < _behaviorTree.Count; i++)
        {
            _behaviorTree[i].TreeUpdate();
        }
    }

    public static void AddTree(IBehaviorTree behaviorTree)
    {
        if (_instance._behaviorTree.Contains(behaviorTree)) return;
        _instance._behaviorTree.Add(behaviorTree);
    }

    public static void RemoveTree(IBehaviorTree behaviorTree)
    {
        if (_instance._behaviorTree.Contains(behaviorTree))
            _instance._behaviorTree.Remove(behaviorTree);
    }

    public static SharedVariableBase GetSharedVariable(List<SharedVariableBase> variables, string variableName)
    {
        if (variables == null || variables.Count == 0) return null;
        for (var i = 0; i < variables.Count; i++)
        {
            var sharedVariable = variables[i];
            if (sharedVariable.VariableName == variableName)
            {
                return sharedVariable;
            }
        }

        return null;
    }

    [Conditional("UNITY_EDITOR")]
    public void ToggleDrawGizmos(bool enable)
    {
        if (_behaviorTree == null || _behaviorTree.Count == 0) return;
        for (int i = 0; i < _behaviorTree.Count; i++)
        {
            IBehaviorTree tree = _behaviorTree[i];
            if (tree?.Tree?.RootNode != null)
            {
                BehaviorTree.Traverse(tree.Tree.RootNode, node => { node.drawGizmos = enable; });
            }
        }
    }
}