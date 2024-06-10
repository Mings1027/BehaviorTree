using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BehaviorTreeTool.Scripts.CustomInterface;
using BehaviorTreeTool.Scripts.TreeUtil;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace BehaviorTreeTool.Editor
{
    [CustomEditor(typeof(Node), true)]
    public class NodeEditor : UnityEditor.Editor
    {
        private string _searchQuery = "";

        private readonly string[] _tabTitles = { "Tasks", "Variables", "Inspector" };
        private Vector2 _scrollPos;
        private Vector2 _noneSharedVarsScrollPos;
        private SharedVariableType _variableType;
        private string _variableName = "";
        private bool[] _foldouts;
        private string _foldoutKey;

        private Texture2D _upArrowTexture;
        private Texture2D _downArrowTexture;
        private Texture2D _leftArrowTexture;
        private Texture2D _rightArrowTexture;
        private Texture2D _removeTexture;

        private SerializedProperty _variablesProperty;
        private SerializedProperty _sharedDataProperty;

        private static bool _arrayFoldout;
        private static int _selectedTab;
        private static bool _showValues;

        // 초기화 시 호출되는 함수
        private void OnEnable()
        {
            _sharedDataProperty = serializedObject.FindProperty("sharedData");

            if (_sharedDataProperty == null)
            {
                Debug.LogError("Failed to find 'sharedData' property. Make sure it exists in the Node class.");
                return;
            }

            _upArrowTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BehaviorTreeTool/Sprites/Arrow Simple Up.png");
            _downArrowTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BehaviorTreeTool/Sprites/Arrow Simple Down.png");
            _leftArrowTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BehaviorTreeTool/Sprites/Arrow Simple Left.png");
            _rightArrowTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BehaviorTreeTool/Sprites/Arrow Simple Right.png");

            _removeTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BehaviorTreeTool/Sprites/Remove.png");

            UpdateSerializedVariables();
        }

        // Inspector GUI를 그리는 함수
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DisplayTreeName();
            DisplayTabs();
            CheckAssignSharedData();

            serializedObject.ApplyModifiedProperties();
        }

        // 변수 리스트 업데이트 함수
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

        // 트리 이름을 표시하는 함수
        private void DisplayTreeName()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
            };

            EditorGUILayout.LabelField($"Behavior Tree : {BehaviorTreeEditor.treeName}", style);

            var node = (Node)target;
            var treeName = node.name; // Replace with the actual way to get the tree name
            var nodeType = TreeUtility.GetNodeTypeName(node.GetType());

            EditorGUILayout.LabelField($"Node : {treeName} - {nodeType}", style);
        }

        // 탭을 표시하는 함수
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

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

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

            EditorGUILayout.EndScrollView();
        }

#region DrawTasksTab

        // Tasks 탭을 그리는 함수
        private void DrawTasksTab()
        {
            DrawSearchField();
            GUILayout.Space(3);
            TreeUtility.DrawHorizontalLine(Color.gray);
            DrawNodeTypeButtons<ActionNode>();
            DrawNodeTypeButtons<CompositeNode>();
            DrawNodeTypeButtons<ConditionNode>();
            DrawNodeTypeButtons<DecoratorNode>();
        }

        // 검색 필드를 그리는 함수
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

        // 노드 타입 버튼을 그리는 함수
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

        // 노드를 생성하는 함수
        private static void CreateNode(Type type)
        {
            var window = EditorWindow.GetWindow<BehaviorTreeEditor>();
            var treeView = window.TreeView;
            treeView?.CreateNode(type);
        }

#endregion

#region DrawVariablesTab

        // Variables 탭을 그리는 함수
        private void DrawVariablesTab()
        {
            var sharedData = (SharedData)serializedObject.FindProperty("sharedData").objectReferenceValue;
            if (!sharedData) return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(50));
            _variableName = EditorGUILayout.TextField(_variableName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type", GUILayout.Width(50));
            _variableType = (SharedVariableType)EditorGUILayout.EnumPopup(_variableType);

            if (GUILayout.Button("Add", GUILayout.Width(50)))
            {
                AddVariable(_variableName, _variableType, sharedData);
            }

            EditorGUILayout.EndHorizontal();

            TreeUtility.DrawHorizontalLine(Color.gray);
            TreeUtility.DrawHorizontalLine(Color.gray);
            DrawVariablesList();
        }

        // 변수를 추가하는 함수
        private void AddVariable(string variableName, SharedVariableType variableType, SharedData sharedData)
        {
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

                var serializedSharedData = new SerializedObject(sharedData);
                _variablesProperty = serializedSharedData.FindProperty("variables");

                Array.Resize(ref _foldouts, _variablesProperty.arraySize);
                _foldouts[_variablesProperty.arraySize - 1] = true;

                EditorUtility.SetDirty(sharedData);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogError("새 변수를 생성하지 못했습니다");
            }

            _variableName = "";
        }

        // 변수 리스트를 그리는 함수
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
                // 스타일 설정
                var style = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 1.0f)) }, // 기본 색상
                    hover = { background = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 1.0f)) }, // 호버 시 색상
                    padding = new RectOffset(10, 10, 5, 5),
                    margin = new RectOffset(4, 4, 2, 2)
                };

                EditorGUILayout.BeginVertical(style);

                EditorGUILayout.BeginHorizontal();

                _foldouts[i] = EditorGUILayout.Foldout(_foldouts[i],
                    $"{variables[i].VariableName} ({TreeUtility.DisplayType(variables[i])})", true);

                if (GUILayout.Button(new GUIContent(_upArrowTexture), GUILayout.Width(21), GUILayout.Height(21)))
                {
                    MoveVariableUp(i);
                    break;
                }

                if (GUILayout.Button(new GUIContent(_downArrowTexture), GUILayout.Width(21), GUILayout.Height(21)))
                {
                    MoveVariableDown(i);
                    break;
                }
                if (GUILayout.Button(new GUIContent(_removeTexture), GUILayout.Width(21), GUILayout.Height(21)))
                {
                    var confirmDelete = EditorUtility.DisplayDialog("Delete Variable",
                        $"Are you sure you want to delete the variable '{variables[i].VariableName}'?", "Yes", "No");
                    if (confirmDelete)
                    {
                        DeleteVariable(variables[i].VariableName, i);
                        TreeUtility.SaveFoldoutStates(_foldoutKey, _foldouts);
                    }
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (_foldouts[i])
                {
                    EditorGUILayout.Space();

                    // IComponent 인터페이스를 상속받았는지 확인하고, UseGetComponent 필드 추가 렌더링
                    if (variables[i] is IObject componentVariable)
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

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Name", GUILayout.Width(50));
                    EditorGUILayout.TextField(variables[i].VariableName);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Type", GUILayout.Width(50));
                    EditorGUILayout.EnumPopup(TreeUtility.DisplayType(variables[i]));
                    EditorGUILayout.EndHorizontal();

                    TreeUtility.DrawSharedVariableValueField(variables[i], "Value");
                }

                EditorGUILayout.Space(1);
                EditorGUILayout.EndVertical();

                TreeUtility.DrawHorizontalLine(Color.gray);
            }
            TreeUtility.SaveFoldoutStates(_foldoutKey, _foldouts);
        }

        // 텍스처 생성
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        // 변수를 위로 이동하는 함수
        private void MoveVariableUp(int index)
        {
            if (index > 0)
            {
                _variablesProperty.MoveArrayElement(index, index - 1);
                _variablesProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        // 변수를 아래로 이동하는 함수
        private void MoveVariableDown(int index)
        {
            if (index < _variablesProperty.arraySize - 1)
            {
                _variablesProperty.MoveArrayElement(index, index + 1);
                _variablesProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        // 변수를 삭제하는 함수
        private void DeleteVariable(string variableName, int index)
        {
            var sharedData = (SharedData)serializedObject.FindProperty("sharedData").objectReferenceValue;

            sharedData.RemoveVariable(variableName);
            _variablesProperty.DeleteArrayElementAtIndex(index);

            Array.Resize(ref _foldouts, _variablesProperty.arraySize);
        }

        // 변수 이름 중복 확인 함수
        private bool IsVariableNameDuplicate(string variableName, SharedData sharedData)
        {
            return sharedData.Variables.Any(variable => variable.VariableName == variableName);
        }

#endregion

#region DrawInspectorTab

        // Inspector 탭을 그리는 함수
        private void DrawInspectorTab()
        {
            var node = (Node)target;

            DrawDescriptionField();
            TreeUtility.DrawHorizontalLine(Color.gray);
            DrawSharedDataField(node);
            TreeUtility.DrawHorizontalLine(Color.gray);
            DrawSharedVariableFields(node);
            TreeUtility.DrawHorizontalLine(Color.gray);
            DrawNonSharedVariableFields(node);
            TreeUtility.DrawHorizontalLine(Color.gray);
            DrawNoneSharedVariables();
        }

        // 설명 필드를 그리는 함수
        private void DrawDescriptionField()
        {
            var descriptionProperty = serializedObject.FindProperty("description");
            if (descriptionProperty != null)
            {
                EditorGUILayout.PropertyField(descriptionProperty, true);
            }
        }

        // SharedData 필드를 그리는 함수
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
            // "Shared Variables" 라벨을 굵은 글씨로 출력
            EditorGUILayout.LabelField("Shared Variables", EditorStyles.boldLabel);

            // Node 클래스에서 모든 SharedVariableBase 타입 필드를 가져와서 처리
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
            var style = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 1.0f)) }, // 기본 색상
                hover = { background = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 1.0f)) }, // 호버 시 색상
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(4, 4, 2, 2)
            };
            EditorGUILayout.BeginVertical(style);
            EditorGUILayout.BeginHorizontal();

            // 버튼을 이용하여 섹션 열고 닫기
            var foldoutIcon = _foldouts[index] ? _downArrowTexture : _rightArrowTexture;
            if (GUILayout.Button(foldoutIcon, GUILayout.Width(20), GUILayout.Height(20)))
            {
                _foldouts[index] = !_foldouts[index];
            }

            EditorGUILayout.LabelField(kvp.Key, GUILayout.MinWidth(100));

            var variableStyle = new GUIStyle(GUI.skin.label);
            if (string.IsNullOrEmpty(kvp.Value.VariableName))
            {
                variableStyle.normal.textColor = new Color(1.0f, 0.5f, 0f);
                variableStyle.fontStyle = FontStyle.Bold;
            }

            var variableNames = node.SharedData.Variables
                .Where(v => v.GetType() == kvp.Value.GetType())
                .Select(v => v.VariableName)
                .ToList();

            variableNames.Insert(0, "(None)");

            var currentIndex = string.IsNullOrEmpty(kvp.Value.VariableName)
                ? 0
                : variableNames.IndexOf(kvp.Value.VariableName);

            var popupStyle = new GUIStyle(EditorStyles.popup);
            if (currentIndex == 0)
            {
                popupStyle.normal.textColor = new Color(1.0f, 0.5f, 0f);
                popupStyle.fontStyle = FontStyle.Bold;
            }

            var selectedIndex = EditorGUILayout.Popup(currentIndex, variableNames.ToArray(), popupStyle,
                GUILayout.Width(150));
            if (selectedIndex != currentIndex)
            {
                if (selectedIndex == 0)
                {
                    kvp.Value.VariableName = string.Empty;
                    kvp.Value.SetValue(null);
                }
                else
                {
                    var selectedVariable =
                        node.SharedData.Variables.First(v => v.VariableName == variableNames[selectedIndex]);
                    kvp.Value.VariableName = selectedVariable.VariableName;
                    kvp.Value.SetValue(selectedVariable.GetValue());
                }

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

        // Shared 변수가 아닌 필드를 그리는 함수
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
                else if (field.IsPublic || field.IsDefined(typeof(SerializeField), false))
                {
                    TreeUtility.DrawField(field, node);
                }
            }
        }

#endregion

        // SharedData 할당을 확인하는 함수
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

        // Shared 변수가 없는 변수들을 그리는 함수
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