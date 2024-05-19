using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using Pathfinding;

namespace BehaviourTree.Scripts.Actions
{
    public class MoveToTarget : ActionNode
    {
        public SharedCollider target;
        private AIPath _aiPath;

        public override void OnAwake()
        {
            // 노드가 깨어날 때 호출됩니다. AIPath 컴포넌트를 초기화합니다.
            _aiPath = nodeTransform.GetComponent<AIPath>();
        }

        protected override void OnStart()
        {
            // 노드가 시작될 때 호출됩니다. AIPath 목적지를 타겟의 위치로 설정합니다.
            _aiPath.destination = target.Value.transform.position;
        }

        protected override void OnStop()
        {
            // 노드가 멈출 때 호출됩니다.
        }

        protected override State OnUpdate()
        {
            return State.Success;
        }
    }
}