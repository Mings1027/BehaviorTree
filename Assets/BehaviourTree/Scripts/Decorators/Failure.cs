using BehaviourTree.Scripts.Runtime;

namespace BehaviourTree.Scripts.Decorators
{
    public class Failure : DecoratorNode
    {
        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            var childState = child.Update();
            return childState == State.Success ? State.Failure : childState;
        }
    }
}