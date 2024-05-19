using UnityEngine;

namespace BehaviourTree.Scripts.Runtime
{
    public abstract class DecoratorNode : Node
    {
        public Node Child
        {
            get => child;
            set => child = value;
        }

        [SerializeField] protected Node child;

        public override Node Clone()
        {
            var node = Instantiate(this);
            node.child = Child.Clone();
            return node;
        }
    }
}