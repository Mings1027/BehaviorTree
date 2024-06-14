using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace BehaviorTreeTool.Scripts.Actions
{
    public class RandomPosition : ActionNode
    {
        public SharedNavMeshAgent agent;
        [SerializeField] private int range;

        protected override void OnStart()
        {
            var randomDir = Random.insideUnitSphere * range;
            randomDir += nodeTransform.position;

            if (NavMesh.SamplePosition(randomDir, out var hit, range, NavMesh.AllAreas))
            {
                var finalPosition = hit.position;
                agent.Value.SetDestination(finalPosition);
            }
        }

        protected override TaskState OnUpdate()
        {
            return TaskState.Success;
        }
    }
}