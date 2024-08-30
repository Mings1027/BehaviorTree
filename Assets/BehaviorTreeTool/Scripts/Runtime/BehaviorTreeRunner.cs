using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Tree
{
    [DisallowMultipleComponent]
    public class BehaviorTreeRunner : MonoBehaviour
    {
#if UNITY_EDITOR
        public BehaviorTree Tree
        {
            get => behaviorTree;
            set
            {
                behaviorTree = value;
                UpdateVariables();
            }
        }

        public bool EnableVariables
        {
            get => enableVariables;
            set
            {
                enableVariables = value;
                UpdateVariables();
            }
        }

        public bool DrawGizmos
        {
            get => drawGizmos;
            set
            {
                drawGizmos = value;
                SetDrawGizmosForAllNodes(value);
            }
        }

        [SerializeField] private bool drawGizmos;
#endif
        [Tooltip("Enable if reference type variables need assignment before play.")] [SerializeField]
        private bool enableVariables;

        [SerializeField] protected BehaviorTree behaviorTree;
        [SerializeReference] private List<SharedVariableBase> variables = new();

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
            behaviorTree = behaviorTree.Clone(transform);
            AssignSharedVariables();
            BehaviorTree.Traverse(behaviorTree.RootNode, n => n.Init());
        }

        public void TreeUpdate()
        {
            behaviorTree.TreeUpdate();
        }

        public void UpdateVariables()
        {
            if (!enableVariables)
            {
                variables.Clear();
                return;
            }

            if (behaviorTree == null || behaviorTree.SharedData.Variables == null ||
                behaviorTree.SharedData.Variables.Count == 0)
            {
                variables.Clear();
                Debug.LogWarning("Behavior tree is null or Shared data has no variables");
                return;
            }

            // Ensure the variables list is initialized.
            variables.Clear();
            foreach (var variable in behaviorTree.SharedData.Variables)
            {
                variables.Add(variable.Clone());
            }
        }

        /// <summary>
        /// Assign shared variables to nodes in the tree.
        /// </summary>
        private void AssignSharedVariables()
        {
            var nodeList = behaviorTree.Nodes; // 트리의 모든 노드가 담긴 노드 리스트
            var sharedVariableList = enableVariables ? variables : behaviorTree.SharedData.Variables;
            if (!enableVariables) variables = null;

            for (var i = 0; i < nodeList.Count; i++)
            {
                var node = nodeList[i];
                // 노드를 순회하며 public 변수들을 가져옵니다.
                var fieldInfos = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                for (var j = 0; j < fieldInfos.Length; j++)
                {
                    var field = fieldInfos[j]; // 가져온 변수들 중 SharedVariableBase타입인것만 고릅니다.
                    if (!typeof(SharedVariableBase).IsAssignableFrom(field.FieldType)) continue;

                    // node에 선언된 SharedVariableBase변수
                    var variable = (SharedVariableBase)field.GetValue(node);

                    // sharedVariableList에서 variable.VariableName인 변수를 찾아 리턴시킵니다.
                    var sharedVariable =
                        BehaviorTreeManager.GetSharedVariable(sharedVariableList, variable.VariableName);

                    // 찾은 변수를 node에 public Shared변수에 할당합니다.
                    field.SetValue(node, sharedVariable);
                }
            }
        }

#if UNITY_EDITOR

        private void SetDrawGizmosForAllNodes(bool value)
        {
            BehaviorTree.Traverse(behaviorTree.RootNode, n => { n.drawGizmos = value; });
        }

        private void OnDrawGizmos()
        {
            if (!behaviorTree) return;

            BehaviorTree.Traverse(behaviorTree.RootNode, n =>
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