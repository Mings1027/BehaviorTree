using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BehaviorTreeTool.Scripts.TreeUtil;
using UnityEditor;
using UnityEngine;

namespace BehaviorTreeTool.Editor
{
    [CustomEditor(typeof(Node), true)]
    public class NodeEditor : UnityEditor.Editor
    {
        private string _searchQuery = "";
        private readonly string[] _tabTitles = { "Tasks", "Variables", "Inspector" };
        private Vector2 _taskScrollPos;
        private Vector2 _variableScrollPos;
        private Vector2 _inspectorScrollPos;
        private Vector2 _noneSharedVarsScrollPos;
        private SharedVariableType _variableType;
        private string _variableName = "";
        private bool[] _foldouts;
        private string _foldoutKey;

        private Texture2D _upArrowTexture;
        private Texture2D _downArrowTexture;
        private Texture2D _rightArrowTexture;
        private Texture2D _removeTexture;

        private SerializedProperty _variablesProperty;
        private SerializedProperty _sharedDataProperty;

        private static int _selectedTab;

        private void OnEnable()
        {
            InitializeProperties();
            LoadTextures();
            UpdateSerializedVariables();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DisplayTreeName();
            DisplayTabs();
            CheckAssignSharedData();

            serializedObject.ApplyModifiedProperties();
        }

        private void InitializeProperties()
        {
            _sharedDataProperty = serializedObject.FindProperty("sharedData");

            if (_sharedDataProperty == null)
            {
                Debug.LogError("Failed to find 'sharedData' property. Make sure it exists in the Node class.");
            }
        }

        private void LoadTextures()
        {
            _upArrowTexture = LoadTexture("Assets/BehaviorTreeTool/Sprites/Arrow Simple Up.png");
            _downArrowTexture = LoadTexture("Assets/BehaviorTreeTool/Sprites/Arrow Simple Down.png");
            _rightArrowTexture = LoadTexture("Assets/BehaviorTreeTool/Sprites/Arrow Simple Right.png");
            _removeTexture = LoadTexture("Assets/BehaviorTreeTool/Sprites/Remove.png");
            return;

            static Texture2D LoadTexture(string path)
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
        }

        private void UpdateSerializedVariables()
        {
            if (_sharedDataProperty.objectReferenceValue is SharedData sharedData)
            {
                var serializedSharedData = new SerializedObject(sharedData);
                _variablesProperty = serializedSharedData.FindProperty("variables");

                if (_variablesProperty == null)
                {
                    Debug.LogError("Failed to find 'variables' property. Make sure it exists in the SharedData class.");
                    return;
                }

                _foldoutKey = $"{BehaviorTreeEditor.treeName}_SharedDataFoldouts";
                _foldouts = TreeUtility.LoadFoldoutStates(_foldoutKey, _variablesProperty.arraySize);

                if (_foldouts.Length != _variablesProperty.arraySize)
                {
                    Array.Resize(ref _foldouts, _variablesProperty.arraySize);
                }
            }
            else
            {
                Debug.LogError("Failed to find 'SharedData' object. Make sure it is assigned in the Node.");
            }
        }

        private void DisplayTreeName()
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
            var node = (Node)target;
            var treeName = node.name;
            var nodeType = TreeUtility.GetNodeTypeName(node.GetType());

            EditorGUILayout.LabelField($"Behavior Tree : {BehaviorTreeEditor.treeName}", style);
            EditorGUILayout.LabelField($"Node : {treeName} - {nodeType}", style);
        }

        private void DisplayTabs()
        {
            var tabStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                fixedHeight = 30
            };

            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabTitles, tabStyle);
            EditorGUILayout.Space(15);

            switch (_selectedTab)
            {
                case 0:
                    DrawTasksTab();
                    break;
                case 1:
                    DrawVariablesTab();
                    break;
                case 2:
                    DrawInspectorTab();
                    break;
            }
        }

#region DrawTasksTab

        private void DrawTasksTab()
        {
            DrawSearchField();
            GUILayout.Space(3);
            TreeUtility.DrawHorizontalLine(Color.gray);
            _taskScrollPos = EditorGUILayout.BeginScrollView(_taskScrollPos);
            DrawNodeTypeButtons<ActionNode>();
            DrawNodeTypeButtons<CompositeNode>();
            DrawNodeTypeButtons<ConditionNode>();
            DrawNodeTypeButtons<DecoratorNode>();
            EditorGUILayout.EndScrollView();
        }

        private void DrawSearchField()
        {
            var searchFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 14,
                fixedHeight = 20
            };

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Search", GUILayout.Width(50));
            _searchQuery = EditorGUILayout.TextField(_searchQuery, searchFieldStyle, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawNodeTypeButtons<T>() where T : Node
        {
            var nodeTypes = TypeCache.GetTypesDerivedFrom<T>()
                .Where(t => t.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));

            var title = typeof(T).Name.Replace("Node", "") + " Nodes";
            var foldout = EditorPrefs.GetBool($"{title}Foldout", true);
            var folderTitleStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };

            foldout = EditorGUILayout.Foldout(foldout, title, true, folderTitleStyle);
            EditorPrefs.SetBool($"{title}Foldout", foldout);

            if (foldout)
            {
                EditorGUI.indentLevel++;
                foreach (var type in nodeTypes)
                {
                    if (GUILayout.Button(type.Name))
                    {
                        CreateNode(type);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        private static void CreateNode(Type type)
        {
            var window = EditorWindow.GetWindow<BehaviorTreeEditor>();
            var treeView = window.TreeView;
            treeView?.CreateNode(type);
        }

#endregion

#region DrawVariablesTab

        private void DrawVariablesTab()
        {
            var sharedData = (SharedData)serializedObject.FindProperty("sharedData").objectReferenceValue;
            if (!sharedData) return;

            DrawVariableInputFields();

            TreeUtility.DrawHorizontalLine(Color.gray);
            _variableScrollPos = EditorGUILayout.BeginScrollView(_variableScrollPos);
            DrawVariablesList();
            EditorGUILayout.EndScrollView();
        }

        private void DrawVariableInputFields()
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
                AddVariable(_variableName, _variableType, (SharedData)_sharedDataProperty.objectReferenceValue);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AddVariable(string variableName, SharedVariableType variableType, SharedData sharedData)
        {
            variableName = variableName.Trim();
            if (string.IsNullOrEmpty(variableName))
            {
                EditorUtility.DisplayDialog("Invalid Variable Name", "Variable name cannot be empty.", "OK");
                return;
            }

            if (IsVariableNameDuplicate(variableName, sharedData))
            {
                EditorUtility.DisplayDialog("Duplicate Variable Name", "Variable name already exists.", "OK");
                return;
            }

            var newVariable = TreeUtility.CreateSharedVariable(variableName, variableType);

            if (newVariable != null)
            {
                sharedData.AddVariable(newVariable);
                UpdateVariablesFoldouts(sharedData);
                EditorUtility.SetDirty(sharedData);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogError("Failed to create new variable.");
            }

            _variableName = "";
        }

        private void UpdateVariablesFoldouts(SharedData sharedData)
        {
            var serializedSharedData = new SerializedObject(sharedData);
            _variablesProperty = serializedSharedData.FindProperty("variables");

            Array.Resize(ref _foldouts, _variablesProperty.arraySize);
            _foldouts[_variablesProperty.arraySize - 1] = true;
        }

        private void DrawVariablesList()
        {
            var node = (Node)target;
            var variables = node.SharedData.Variables;
            if (_foldouts.Length != variables.Count)
            {
                Array.Resize(ref _foldouts, variables.Count);
            }

            for (int i = 0; i < variables.Count; i++)
            {
                DrawVariableItem(variables[i], i);
            }

            TreeUtility.SaveFoldoutStates(_foldoutKey, _foldouts);
        }

        private void DrawVariableItem(SharedVariableBase variable, int index)
        {
            var style = CreateBoxStyle();

            EditorGUILayout.BeginVertical(style);
            EditorGUILayout.BeginHorizontal();

            _foldouts[index] = EditorGUILayout.Foldout(_foldouts[index],
                $"{variable.VariableName} ({TreeUtility.DisplayType(variable)})", true);

            DrawMoveButtons(index);
            DrawRemoveButton(variable, index);

            EditorGUILayout.EndHorizontal();

            if (_foldouts[index])
            {
                DrawVariableDetails(variable);
            }

            EditorGUILayout.EndVertical();
            TreeUtility.DrawHorizontalLine(Color.gray);
        }

        private GUIStyle CreateBoxStyle()
        {
            return new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 1.0f)) },
                hover = { background = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 1.0f)) },
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(4, 4, 2, 2)
            };
        }

        private void DrawMoveButtons(int index)
        {
            if (GUILayout.Button(new GUIContent(_upArrowTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                MoveVariableUp(index);
            }

            if (GUILayout.Button(new GUIContent(_downArrowTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                MoveVariableDown(index);
            }
        }

        private void DrawRemoveButton(SharedVariableBase variable, int index)
        {
            if (GUILayout.Button(new GUIContent(_removeTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                var confirmDelete = EditorUtility.DisplayDialog("Delete Variable",
                    $"Are you sure you want to delete the variable '{variable.VariableName}'?", "Yes", "No");
                if (confirmDelete)
                {
                    DeleteVariable(variable.VariableName, index);
                }
            }
        }

        private void DrawVariableDetails(SharedVariableBase variable)
        {
            EditorGUILayout.Space();

            if (variable is IComponentObject componentVariable)
            {
                DrawUseGetComponentField(componentVariable);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(50));
            EditorGUILayout.TextField(variable.VariableName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type", GUILayout.Width(50));
            EditorGUILayout.EnumPopup(TreeUtility.DisplayType(variable));
            EditorGUILayout.EndHorizontal();

            TreeUtility.DrawSharedVariableValueField(variable, "Value");
        }

        private void DrawUseGetComponentField(IComponentObject componentVariable)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Use GetComponent", GUILayout.Width(120));
            var useGetComponent = EditorGUILayout.Toggle(componentVariable.UseGetComponent);
            if (useGetComponent != componentVariable.UseGetComponent)
            {
                componentVariable.UseGetComponent = useGetComponent;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void MoveVariableUp(int index)
        {
            if (index > 0)
            {
                _variablesProperty.MoveArrayElement(index, index - 1);
                _variablesProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private void MoveVariableDown(int index)
        {
            if (index < _variablesProperty.arraySize - 1)
            {
                _variablesProperty.MoveArrayElement(index, index + 1);
                _variablesProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private void DeleteVariable(string variableName, int index)
        {
            var sharedData = (SharedData)serializedObject.FindProperty("sharedData").objectReferenceValue;

            sharedData.RemoveVariable(variableName);
            _variablesProperty.DeleteArrayElementAtIndex(index);

            Array.Resize(ref _foldouts, _variablesProperty.arraySize);
        }

        private bool IsVariableNameDuplicate(string variableName, SharedData sharedData)
        {
            return sharedData.Variables.Any(variable => variable.VariableName == variableName);
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

#endregion

#region DrawInspectorTab

        private void DrawInspectorTab()
        {
            var node = (Node)target;

            DrawDescriptionField();
            TreeUtility.DrawHorizontalLine(Color.gray);

            _inspectorScrollPos = EditorGUILayout.BeginScrollView(_inspectorScrollPos);
            DrawSharedDataField(node);
            TreeUtility.DrawHorizontalLine(Color.gray);
            DrawSharedVariableFields(node);
            TreeUtility.DrawHorizontalLine(Color.gray);
            DrawNonSharedVariableFields(node);
            TreeUtility.DrawHorizontalLine(Color.gray);
            DrawNoneSharedVariables();
            EditorGUILayout.EndScrollView();
        }

        private void DrawDescriptionField()
        {
            var descriptionProperty = serializedObject.FindProperty("description");
            if (descriptionProperty != null)
            {
                EditorGUILayout.PropertyField(descriptionProperty, true);
            }
        }

        private void DrawSharedDataField(Node node)
        {
            var sharedDataProperty = serializedObject.FindProperty("sharedData");
            EditorGUILayout.PropertyField(sharedDataProperty, new GUIContent("Shared Data"));

            if (sharedDataProperty.objectReferenceValue != null)
            {
                node.SharedData = (SharedData)sharedDataProperty.objectReferenceValue;
            }
        }

        private void DrawSharedVariableFields(Node node)
        {
            EditorGUILayout.LabelField("Shared Variables", EditorStyles.boldLabel);

            var sharedVariables = node.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => typeof(SharedVariableBase).IsAssignableFrom(field.FieldType))
                .Select(field =>
                    new KeyValuePair<string, SharedVariableBase>(field.Name, (SharedVariableBase)field.GetValue(node)))
                .ToList();

            if (_foldouts == null || _foldouts.Length != sharedVariables.Count)
            {
                _foldouts = new bool[sharedVariables.Count];
            }

            for (int i = 0; i < sharedVariables.Count; i++)
            {
                DrawSharedVariableField(node, sharedVariables[i], i);
                EditorGUILayout.Space(3);
            }
            TreeUtility.SaveFoldoutStates(_foldoutKey, _foldouts);
        }

        private void DrawSharedVariableField(Node node, KeyValuePair<string, SharedVariableBase> kvp, int index)
        {
            var style = CreateBoxStyle();
            EditorGUILayout.BeginVertical(style);
            EditorGUILayout.BeginHorizontal();

            var foldoutIcon = _foldouts[index] ? _downArrowTexture : _rightArrowTexture;
            if (GUILayout.Button(foldoutIcon, GUILayout.Width(20), GUILayout.Height(20)))
            {
                _foldouts[index] = !_foldouts[index];
            }

            EditorGUILayout.LabelField(kvp.Key, GUILayout.MinWidth(100));

            var variableNames = node.SharedData.Variables
                .Where(v => v.GetType() == kvp.Value.GetType())
                .Select(v => v.VariableName)
                .ToList();

            variableNames.Insert(0, "(None)");

            var currentIndex = string.IsNullOrEmpty(kvp.Value.VariableName)
                ? 0
                : variableNames.IndexOf(kvp.Value.VariableName);

            var selectedIndex = EditorGUILayout.Popup(currentIndex, variableNames.ToArray(), GUILayout.Width(150));
            if (selectedIndex != currentIndex)
            {
                UpdateVariableSelection(node, kvp.Value, variableNames, selectedIndex);
                EditorUtility.SetDirty(node);
            }

            EditorGUILayout.EndHorizontal();

            if (_foldouts[index] && selectedIndex != 0)
            {
                EditorGUI.indentLevel++;
                TreeUtility.DrawSharedVariableValueField(kvp.Value, "Value");
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }

        private void UpdateVariableSelection(Node node, SharedVariableBase variable, IReadOnlyList<string> variableNames,
            int selectedIndex)
        {
            if (selectedIndex == 0)
            {
                variable.VariableName = string.Empty;
                variable.SetValue(null);
            }
            else
            {
                var selectedVariable =
                    node.SharedData.Variables.First(v => v.VariableName == variableNames[selectedIndex]);
                variable.VariableName = selectedVariable.VariableName;
                variable.SetValue(selectedVariable.GetValue());
            }
        }

        private void DrawNonSharedVariableFields(Node node)
        {
            EditorGUILayout.LabelField("Non-Shared Variables", EditorStyles.boldLabel);

            foreach (var field in node.GetType()
                         .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (typeof(SharedVariableBase).IsAssignableFrom(field.FieldType) ||
                    field.IsDefined(typeof(HideInInspector), false))
                {
                    continue;
                }

                var property = serializedObject.FindProperty(field.Name);
                if (property != null)
                {
                    EditorGUILayout.PropertyField(property, new GUIContent(field.Name), true);
                }
                // else if (field.IsPublic || field.IsDefined(typeof(SerializeField), false))
                // {
                //     TreeUtility.DrawField(field, node);
                // }
            }
        }

#endregion

        private void CheckAssignSharedData()
        {
            var tree = BehaviorTreeEditor.tree;

            if (!tree) return;
            var nodesWithoutSharedData = new List<string>();
            BehaviorTree.Traverse(tree.RootNode, node =>
            {
                if (!node.SharedData)
                {
                    nodesWithoutSharedData.Add(node.name);
                }
            });
            if (nodesWithoutSharedData.Count > 0)
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                    normal = { textColor = new Color(1.0f, 0.5f, 0f) }
                };

                GUILayout.Label("*Please assign Shared Data in the Root Node*", style);
                GUILayout.Label("Unassigned Nodes", style);

                foreach (var nodeName in nodesWithoutSharedData)
                {
                    EditorGUILayout.LabelField(nodeName, style);
                }
            }
        }

        private void DrawNoneSharedVariables()
        {
            var tree = BehaviorTreeEditor.tree;

            if (!tree) return;

            var noneSharedVariables = new List<string>();

            BehaviorTree.Traverse(tree.RootNode, node =>
            {
                var nodeType = node.GetType();
                var sharedVariableFields = nodeType
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(field => typeof(SharedVariableBase).IsAssignableFrom(field.FieldType));

                foreach (var field in sharedVariableFields)
                {
                    var sharedVariable = (SharedVariableBase)field.GetValue(node);
                    if (sharedVariable != null && string.IsNullOrEmpty(sharedVariable.VariableName))
                    {
                        noneSharedVariables.Add($"{node.name} - {field.Name}");
                    }
                }
            });

            if (noneSharedVariables.Count > 0)
            {
                var style = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(1.0f, 0.5f, 0f) }
                };

                EditorGUILayout.LabelField("Assign names in the Inspector tab.", style);

                var headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

                var nodeNameStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

                var variableNameStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(1.0f, 0.5f, 0f) },
                    alignment = TextAnchor.MiddleCenter
                };

                EditorGUILayout.BeginHorizontal("box");
                GUILayout.Label("Node Name", headerStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label("Variable Name", headerStyle, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                _noneSharedVarsScrollPos =
                    EditorGUILayout.BeginScrollView(_noneSharedVarsScrollPos, GUILayout.Height(200));

                foreach (var noneSharedVariable in noneSharedVariables)
                {
                    var parts = noneSharedVariable.Split(new[] { " - " }, StringSplitOptions.None);
                    var nodeName = parts[0];
                    var variableName = parts[1];

                    EditorGUILayout.BeginHorizontal("box");
                    GUILayout.Label(nodeName, nodeNameStyle, GUILayout.ExpandWidth(true));
                    GUILayout.Label(variableName, variableNameStyle, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }
}