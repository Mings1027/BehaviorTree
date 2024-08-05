using System.Collections.Generic;
using UnityEngine;

namespace Tree
{
    public abstract class CompositeNode : BaseNode
    {
        public List<BaseNode> Children => children;
        [HideInInspector, SerializeField] protected List<BaseNode> children;

        private void OnEnable()
        {
            children ??= new List<BaseNode>();
        }

        public override BaseNode Clone()
        {
            var node = Instantiate(this);
            node.children = children.ConvertAll(c => c.Clone());
            return node;
        }
    }
}