using BehaviourTree.Scripts.Runtime;

namespace BehaviourTree.Scripts.Decorators
{
    public class Repeat : DecoratorNode
    {
        public bool restartOnSuccess = true;
        public bool restartOnFailure;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            switch (Child.Update())
            {
                case State.Running:
                    break;
                case State.Failure:
                    return restartOnFailure ? State.Running : State.Failure;

                case State.Success:
                    return restartOnSuccess ? State.Running : State.Success;
            }

            return State.Running;
        }
    }
}