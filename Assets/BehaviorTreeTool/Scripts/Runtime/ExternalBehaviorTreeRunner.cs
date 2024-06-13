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

                var variableFromVariables = GetVariableFromVariables(sharedVariable.VariableName);

                field.SetValue(node, variableFromVariables);

                // 중복되지 않는 변수만 sharedVariablesTable에 추가
                if (variableNameSet.Add(sharedVariable.VariableName))
                {
                    sharedVariablesTable.Add(variableFromVariables);
                }
            }
        }

        for (int i = 0; i < sharedVariablesTable.Count; i++)
        {
            var sharedVariable = sharedVariablesTable[i];
            var value = sharedVariable.GetValue();
            if (sharedVariable is IComponentObject { UseGetComponent: true })
            {
                var componentType = value.GetType();
                if (transform.TryGetComponent(componentType, out var component))
                {
                    sharedVariable.SetValue(component);
                }
            }
        }

    }

    private SharedVariableBase GetVariableFromVariables(string variableName)
    {
        for (var i = 0; i < variables.Count; i++)
        {
            var variable = variables[i];
            if (variable.VariableName == variableName)
            {
                return variable;
            }
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