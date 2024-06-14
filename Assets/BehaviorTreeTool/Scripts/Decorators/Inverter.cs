using BehaviorTreeTool.Scripts.Runtime;

namespace BehaviorTreeTool.Scripts.Decorators
{
    public class Inverter : DecoratorNode
    {
        protected override void OnStart() { }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            return child.Update() switch
            {
                TaskState.Running => TaskState.Running,
                TaskState.Failure => TaskState.Success,
                TaskState.Success => TaskState.Failure,
                _ => TaskState.Failure
            };
        }
    }
}