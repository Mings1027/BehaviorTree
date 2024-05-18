using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using UnityEngine;

namespace BehaviourTree.Scripts.Conditions
{
    public class WithInSight : ConditionNode
    {
        public SharedCollider target;
        [SerializeField] private int detectRange;
        [SerializeField] private Collider[] targetColliders;
        [SerializeField] private LayerMask targetLayer;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            var size = Physics.OverlapSphereNonAlloc(nodeTransform.position,
                detectRange, targetColliders, targetLayer);
            if (size <= 0)
            {
                target.Value = null;
                return State.Failure;
            }

            target.Value = targetColliders[0];
            return State.Success;
        }

        public override void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(nodeTransform.position, detectRange);
        }
    }
}