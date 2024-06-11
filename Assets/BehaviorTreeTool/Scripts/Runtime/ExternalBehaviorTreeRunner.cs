using System.Collections.Generic;
using UnityEngine;

public class ExternalBehaviorTreeRunner : MonoBehaviour, IBehaviorTree
{
#if UNITY_EDITOR
    public ExternalBehaviorTree ExternalBehaviorTree
    {
        get => behaviorTree;
        set
        {
            behaviorTree = value;
            if (behaviorTree == null)
            {
                variables.Clear();
            }
            else
            {
                UpdateVariables();
            }
        }
    }

    public BehaviorTree Tree
    {
        get => behaviorTree;
        set => behaviorTree = (ExternalBehaviorTree)value;
    }

#endif
    [SerializeField] private ExternalBehaviorTree behaviorTree;

    [SerializeReference] private List<SharedVariableBase> variables;

    private void OnEnable()
    {
        BehaviorTreeManager.AddTree(this);
    }

    private void Start()
    {
        InitializeTree();
    }

    private void OnDisable()
    {
        BehaviorTreeManager.RemoveTree(this);
    }

    private void InitializeTree()
    {
        var clonedTree = (ExternalBehaviorTree)behaviorTree.Clone(transform);
        behaviorTree = clonedTree;
        SyncVariableValues();
    }

    public void TreeUpdate()
    {
        behaviorTree.TreeUpdate();
    }

    private void UpdateVariables()
    {
        variables.Clear();
        if (behaviorTree?.RootNode?.SharedData?.Variables != null)
        {
            variables = new List<SharedVariableBase>();
            foreach (var variable in behaviorTree.RootNode.SharedData.Variables)
            {
                variables.Add(variable.Clone());
            }
        }
    }

    private void SyncVariableValues()
    {
        for (var i = 0; i < variables.Count; i++)
        {
            var variable = variables[i];
            var sharedVariable = behaviorTree.RootNode.SharedData.Variables[i];

            if (variable is not IComponentObject { UseGetComponent: true })
            {
                sharedVariable.SetValue(variable.GetValue());
            }
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