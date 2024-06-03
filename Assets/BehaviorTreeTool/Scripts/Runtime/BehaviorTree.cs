using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "BehaviorTree/Tree")]
public class BehaviorTree : ScriptableObject
{
    public Node RootNode
    {
        get => rootNode;
        set => rootNode = value;
    }

    public List<Node> Nodes => nodes;
    private SharedData _sharedData;

    [SerializeField] private Node rootNode;
    [SerializeField] private List<Node> nodes = new();

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
            n.Init();
        });

        return tree;
    }

#if UNITY_EDITOR
    public Node CreateNode(Type type)
    {
        var node = CreateInstance(type) as Node;
        if (node != null)
        {
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            if (rootNode.SharedData)
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
}