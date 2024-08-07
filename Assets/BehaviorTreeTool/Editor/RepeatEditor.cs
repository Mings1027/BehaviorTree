using UnityEditor;
using Tree;

namespace BehaviorTreeTool.Editor
{
    [CustomEditor(typeof(Repeat))]
    public class RepeatEditor : UnityEditor.Editor
    {
        private SerializedProperty repeatForeverProperty;
        private SerializedProperty repeatCountProperty;

        private void OnEnable()
        {
            repeatForeverProperty = serializedObject.FindProperty("repeatForever");
            repeatCountProperty = serializedObject.FindProperty("repeatCount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(repeatForeverProperty);

            if (!repeatForeverProperty.boolValue)
            {
                EditorGUILayout.PropertyField(repeatCountProperty);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}