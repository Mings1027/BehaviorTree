using BehaviourTree.Scripts.Runtime;

namespace BehaviourTree.Scripts.Composites
{
    public class Selector : CompositeNode
    {
        protected int current;

        protected override void OnStart()
        {
            current = 0;
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            while (current < children.Count)
            {
                var child = children[current];
                var childState = child.Update();

                if (childState is State.Running or State.Success)
                {
                    return childState;
                }

                current++;
            }

            return State.Failure;
        }
    }
}