public class Repeat : DecoratorNode
{
    public bool restartOnSuccess = true;
    public bool restartOnFailure;

    protected override void OnStart() { }

    protected override void OnEnd() { }

    protected override TaskState OnUpdate()
    {
        switch (child.Update())
        {
            case TaskState.Running:
                break;
            case TaskState.Failure:
                return restartOnFailure ? TaskState.Running : TaskState.Failure;

            case TaskState.Success:
                return restartOnSuccess ? TaskState.Running : TaskState.Success;
        }

        return TaskState.Running;
    }
}