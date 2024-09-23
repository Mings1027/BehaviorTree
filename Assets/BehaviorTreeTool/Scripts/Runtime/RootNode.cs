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

        [HideInInspector, SerializeField] private BaseNode child;
        
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


#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
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