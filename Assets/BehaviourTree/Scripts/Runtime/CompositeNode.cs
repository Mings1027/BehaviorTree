namespace BehaviourTree.Scripts.Runtime
{
    public abstract class CompositeNode : Node
    {
        public override Node Clone()
        {
            var node = Instantiate(this);
            node.children = children.ConvertAll(c => c.Clone());
            return node;
        }
    }
}