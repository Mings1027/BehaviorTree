using BehaviourTree.Scripts.Runtime;

namespace BehaviourTree.Scripts.Composites
{
    public class Sequencer : CompositeNode
    {
        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            for (var i = 0; i < children.Count; i++)
            {
                var state = children[i].Update();
                if (state == State.Success) continue;
                return state;
            }

            return State.Success;
        }
    }
}