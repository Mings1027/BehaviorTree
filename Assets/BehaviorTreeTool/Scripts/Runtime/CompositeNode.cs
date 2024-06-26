using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.Runtime
{
    public abstract class CompositeNode : Node
    {
        public List<Node> Children => children;
        [HideInInspector, SerializeField] protected List<Node> children;

        private void OnEnable()
        {
            children ??= new List<Node>();
        }

        public override Node Clone()
        {
            var node = Instantiate(this);
            node.children = children.ConvertAll(c => c.Clone());
            return node;
        }
    }
}