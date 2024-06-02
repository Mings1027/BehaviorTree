using System.Collections.Generic;
using UnityEngine;

public class BehaviorTreeManager : MonoBehaviour
{
    private static BehaviorTreeManager _instance;
    private List<BehaviorTreeRunner> _behaviorTreeRunners;

    private void Awake()
    {
        _instance = this;
        _behaviorTreeRunners = new List<BehaviorTreeRunner>();
    }

    private void Update()
    {
        for (int i = 0; i < _behaviorTreeRunners.Count; i++)
        {
            _behaviorTreeRunners[i].TreeUpdate();
        }
    }

    public static void AddTree(BehaviorTreeRunner BehaviorTreeRunner)
    {
        if (_instance._behaviorTreeRunners.Contains(BehaviorTreeRunner)) return;
        _instance._behaviorTreeRunners.Add(BehaviorTreeRunner);
    }

    public static void RemoveTree(BehaviorTreeRunner BehaviorTreeRunner)
    {
        if (_instance._behaviorTreeRunners.Contains(BehaviorTreeRunner))
            _instance._behaviorTreeRunners.Remove(BehaviorTreeRunner);
    }
}