public class Succeed : DecoratorNode
{
    protected override void OnStart() { }

    protected override void OnEnd() { }

    protected override TaskState OnUpdate()
    {
        var childState = child.Update();
        return childState == TaskState.Failure ? TaskState.Success : childState;
    }
}