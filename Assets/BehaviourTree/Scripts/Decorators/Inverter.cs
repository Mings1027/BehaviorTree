using BehaviourTree.Scripts.Runtime;

namespace BehaviourTree.Scripts.Decorators
{
    public class Inverter : DecoratorNode
    {
        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            return child.Update() switch
            {
                State.Running => State.Running,
                State.Failure => State.Success,
                State.Success => State.Failure,
                _ => State.Failure
            };
        }
    }
}