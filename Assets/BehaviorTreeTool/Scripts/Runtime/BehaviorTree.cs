using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tree
{
    [CreateAssetMenu(menuName = "BehaviorTree/Behavior Tree")]
    public sealed class BehaviorTree : ScriptableObject
    {
        public SharedData SharedData
        {
            get => sharedData;
            set => sharedData = value;
        }

        [SerializeField, HideInInspector] private SharedData sharedData;

        public BaseNode RootNode => rootNode;
        [SerializeField] private BaseNode rootNode;

        public List<BaseNode> Nodes => nodes;
        [SerializeField] private List<BaseNode> nodes = new();

        public void TreeUpdate()
        {
            if (rootNode.NodeTaskState == TaskState.Running)
            {
                rootNode.Update();
            }
        }

        public static List<BaseNode> GetChildren(BaseNode parent)
        {
            var children = new List<BaseNode>();

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

        public static void Traverse(BaseNode node, Action<BaseNode> visitor)
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
            tree.nodes = new List<BaseNode>();
            Traverse(tree.rootNode, n =>
            {
                tree.nodes.Add(n);
                n.SetTransform(transform);
            });
            if (sharedData != null)
            {
                tree.sharedData = tree.sharedData.Clone();
            }

            return tree;
        }

        public SharedVariableBase GetVariable(string variableName)
        {
            var variables = sharedData.Variables;
            for (var i = 0; i < variables.Count; i++)
            {
                if (variables[i].VariableName == variableName)
                    return variables[i];
            }

            return null;
        }

        public void SetVariable(string variableName, object value)
        {
            var variable = GetVariable(variableName);
            variable.SetValue(value);
        }

#if UNITY_EDITOR

        public void SetRootNode(RootNode root)
        {
            rootNode = root;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes.Contains(root)) nodes.RemoveAt(i);
            }
        }

        public BaseNode CreateNode(Type type)
        {
            var node = CreateInstance(type) as BaseNode;
            if (node != null)
            {
                node.name = type.Name;
                node.guid = GUID.Generate().ToString();

                Undo.RecordObject(this, "Behavior Tree (CreateNode)");
                nodes.Add(node);

                if (!Application.isPlaying)
                {
                    AssetDatabase.AddObjectToAsset(node, this);
                    Undo.RegisterCreatedObjectUndo(node, "Behavior Tree (CreateNode)");
                }

                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                return node;
            }

            throw new InvalidOperationException();
        }

        public void DeleteNode(BaseNode node)
        {
            Undo.RecordObject(this, "Behavior Tree (DeleteNode)");
            nodes.Remove(node);

            Undo.DestroyObjectImmediate(node);

            AssetDatabase.SaveAssets();
        }

        public static void AddChild(BaseNode parent, BaseNode child)
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

        public static void RemoveChild(BaseNode parent, BaseNode child)
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
}