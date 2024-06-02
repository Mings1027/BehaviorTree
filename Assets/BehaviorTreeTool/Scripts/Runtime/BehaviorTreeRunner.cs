using UnityEngine;

/// <summary>
/// For any object that intends to use behavior trees, this class is essential.
/// </summary>
public class BehaviorTreeRunner : MonoBehaviour
{
#if UNITY_EDITOR
    public BehaviorTree Tree => tree;
#endif
    [SerializeField] private BehaviorTree tree;

    private void OnEnable()
    {
        BehaviorTreeManager.AddTree(this);
    }

    private void Start()
    {
        var clonedTree = tree.Clone(transform);
        tree = clonedTree;
    }

    private void OnDisable()
    {
        BehaviorTreeManager.RemoveTree(this);
    }

    /// <summary>
    /// If you don't want to use BehaviorTreeManager then You can change to Update
    /// </summary>
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