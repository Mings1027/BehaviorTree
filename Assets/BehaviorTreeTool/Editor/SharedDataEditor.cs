using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Tree;

namespace BehaviorTreeTool.Editor
{
    [CustomEditor(typeof(SharedData))]
    public class SharedDataEditor : UnityEditor.Editor
    {
        private Texture2D _upArrowTexture;
        private Texture2D _downArrowTexture;
        private Texture2D _removeTexture;
        private Vector2 _variablesScrollPos;
        private SerializedProperty _variablesProperty;
        private string _variableName;
        private SharedVariableType _variableType;

        private bool[] _foldouts;
        private const string FoldoutKeyPrefix = "SharedDataEditor_Foldout_";
        private readonly Dictionary<string, bool> _foldoutStates = new();

        private void OnEnable()
        {
            if (target is SharedData sharedData)
            {
                if (sharedData.Variables != null)
                {
                    _variablesProperty = serializedObject.FindProperty("variables");
                    LoadTextures();
                    LoadFoldoutStates();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawVariableInputField();
            TreeUtility.DrawHorizontalLine(Color.gray);

            if (_variablesProperty.arraySize == 0)
            {
                DrawNoVariablesMessage();
            }
            else
            {
                DrawVariables();
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
            EditorGUILayout.LabelField("Name", GUILayout.Width(70));
            _variableName = EditorGUILayout.TextField(_variableName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type", GUILayout.Width(70));
            _variableType = (SharedVariableType)EditorGUILayout.EnumPopup(_variableType);

            if (GUILayout.Button("Add", GUILayout.Width(60)))
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

            _variableName = string.Empty;
        }

        private bool IsVariableNameDuplicate()
        {
            var sharedData = (SharedData)target;
            return sharedData.Variables.Any(variable => variable.VariableName == _variableName);
        }

        private void DrawNoVariablesMessage()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1.0f, 0.5f, 0f) }
            };

            GUILayout.Label("No variables available.\nAdd a new variable to get started.", style);
            EditorGUILayout.EndVertical();
        }

        private void DrawVariables()
        {
            _variablesScrollPos = EditorGUILayout.BeginScrollView(_variablesScrollPos);
            for (var i = 0; i < _variablesProperty.arraySize; i++)
            {
                var variableProperty = _variablesProperty.GetArrayElementAtIndex(i);
                DrawVariable(variableProperty, i);
                TreeUtility.DrawHorizontalLine(Color.gray);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawVariable(SerializedProperty variableProperty, int index)
        {
            var variableNameProperty = variableProperty.FindPropertyRelative("variableName");
            var variableTypeProperty = variableProperty.FindPropertyRelative("variableType");

            var variableName = variableNameProperty.stringValue;

            var style = new GUIStyle(GUI.skin.box)
            {
                normal = { background = TreeUtility.MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 1.0f)) },
                hover = { background = TreeUtility.MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 1.0f)) },
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(4, 4, 2, 2)
            };

            EditorGUILayout.BeginVertical(style);
            EditorGUILayout.BeginHorizontal();

            _foldouts[index] = EditorGUILayout.Foldout(_foldouts[index], $"{variableName}", true);

            if (GUILayout.Button(new GUIContent(_upArrowTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                MoveVariable(index, index - 1);
                return;
            }

            if (GUILayout.Button(new GUIContent(_downArrowTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                MoveVariable(index, index + 1);
                return;
            }

            if (GUILayout.Button(new GUIContent(_removeTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                if (EditorUtility.DisplayDialog("Delete Variable",
                        $"Are you sure you want to delete the variable '{variableNameProperty}'?", "Yes", "No"))
                {
                    RemoveVariable(index);
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();

            if (_foldouts[index])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Name", GUILayout.Width(70));
                EditorGUILayout.PropertyField(variableProperty.FindPropertyRelative("variableName"), GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Type", GUILayout.Width(70));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(variableProperty.FindPropertyRelative("variableType"), GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    var newVariable = TreeUtility.CreateSharedVariable(variableNameProperty.stringValue, (SharedVariableType)variableTypeProperty.enumValueIndex);
                    if (newVariable != null)
                    {
                        variableProperty.managedReferenceValue = newVariable;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                EditorGUILayout.EndHorizontal();

                DrawSharedVariableField(variableProperty);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            SaveFoldoutStates(); // Save updated foldout states
        }

        private void DrawSharedVariableField(SerializedProperty variableProperty)
        {
            var valueProperty = variableProperty.FindPropertyRelative("value");
            EditorGUILayout.PropertyField(valueProperty, true);
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
