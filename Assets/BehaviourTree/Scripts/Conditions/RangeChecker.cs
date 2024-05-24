using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using UnityEngine;

namespace BehaviourTree.Scripts.Conditions
{
    public class RangeChecker : ConditionNode
    {
        public SharedCollider target;
        [SerializeField] private int detectRange;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private Collider[] targetColliders;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            if (target.Value && target.Value.enabled) return State.Success;

            var curRange = 0f;
            while (curRange <= detectRange)
            {
                var size = Physics.OverlapSphereNonAlloc(nodeTransform.position, curRange, targetColliders,
                    targetLayer);
                if (size > 0)
                {
                    target.Value = targetColliders[0];
                    return State.Success;
                }

                curRange += 1;
            }

            target.Value = null;
            return State.Failure;
        }

        public override void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var curRange = 0f;
            while (curRange <= detectRange)
            {
                Gizmos.DrawWireSphere(nodeTransform.position, curRange);
                curRange += 1;
            }
        }
    }
}