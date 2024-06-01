using BehaviourTree.Scripts.Runtime;
using UnityEngine;

namespace BehaviourTree.Scripts.Composites
{
    public class RandomSelector : CompositeNode
    {
        protected int current;

        protected override void OnStart()
        {
            current = Random.Range(0, children.Count);
        }

        protected override void OnEnd()
        {
        }

        protected override TaskState OnUpdate()
        {
            var child = children[current];
            return child.Update();
        }
    }
}