using BehaviourTree.Scripts.TreeSharedData;
using UnityEngine;

namespace BehaviourTree.Scripts.Runtime
{
    public class RootNode : Node
    {
        public Node Child
        {
            get => child;
            set => child = value;
        }

        [SerializeField] private Node child;

        public override Node Clone()
        {
            var clone = Instantiate(this);
            clone.Child = Child.Clone();
            return clone;
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            return Child != null ? Child.Update() : State.Failure;
        }

        public override SharedData SharedData
        {
            get => base.SharedData;
            set
            {
                base.SharedData = value;
                TraverseChildrenAndSetSharedData(this, value);
            }
        }

        public void OnValidate()
        {
            if (SharedData != null)
            {
                TraverseChildrenAndSetSharedData(this, SharedData);
            }
        }

        private void TraverseChildrenAndSetSharedData(Node node, SharedData sharedData)
        {
            if (node is CompositeNode compositeNode)
            {
                foreach (var child in compositeNode.Children)
                {
                    child.SharedData = sharedData;
                    TraverseChildrenAndSetSharedData(child, sharedData);
                }
            }
            else if (node is DecoratorNode decoratorNode)
            {
                if (decoratorNode.Child != null)
                {
                    decoratorNode.Child.SharedData = sharedData;
                    TraverseChildrenAndSetSharedData(decoratorNode.Child, sharedData);
                }
            }
            else if (node is RootNode rootNode)
            {
                if (rootNode.Child != null)
                {
                    rootNode.Child.SharedData = sharedData;
                    TraverseChildrenAndSetSharedData(rootNode.Child, sharedData);
                }
            }
            else if (node != null)
            {
                node.SharedData = sharedData;
            }
        }
    }
}