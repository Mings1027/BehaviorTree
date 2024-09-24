using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Tree
{
    public class GlobalVariablesWindow : EditorWindow
    {
        private static GlobalVariablesWindow _instance;

        private Texture2D _upArrowTexture;
        private Texture2D _downArrowTexture;
        private Texture2D _removeTexture;

        private static string _variableName;
        private SharedVariableType _variableType;
        private Vector2 _scrollPosition;
        private GlobalVariables _globalVariableComponent;
        private List<SharedVariableBase> _globalData = new();

        [SerializeField, HideInInspector] private GameObject globalObject;

        // Add a menu item to open the editor window
        [MenuItem("BehaviorTree/Global Variables")]
        public static void ShowWindow()
        {
            var window =
                (GlobalVariablesWindow)GetWindow(typeof(GlobalVariablesWindow), false, "Global Variables", true);
            window.Show();
            _instance = window;
        }

        private void OnEnable()
        {
            if (_instance == null) ShowWindow();
            LoadTextures();
        }

        private void OnGUI()
        {
            DrawGlobalGUI();

            globalObject =
                (GameObject)EditorGUILayout.ObjectField("Global Object", globalObject, typeof(GameObject), true);
            if (globalObject == null)
            {
                globalObject = FindFirstObjectByType<GlobalVariables>()?.gameObject;
            }

            if (globalObject != null)
            {
                _globalVariableComponent = globalObject.GetComponent<GlobalVariables>();
                if (_globalVariableComponent.Variables.Count > 0)
                {
                    _globalData = _globalVariableComponent.Variables;
                }
                else
                {
                    _globalData.Clear();
                }
            }
            else
            {
                if (GUILayout.Button("Create Global Variable Object"))
                {
                    if (EditorUtility.DisplayDialog("Create a new Global Variables Object?",
                            "No Global Variables Object Found, create a new Object?", "Yes", "No"))
                    {
                        CreateGlobalVariablesObject();
                    }
                }

                EditorGUILayout.LabelField("No Global Variables Object, create a new one", EditorStyles.label);
                return;
            }

            TreeUtility.DrawHorizontalLine(Color.gray);
            EditorGUILayout.Space();

            DrawVariableInputField();

            if (_globalVariableComponent.Variables.Count == 0)
            {
                var notFoundStyle = new GUIStyle
                {
                    fontSize = 16,
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter,
                };
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("No global variables found.", notFoundStyle);
                GUILayout.FlexibleSpace();
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            TreeUtility.DrawHorizontalLine(Color.gray);
            DrawVariableList();
        }

        private void LoadTextures()
        {
            _upArrowTexture = TreeUtility.LoadTexture("Assets/BehaviorTreeTool/Sprites/Arrow Simple Up.png");
            _downArrowTexture = TreeUtility.LoadTexture("Assets/BehaviorTreeTool/Sprites/Arrow Simple Down.png");
            _removeTexture = TreeUtility.LoadTexture("Assets/BehaviorTreeTool/Sprites/Remove.png");
        }

        private void DrawGlobalGUI()
        {
            var globalStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.white },
                fontSize = 15
            };
            EditorGUILayout.LabelField("Global Variables", globalStyle);
        }

        private void CreateGlobalVariablesObject()
        {
            globalObject = new GameObject("Global Variables");
            _globalVariableComponent = globalObject.AddComponent<GlobalVariables>();
            _globalData = new List<SharedVariableBase>();
            _globalVariableComponent.Variables = _globalData;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
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

            if (GUILayout.Button("Add", GUILayout.Width(60)))
            {
                AddGlobalVariable(_variableName, _variableType);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawVariableList()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < _globalData.Count; i++)
            {
                DrawVariable(_globalData[i], i);
                TreeUtility.DrawHorizontalLine(Color.gray);
            }

            if (GUILayout.Button("Clear Global Variable List"))
            {
                if (EditorUtility.DisplayDialog("Remove All Global Variables",
                        "Are you sure you want to remove all variables?", "Yes", "No"))
                {
                    _globalData.Clear();
                    EditorUtility.SetDirty(globalObject);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawVariable(SharedVariableBase variable, int index)
        {
            var style = new GUIStyle
            {
                normal = { background = TreeUtility.MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 1.0f)) },
                hover = { background = TreeUtility.MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 1.0f)) },
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(4, 4, 2, 2)
            };

            EditorGUILayout.BeginVertical(style);
            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            // Move Up Button
            if (GUILayout.Button(new GUIContent(_upArrowTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                if (index > 0) // Ensure it's not the first item
                {
                    MoveVariable(index, index - 1);
                }
            }

            // Move Down Button
            if (GUILayout.Button(new GUIContent(_downArrowTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                if (index < _globalData.Count - 1) // Ensure it's not the last item
                {
                    MoveVariable(index, index + 1);
                }
            }

            // Remove Button
            if (GUILayout.Button(new GUIContent(_removeTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                if (EditorUtility.DisplayDialog("Delete Variable",
                        $"Are you sure you want to delete the variable '{variable.VariableName}'?", "Yes", "No"))
                {
                    _globalData.RemoveAt(index);
                    Repaint();
                }

                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(50));
            variable.VariableName = EditorGUILayout.TextField(variable.VariableName);
            EditorGUILayout.EndHorizontal();

            var currentVariableType = variable.VariableType;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type", GUILayout.Width(50));
            var newVariableType = (SharedVariableType)EditorGUILayout.EnumPopup(currentVariableType);
            EditorGUILayout.EndHorizontal();

            if (newVariableType != currentVariableType)
            {
                var newVariable = TreeUtility.CreateSharedVariable(variable.VariableName, newVariableType);
                if (newVariable != null)
                {
                    newVariable.SetValue(variable.GetValue());
                    _globalData[index] = newVariable;
                    variable = newVariable;
                }
            }

            TreeUtility.DrawSharedVariableValueField(variable, "Value");


            EditorGUILayout.EndVertical();
        }

        private void MoveVariable(int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex || oldIndex < 0 || oldIndex >= _globalData.Count ||
                newIndex < 0 || newIndex >= _globalData.Count)
            {
                return; // No valid movement
            }

            (_globalData[oldIndex], _globalData[newIndex]) =
                (_globalData[newIndex], _globalData[oldIndex]);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        public static void AddVariable(string variableName, SharedVariableType variableType)
        {
            if (_instance == null)
            {
                ShowWindow();
            }

            _instance.AddGlobalVariable(variableName, variableType);
        }

        private void AddGlobalVariable(string variableName, SharedVariableType variableType)
        {
            variableName = variableName.Trim();
            if (string.IsNullOrEmpty(variableName))
            {
                EditorUtility.DisplayDialog("Invalid Variable Name", "Variable name cannot be empty.", "OK");
                return;
            }

            if (IsVariableNameDuplicate(variableName))
            {
                EditorUtility.DisplayDialog("Duplicate Variable Name", "Variable name already exists.", "OK");
                return;
            }

            var newVariable = TreeUtility.CreateSharedVariable(variableName, variableType);
            if (newVariable != null)
            {
                if (globalObject != null)
                {
                    if (_globalData != null)
                    {
                        _globalVariableComponent.Variables.Add(newVariable);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "GlobalVariables object not found in the scene.", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Failed to create new variable.", "OK");
            }

            _variableName = string.Empty; // clearing the static field
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static bool IsVariableNameDuplicate(string variableName)
        {
            return FindFirstObjectByType<GlobalVariables>().Variables
                                                           .Any(variable => variable.VariableName == variableName);
        }
    }
}