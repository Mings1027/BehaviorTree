using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using Pathfinding;
using UnityEngine;

namespace BehaviourTree.Scripts.Actions
{
    public class MoveToTarget : ActionNode
    {
        public SharedCollider target;
        public SharedInt attackRange;
        private AIPath _aiPath;

        /// <summary>
        /// Called when the node is awakened. Initializes the AIPath component.
        /// </summary>
        public override void OnAwake()
        {
            // 노드가 깨어날 때 호출됩니다. AIPath 컴포넌트를 초기화합니다.
            _aiPath = nodeTransform.GetComponent<AIPath>();
        }

        /// <summary>
        /// Called when the node starts. Sets the AIPath destination to the target's position.
        /// </summary>
        protected override void OnStart()
        {
            // 노드가 시작될 때 호출됩니다. AIPath 목적지를 타겟의 위치로 설정합니다.
            _aiPath.destination = target.Value.transform.position;
        }

        /// <summary>
        /// Called when the node stops.
        /// </summary>
        protected override void OnStop()
        {
            // 노드가 멈출 때 호출됩니다.
        }

        /// <summary>
        /// Called every frame while the node is running. Updates the AIPath destination.
        /// </summary>
        /// <returns>State.Success if within attack range, otherwise State.Running.</returns>
        protected override State OnUpdate()
        {
            // 노드가 실행되는 동안 매 프레임 호출됩니다. AIPath 목적지를 업데이트합니다.
            float distanceToTarget = Vector3.Distance(nodeTransform.position, target.Value.transform.position);

            if (distanceToTarget <= attackRange.Value)
            {
                _aiPath.isStopped = true;
                return State.Success;
            }
            else
            {
                _aiPath.isStopped = false;
                _aiPath.destination = target.Value.transform.position;
                return State.Running;
            }
        }
    }
}