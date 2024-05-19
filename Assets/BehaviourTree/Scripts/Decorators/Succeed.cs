using BehaviourTree.Scripts.Runtime;

namespace BehaviourTree.Scripts.Decorators
{
    public class Succeed : DecoratorNode
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
            return childState == State.Failure ? State.Success : childState;
        }
    }
}