using System;
using BehaviorTreeTool.Scripts.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviorTreeTool.Editor
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Action<NodeView> OnNodeSelected { get; set; }
        public Node Node { get; private set; }
        public Port Input { get; private set; }
        public Port Output { get; private set; }

        public NodeView(Node node) : base(
            AssetDatabase.GetAssetPath(BehaviorTreeSettings.GetOrCreateSettings().NodeXml))
        {
            Initialize();
            SetNode(node);
        }

        private void Initialize()
        {
            // 이 메서드는 포트와 스타일을 초기화합니다.
            ClearPorts();
            RemoveFromClassList("condition");
            RemoveFromClassList("action");
            RemoveFromClassList("composite");
            RemoveFromClassList("decorator");
            RemoveFromClassList("root");
        }

        public void SetNode(Node node)
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
            // CompositeNode에만 Multi Output 포트를 추가합니다.
            if (Node is CompositeNode)
            {
                Output = new NodePort(Direction.Output, Port.Capacity.Multi);
            }
            // DecoratorNode와 RootNode에만 Single Output 포트를 추가합니다.
            else if (Node is DecoratorNode or RootNode)
            {
                Output = new NodePort(Direction.Output, Port.Capacity.Single);
            }
            // ConditionNode에는 Output 포트를 추가하지 않습니다.
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
                switch (Node.NodeTaskState)
                {
                    case Node.TaskState.Running:
                        if (Node.TaskStarted) AddToClassList("running");
                        break;
                    case Node.TaskState.Failure:
                        AddToClassList("failure");
                        break;
                    case Node.TaskState.Success:
                        AddToClassList("success");
                        break;
                }
            }
        }
    }
}