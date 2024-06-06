using UnityEngine;
using UnityEngine.AI;

namespace BehaviorTreeTool.Scripts.Actions
{
    public class RandomPosition : ActionNode
    {
        public SharedNavMeshAgent navMeshAgent;
        [SerializeField] private float minPos;
        [SerializeField] private float maxPos;

        protected override void OnStart()
        {
            navMeshAgent.Value.destination = new Vector3(Random.Range(minPos, maxPos), 0, Random.Range(minPos, maxPos));
        }

        protected override TaskState OnUpdate()
        {
            return TaskState.Success;
        }
    }
}