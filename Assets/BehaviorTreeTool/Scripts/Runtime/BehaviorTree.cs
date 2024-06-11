using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "BehaviorTree/Behavior Tree")]
public class BehaviorTree : ScriptableObject
{
    public Node RootNode => rootNode;
    public List<Node> Nodes => nodes;

    [SerializeField] protected Node rootNode;
    [SerializeField] protected List<Node> nodes = new();

    public void TreeUpdate()
    {
        if (rootNode.NodeTaskState == Node.TaskState.Running)
        {
            rootNode.Update();
        }
    }

    public static List<Node> GetChildren(Node parent)
    {
        var children = new List<Node>();

        switch (parent)
        {
            case CompositeNode composite:
                return composite.Children;
            case DecoratorNode decorator when decorator.Child:
                children.Add(decorator.Child);
                break;
            case RootNode rootNode when rootNode.Child:
                children.Add(rootNode.Child);
                break;
        }

        return children;
    }

    public static void Traverse(Node node, Action<Node> visitor)
    {
        if (!node) return;
        visitor.Invoke(node);
        var children = GetChildren(node);
        for (var i = 0; i < children.Count; i++)
        {
            Traverse(children[i], visitor);
        }
    }

    public BehaviorTree Clone(Transform transform)
    {
        var tree = Instantiate(this);
        tree.rootNode = tree.rootNode.Clone();
        tree.nodes = new List<Node>();
        var sharedData = rootNode.SharedData.Clone();
        Traverse(tree.rootNode, n =>
        {
            tree.nodes.Add(n);
            n.SetData(transform, sharedData);
        });
        // AssignSharedVariables(tree.nodes);
        // Traverse(tree.rootNode, n => n.Init());

        return tree;
    }

    public virtual void AssignSharedVariables(IReadOnlyList<Node> nodeList)
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
    protected static SharedVariableBase GetSharedVariable(SharedData sharedData, string variableName)
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

#region Use Only Editor

#if UNITY_EDITOR
    public void SetRootNode(RootNode rootNode)
    {
        this.rootNode = rootNode;
        if (!nodes.Contains(rootNode))
        {
            nodes.Add(rootNode);
        }
    }

    public Node CreateNode(Type type)
    {
        var node = CreateInstance(type) as Node;
        if (node != null)
        {
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            if (rootNode && rootNode.SharedData)
            {
                node.SharedData = rootNode.SharedData;
            }

            Undo.RecordObject(this, "Behavior Tree (CreateNode)");
            nodes.Add(node);

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
            }

            Undo.RegisterCreatedObjectUndo(node, "Behavior Tree (CreateNode)");

            AssetDatabase.SaveAssets();
            return node;
        }

        throw new InvalidOperationException();
    }

    public void DeleteNode(Node node)
    {
        Undo.RecordObject(this, "Behavior Tree (DeleteNode)");
        nodes.Remove(node);

        Undo.DestroyObjectImmediate(node);

        AssetDatabase.SaveAssets();
    }

    public static void AddChild(Node parent, Node child)
    {
        if (parent is DecoratorNode decorator)
        {
            Undo.RecordObject(decorator, "Behavior Tree (AddChild)");
            decorator.Child = child;
            EditorUtility.SetDirty(decorator);
        }

        if (parent is RootNode rootNode)
        {
            Undo.RecordObject(rootNode, "Behavior Tree (AddChild)");
            rootNode.Child = child;
            EditorUtility.SetDirty(rootNode);
        }

        if (parent is CompositeNode composite)
        {
            Undo.RecordObject(composite, "Behavior Tree (AddChild)");
            composite.Children.Add(child);
            EditorUtility.SetDirty(composite);
        }
    }

    public static void RemoveChild(Node parent, Node child)
    {
        if (parent is DecoratorNode decorator)
        {
            Undo.RecordObject(decorator, "Behavior Tree (RemoveChild)");
            decorator.Child = null;
            EditorUtility.SetDirty(decorator);
        }

        if (parent is RootNode rootNode)
        {
            Undo.RecordObject(rootNode, "Behavior Tree (RemoveChild)");
            rootNode.Child = null;
            EditorUtility.SetDirty(rootNode);
        }

        if (parent is CompositeNode composite)
        {
            Undo.RecordObject(composite, "Behavior Tree (RemoveChild)");
            composite.Children.Remove(child);
            EditorUtility.SetDirty(composite);
        }
    }
#endif

#endregion
}