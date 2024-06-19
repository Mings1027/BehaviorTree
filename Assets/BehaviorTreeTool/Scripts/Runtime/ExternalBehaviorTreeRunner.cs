using System.Collections.Generic;
using System.Reflection;
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

    public BehaviorTree Tree => behaviorTree;
    public string Name => name;
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
        AssignSharedVariables();
        BehaviorTree.Traverse(behaviorTree.RootNode, n => n.Init());
    }

    public void TreeUpdate()
    {
        behaviorTree.TreeUpdate();
    }

    private void UpdateVariables()
    {
        variables ??= new List<SharedVariableBase>();
        variables.Clear();

        variables = new List<SharedVariableBase>();
        for (int i = 0; i < behaviorTree.RootNode.SharedData.Variables.Count; i++)
        {
            var variable = behaviorTree.RootNode.SharedData.Variables[i];
            variables.Add(variable.Clone());
        }
    }

    private void AssignSharedVariables()
    {
        var nodes = behaviorTree.Nodes;
        var sharedVariablesTable = new List<SharedVariableBase>();
        var variableNameSet = new HashSet<string>();

        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var allFields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (var j = 0; j < allFields.Length; j++)
            {
                var field = allFields[j];
                if (!typeof(SharedVariableBase).IsAssignableFrom(field.FieldType)) continue;

                var sharedVariable = (SharedVariableBase)field.GetValue(node);
                if (sharedVariable == null || string.IsNullOrEmpty(sharedVariable.VariableName)) continue;

                var variableFromVariables =
                    BehaviorTreeManager.GetSharedVariable(variables, sharedVariable.VariableName);

                field.SetValue(node, variableFromVariables);

                if (variableNameSet.Add(sharedVariable.VariableName))
                {
                    sharedVariablesTable.Add(variableFromVariables);
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
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