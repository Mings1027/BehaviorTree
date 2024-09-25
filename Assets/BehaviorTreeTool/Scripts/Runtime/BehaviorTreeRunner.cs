using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tree
{
    [DisallowMultipleComponent]
    public class BehaviorTreeRunner : MonoBehaviour
    {
#if UNITY_EDITOR
        public BehaviorTree Tree
        {
            get => behaviorTree;
            set => behaviorTree = value;
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
        [SerializeField] protected BehaviorTree behaviorTree;
        [SerializeReference] private List<SharedVariableBase> variables = new();
        // [SerializeReference] private List<SharedVariableBase> runtimeVariables = new();


        private void OnEnable()
        {
            BehaviorManager.AddTree(this);
        }

        private void Awake()
        {
            InitializeTree();
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


        /// <summary>
        /// Assign shared variables to nodes in the tree.
        /// </summary>
        private void AssignSharedVariables()
        {
            var nodeList = behaviorTree.Nodes; // 트리의 모든 노드가 담긴 노드 리스트

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
                    var sharedVariable = GetSharedVariable(variables, variable.VariableName);

                    // 찾은 변수를 node의 public Shared변수에 할당합니다.
                    field.SetValue(node, sharedVariable);
                }
            }
        }

        private static SharedVariableBase GetSharedVariable(List<SharedVariableBase> variables, string variableName)
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

        public SharedVariableBase GetVariable(string variableName)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i].VariableName == variableName)
                    return variables[i];
            }

            return null;
        }

        public void SetVariable(string variableName, object value)
        {
            var variable = GetVariable(variableName);
            if (variable != null) variable.SetValue(value);
            else
            {
                throw new ArgumentException(
                    $"The variable with the name '{variableName}' does not exist in the runtimeVariables list.");
            }
        }
#if UNITY_EDITOR

        public void UpdateVariables()
        {
            var sharedDataVariables = behaviorTree.SharedData.Variables;

            if (behaviorTree == null || sharedDataVariables == null || sharedDataVariables.Count == 0)
            {
                variables.Clear();
                Debug.LogWarning("Behavior tree is null or Shared data has no variables");
                return;
            }

            // Ensure the variables list is initialized.
            variables.Clear();
            for (var i = 0; i < sharedDataVariables.Count; i++)
            {
                var variable = sharedDataVariables[i];
                variables.Add(variable.Clone());
            }
        }

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