using System.Collections.Generic;
using UnityEngine;

namespace Tree
{
    public class BehaviorManager : MonoBehaviour
    {
        private static BehaviorManager _instance;

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