using BehaviourTree.Scripts.TreeSharedData;
using UnityEngine;

namespace BehaviourTree.Scripts.Runtime
{
    public class RootNode : Node
    {
        public Node Child { get; set; }

        public override Node Clone()
        {
            var clone = Instantiate(this);
            clone.Child = Child != null ? Child.Clone() : null;
            clone.sharedData = sharedData;
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
            return Child.Update();
        }

        public void OnValidate()
        {
            if (sharedData != null)
            {
                TraverseChildrenAndSetSharedData(this, sharedData);
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
            else
            {
                node.SharedData = sharedData;
            }
        }
    }
}