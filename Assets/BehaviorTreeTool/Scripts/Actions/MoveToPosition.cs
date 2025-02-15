using UnityEngine;
using UnityEngine.AI;

namespace Tree
{
    [NodeCategory("NavMeshAgent")]
    public class MoveToPosition : ActionNode
    {
        public NavMeshAgent agent;
        
        protected override void OnAwake()
        {
            agent = objectTransform.GetComponent<NavMeshAgent>();
        }

        protected override void OnStart()
        {   
            var target = GlobalVariables.GetVariable<Transform>("Global Target");
            agent.destination = target.Value.position;
        }

        protected override TaskState OnUpdate()
        {
            return TaskState.Success;
        }
    }
}