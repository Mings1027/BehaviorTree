using BehaviorTreeTool.Scripts.Runtime;
using UnityEngine;
namespace BehaviorTreeTool.Scripts.Conditions
{
    public class TestCondition : ConditionNode
    {
        public SharedCollider sharedCollider;
        public SharedAnimator sharedAnimator;

        protected override void OnStart()
        {
            Debug.Log(sharedCollider);
            Debug.Log(sharedAnimator);
        }
    }
}