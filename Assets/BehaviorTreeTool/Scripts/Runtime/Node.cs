using UnityEngine;

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
    [HideInInspector][TextArea] public string description;
    public bool TaskStarted => _taskStarted;
    public bool drawGizmos;
#endif
    public TaskState NodeTaskState => _taskState;

    public virtual SharedData SharedData
    {
        get => sharedData;
        set => sharedData = value;
    }

    public Transform NodeTransform => nodeTransform;

    protected Transform nodeTransform;

    private TaskState _taskState = TaskState.Running;
    private bool _taskStarted;

    [ SerializeField] protected SharedData sharedData;

    public void SetData(Transform transform, SharedData sharedData)
    {
        nodeTransform = transform;
        this.sharedData = sharedData;
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
        BehaviorTree.Traverse(this, node =>
        {
            node._taskStarted = false;
            node._taskState = TaskState.Running;
            node.OnEnd();
        });
    }

    #region Behavior Tree Life Cycle

    public virtual void Init()
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

#if UNITY_EDITOR
    public virtual void OnDrawGizmos() { }
#endif
}