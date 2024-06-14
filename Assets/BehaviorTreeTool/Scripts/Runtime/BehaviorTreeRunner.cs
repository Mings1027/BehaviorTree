using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class BehaviorTreeRunner : MonoBehaviour, IBehaviorTree
{
#if UNITY_EDITOR
    public BehaviorTree Tree => tree;
#endif
    [SerializeField] protected BehaviorTree tree;

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
        var clonedTree = tree.Clone(transform);
        tree = clonedTree;
        AssignSharedVariables(tree.Nodes);
        BehaviorTree.Traverse(tree.RootNode, n => n.Init());
    }

    public void TreeUpdate()
    {
        tree.TreeUpdate();
    }

    private void AssignSharedVariables(List<Node> nodeList)
    {
        var sharedVariablesTable = new List<(Node Node, SharedVariableBase SharedVariable)>();
        var variableNameSet = new HashSet<string>();

        // 모든 노드에 대해 필드를 캐싱하고 필요한 작업을 한 번의 반복에서 수행
        for (var i = 0; i < nodeList.Count; i++)
        {
            var node = nodeList[i];
            var allFields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (var j = 0; j < allFields.Length; j++)
            {
                var field = allFields[j];
                if (!typeof(SharedVariableBase).IsAssignableFrom(field.FieldType)) continue;

                var sharedVariable = (SharedVariableBase)field.GetValue(node);
                if (sharedVariable == null || string.IsNullOrEmpty(sharedVariable.VariableName)) continue;

                var sharedDataVariable = GetSharedVariable(node.SharedData, sharedVariable.VariableName);
                if (sharedDataVariable == null || sharedDataVariable.GetType() != sharedVariable.GetType()) continue;

                field.SetValue(node, sharedDataVariable);

                // 중복되지 않는 변수만 sharedVariablesTable에 추가
                if (variableNameSet.Add(sharedVariable.VariableName))
                {
                    sharedVariablesTable.Add((node, sharedDataVariable));
                }
            }
        }

        // 추가된 sharedVariablesTable 항목에 대해 필요한 Component 설정
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

// GetSharedVariable 메서드는 동일하게 유지됩니다.
    private static SharedVariableBase GetSharedVariable(SharedData sharedData, string variableName)
    {
        if (sharedData == null || sharedData.Variables == null) return null;

        var variables = sharedData.Variables;
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!tree)
        {
            return;
        }

        BehaviorTree.Traverse(tree.RootNode, n =>
        {
            if (n.drawGizmos)
            {
                n.OnDrawGizmos();
            }
        });
    }
#endif
}