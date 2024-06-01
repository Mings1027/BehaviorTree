using UnityEngine;

namespace BehaviourTree.Scripts.Runtime
{
    /// <summary>
    /// For any object that intends to use behavior trees, this class is essential.
    /// </summary>
    public class BehaviourTreeRunner : MonoBehaviour
    {
#if UNITY_EDITOR
        public BehaviourTree Tree => tree;
#endif
        [SerializeField] private BehaviourTree tree;

        private void OnEnable()
        {
            BehaviourTreeManager.AddTree(this);
        }

        private void Start()
        {
            var clonedTree = tree.Clone(transform);
            tree = clonedTree;
        }

        private void OnDisable()
        {
            BehaviourTreeManager.RemoveTree(this);
        }

        /// <summary>
        /// If you don't want to use BehaviourTreeManager then You can change to Update
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

            BehaviourTree.Traverse(tree.RootNode, n =>
            {
                if (n.drawGizmos)
                {
                    n.OnDrawGizmos();
                }
            });
        }
#endif
    }
}