namespace Tree
{
    public class InterruptSelector : Selector
    {
        protected override TaskState OnUpdate()
        {
            var previous = current;
            base.OnStart();
            var status = base.OnUpdate();
            if (previous != current && children[previous].NodeTaskState == TaskState.Running)
            {
                children[previous].Abort();
            }

            return status;
        }
    }
}