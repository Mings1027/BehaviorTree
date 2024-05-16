using UnityEngine;

namespace BehaviourTree.Scripts.Runtime
{
    public abstract class DecoratorNode : Node
    {
        [HideInInspector] public Node child;

        public override void OnAwake()
        {
        }

        public override Node Clone()
        {
            DecoratorNode node = Instantiate(this);
            node.child = child.Clone();
            return node;
        }
    }
}