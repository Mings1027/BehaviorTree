namespace BehaviourTree.Scripts.Runtime
{
    public abstract class CompositeNode : Node
    {
        public override void OnAwake()
        {
        }

        public override Node Clone()
        {
            CompositeNode node = Instantiate(this);
            node.children = children.ConvertAll(c => c.Clone());
            return node;
        }
    }
}