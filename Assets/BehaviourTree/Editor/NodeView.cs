using System;
using BehaviourTree.Scripts.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Node = BehaviourTree.Scripts.Runtime.Node;

namespace BehaviourTree.Editor
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Action<NodeView> OnNodeSelected { get; set; }
        public Node Node { get; }
        public Port Input { get; private set; }
        public Port Output { get; private set; }

        public NodeView(Node node) : base(
            AssetDatabase.GetAssetPath(BehaviourTreeSettings.GetOrCreateSettings().nodeXml))
        {
            Node = node;
            Node.name = node.GetType().Name;
            title = node.name.Replace("(Clone)", "").Replace("Node", "");
            viewDataKey = node.guid;

            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            SetupDataBinding();
        }

        private void SetupDataBinding()
        {
            var descriptionLabel = this.Q<Label>("description");
            descriptionLabel.bindingPath = "description";
            descriptionLabel.Bind(new SerializedObject(Node));
        }

        private void SetupClasses()
        {
            if (Node is ConditionNode)
            {
                AddToClassList("condition");
            }
            else if (Node is ActionNode)
            {
                AddToClassList("action");
            }
            else if (Node is CompositeNode)
            {
                AddToClassList("composite");
            }
            else if (Node is DecoratorNode)
            {
                AddToClassList("decorator");
            }
            else if (Node is RootNode)
            {
                AddToClassList("root");
            }
        }

        private void CreateInputPorts()
        {
            if (Node is ConditionNode)
            {
                Input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (Node is ActionNode)
            {
                Input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (Node is CompositeNode)
            {
                Input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (Node is DecoratorNode)
            {
                Input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (Node is RootNode)
            {
            }

            if (Input != null)
            {
                Input.portName = "";
                Input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(Input);
            }
        }

        private void CreateOutputPorts()
        {
            if (Node is ConditionNode)
            {
            }
            else if (Node is ActionNode)
            {
            }
            else if (Node is CompositeNode)
            {
                Output = new NodePort(Direction.Output, Port.Capacity.Multi);
            }
            else if (Node is DecoratorNode)
            {
                Output = new NodePort(Direction.Output, Port.Capacity.Single);
            }
            else if (Node is RootNode)
            {
                Output = new NodePort(Direction.Output, Port.Capacity.Single);
            }

            if (Output != null)
            {
                Output.portName = "";
                Output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(Output);
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(Node, "Behaviour Tree (Set Position");
            Node.position.x = newPos.xMin;
            Node.position.y = newPos.yMin;
            EditorUtility.SetDirty(Node);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }

        public void SortChildren()
        {
            if (Node is CompositeNode composite)
            {
                composite.Children.Sort(SortByHorizontalPosition);
            }
        }

        private int SortByHorizontalPosition(Node left, Node right)
        {
            return left.position.x < right.position.x ? -1 : 1;
        }

        public void UpdateState()
        {
            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");

            if (Application.isPlaying)
            {
                switch (Node.NodeState)
                {
                    case Node.State.Running:
                        if (Node.Started)
                        {
                            AddToClassList("running");
                        }

                        break;
                    case Node.State.Failure:
                        AddToClassList("failure");
                        break;
                    case Node.State.Success:
                        AddToClassList("success");
                        break;
                }
            }
        }
    }
}