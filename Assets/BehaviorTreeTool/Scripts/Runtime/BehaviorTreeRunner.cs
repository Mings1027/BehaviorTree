using UnityEngine;

public class BehaviorTreeRunner : MonoBehaviour, IBehaviorTree
{
#if UNITY_EDITOR
    public BehaviorTree Tree
    {
        get => tree;
        set => tree = value;
    }
#endif
    [SerializeField] protected BehaviorTree tree;

    private void OnEnable()
    {
        BehaviorTreeManager.AddTree(this);
    }

    private void Start()
    {
        TreeInit();
    }

    private void OnDisable()
    {
        BehaviorTreeManager.RemoveTree(this);
    }

    private void TreeInit()
    {
        var clonedTree = tree.Clone(transform);
        tree = clonedTree;
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