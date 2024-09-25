using UnityEngine;

namespace Tree
{
    public enum TaskState
    {
        Running,
        Failure,
        Success
    }

    public abstract class BaseNode : ScriptableObject
    {
#if UNITY_EDITOR
        [HideInInspector] public Vector2 position;
        [HideInInspector] public string guid;
        [HideInInspector] [TextArea] public string description;
        public bool TaskStarted => _taskStarted;

        public bool drawGizmos;
#endif
        public TaskState NodeTaskState => _taskState;
        private TaskState _taskState = TaskState.Running;

        protected Transform nodeTransform;

        private bool _taskStarted;

        public void SetTransform(Transform transform)
        {
            nodeTransform = transform;
        }

        public virtual BaseNode Clone()
        {
            var clone = Instantiate(this);
            return clone;
        }

        public void Abort()
        {
            BehaviorTree.Traverse(this, node =>
            {
                node._taskStarted = false;
                node._taskState = TaskState.Running;
                node.OnEnd();
            });
        }

        #region Behavior Tree Life Cycle

        public void Init()
        {
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
                OnStart();
                _taskStarted = true;
            }

            _taskState = OnUpdate();

            if (_taskState != TaskState.Running)
            {
                OnEnd();
                _taskStarted = false;
            }

            return _taskState;
        }

        #endregion


#if UNITY_EDITOR
        public virtual void OnDrawGizmos() { }

#endif
    }
}