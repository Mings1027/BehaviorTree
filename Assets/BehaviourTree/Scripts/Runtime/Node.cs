using System.Reflection;
using BehaviourTree.Scripts.TreeData;
using UnityEngine;

namespace BehaviourTree.Scripts.Runtime
{
    public abstract class Node : ScriptableObject
    {
        public enum TaskState
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
        public bool TaskStarted => _taskStarted;
#endif
        public TaskState NodeTaskState => _taskState;

        public virtual SharedData SharedData
        {
            get => sharedData;
            set => sharedData = value;
        }

        protected Transform nodeTransform;

        private TaskState _taskState = TaskState.Running;
        private bool _taskStarted;

        [HideInInspector, SerializeField] protected SharedData sharedData;

        public void SetData(Transform transform, SharedData sharedData)
        {
            nodeTransform = transform;
            SharedData = sharedData;
        }

        public virtual Node Clone()
        {
            var clone = Instantiate(this);
            clone.sharedData = sharedData; // 각 노드가 동일한 SharedData 인스턴스를 가리키도록 설정
            clone.nodeTransform = nodeTransform;
            return clone;
        }

        public void Abort()
        {
            BehaviourTree.Traverse(this, node =>
            {
                node._taskStarted = false;
                node._taskState = TaskState.Running;
                node.OnEnd();
            });
        }

#region Behavior Tree Life Cycle

        public virtual void Init()
        {
            AssignSharedVariables();
            OnAwake();
        }

        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }
        protected virtual TaskState OnUpdate() => TaskState.Running;
        protected virtual void OnEnd() { }

        public TaskState Update()
        {
            if (!_taskStarted)
            {
                OnStart(); // Expensive because Breakpoint Node
                _taskStarted = true;
            }

            _taskState = OnUpdate(); // Expensive because Attack Node

            if (_taskState != TaskState.Running)
            {
                OnEnd();
                _taskStarted = false;
            }

            return _taskState;
        }

#endregion

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

#if UNITY_EDITOR
        public virtual void OnDrawGizmos() { }

        public void SetSharedData(SharedData sharedData)
        {
            this.sharedData = sharedData;
        }
#endif
    }
}