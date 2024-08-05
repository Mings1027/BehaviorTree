using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Tree;

namespace BehaviorTreeTool.Editor
{
    public class NodeView : Node
    {
        public Action<NodeView> OnNodeSelected { get; set; }
        public BaseNode Node { get; private set; }
        public Port Input { get; private set; }
        public Port Output { get; private set; }

        public NodeView(BaseNode node) : base(
            AssetDatabase.GetAssetPath(BehaviorTreeSettings.GetOrCreateSettings().NodeXml))
        {
            Initialize();
            SetNode(node);
        }

        private void Initialize()
        {
            ClearPorts();
            RemoveFromClassList("condition");
            RemoveFromClassList("action");
            RemoveFromClassList("composite");
            RemoveFromClassList("decorator");
            RemoveFromClassList("root");
        }

        public void SetNode(BaseNode node)
        {
            Node = node;
            Node.name = node.GetType().Name;
            title = node.name.Replace("(Clone)", "").Replace("Node", "");
            viewDataKey = node.guid;

            style.left = node.position.x;
            style.top = node.position.y;

            Initialize();
            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            SetupDataBinding();
            style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            style.display = DisplayStyle.None;
        }

        private void ClearPorts()
        {
            inputContainer.Clear();
            outputContainer.Clear();
        }

        private void SetupDataBinding()
        {
            var descriptionLabel = this.Q<Label>("description");
            descriptionLabel.bindingPath = "description";
            descriptionLabel.Bind(new SerializedObject(Node));
        }

        private void SetupClasses()
        {
            switch (Node)
            {
                case ConditionNode:
                    AddToClassList("condition");
                    break;
                case ActionNode:
                    AddToClassList("action");
                    break;
                case CompositeNode:
                    AddToClassList("composite");
                    break;
                case DecoratorNode:
                    AddToClassList("decorator");
                    break;
                case RootNode:
                    AddToClassList("root");
                    break;
            }
        }

        private void CreateInputPorts()
        {
            if (Node is ActionNode)
            {
                Input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (Node is CompositeNode)
            {
                Input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (Node is ConditionNode)
            {
                Input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (Node is DecoratorNode)
            {
                Input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (Node is RootNode) { }
            if (Input != null)
            {
                Input.portName = "";
                Input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(Input);
            }
        }

        private void CreateOutputPorts()
        {
            if (Node is CompositeNode)
            {
                Output = new NodePort(Direction.Output, Port.Capacity.Multi);
            }
            else if (Node is DecoratorNode or RootNode)
            {
                Output = new NodePort(Direction.Output, Port.Capacity.Single);
            }
            else if (Node is ConditionNode or ActionNode)
            {
                Output = null;
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
            Undo.RecordObject(Node, "Behavior Tree (Set Position");
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

        private int SortByHorizontalPosition(BaseNode left, BaseNode right)
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
                switch (Node.NodeTaskState)
                {
                    case TaskState.Running:
                        if (Node.TaskStarted) AddToClassList("running");
                        break;
                    case TaskState.Failure:
                        AddToClassList("failure");
                        break;
                    case TaskState.Success:
                        AddToClassList("success");
                        break;
                }
            }
        }
    }
}