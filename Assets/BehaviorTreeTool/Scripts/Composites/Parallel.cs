using System.Collections.Generic;

namespace BehaviorTreeTool.Scripts.Composites
{
    public class Parallel : CompositeNode
    {
        private readonly List<TaskState> _childrenLeftToExecute = new();

        protected override void OnStart()
        {
            _childrenLeftToExecute.Clear();
            for (var i = 0; i < children.Count; i++)
            {
                _childrenLeftToExecute.Add(TaskState.Running);
            }
        }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            var stillRunning = false;
            for (var i = 0; i < _childrenLeftToExecute.Count; ++i)
            {
                if (_childrenLeftToExecute[i] == TaskState.Running)
                {
                    var status = children[i].Update();
                    if (status == TaskState.Failure)
                    {
                        AbortRunningChildren();
                        return TaskState.Failure;
                    }

                    if (status == TaskState.Running)
                    {
                        stillRunning = true;
                    }

                    _childrenLeftToExecute[i] = status;
                }
            }

            return stillRunning ? TaskState.Running : TaskState.Success;
        }

        void AbortRunningChildren()
        {
            for (int i = 0; i < _childrenLeftToExecute.Count; ++i)
            {
                if (_childrenLeftToExecute[i] == TaskState.Running)
                {
                    children[i].Abort();
                }
            }
        }
    }
}