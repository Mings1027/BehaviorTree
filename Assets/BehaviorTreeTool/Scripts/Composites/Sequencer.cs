namespace BehaviorTreeTool.Scripts.Composites
{
    public class Sequencer : CompositeNode
    {
        protected override void OnStart() { }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            for (var i = 0; i < children.Count; i++)
            {
                var state = children[i].Update();
                if (state == TaskState.Success) continue;
                return state;
            }

            return TaskState.Success;
        }
    }
}