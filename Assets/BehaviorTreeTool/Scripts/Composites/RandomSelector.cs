using UnityEngine;

namespace Tree
{
    public class RandomSelector : CompositeNode
    {
        private int _current;

        protected override void OnStart()
        {
            _current = Random.Range(0, children.Count);
        }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            var child = children[_current];
            return child.Update();
        }
    }
}