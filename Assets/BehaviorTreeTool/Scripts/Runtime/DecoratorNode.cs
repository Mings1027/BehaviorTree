using UnityEngine;

namespace Tree
{
    public abstract class DecoratorNode : BaseNode
    {
        public BaseNode Child
        {
            get => child;
            set => child = value;
        }

        [HideInInspector, SerializeField] protected BaseNode child;

        public override BaseNode Clone()
        {
            var node = Instantiate(this);
            node.child = child.Clone();
            return node;
        }
    }
}