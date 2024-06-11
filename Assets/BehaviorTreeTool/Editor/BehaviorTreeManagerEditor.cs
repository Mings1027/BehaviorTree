using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviorTreeTool.Editor
{
    [CustomEditor(typeof(BehaviorTreeManager))]
    public class BehaviorTreeManagerEditor : UnityEditor.Editor
    {
        private BehaviorTreeManager _manager;

        private void OnEnable()
        {
            _manager = (BehaviorTreeManager)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

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
                    if (GUILayout.Button(_manager.drawGizmos ? "Disable Gizmos" : "Enable Gizmos"))
                    {
                        _manager.ToggleDrawGizmos();
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("Run the game to see the behavior trees.");
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