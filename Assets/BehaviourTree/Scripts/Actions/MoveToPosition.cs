using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using Pathfinding;
using UnityEngine;

namespace BehaviourTree.Scripts.Actions
{
    public class MoveToPosition : ActionNode
    {
        [SerializeField] private SharedAIPath aiPath;
        [SerializeField] private SharedVector3 destination;

        public override void OnAwake()
        {
            aiPath.Value = nodeTransform.GetComponent<AIPath>();
        }

        protected override void OnStart()
        {
            aiPath.Value.destination = destination.Value;
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            return State.Success;
        }
    }
}