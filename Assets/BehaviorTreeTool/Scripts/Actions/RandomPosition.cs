using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Tree
{
    [NodeCategory("NavMeshAgent")]
    public class RandomPosition : ActionNode
    {
        private NavMeshAgent agent;

        [SerializeField] private int range;

        protected override void OnAwake()
        {
            agent = objectTransform.GetComponent<NavMeshAgent>();
        }

        protected override void OnStart()
        {
            var randomDir = Random.insideUnitSphere * range;
            randomDir += objectTransform.position;

            if (NavMesh.SamplePosition(randomDir, out var hit, range, NavMesh.AllAreas))
            {
                var finalPosition = hit.position;
                agent.SetDestination(finalPosition);
            }
        }

        protected override TaskState OnUpdate()
        {
            return TaskState.Success;
        }
    }
}