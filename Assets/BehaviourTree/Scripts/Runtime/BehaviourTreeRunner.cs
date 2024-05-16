using BehaviourTree.Scripts.TreeSharedData;
using UnityEngine;

namespace BehaviourTree.Scripts.Runtime
{
    public class BehaviourTreeRunner : MonoBehaviour
    {
        public BehaviourTree tree;

        [SerializeField] private SharedData sharedData;

        private void Start()
        {
            // Clone the tree
            tree = tree.Clone(transform);
            sharedData = sharedData.Clone();

            // Initialize the tree
            tree.Init();
        }

        private void Update()
        {
            tree.Update();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void OnDrawGizmosSelected()
        {
            if (!tree)
            {
                return;
            }

            BehaviourTree.Traverse(tree.rootNode, (n) =>
            {
                if (n.drawGizmos)
                {
                    n.OnDrawGizmos();
                }
            });
        }
    }
}