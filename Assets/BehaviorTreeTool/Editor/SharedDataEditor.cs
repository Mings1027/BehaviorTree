using System;
using System.Linq;
using BehaviorTreeTool.Scripts.TreeUtil;
using UnityEditor;
using UnityEngine;

namespace BehaviorTreeTool.Editor
{
    [CustomEditor(typeof(SharedData))]
    public class SharedDataEditor : UnityEditor.Editor
    {
        private Texture2D _upArrowTexture;
        private Texture2D _downArrowTexture;
        private Texture2D _removeTexture;

        private SerializedProperty _variablesProperty;
        private string _variableName;
        private SharedVariableType _variableType;

        private bool[] _foldouts;
        private const string FoldoutKeyPrefix = "SharedDataEditor_Foldout_";

        private void OnEnable()
        {
            _variablesProperty = serializedObject.FindProperty("variables");
            LoadTextures();
            LoadFoldoutStates();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawVariableInputField();
            if (_variablesProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No variables available. Add a new variable to get started.", MessageType.Info);
            }
            else
            {
                for (var i = 0; i < _variablesProperty.arraySize; i++)
                {
                    var variableProperty = _variablesProperty.GetArrayElementAtIndex(i);
                    DrawVariable(variableProperty, i);
                    TreeUtility.DrawHorizontalLine(Color.gray);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void LoadTextures()
        {
            _upArrowTexture = TreeUtility.LoadTexture("Assets/BehaviorTreeTool/Sprites/Arrow Simple Up.png");
            _downArrowTexture = TreeUtility.LoadTexture("Assets/BehaviorTreeTool/Sprites/Arrow Simple Down.png");
            _removeTexture = TreeUtility.LoadTexture("Assets/BehaviorTreeTool/Sprites/Remove.png");

        }

        private void DrawVariableInputField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(50));
            _variableName = EditorGUILayout.TextField(_variableName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type", GUILayout.Width(50));
            _variableType = (SharedVariableType)EditorGUILayout.EnumPopup(_variableType);

            if (GUILayout.Button("Add", GUILayout.Width(50)))
            {
                AddVariable();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void AddVariable()
        {
            _variableName = _variableName.Trim();
            if (string.IsNullOrEmpty(_variableName))
            {
                EditorUtility.DisplayDialog("Invalid Variable Name", "Variable name cannot be empty.", "OK");
                return;
            }
            if (IsVariableNameDuplicate())
            {
                EditorUtility.DisplayDialog("Duplicate Variable Name", "Variable name already exists.", "OK");
                return;
            }
            var newVariable = TreeUtility.CreateSharedVariable(_variableName, _variableType);

            if (newVariable != null)
            {
                _variablesProperty.InsertArrayElementAtIndex(_variablesProperty.arraySize);
                var newVariableProperty =
                    _variablesProperty.GetArrayElementAtIndex(_variablesProperty.arraySize - 1);
                newVariableProperty.managedReferenceValue = newVariable;
                serializedObject.ApplyModifiedProperties();
                Array.Resize(ref _foldouts, _variablesProperty.arraySize); // Resize foldouts array
                SaveFoldoutStates(); // Save updated foldout states
            }
        }

        private bool IsVariableNameDuplicate()
        {
            var sharedData = (SharedData)target;
            return sharedData.Variables.Any(variable => variable.VariableName == _variableName);
        }

        private void DrawVariable(SerializedProperty variableProperty, int index)
        {
            var variableName = variableProperty.FindPropertyRelative("variableName").stringValue;
            var variableTypeProperty = variableProperty.FindPropertyRelative("variableType");
            var variableType = (SharedVariableType)variableTypeProperty.enumValueIndex;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();

            _foldouts[index] = EditorGUILayout.Foldout(_foldouts[index], $"{variableName} ({variableType})", true);

            if (GUILayout.Button(new GUIContent(_upArrowTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                MoveVariable(index, index - 1);
            }

            if (GUILayout.Button(new GUIContent(_downArrowTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                MoveVariable(index, index + 1);
            }

            if (GUILayout.Button(new GUIContent(_removeTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                if (EditorUtility.DisplayDialog("Delete Variable",
                        $"Are you sure you want to delete the variable '{variableName}'?", "Yes", "No"))
                {
                    RemoveVariable(index);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (_foldouts[index])
            {
                if (variableProperty.managedReferenceValue is IComponentObject componentObject)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Use GetComponent", GUILayout.Width(120));
                    componentObject.UseGetComponent = EditorGUILayout.Toggle(componentObject.UseGetComponent);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Name", GUILayout.Width(50));
                EditorGUILayout.PropertyField(variableProperty.FindPropertyRelative("variableName"), GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Type", GUILayout.Width(50));
                EditorGUI.BeginChangeCheck();
                var newVariableType = (SharedVariableType)EditorGUILayout.EnumPopup(variableType);
                if (EditorGUI.EndChangeCheck())
                {
                    ChangeVariableType(index, newVariableType);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Value", GUILayout.Width(50));
                EditorGUILayout.PropertyField(variableProperty.FindPropertyRelative("value"), GUIContent.none);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            SaveFoldoutStates(); // Save updated foldout states
        }

        private void ChangeVariableType(int index, SharedVariableType newVariableType)
        {
            var variableProperty = _variablesProperty.GetArrayElementAtIndex(index);
            var variableName = variableProperty.FindPropertyRelative("variableName").stringValue;

            var newVariable = TreeUtility.CreateSharedVariable(variableName, newVariableType);

            if (newVariable != null)
            {
                _variablesProperty.GetArrayElementAtIndex(index).managedReferenceValue = newVariable;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void MoveVariable(int oldIndex, int newIndex)
        {
            if (newIndex >= 0 && newIndex < _variablesProperty.arraySize)
            {
                _variablesProperty.MoveArrayElement(oldIndex, newIndex);
                (_foldouts[oldIndex], _foldouts[newIndex]) = (_foldouts[newIndex], _foldouts[oldIndex]);
                serializedObject.ApplyModifiedProperties();
                SaveFoldoutStates(); // Save updated foldout states
            }
        }

        private void RemoveVariable(int index)
        {
            _variablesProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            Array.Resize(ref _foldouts, _variablesProperty.arraySize); // Resize foldouts array
            SaveFoldoutStates(); // Save updated foldout states
        }

        private void SaveFoldoutStates()
        {
            for (var i = 0; i < _foldouts.Length; i++)
            {
                EditorPrefs.SetBool(FoldoutKeyPrefix + i, _foldouts[i]);
            }
        }

        private void LoadFoldoutStates()
        {
            _foldouts = new bool[_variablesProperty.arraySize];
            for (var i = 0; i < _foldouts.Length; i++)
            {
                _foldouts[i] = EditorPrefs.GetBool(FoldoutKeyPrefix + i, false);
            }
        }
    }
}