using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Tree
{
    public class BehaviorTreeManager : MonoBehaviour
    {
        private static BehaviorTreeManager _instance;
        [SerializeField] private List<BehaviorTreeRunner> behaviorTrees;
#if UNITY_EDITOR
        public List<BehaviorTreeRunner> BehaviorTrees => behaviorTrees;
#endif
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            behaviorTrees = new List<BehaviorTreeRunner>();
        }

        private void Update()
        {
            for (int i = 0; i < behaviorTrees.Count; i++)
            {
                behaviorTrees[i].TreeUpdate();
            }
        }

        public static void AddTree(BehaviorTreeRunner behaviorTree)
        {
            if (_instance.behaviorTrees.Contains(behaviorTree)) return;
            _instance.behaviorTrees.Add(behaviorTree);
        }

        public static void RemoveTree(BehaviorTreeRunner behaviorTree)
        {
            if (_instance.behaviorTrees.Contains(behaviorTree))
                _instance.behaviorTrees.Remove(behaviorTree);
        }

        public static SharedVariableBase GetSharedVariable(List<SharedVariableBase> variables, string variableName)
        {
            if (variables == null || variables.Count == 0) return null;
            for (var i = 0; i < variables.Count; i++)
            {
                var sharedVariable = variables[i];
                if (sharedVariable.VariableName == variableName)
                {
                    return sharedVariable;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        public void ToggleDrawGizmos(bool enable)
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