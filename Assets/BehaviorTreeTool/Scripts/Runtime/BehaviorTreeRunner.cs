using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class BehaviorTreeRunner : MonoBehaviour, IBehaviorTree
{
#if UNITY_EDITOR
    public BehaviorTree Tree => behaviorTree;
    public string Name => name;
#endif
    [SerializeField] protected BehaviorTree behaviorTree;

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

    /// <summary>
    /// Assign shared variables to nodes in the tree.
    /// </summary>
    private void AssignSharedVariables()
    {
        var nodes = behaviorTree.Nodes;
        var sharedVariablesTable = new List<(Node Node, SharedVariableBase SharedVariable)>();
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

                var sharedDataVariable = BehaviorTreeManager.GetSharedVariable(node.SharedData.Variables, sharedVariable.VariableName);

                field.SetValue(node, sharedDataVariable);

                if (variableNameSet.Add(sharedVariable.VariableName))
                {
                    sharedVariablesTable.Add((node, sharedDataVariable));
                }
            }
        }

        for (var i = 0; i < sharedVariablesTable.Count; i++)
        {
            var (node, sharedVariable) = sharedVariablesTable[i];
            var value = sharedVariable.GetValue();
            if (sharedVariable is IComponentObject { UseGetComponent: true } && value is Component)
            {
                var componentType = value.GetType();
                if (node.NodeTransform.TryGetComponent(componentType, out var component))
                {
                    sharedVariable.SetValue(component);
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