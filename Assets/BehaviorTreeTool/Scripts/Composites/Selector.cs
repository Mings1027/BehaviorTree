namespace Tree
{
    public class Selector : CompositeNode
    {
        protected int current;

        protected override void OnStart()
        {
            current = 0;
        }

        protected override TaskState OnUpdate()
        {
            for (int i = current; i < children.Count; ++i)
            {
                current = i;
                var child = children[current];

                switch (child.Update())
                {
                    case TaskState.Running:
                        return TaskState.Running;
                    case TaskState.Success:
                        return TaskState.Success;
                    case TaskState.Failure:
                        continue;
                }
            }

            return TaskState.Failure;
        }
    }
}