using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviorTreeTool.Editor
{
    [CustomEditor(typeof(BehaviorTreeManager))]
    public class BehaviorTreeManagerEditor : UnityEditor.Editor
    {
        private BehaviorTreeManager _manager;
        private bool _gizmosEnabled;
        private const string GizmosPrefKey = "BehaviorTreeManager_GizmosEnabled";

        private void OnEnable()
        {
            _manager = (BehaviorTreeManager)target;
            _gizmosEnabled = EditorPrefs.GetBool(GizmosPrefKey, false);
            _manager.ToggleDrawGizmos(_gizmosEnabled);
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
                            if (GUILayout.Button($"Tree {i + 1}: {behaviorTree.Name}"))
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
                        EditorPrefs.SetBool(GizmosPrefKey, _gizmosEnabled);
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
