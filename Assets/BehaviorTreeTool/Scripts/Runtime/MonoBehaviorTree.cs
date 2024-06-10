using System.Collections.Generic;
using UnityEngine;
using BehaviorTreeTool.Scripts.CustomInterface;

public class MonoBehaviorTree : MonoBehaviour, IBehaviorTree
{
    public ExternalBehaviorTree ExternalBehaviorTree
    {
        get => behaviorTree;
        set
        {
            behaviorTree = value;
            UpdateVariables();
        }
    }

    [SerializeField] private ExternalBehaviorTree behaviorTree;

    [SerializeReference]
    private List<SharedVariableBase> variables;

    private void OnEnable()
    {
        BehaviorTreeManager.AddTree(this);
    }

    private void Start()
    {
        // 클론시 새로운 변수 리스트 전달
        var clonedVariables = new List<SharedVariableBase>();
        for (var i = 0; i < variables.Count; i++)
        {
            var variable = variables[i];
            clonedVariables.Add(variable.Clone());
        }

        var clonedTree = (ExternalBehaviorTree)behaviorTree.Clone(transform);
        behaviorTree = clonedTree;
        CopyVariablesToBehaviorTree();
    }

    private void OnDisable()
    {
        BehaviorTreeManager.RemoveTree(this);
    }

    public void TreeUpdate()
    {
        behaviorTree.TreeUpdate();
    }

    private void UpdateVariables()
    {
        if (behaviorTree?.RootNode?.SharedData?.Variables != null)
        {
            variables = new List<SharedVariableBase>();
            foreach (var variable in behaviorTree.RootNode.SharedData.Variables)
            {
                variables.Add(variable.Clone());
            }
        }
    }

    private void CopyVariablesToBehaviorTree()
    {
        behaviorTree.RootNode.SharedData.Variables.Clear();
     Debug.Log("=======================================");
        for (int i = 0; i < variables.Count; i++)
        {
            var clonedVariable = variables[i].Clone();
            behaviorTree.RootNode.SharedData.Variables.Add(clonedVariable);
            Debug.Log(behaviorTree.RootNode.SharedData.Variables[i].GetValue());
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!behaviorTree)
        {
            return;
        }

        BehaviorTree.Traverse(behaviorTree.RootNode, n =>
        {
            if (n.drawGizmos)
            {
                n.OnDrawGizmos();
            }
        });
    }
#endif
}