using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;
namespace BehaviorTreeTool.Scripts.Conditions
{
    public class TestCondition : ConditionNode
    {
        public SharedCollider sharedCollider;
        public SharedAnimator sharedAnimator;
        public SharedBool sharedBool;
        public SharedColliderArray sharedColliderArray;
        public SharedColor sharedColor;
        public SharedFloat sharedFloat;
        public SharedGameObject sharedGameObject;
        public SharedGameObjectList sharedGameObjectList;
        public SharedInt sharedInt;
        public SharedLayerMask sharedLayerMask;
        public SharedMaterial sharedMaterial;
        public SharedNavMeshAgent sharedNavMeshAgent;
        public SharedQuaternion sharedQuaternion;
        public SharedRect sharedRect;
        public SharedString sharedString;
        public SharedTransform sharedTransform;
        public SharedTransformArray sharedTransformArray;
        public SharedVector2 sharedVector2;
        public SharedVector2Int sharedVector2Int;
        public SharedVector3 sharedVector3;
        public SharedVector3Int sharedVector3Int;

        protected override TaskState OnUpdate()
        {
            nodeTransform.rotation = sharedQuaternion.Value;
            return TaskState.Success;
        }
    }
}