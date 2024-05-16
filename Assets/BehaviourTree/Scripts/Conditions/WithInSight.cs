using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using UnityEngine;

namespace BehaviourTree.Scripts.Conditions
{
    public class WithInSight : ConditionNode
    {
        [SerializeField] private SharedInt detectRange;
        [SerializeField] private SharedColliderArray targetColliders;
        [SerializeField] private SharedCollider target;
        [SerializeField] private SharedLayerMask targetLayer;

        public override void OnAwake()
        {
            target = (SharedCollider)GetSharedVariable(target.variableName);
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            var size = Physics.OverlapSphereNonAlloc(nodeTransform.position,
                detectRange.Value, targetColliders.Value, targetLayer.Value);
            if (size <= 0)
            {
                target.Value = null;
                return State.Failure;
            }

            target.Value = targetColliders.Value[0];
            return State.Success;
        }

        public override void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(nodeTransform.position, detectRange.Value);
        }
    }
}