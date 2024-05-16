using System.Collections.Generic;
using BehaviourTree.Scripts.TreeSharedData;
using UnityEngine;

namespace BehaviourTree.Scripts.Runtime
{
    public abstract class Node : ScriptableObject
    {
        public enum State
        {
            Running,
            Failure,
            Success
        }

#if UNITY_EDITOR
        [HideInInspector] public Vector2 position;
        [HideInInspector] public string guid;
        [HideInInspector] [TextArea] public string description;
        public bool drawGizmos;
#endif
        [HideInInspector] public State state = State.Running;
        [HideInInspector] public bool started;
        [HideInInspector] public SharedData sharedData;
        protected Transform nodeTransform;
        [HideInInspector] public List<Node> children;

        private void OnEnable()
        {
            children ??= new List<Node>();
        }

        public State Update()
        {
            if (!started)
            {
                OnStart();
                started = true;
            }

            state = OnUpdate();

            if (state != State.Running)
            {
                OnStop();
                started = false;
            }

            return state;
        }

        public void SetTransform(Transform transform)
        {
            nodeTransform = transform;
        }

        public virtual Node Clone()
        {
            var clone = Instantiate(this);
            clone.sharedData = sharedData.Clone();
            clone.children = new List<Node>(children);
            clone.nodeTransform = nodeTransform;
            return clone;
        }

        public void Abort()
        {
            BehaviourTree.Traverse(this, node =>
            {
                node.started = false;
                node.state = State.Running;
                node.OnStop();
            });
        }

#region Behavior Tree Life Cycle

        public abstract void OnAwake();
        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnUpdate();

#endregion

        public virtual void OnDrawGizmos()
        {
        }

        public void SetSharedData(SharedData sharedData)
        {
            this.sharedData = sharedData;
        }

        protected SharedVariableBase GetSharedVariable(string variableName)
        {
            // sharedData의 variables 리스트에서 검색합니다.
            if (sharedData != null && sharedData.Variables != null)
            {
                for (var i = 0; i < sharedData.Variables.Count; i++)
                {
                    var sharedVariable = sharedData.Variables[i];
                    if (sharedVariable.variableName == variableName)
                    {
                        return sharedVariable;
                    }
                }
            }

            // 해당 변수를 찾을 수 없는 경우 null을 반환합니다.
            return null;
        }
    }
}