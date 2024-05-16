using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using Pathfinding;
using UnityEngine;

namespace BehaviourTree.Scripts.Actions
{
    public class MoveToTarget : ActionNode
    {
        [SerializeField] private SharedCollider target;
        [SerializeField] private SharedAIPath aiPath;

        public override void OnAwake()
        {
            target = (SharedCollider)GetSharedVariable(target.variableName);
            aiPath.Value = nodeTransform.GetComponent<AIPath>();
        }

        protected override void OnStart()
        {
            aiPath.Value.destination = target.Value.transform.position;
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