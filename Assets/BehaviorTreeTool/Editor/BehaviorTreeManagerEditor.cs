using UnityEngine;
using UnityEditor;
using Tree;

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
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Behavior Trees", EditorStyles.boldLabel);

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("Please enter Play Mode.");
                return;
            }

            bool enable = !_manager.BehaviorTrees[0]?.Tree?.RootNode?.drawGizmos ?? false;
            var buttonText = enable ? "Enable Draw Gizmos" : "Disable Draw Gizmos";

            if (GUILayout.Button(buttonText))
            {
                _manager.ToggleDrawGizmos(enable);
            }

            EditorGUILayout.Space();

            if (_manager.BehaviorTrees != null && _manager.BehaviorTrees.Count > 0)
            {
                for (int i = 0; i < _manager.BehaviorTrees.Count; i++)
                {
                    EditorGUILayout.ObjectField(_manager.BehaviorTrees[i], typeof(BehaviorTreeRunner), true);
                }
            }
            else
            {
                EditorGUILayout.LabelField("No Behavior Trees found.");
            }
        }
    }
}