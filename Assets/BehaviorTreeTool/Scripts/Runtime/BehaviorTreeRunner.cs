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

    public bool EnableVariables
    {
        get => enableVariables;
        set
        {
            enableVariables = value;
            UpdateVariables();
        }
    }

    public bool DrawGizmos
    {
        get => drawGizmos;
        set
        {
            drawGizmos = value;
            SetDrawGismosForAllNodes(value);
        }
    }
    [SerializeField] private bool drawGizmos;
#endif
    [Tooltip("Enable if reference type variables need assignment before play.")]
    [SerializeField] private bool enableVariables;
    [SerializeField] protected BehaviorTree behaviorTree;
    [SerializeReference] private List<SharedVariableBase> variables = new();

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

    public void UpdateVariables()
    {
        if (!enableVariables)
        {
            variables.Clear();
            return;
        }

        if (behaviorTree == null || behaviorTree.RootNode.SharedData.Variables == null || behaviorTree.RootNode.SharedData.Variables.Count == 0)
        {
            variables.Clear();
            Debug.LogWarning("Behavior tree is null or Shared data has no variables");
            return;
        }

        // Ensure the variables list is initialized.
        variables.Clear();
        foreach (var variable in behaviorTree.RootNode.SharedData.Variables)
        {
            variables.Add(variable.Clone());
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
        var sharedVariables = enableVariables ? variables : behaviorTree.RootNode.SharedData.Variables;
        if (!enableVariables) variables = null;

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

    private void SetDrawGismosForAllNodes(bool value)
    {
        BehaviorTree.Traverse(behaviorTree.RootNode, n => { n.drawGizmos = value; });
    }

    private void OnDrawGizmos()
    {
        if (!behaviorTree) return;

        BehaviorTree.Traverse(behaviorTree.RootNode, n =>
        {
            if (n.NodeTransform == null)
            {
                Debug.LogError("Node transform is null");
                n.NodeTransform = transform;
            }

            if (n.drawGizmos)
            {
                n.OnDrawGizmos();
            }
        });
    }
#endif
}
