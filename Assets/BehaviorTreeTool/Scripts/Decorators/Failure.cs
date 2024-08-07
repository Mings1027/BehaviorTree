namespace Tree
{
    public class Failure : DecoratorNode
    {
        protected override void OnStart() { }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            var childState = child.Update();
            return childState == TaskState.Success ? TaskState.Failure : childState;
        }
    }
}