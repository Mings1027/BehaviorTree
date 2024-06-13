namespace BehaviorTreeTool.Scripts.Composites
{
    public class Selector : CompositeNode
    {
        protected int current;

        protected override void OnStart()
        {
            current = 0;
        }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            while (current < children.Count)
            {
                var child = children[current];
                var childState = child.Update();

                if (childState is TaskState.Running or TaskState.Success)
                {
                    return childState;
                }

                current++;
            }

            return TaskState.Failure;
        }
    }
}