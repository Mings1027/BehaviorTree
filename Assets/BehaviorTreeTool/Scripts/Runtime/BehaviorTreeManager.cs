using System.Collections.Generic;
using BehaviorTreeTool.Scripts.CustomInterface;
using UnityEngine;

public class BehaviorTreeManager : MonoBehaviour
{
    private static BehaviorTreeManager _instance;
    [SerializeField] private List<IBehaviorTree> _behaviorTree;

    private void Awake()
    {
        _instance = this;
        _behaviorTree = new List<IBehaviorTree>();
    }

    private void Update()
    {
        for (int i = 0; i < _behaviorTree.Count; i++)
        {
            _behaviorTree[i].TreeUpdate();
        }
    }

    public static void AddTree(IBehaviorTree behaviorTree)
    {
        if (_instance._behaviorTree.Contains(behaviorTree)) return;
        _instance._behaviorTree.Add(behaviorTree);
    }

    public static void RemoveTree(IBehaviorTree behaviorTree)
    {
        if (_instance._behaviorTree.Contains(behaviorTree))
            _instance._behaviorTree.Remove(behaviorTree);
    }
}