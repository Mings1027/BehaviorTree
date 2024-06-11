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
        Assign();
        // behaviorTree.AssignSharedVariables(behaviorTree.Nodes);
        // SyncVariableValues();
        BehaviorTree.Traverse(behaviorTree.RootNode, n => n.Init());
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

    private void Assign()
    {
        var variableList = new List<(Node Node, SharedVariableBase sharedVariable)>();
        var nodes = behaviorTree.Nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var allFields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int j = 0; j < allFields.Length; j++)
            {
                var field = allFields[j];
                if (!typeof(SharedVariableBase).IsAssignableFrom(field.FieldType)) continue;
                var sharedVariable = (SharedVariableBase)field.GetValue(node);
                field.SetValue(node, GetSharedVariable(sharedVariable));
                variableList.Add((node, sharedVariable));
            }
        }
        var v = behaviorTree.RootNode.SharedData.Variables;
        for (int i = 0; i < v.Count; i++)
        {
            if (v[i] is IComponentObject { UseGetComponent: true })
            {
                for (int j = 0; j < variableList.Count; j++)
                {
                    if (v[i].VariableName == variableList[j].sharedVariable.VariableName)
                    {
                        var value = v[i].GetValue();
                        var componentType = value.GetType();
                        if (variableList[j].Node.NodeTransform.TryGetComponent(componentType, out var component))
                        {
                            variableList[j].sharedVariable.SetValue(component);
                        }
                    }
                }
            }
        }
    }

    private SharedVariableBase GetSharedVariable(SharedVariableBase sharedVariable)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            if (sharedVariable.VariableName == variables[i].VariableName)
                return variables[i];
        }
        return null;
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