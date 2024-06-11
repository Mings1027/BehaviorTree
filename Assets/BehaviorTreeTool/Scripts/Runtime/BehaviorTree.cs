using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "BehaviorTree/Behavior Tree")]
public class BehaviorTree : ScriptableObject
{
    public Node RootNode => rootNode;
    public List<Node> Nodes => nodes;

    private SharedData _sharedData;

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
        tree._sharedData = rootNode.SharedData.Clone();
        Traverse(tree.rootNode, n =>
        {
            tree.nodes.Add(n);
            n.SetData(transform, tree._sharedData);
        });
        AssignSharedVariables(tree.nodes);
        Traverse(tree.rootNode, n => n.Init());

        return tree;
    }

    private static void AssignSharedVariables(IReadOnlyList<Node> nodeList)
    {
        // 우선 배열의 최대 크기를 계산하기 위해 전체 필드 개수를 셉니다.
        var totalFieldCount = 0;

        for (var i = 0; i < nodeList.Count; i++)
        {
            var node = nodeList[i];
            var allFields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (var j = 0; j < allFields.Length; j++)
            {
                var field = allFields[j];
                if (typeof(SharedVariableBase).IsAssignableFrom(field.FieldType))
                {
                    totalFieldCount++;
                }
            }
        }

        var sharedVariablesTable = new (Node Node, SharedVariableBase SharedVariable)[totalFieldCount];
        var currentIndex = 0;

        for (var i = 0; i < nodeList.Count; i++)
        {
            var node = nodeList[i];
            var allFields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (var j = 0; j < allFields.Length; j++)
            {
                var field = allFields[j];
                if (!typeof(SharedVariableBase).IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                var sharedVariable = (SharedVariableBase)field.GetValue(node);

                if (sharedVariable == null || string.IsNullOrEmpty(sharedVariable.VariableName))
                {
                    continue;
                }

                var sharedDataVariable = GetSharedVariable(node.SharedData, sharedVariable.VariableName);
                if (sharedDataVariable == null || sharedDataVariable.GetType() != sharedVariable.GetType())
                {
                    continue;
                }

                var currentValue = sharedVariable.GetValue();
                field.SetValue(node, sharedDataVariable);

                if (currentValue != null)
                {
                    sharedDataVariable.SetValue(currentValue);
                }

                sharedVariablesTable[currentIndex++] = (node, sharedDataVariable);
            }
        }

        for (var i = 0; i < currentIndex; i++)
        {
            var kvp = sharedVariablesTable[i];
            var value = kvp.SharedVariable.GetValue();

            if (kvp.SharedVariable is IComponentObject { UseGetComponent: true } && value is Component)
            {
                var componentType = value.GetType();
                if (kvp.Node.NodeTransform.TryGetComponent(componentType, out var component))
                {
                    kvp.SharedVariable.SetValue(component);
                }
            }
        }
    }

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