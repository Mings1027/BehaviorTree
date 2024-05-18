using System.Collections.Generic;
using System.Linq;
using BehaviourTree.Scripts.Runtime;

namespace BehaviourTree.Scripts.Composites
{
    public class Parallel : CompositeNode
    {
        private readonly List<State> _childrenLeftToExecute = new();

        protected override void OnStart()
        {
            _childrenLeftToExecute.Clear();
            for (var i = 0; i < children.Count; i++)
            {
                _childrenLeftToExecute.Add(State.Running);
            }
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            var stillRunning = false;
            for (var i = 0; i < _childrenLeftToExecute.Count(); ++i)
            {
                if (_childrenLeftToExecute[i] == State.Running)
                {
                    var status = children[i].Update();
                    if (status == State.Failure)
                    {
                        AbortRunningChildren();
                        return State.Failure;
                    }

                    if (status == State.Running)
                    {
                        stillRunning = true;
                    }

                    _childrenLeftToExecute[i] = status;
                }
            }

            return stillRunning ? State.Running : State.Success;
        }

        void AbortRunningChildren()
        {
            for (int i = 0; i < _childrenLeftToExecute.Count; ++i)
            {
                if (_childrenLeftToExecute[i] == State.Running)
                {
                    children[i].Abort();
                }
            }
        }
    }
}