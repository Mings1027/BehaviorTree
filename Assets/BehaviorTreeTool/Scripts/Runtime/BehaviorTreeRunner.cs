using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class BehaviorTreeRunner : MonoBehaviour
{
#if UNITY_EDITOR
    public BehaviorTree Tree
    {
        get => behaviorTree;
        set
        {
            behaviorTree = value;
            UpdateVariables();
        }
    }

    public TreeType TreeType
    {
        get => treeType;
        set
        {
            treeType = value;
            UpdateVariables();
        }
    }
#endif

    [SerializeField] private TreeType treeType;
    [SerializeField] protected BehaviorTree behaviorTree;
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
        behaviorTree = behaviorTree.Clone(transform);
        AssignSharedVariables();
        BehaviorTree.Traverse(behaviorTree.RootNode, n => n.Init());
    }

    public void TreeUpdate()
    {
        behaviorTree.TreeUpdate();
    }

    private void UpdateVariables()
    {
        if (treeType == TreeType.BehaviorTree)
        {
            variables.Clear();
        }
        else if (treeType == TreeType.ExternalBehaviorTree)
        {
            variables ??= new List<SharedVariableBase>();
            variables.Clear();
            if (behaviorTree == null) return;
            for (int i = 0; i < behaviorTree.RootNode.SharedData.Variables.Count; i++)
            {
                var variable = behaviorTree.RootNode.SharedData.Variables[i];
                variables.Add(variable.Clone());
            }
        }
    }

    /// <summary>
    /// Assign shared variables to nodes in the tree.
    /// </summary>
    private void AssignSharedVariables()
    {
        var nodes = behaviorTree.Nodes;
        var sharedVariablesTable = new List<SharedVariableBase>();
        var variableNameSet = new HashSet<string>();
        var sharedVariables = treeType == TreeType.BehaviorTree ? behaviorTree.RootNode.SharedData.Variables : variables;
        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var allFields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (var j = 0; j < allFields.Length; j++)
            {
                var field = allFields[j];
                if (!typeof(SharedVariableBase).IsAssignableFrom(field.FieldType)) continue;

                var variable = (SharedVariableBase)field.GetValue(node);
                if (variable == null || string.IsNullOrEmpty(variable.VariableName)) continue;

                var sharedDataVariable = BehaviorTreeManager.GetSharedVariable(sharedVariables, variable.VariableName);

                field.SetValue(node, sharedDataVariable);

                if (variableNameSet.Add(variable.VariableName))
                {
                    sharedVariablesTable.Add(sharedDataVariable);
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
