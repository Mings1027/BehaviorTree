using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviorTreeTool.Editor
{
    [CustomEditor(typeof(BehaviorTreeManager))]
    public class BehaviorTreeManagerEditor : UnityEditor.Editor
    {
        private BehaviorTreeManager _manager;
        private bool _gizmosEnabled = false;

        private void OnEnable()
        {
            _manager = (BehaviorTreeManager)target;
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Behavior Trees", EditorStyles.boldLabel);

                if (_manager != null)
                {
                    var behaviorTrees = GetPrivateField<List<IBehaviorTree>>(_manager, "_behaviorTree");

                    if (behaviorTrees != null)
                    {
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < behaviorTrees.Count; i++)
                        {
                            var behaviorTree = behaviorTrees[i];
                            if (GUILayout.Button($"Tree {i + 1}: {behaviorTree.GetType().Name}"))
                            {
                                var treeObject = behaviorTree as MonoBehaviour;
                                if (treeObject != null)
                                {
                                    Selection.activeObject = treeObject.gameObject;
                                }
                            }
                        }
                        EditorGUI.indentLevel--;
                    }

                    // Draw the drawGizmos toggle
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Debug Options", EditorStyles.boldLabel);
                    if (GUILayout.Button(_gizmosEnabled ? "Disable Gizmos" : "Enable Gizmos"))
                    {
                        _gizmosEnabled = !_gizmosEnabled;
                        _manager.ToggleDrawGizmos(_gizmosEnabled);
                    }

                    // Add a button to select all behavior trees
                    if (GUILayout.Button("Select All Behavior Trees"))
                    {
                        SelectAllBehaviorTrees(behaviorTrees);
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("Run the game to see the behavior trees.");
            }
        }

        private void SelectAllBehaviorTrees(List<IBehaviorTree> behaviorTrees)
        {
            if (behaviorTrees != null && behaviorTrees.Count > 0)
            {
                // Use a list to store the transforms of all selected GameObjects
                var transforms = new List<Transform>();

                foreach (var behaviorTree in behaviorTrees)
                {
                    var treeObject = behaviorTree as MonoBehaviour;
                    if (treeObject != null)
                    {
                        transforms.Add(treeObject.transform);
                    }
                }

                // Use Selection.objects to set the transforms without changing their hierarchy
                Selection.objects = transforms.ConvertAll(t => t.gameObject).ToArray();
            }
        }

        private T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (T)field.GetValue(target);
            }
            return default;
        }
    }
}