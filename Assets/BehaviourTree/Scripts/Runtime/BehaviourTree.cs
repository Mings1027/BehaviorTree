using System.Collections.Generic;
using BehaviourTree.Scripts.TreeSharedData;
using UnityEditor;
using UnityEngine;

namespace BehaviourTree.Scripts.Runtime
{
    [CreateAssetMenu]
    public class BehaviourTree : ScriptableObject
    {
        public Node RootNode
        {
            get => rootNode;
            set => rootNode = value;
        }

        public List<Node> Nodes => nodes;

        [SerializeField] private Node rootNode;
        [SerializeField] private List<Node> nodes = new();

        public void Init(SharedData sharedData)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].SetSharedData(sharedData);
                nodes[i].Init();
                nodes[i].OnAwake();
            }
        }

        public void TreeUpdate()
        {
            if (rootNode.NodeState == Node.State.Running)
            {
                rootNode.Update();
            }
        }

        public static List<Node> GetChildren(Node parent)
        {
            List<Node> children = new List<Node>();

            if (parent is DecoratorNode decorator && decorator.Child)
            {
                children.Add(decorator.Child);
            }

            if (parent is RootNode rootNode && rootNode.Child)
            {
                children.Add(rootNode.Child);
            }

            if (parent is CompositeNode composite)
            {
                return composite.Children;
            }

            return children;
        }

        public static void Traverse(Node node, System.Action<Node> visitor)
        {
            if (node)
            {
                visitor.Invoke(node);
                var children = GetChildren(node);
                for (var i = 0; i < children.Count; i++)
                {
                    Traverse(children[i], visitor);
                }
            }
        }

        public BehaviourTree Clone(Transform transform)
        {
            var tree = Instantiate(this);
            tree.rootNode = tree.rootNode.Clone();
            tree.nodes = new List<Node>();
            Traverse(tree.rootNode, n =>
            {
                n.SetTransform(transform);
                tree.nodes.Add(n);
            });

            return tree;
        }

#if UNITY_EDITOR
        public Node CreateNode(System.Type type)
        {
            var node = CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
            nodes.Add(node);

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
            }

            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");

            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(Node node)
        {
            Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            nodes.Remove(node);

            Undo.DestroyObjectImmediate(node);

            AssetDatabase.SaveAssets();
        }

        public void AddChild(Node parent, Node child)
        {
            if (parent is DecoratorNode decorator)
            {
                Undo.RecordObject(decorator, "Behaviour Tree (AddChild)");
                decorator.Child = child;
                EditorUtility.SetDirty(decorator);
            }

            if (parent is RootNode rootNode)
            {
                Undo.RecordObject(rootNode, "Behaviour Tree (AddChild)");
                rootNode.Child = child;
                EditorUtility.SetDirty(rootNode);
            }

            if (parent is CompositeNode composite)
            {
                Undo.RecordObject(composite, "Behaviour Tree (AddChild)");
                composite.Children.Add(child);
                EditorUtility.SetDirty(composite);
            }
        }

        public void RemoveChild(Node parent, Node child)
        {
            if (parent is DecoratorNode decorator)
            {
                Undo.RecordObject(decorator, "Behaviour Tree (RemoveChild)");
                decorator.Child = null;
                EditorUtility.SetDirty(decorator);
            }

            if (parent is RootNode rootNode)
            {
                Undo.RecordObject(rootNode, "Behaviour Tree (RemoveChild)");
                rootNode.Child = null;
                EditorUtility.SetDirty(rootNode);
            }

            if (parent is CompositeNode composite)
            {
                Undo.RecordObject(composite, "Behaviour Tree (RemoveChild)");
                composite.Children.Remove(child);
                EditorUtility.SetDirty(composite);
            }
        }
#endif
    }
}