using UnityEngine;

namespace Tree
{
    public class RandomFailure : ActionNode
    {
        [Range(0, 1)]
        public float chanceOfFailure = 0.5f;

        protected override void OnStart() { }

        protected override void OnEnd() { }

        protected override TaskState OnUpdate()
        {
            float value = Random.value;
            if (value > chanceOfFailure)
            {
                return TaskState.Failure;
            }

            return TaskState.Success;
        }
    }
}