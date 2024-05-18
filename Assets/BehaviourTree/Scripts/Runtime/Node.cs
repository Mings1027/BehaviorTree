using System.Collections.Generic;
using System.Reflection;
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
        public bool Started => _started;
#endif
        public State NodeState => _state;

        public SharedData SharedData
        {
            get => sharedData;
            set => sharedData = value;
        }

        public List<Node> Children => children;

        protected Transform nodeTransform;

        private State _state = State.Running;
        private bool _started;

        [HideInInspector, SerializeField] protected SharedData sharedData;
        [HideInInspector, SerializeField] protected List<Node> children;

        private void OnEnable()
        {
            children ??= new List<Node>();
        }

        public void SetTransform(Transform transform)
        {
            nodeTransform = transform;
        }

        public virtual Node Clone()
        {
            var clone = Instantiate(this);
            clone.sharedData = sharedData; // 각 노드가 동일한 SharedData 인스턴스를 가리키도록 설정
            clone.children = new List<Node>(children);
            clone.nodeTransform = nodeTransform;
            return clone;
        }

        public void Abort()
        {
            BehaviourTree.Traverse(this, node =>
            {
                node._started = false;
                node._state = State.Running;
                node.OnStop();
            });
        }

#region Behavior Tree Life Cycle

        public virtual void Init()
        {
            AssignSharedVariables();
        }

        public virtual void OnAwake()
        {
        }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnUpdate();

        public State Update()
        {
            if (!_started)
            {
                OnStart(); // Expensive because Breakpoint Node
                _started = true;
            }

            _state = OnUpdate(); // Expensive because Attack Node

            if (_state != State.Running)
            {
                OnStop();
                _started = false;
            }

            return _state;
        }

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
            if (sharedData != null && sharedData.Variables != null)
            {
                for (var i = 0; i < sharedData.Variables.Count; i++)
                {
                    var sharedVariable = sharedData.Variables[i];
                    if (sharedVariable.VariableName == variableName)
                    {
                        return sharedVariable;
                    }
                }
            }

            return null;
        }

        private void AssignSharedVariables()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (typeof(SharedVariableBase).IsAssignableFrom(field.FieldType))
                {
                    var sharedVariable = (SharedVariableBase)field.GetValue(this);
                    if (sharedVariable != null && !string.IsNullOrEmpty(sharedVariable.VariableName))
                    {
                        var sharedDataVariable = GetSharedVariable(sharedVariable.VariableName);
                        if (sharedDataVariable != null && sharedDataVariable.GetType() == sharedVariable.GetType())
                        {
                            field.SetValue(this, sharedDataVariable);
                        }
                    }
                }
            }
        }
    }
}