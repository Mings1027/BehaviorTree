using BehaviourTree.Scripts.TreeSharedData;
using UnityEngine;

namespace BehaviourTree.Scripts.Runtime
{
    public class RootNode : Node
    {
        [HideInInspector]
        public Node child;

        public override void OnAwake()
        {
        }

        public override Node Clone()
        {
            RootNode clone = Instantiate(this);
            clone.child = child != null ? child.Clone() : null;
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
            return child.Update();
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
                foreach (var child in compositeNode.children)
                {
                    child.sharedData = sharedData;
                    TraverseChildrenAndSetSharedData(child, sharedData);
                }
            }
            else if (node is DecoratorNode decoratorNode)
            {
                if (decoratorNode.child != null)
                {
                    decoratorNode.child.sharedData = sharedData;
                    TraverseChildrenAndSetSharedData(decoratorNode.child, sharedData);
                }
            }
            else if (node is RootNode rootNode)
            {
                if (rootNode.child != null)
                {
                    rootNode.child.sharedData = sharedData;
                    TraverseChildrenAndSetSharedData(rootNode.child, sharedData);
                }
            }
            else
            {
                node.sharedData = sharedData;
            }
        }
    }
}