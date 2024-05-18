using BehaviourTree.Scripts.TreeSharedData;
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
        [SerializeField] private SharedData sharedData;

        private void OnEnable()
        {
            BehaviourTreeManager.AddTree(this);
        }

        private void OnDisable()
        {
            BehaviourTreeManager.RemoveTree(this);
        }

        /// <summary>
        ///  If you don't want to use BehaviourTreeManager then You can change to Awake or Start
        /// </summary>
        public void Init()
        {
            // Clone the tree and its shared data
            tree = tree.Clone(transform);
            sharedData = sharedData.Clone();

            // Initialize the tree
            tree.Init(sharedData);
        }

        /// <summary>
        /// If you don't want to use BehaviourTreeManager then You can change to Update
        /// </summary>
        public void TreeUpdate()
        {
            tree.TreeUpdate();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
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
    }
}