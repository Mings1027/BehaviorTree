using BehaviorTreeTool.Scripts.Runtime;

namespace BehaviorTreeTool.Scripts.Composites
{
    public class Sequencer : CompositeNode
    {
        private int current;

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
                    case TaskState.Failure:
                        return TaskState.Failure;
                    case TaskState.Success:
                        continue;
                }
            }

            return TaskState.Success;
        }
    }
}