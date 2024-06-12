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
        // 먼저 variables 리스트를 순회하면서 UseGetComponent가 true인 경우 처리
        AssignComponentVariables();

        // variables 리스트에서 VariableName과 매칭되는 변수를 빠르게 찾기 위해 사전 생성
        var variableDict = new Dictionary<string, SharedVariableBase>();
        foreach (var variable in variables)
        {
            if (!string.IsNullOrEmpty(variable.VariableName))
            {
                variableDict[variable.VariableName] = variable;
            }
        }

        // 노드와 필드, SharedVariable 쌍을 저장할 리스트를 생성하고 할당을 한 번에 처리
        var nodeVariablePairs = GetNodeVariablePairs(behaviorTree.Nodes);

        foreach (var pair in nodeVariablePairs)
        {
            if (variableDict.TryGetValue(pair.sharedVariable.VariableName, out var matchingVariable))
            {
                pair.field.SetValue(pair.node, matchingVariable);
            }
        }
    }

// UseGetComponent가 true인 경우 처리
    private void AssignComponentVariables()
    {
        foreach (var variable in variables)
        {
            if (variable is IComponentObject componentObject && componentObject.UseGetComponent)
            {
                var componentType = variable.GetValue().GetType();
                if (transform.TryGetComponent(componentType, out var component))
                {
                    variable.SetValue(component);
                }
            }
        }
    }

// 각 노드를 순회하면서 SharedVariableBase 타입의 public 필드를 가져와서 리스트로 반환
    private List<(Node node, FieldInfo field, SharedVariableBase sharedVariable)> GetNodeVariablePairs(List<Node> nodes)
    {
        var nodeVariablePairs = new List<(Node node, FieldInfo field, SharedVariableBase sharedVariable)>();

        foreach (var node in nodes)
        {
            var allFields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in allFields)
            {
                if (typeof(SharedVariableBase).IsAssignableFrom(field.FieldType))
                {
                    var sharedVariable = (SharedVariableBase)field.GetValue(node);
                    if (sharedVariable != null)
                    {
                        nodeVariablePairs.Add((node, field, sharedVariable));
                    }
                }
            }
        }

        return nodeVariablePairs;
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