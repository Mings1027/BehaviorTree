using System.Collections.Generic;
using UnityEngine;
using BehaviourTree.Scripts.TreeSharedData;
using UnityEditor;

namespace BehaviourTree.Scripts.Runtime
{
    [CreateAssetMenu]
    public class BehaviourTree : ScriptableObject
    {
        public Node rootNode;
        public Node.State treeState = Node.State.Running;
        public List<Node> nodes = new();

        public void Init()
        {
            rootNode.OnAwake();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].OnAwake();
            }
        }

        public Node.State Update()
        {
            if (rootNode.state == Node.State.Running)
            {
                treeState = rootNode.Update();
            }

            return treeState;
        }

        public static List<Node> GetChildren(Node parent)
        {
            List<Node> children = new List<Node>();

            if (parent is DecoratorNode decorator && decorator.child)
            {
                children.Add(decorator.child);
            }

            if (parent is RootNode rootNode && rootNode.child)
            {
                children.Add(rootNode.child);
            }

            if (parent is CompositeNode composite)
            {
                return composite.children;
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
                    var n = children[i];
                    Traverse(n, visitor);
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
            Node node = CreateInstance(type) as Node;
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
                decorator.child = child;
                EditorUtility.SetDirty(decorator);
            }

            if (parent is RootNode rootNode)
            {
                Undo.RecordObject(rootNode, "Behaviour Tree (AddChild)");
                rootNode.child = child;
                EditorUtility.SetDirty(rootNode);
            }

            if (parent is CompositeNode composite)
            {
                Undo.RecordObject(composite, "Behaviour Tree (AddChild)");
                composite.children.Add(child);
                EditorUtility.SetDirty(composite);
            }
        }

        public void RemoveChild(Node parent, Node child)
        {
            if (parent is DecoratorNode decorator)
            {
                Undo.RecordObject(decorator, "Behaviour Tree (RemoveChild)");
                decorator.child = null;
                EditorUtility.SetDirty(decorator);
            }

            if (parent is RootNode rootNode)
            {
                Undo.RecordObject(rootNode, "Behaviour Tree (RemoveChild)");
                rootNode.child = null;
                EditorUtility.SetDirty(rootNode);
            }

            if (parent is CompositeNode composite)
            {
                Undo.RecordObject(composite, "Behaviour Tree (RemoveChild)");
                composite.children.Remove(child);
                EditorUtility.SetDirty(composite);
            }
        }
#endif
    }
}