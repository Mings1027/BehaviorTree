using UnityEngine;

namespace Tree
{
    public class RootNode : BaseNode
    {
        public BaseNode Child
        {
            get => child;
            set => child = value;
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

        [HideInInspector, SerializeField] private BaseNode child;

        public void OnValidate()
        {
            if (SharedData != null)
            {
                TraverseChildrenAndSetSharedData(this, SharedData);
            }
        }

        public override BaseNode Clone()
        {
            var clone = Instantiate(this);
            clone.child = child.Clone();
            return clone;
        }

        protected override void OnStart() { }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            return child != null ? child.Update() : TaskState.Failure;
        }

        private void TraverseChildrenAndSetSharedData(BaseNode node, SharedData sharedData)
        {
            if (node is CompositeNode compositeNode)
            {
                for (int i = 0; i < compositeNode.Children.Count; i++)
                {
                    BaseNode child = compositeNode.Children[i];
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

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (child)
            {
                TraverseDrawGizmos(child);
            }
        }

        private void TraverseDrawGizmos(BaseNode node)
        {
            node.OnDrawGizmos();
            if (node is CompositeNode compositeNode)
            {
                for (var i = 0; i < compositeNode.Children.Count; i++)
                {
                    TraverseDrawGizmos(compositeNode.Children[i]);
                }
            }
            else if (node is DecoratorNode decoratorNode)
            {
                if (decoratorNode.Child)
                {
                    TraverseDrawGizmos(decoratorNode.Child);
                }
            }
        }
#endif
    }
}