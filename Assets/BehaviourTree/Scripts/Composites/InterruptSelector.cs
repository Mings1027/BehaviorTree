namespace BehaviourTree.Scripts.Composites
{
  public class InterruptSelector : Selector
    {
        protected override State OnUpdate()
        {
            var previous = current;
            base.OnStart();
            var status = base.OnUpdate();
            if (previous != current && children[previous].NodeState == State.Running)
            {
                children[previous].Abort();
            }

            return status;
        }
    }
}