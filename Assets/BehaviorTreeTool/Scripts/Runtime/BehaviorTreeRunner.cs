using UnityEngine;

public class BehaviorTreeRunner : MonoBehaviour, IBehaviorTree
{
#if UNITY_EDITOR
    public BehaviorTree Tree => tree;
#endif
    [SerializeField] protected BehaviorTree tree;

    private void OnEnable()
    {
        BehaviorTreeManager.AddTree(this);
    }

    private void Start()
    {
        InitializeTree();
    }

    private void OnDisable()
    {
        BehaviorTreeManager.RemoveTree(this);
    }

    private void InitializeTree()
    {
        var clonedTree = tree.Clone(transform);
        tree = clonedTree;
        tree.AssignSharedVariables(tree.Nodes);
        BehaviorTree.Traverse(tree.RootNode, n => n.Init());
    }

    public void TreeUpdate()
    {
        tree.TreeUpdate();
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!tree)
        {
            return;
        }

        BehaviorTree.Traverse(tree.RootNode, n =>
        {
            if (n.drawGizmos)
            {
                n.OnDrawGizmos();
            }
        });
    }
#endif
}