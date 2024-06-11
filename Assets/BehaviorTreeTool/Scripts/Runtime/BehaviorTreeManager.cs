using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BehaviorTreeManager : MonoBehaviour
{
    private static BehaviorTreeManager _instance;
    private List<IBehaviorTree> _behaviorTree;

    public bool drawGizmos = false; // 모든 트리의 drawGizmos 속성을 제어하는 토글

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

    [Conditional("UNITY_EDITOR")]
    public void ToggleDrawGizmos()
    {
        drawGizmos = !drawGizmos;
        foreach (var tree in _behaviorTree)
        {
            if (tree?.Tree?.RootNode != null)
            {
                BehaviorTree.Traverse(tree.Tree.RootNode, node =>
                {
                    node.drawGizmos = drawGizmos;
                });
            }
        }
    }
}
