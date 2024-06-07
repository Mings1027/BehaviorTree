using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private SharedVariableType _selectedType;
        private string _variableName = "";
        private bool[] _foldouts;
        private string _foldoutKey;

        private Texture2D _upArrowTexture;
        private Texture2D _downArrowTexture;
        private Texture2D _closeTexture;
        private Texture2D _plusTexture;
        private Texture2D _minusTexture;

        private SerializedProperty _variablesProperty;
        private SerializedProperty _sharedDataProperty;

        private readonly Dictionary<string, bool> _arrayFoldouts = new();
        private readonly Dictionary<string, bool> _listFoldouts = new();

        private static bool _arrayFoldout;
        private static int _selectedTab;
        private static bool _showValues;

        private static readonly Dictionary<Type, string> NodeTypeNames = new()
        {
            { typeof(ActionNode), "Action" },
            { typeof(CompositeNode), "Composite" },
            { typeof(ConditionNode), "Condition" },
            { typeof(DecoratorNode), "Decorator" },
            { typeof(RootNode), "Root" }
        };

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
            _closeTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BehaviorTreeTool/Sprites/Close.png");
            _plusTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BehaviorTreeTool/Sprites/Plus.png");
            _minusTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BehaviorTreeTool/Sprites/Minus.png");
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
            if (_sharedDataProperty.objectReferenceValue is SharedData sharedDataObject)
            {
                var serializedSharedData = new SerializedObject(sharedDataObject);
                _variablesProperty = serializedSharedData.FindProperty("variables");

                if (_variablesProperty == null)
                {
                    Debug.LogError("Failed to find 'variables' property. Make sure it exists in the SharedData class.");
                    return;
                }

                _foldoutKey = $"{BehaviorTreeEditor.treeName}_SharedDataFoldouts";
                _foldouts = LoadFoldoutStates(_foldoutKey, _variablesProperty.arraySize);

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
            var nodeType = GetNodeTypeName(node.GetType());

            EditorGUILayout.LabelField($"Node : {treeName} - {nodeType}", style);
        }

        // 노드 타입 이름을 가져오는 함수
        private static string GetNodeTypeName(Type type)
        {
            return NodeTypeNames.FirstOrDefault(nodeType => nodeType.Key.IsAssignableFrom(type)).Value ?? "Unknown";
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
            DrawHorizontalLine(Color.gray);
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
            _selectedType = (SharedVariableType)EditorGUILayout.EnumPopup(_selectedType);

            if (GUILayout.Button("Add", GUILayout.Width(50)))
            {
                AddVariable(_selectedType, _variableName, sharedData);
            }

            EditorGUILayout.EndHorizontal();

            DrawHorizontalLine(Color.gray);
            DrawHorizontalLine(Color.gray);
            DrawVariablesList();
        }

        // 변수를 추가하는 함수
        private void AddVariable(SharedVariableType type, string variableName, SharedData sharedData)
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

            var newVariable = CreateSharedVariable(type, variableName);

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
                EditorGUILayout.BeginHorizontal();
                _foldouts[i] = EditorGUILayout.Foldout(_foldouts[i],
                    $"{variables[i].VariableName} ({DisplayType(variables[i])})", true);
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
                if (GUILayout.Button(new GUIContent(_closeTexture), GUILayout.Width(21), GUILayout.Height(21)))
                {
                    DeleteVariable(variables[i].VariableName, i);
                    SaveFoldoutStates(_foldoutKey, _foldouts);
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                if (_foldouts[i])
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    if (variables[i] is IComponent componentVariable)
                    {
                        componentVariable.UseGetComponent =
                            EditorGUILayout.Toggle("Use GetComponent", componentVariable.UseGetComponent);
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Name", GUILayout.Width(50));
                    EditorGUILayout.TextField(variables[i].VariableName);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Type", GUILayout.Width(50));
                    EditorGUILayout.EnumPopup(DisplayType(variables[i]));
                    EditorGUILayout.EndHorizontal();

                    // DrawSharedVariableValueField(variables[i], "Value");

                    EditorGUILayout.Space(2);
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.Space(3);

                DrawHorizontalLine(Color.gray);
            }
        }

        private static SharedVariableType DisplayType(SharedVariableBase variable)
        {
            var sharedType = variable switch
            {
                SharedBool => SharedVariableType.Bool,
                SharedCollider => SharedVariableType.Collider,
                SharedColliderArray => SharedVariableType.ColliderArray,
                SharedColor => SharedVariableType.Color,
                SharedFloat => SharedVariableType.Float,
                SharedGameObject => SharedVariableType.GameObject,
                SharedGameObjectList => SharedVariableType.GameObjectList,
                SharedInt => SharedVariableType.Int,
                SharedLayerMask => SharedVariableType.LayerMask,
                SharedMaterial => SharedVariableType.Material,
                SharedNavMeshAgent => SharedVariableType.NavMeshAgent,
                SharedQuaternion => SharedVariableType.Quaternion,
                SharedRect => SharedVariableType.Rect,
                SharedString => SharedVariableType.String,
                SharedTransform => SharedVariableType.Transform,
                SharedTransformArray => SharedVariableType.TransformArray,
                SharedVector2 => SharedVariableType.Vector2,
                SharedVector2Int => SharedVariableType.Vector2Int,
                SharedVector3 => SharedVariableType.Vector3,
                SharedVector3Int => SharedVariableType.Vector3Int,
                _ => SharedVariableType.Bool
            };
            return sharedType;
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

        // 폴드아웃 상태를 저장하는 함수
        private static void SaveFoldoutStates(string foldoutKey, IReadOnlyList<bool> foldouts)
        {
            for (var i = 0; i < foldouts.Count; i++)
            {
                EditorPrefs.SetBool($"{foldoutKey}_{i}", foldouts[i]);
            }
        }

        // 폴드아웃 상태를 불러오는 함수
        private static bool[] LoadFoldoutStates(string foldoutKey, int size)
        {
            var foldouts = new bool[size];
            for (var i = 0; i < size; i++)
            {
                foldouts[i] = EditorPrefs.GetBool($"{foldoutKey}_{i}", false);
            }

            return foldouts;
        }

        // Shared 변수를 생성하는 함수
        private static SharedVariableBase CreateSharedVariable(SharedVariableType type, string variableName)
        {
            return type switch
            {
                SharedVariableType.Int => new SharedInt { VariableName = variableName },
                SharedVariableType.Float => new SharedFloat { VariableName = variableName },
                SharedVariableType.Transform => new SharedTransform { VariableName = variableName },
                SharedVariableType.Collider => new SharedCollider { VariableName = variableName },
                SharedVariableType.ColliderArray => new SharedColliderArray { VariableName = variableName },
                SharedVariableType.LayerMask => new SharedLayerMask { VariableName = variableName },
                SharedVariableType.Vector3 => new SharedVector3 { VariableName = variableName },
                SharedVariableType.TransformArray => new SharedTransformArray { VariableName = variableName },
                SharedVariableType.Bool => new SharedBool { VariableName = variableName },
                SharedVariableType.Color => new SharedColor { VariableName = variableName },
                SharedVariableType.GameObject => new SharedGameObject { VariableName = variableName },
                SharedVariableType.GameObjectList => new SharedGameObjectList { VariableName = variableName },
                SharedVariableType.Material => new SharedMaterial { VariableName = variableName },
                SharedVariableType.NavMeshAgent => new SharedNavMeshAgent { VariableName = variableName },
                SharedVariableType.Quaternion => new SharedQuaternion { VariableName = variableName },
                SharedVariableType.Rect => new SharedRect { VariableName = variableName },
                SharedVariableType.String => new SharedString { VariableName = variableName },
                SharedVariableType.Vector2 => new SharedVector2 { VariableName = variableName },
                SharedVariableType.Vector2Int => new SharedVector2Int { VariableName = variableName },
                SharedVariableType.Vector3Int => new SharedVector3Int { VariableName = variableName },
                _ => null
            };
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
            DrawHorizontalLine(Color.gray);
            DrawSharedDataField(node);
            DrawHorizontalLine(Color.gray);
            DrawSharedVariableFields(node);
            DrawHorizontalLine(Color.gray);
            DrawNonSharedVariableFields(node);
            DrawHorizontalLine(Color.gray);
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

            // 변수 값 표시 여부를 토글하는 UI를 생성
            _showValues = EditorGUILayout.Toggle("Show Values", _showValues);

            // Node 클래스에서 모든 SharedVariableBase 타입 필드를 가져와서 처리
            var sharedVariables = node.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => typeof(SharedVariableBase).IsAssignableFrom(field.FieldType))
                .Select(field =>
                    new KeyValuePair<string, SharedVariableBase>(field.Name, (SharedVariableBase)field.GetValue(node)));

            foreach (var kvp in sharedVariables)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawSharedVariableField(node, kvp);
                EditorGUILayout.Space(2);
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawSharedVariableField(Node node, KeyValuePair<string, SharedVariableBase> kvp)
        {
            EditorGUILayout.BeginHorizontal();
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
                GUILayout.MinWidth(100));
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

            if (_showValues && selectedIndex != 0)
            {
                EditorGUI.indentLevel++;
                DrawSharedVariableValueField(kvp.Value, "Value");
                EditorGUI.indentLevel--;
            }
        }

        private void DrawSharedVariableValueField(SharedVariableBase variable, string valueLabel)
        {
            switch (variable)
            {
                case SharedInt sharedInt:
                    var newIntValue = EditorGUILayout.IntField(valueLabel, sharedInt.Value);
                    if (sharedInt.Value != newIntValue)
                    {
                        sharedInt.SetValue(newIntValue);
                    }
                    break;
                case SharedFloat sharedFloat:
                    var newFloatValue = EditorGUILayout.FloatField(valueLabel, sharedFloat.Value);
                    if (!Mathf.Approximately(sharedFloat.Value, newFloatValue))
                    {
                        sharedFloat.SetValue(newFloatValue);
                    }
                    break;
                case SharedTransform sharedTransform:
                    var newTransformValue =
                        (Transform)EditorGUILayout.ObjectField(valueLabel, sharedTransform.Value, typeof(Transform),
                            true);
                    if (sharedTransform.Value != newTransformValue)
                    {
                        sharedTransform.SetValue(newTransformValue);
                    }
                    break;
                case SharedCollider sharedCollider:
                    var newColliderValue =
                        (Collider)EditorGUILayout.ObjectField(valueLabel, sharedCollider.Value, typeof(Collider), true);
                    if (sharedCollider.Value != newColliderValue)
                    {
                        sharedCollider.SetValue(newColliderValue);
                    }
                    break;
                case SharedColliderArray sharedColliderArray:
                    DrawArrayField(sharedColliderArray);
                    break;
                case SharedLayerMask sharedLayerMask:
                    LayerMask newLayerMaskValue = EditorGUILayout.LayerField(valueLabel, sharedLayerMask.Value);
                    if (sharedLayerMask.Value != newLayerMaskValue)
                    {
                        sharedLayerMask.SetValue(newLayerMaskValue);
                    }
                    break;
                case SharedVector3 sharedVector3:
                    var newVector3Value = EditorGUILayout.Vector3Field(valueLabel, sharedVector3.Value);
                    if (sharedVector3.Value != newVector3Value)
                    {
                        sharedVector3.SetValue(newVector3Value);
                    }
                    break;
                case SharedTransformArray sharedTransformArray:
                    DrawArrayField(sharedTransformArray);
                    break;
                case SharedBool sharedBool:
                    var newBoolValue = EditorGUILayout.Toggle(valueLabel, sharedBool.Value);
                    if (sharedBool.Value != newBoolValue)
                    {
                        sharedBool.SetValue(newBoolValue);
                    }
                    break;
                case SharedColor sharedColor:
                    var newColorValue = EditorGUILayout.ColorField(valueLabel, sharedColor.Value);
                    if (sharedColor.Value != newColorValue)
                    {
                        sharedColor.SetValue(newColorValue);
                    }
                    break;
                case SharedGameObject sharedGameObject:
                    var newGameObjectValue =
                        (GameObject)EditorGUILayout.ObjectField(valueLabel, sharedGameObject.Value, typeof(GameObject),
                            true);
                    if (sharedGameObject.Value != newGameObjectValue)
                    {
                        sharedGameObject.SetValue(newGameObjectValue);
                    }
                    break;
                case SharedGameObjectList sharedGameObjectList:
                    DrawListField(sharedGameObjectList);
                    break;
                case SharedMaterial sharedMaterial:
                    var newMaterialValue =
                        (Material)EditorGUILayout.ObjectField(valueLabel, sharedMaterial.Value, typeof(Material), true);
                    if (sharedMaterial.Value != newMaterialValue)
                    {
                        sharedMaterial.SetValue(newMaterialValue);
                    }
                    break;
                case SharedNavMeshAgent sharedNavMeshAgent:
                    var newNavMeshAgentValue = (NavMeshAgent)EditorGUILayout.ObjectField(valueLabel,
                        sharedNavMeshAgent.Value, typeof(NavMeshAgent), true);
                    if (sharedNavMeshAgent.Value != newNavMeshAgentValue)
                    {
                        sharedNavMeshAgent.SetValue(newNavMeshAgentValue);
                    }
                    break;
                case SharedQuaternion sharedQuaternion:
                {
                    var newEulerAngles =
                        EditorGUILayout.Vector3Field(valueLabel, sharedQuaternion.Value.eulerAngles);
                    if (sharedQuaternion.Value.eulerAngles != newEulerAngles)
                    {
                        sharedQuaternion.SetValue(Quaternion.Euler(newEulerAngles));
                    }
                    break;
                }
                case SharedRect sharedRect:
                    var newRectValue = EditorGUILayout.RectField(valueLabel, sharedRect.Value);
                    if (sharedRect.Value != newRectValue)
                    {
                        sharedRect.SetValue(newRectValue);
                    }
                    break;
                case SharedString sharedString:
                    var newStringValue = EditorGUILayout.TextField(valueLabel, sharedString.Value);
                    if (sharedString.Value != newStringValue)
                    {
                        sharedString.SetValue(newStringValue);
                    }
                    break;
                case SharedVector2 sharedVector2:
                    var newVector2Value = EditorGUILayout.Vector2Field(valueLabel, sharedVector2.Value);
                    if (sharedVector2.Value != newVector2Value)
                    {
                        sharedVector2.SetValue(newVector2Value);
                    }
                    break;
                case SharedVector2Int sharedVector2Int:
                    var newVector2IntValue = EditorGUILayout.Vector2IntField(valueLabel, sharedVector2Int.Value);
                    if (sharedVector2Int.Value != newVector2IntValue)
                    {
                        sharedVector2Int.SetValue(newVector2IntValue);
                    }
                    break;
                case SharedVector3Int sharedVector3Int:
                    var newVector3IntValue = EditorGUILayout.Vector3IntField(valueLabel, sharedVector3Int.Value);
                    if (sharedVector3Int.Value != newVector3IntValue)
                    {
                        sharedVector3Int.SetValue(newVector3IntValue);
                    }
                    break;
                default:
                    EditorGUILayout.LabelField("Unsupported SharedVariable type");
                    break;
            }
        }

        private void DrawArrayField<T>(SharedVariable<T[]> sharedVariableArray)
        {
            var array = sharedVariableArray.Value ?? Array.Empty<T>();

            EditorGUILayout.BeginHorizontal();

            _arrayFoldouts.TryAdd(sharedVariableArray.VariableName, false);

            _arrayFoldouts[sharedVariableArray.VariableName] =
                EditorGUILayout.Foldout(_arrayFoldouts[sharedVariableArray.VariableName],
                    $"Array Elements [{array.Length}]", true);

            var newSize = EditorGUILayout.IntField(array.Length, GUILayout.Width(50));
            if (GUILayout.Button(new GUIContent(_plusTexture), GUILayout.Width(20), GUILayout.Height(20)))
            {
                newSize++;
            }

            if (GUILayout.Button(new GUIContent(_minusTexture), GUILayout.Width(20), GUILayout.Height(20)))
            {
                newSize--;
            }

            EditorGUILayout.EndHorizontal();

            if (newSize < 0) newSize = 0;
            if (newSize != array.Length)
            {
                var newArray = new T[newSize];
                for (var i = 0; i < Mathf.Min(newSize, array.Length); i++)
                {
                    newArray[i] = array[i];
                }
                sharedVariableArray.SetValue(newArray);
                array = newArray;
            }

            if (_arrayFoldouts[sharedVariableArray.VariableName])
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < array.Length; i++)
                {
                    var newValue = (T)DrawFieldForType(typeof(T), array[i], $"Element {i}");
                    if (!EqualityComparer<T>.Default.Equals(array[i], newValue))
                    {
                        array[i] = newValue;
                        sharedVariableArray.SetValue(array);
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawListField<T>(SharedVariable<List<T>> sharedVariableList)
            where T : Object
        {
            var list = sharedVariableList.Value ?? new List<T>();

            _listFoldouts.TryAdd(sharedVariableList.VariableName, false);

            EditorGUILayout.BeginHorizontal();

            _listFoldouts[sharedVariableList.VariableName] =
                EditorGUILayout.Foldout(_listFoldouts[sharedVariableList.VariableName],
                    $"List Elements [{list.Count}]", true);

            var newSize = EditorGUILayout.IntField(list.Count, GUILayout.Width(50));
            if (GUILayout.Button(new GUIContent(_plusTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                newSize++;
            }

            if (GUILayout.Button(new GUIContent(_minusTexture), GUILayout.Width(21), GUILayout.Height(21)))
            {
                newSize--;
            }

            EditorGUILayout.EndHorizontal();

            if (newSize < 0) newSize = 0;
            if (newSize != list.Count)
            {
                var newList = new List<T>(newSize);
                for (var i = 0; i < Mathf.Min(newSize, list.Count); i++)
                {
                    newList.Add(list[i]);
                }
                for (var i = list.Count; i < newSize; i++)
                {
                    newList.Add(null);
                }
                sharedVariableList.SetValue(newList);
                list = newList;
            }

            if (_listFoldouts[sharedVariableList.VariableName])
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < list.Count; i++)
                {
                    var newValue = (T)EditorGUILayout.ObjectField($"Element {i}", list[i], typeof(T), true);
                    if (list[i] != newValue)
                    {
                        list[i] = newValue;
                        sharedVariableList.SetValue(list);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        // 타입에 맞는 필드를 그리는 함수
        private object DrawFieldForType(Type type, object value, string label)
        {
            if (type == typeof(int))
            {
                return EditorGUILayout.IntField(label, (int)value);
            }

            if (type == typeof(float))
            {
                return EditorGUILayout.FloatField(label, (float)value);
            }

            if (type == typeof(Transform))
            {
                return EditorGUILayout.ObjectField(label, (Transform)value, typeof(Transform), true);
            }

            if (type == typeof(Collider))
            {
                return EditorGUILayout.ObjectField(label, (Collider)value, typeof(Collider), true);
            }

            if (type == typeof(LayerMask))
            {
                return EditorGUILayout.LayerField(label, (LayerMask)value);
            }

            if (type == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(label, (Vector3)value);
            }

            if (type == typeof(Transform[]))
            {
                EditorGUILayout.LabelField(label, "Nested arrays not supported in custom editor.");
                return value;
            }

            EditorGUILayout.LabelField(label, "Unsupported type");
            return value;
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
                    DrawField(field, node);
                }
            }
        }

        // 필드를 그리는 함수
        private static void DrawField(FieldInfo field, Node node)
        {
            var fieldType = field.FieldType;
            var fieldValue = field.GetValue(node);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(field.Name, GUILayout.Width(EditorGUIUtility.labelWidth));

            if (fieldType == typeof(int))
            {
                field.SetValue(node, EditorGUILayout.IntField((int)fieldValue));
            }
            else if (fieldType == typeof(float))
            {
                field.SetValue(node, EditorGUILayout.FloatField((float)fieldValue));
            }
            else if (fieldType == typeof(string))
            {
                field.SetValue(node, EditorGUILayout.TextField((string)fieldValue));
            }
            else if (fieldType == typeof(bool))
            {
                field.SetValue(node, EditorGUILayout.Toggle((bool)fieldValue));
            }
            else if (fieldType == typeof(Vector3))
            {
                field.SetValue(node, EditorGUILayout.Vector3Field("", (Vector3)fieldValue));
            }
            else if (fieldType == typeof(Color))
            {
                field.SetValue(node, EditorGUILayout.ColorField((Color)fieldValue));
            }
            else if (typeof(Object).IsAssignableFrom(fieldType))
            {
                field.SetValue(node, EditorGUILayout.ObjectField((Object)fieldValue, fieldType, true));
            }
            else
            {
                EditorGUILayout.LabelField($"Unsupported field type: {fieldType}");
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
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

        // 가로선을 그리는 함수
        private static void DrawHorizontalLine(Color color, int thickness = 1)
        {
            var rect = EditorGUILayout.GetControlRect(false, thickness);
            EditorGUI.DrawRect(rect, color);
        }
    }

    // LayerMask 필드를 확장하는 클래스
    public static class EditorGUILayoutExtensions
    {
        public static LayerMask LayerMaskField(string label, LayerMask selected)
        {
            var layers = new List<string>();
            var layerNumbers = new List<int>();

            for (var i = 0; i < 32; i++)
            {
                var layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }

            var maskWithoutEmpty = 0;
            for (var i = 0; i < layerNumbers.Count; i++)
            {
                if ((selected & (1 << layerNumbers[i])) > 0)
                    maskWithoutEmpty |= 1 << i;
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());

            var mask = 0;
            for (var i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= 1 << layerNumbers[i];
            }

            selected.value = mask;
            return selected;
        }
    }
}