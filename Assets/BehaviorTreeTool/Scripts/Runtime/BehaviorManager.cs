using System.Collections.Generic;
using UnityEngine;

namespace Tree
{
    public class BehaviorManager : MonoSingleton<BehaviorManager>
    {
#if UNITY_EDITOR
        [SerializeField] private bool enableAllTreeGizmos;
#endif
        [SerializeField] private List<BehaviorTreeRunner> behaviorTrees;

#if UNITY_EDITOR
        private void OnValidate()
        {
            ToggleDrawGizmos(enableAllTreeGizmos);
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            behaviorTrees = new List<BehaviorTreeRunner>();
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            GlobalVariables.SetVariable("IsPlaying", true);
        }

        private void Update()
        {
            for (var i = behaviorTrees.Count - 1; i >= 0; i--)
            {
                if (!behaviorTrees[i].gameObject.activeSelf || !behaviorTrees[i].enabled)
                {
                    RemoveTree(behaviorTrees[i]);
                    continue;
                }

                if (behaviorTrees[i])
                {
                    behaviorTrees[i].TreeUpdate();
                }
            }
        }

        private void OnDisable()
        {
            GlobalVariables.SetVariable("IsPlaying", false);
        }

        public static void AddTree(BehaviorTreeRunner behaviorTree)
        {
            if (Instance.behaviorTrees.Contains(behaviorTree)) return;
            Instance.behaviorTrees.Add(behaviorTree);
        }

        private static void RemoveTree(BehaviorTreeRunner behaviorTree)
        {
            if (Instance.behaviorTrees.Contains(behaviorTree))
                Instance.behaviorTrees.Remove(behaviorTree);
        }
        
#if UNITY_EDITOR

        private void ToggleDrawGizmos(bool enable)
        {
            if (behaviorTrees == null || behaviorTrees.Count == 0) return;
            for (int i = 0; i < behaviorTrees.Count; i++)
            {
                var tree = behaviorTrees[i];
                if (tree?.Tree?.RootNode != null)
                {
                    BehaviorTree.Traverse(tree.Tree.RootNode, node => { node.drawGizmos = enable; });
                }
            }
        }
#endif
    }
}