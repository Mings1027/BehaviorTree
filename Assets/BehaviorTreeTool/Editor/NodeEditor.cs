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

        private SerializedProperty _variablesProperty;
        private SerializedProperty _sharedDataProperty;

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
            UpdateSerializedVariables();
        }

        // Inspector GUI를 그리는 함수
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DisplayTreeName();
            DisplayTabs();
            CheckAssignSharedData();
            // DrawNoneSharedVariables();

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
            GUILayout.Label("Variables", EditorStyles.boldLabel);

            if (_variablesProperty is { isArray: true })
            {
                var serializedSharedData = _variablesProperty.serializedObject;
                serializedSharedData.Update();
                if (_foldouts.Length != _variablesProperty.arraySize)
                {
                    Array.Resize(ref _foldouts, _variablesProperty.arraySize);
                }

                for (var i = 0; i < _variablesProperty.arraySize; i++)
                {
                    var variableProperty = _variablesProperty.GetArrayElementAtIndex(i);
                    if (variableProperty is { managedReferenceValue: not null })
                    {
                        var variableNameProperty = variableProperty.FindPropertyRelative("variableName");
                        EditorGUILayout.BeginHorizontal();

                        _foldouts[i] = EditorGUILayout.Foldout(_foldouts[i],
                            $"{variableNameProperty.stringValue} ({DisplayTypeName(variableProperty)})", true);

                        if (GUILayout.Button(new GUIContent(_upArrowTexture), GUILayout.Width(23),
                                GUILayout.Height(23)))
                        {
                            MoveVariableUp(i);
                        }

                        if (GUILayout.Button(new GUIContent(_downArrowTexture), GUILayout.Width(23),
                                GUILayout.Height(23)))
                        {
                            MoveVariableDown(i);
                        }

                        if (GUILayout.Button(new GUIContent(_closeTexture), GUILayout.Width(23), GUILayout.Height(23)))
                        {
                            DeleteVariable(variableNameProperty.stringValue, i);
                            SaveFoldoutStates(_foldoutKey, _foldouts);
                            EditorGUILayout.EndHorizontal();
                            break;
                        }

                        EditorGUILayout.EndHorizontal();

                        if (_foldouts[i])
                        {
                            EditorGUILayout.BeginVertical("box");
                            DrawVariableNameAndType(variableProperty);
                            DrawVariableFields(variableProperty);
                            EditorGUILayout.EndVertical();
                        }

                        if (i < _variablesProperty.arraySize - 1)
                        {
                            GUILayout.Space(3);
                            DrawHorizontalLine(Color.gray);
                        }
                    }
                }

                serializedSharedData.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogError("_variablesProperty는 null이거나 배열이 아닙니다.");
            }

            SaveFoldoutStates(_foldoutKey, _foldouts);
        }

        // 변수를 위로 이동하는 함수
        private void MoveVariableUp(int index)
        {
            if (index > 0)
            {
                _variablesProperty.MoveArrayElement(index, index - 1);
            }
        }

        // 변수를 아래로 이동하는 함수
        private void MoveVariableDown(int index)
        {
            if (index < _variablesProperty.arraySize - 1)
            {
                _variablesProperty.MoveArrayElement(index, index + 1);
            }
        }

        // 변수 이름과 타입을 그리는 함수
        private static void DrawVariableNameAndType(SerializedProperty variableProperty)
        {
            var variableNameProperty = variableProperty.FindPropertyRelative("variableName");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(50));
            EditorGUILayout.TextField(variableNameProperty.stringValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type", GUILayout.Width(50));
            EditorGUILayout.EnumPopup(DisplayTypeName(variableProperty));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawVariableFields(SerializedProperty variableProperty)
        {
            var variableType = variableProperty.managedReferenceFullTypename;
            var valueProperty = variableProperty.FindPropertyRelative("_value");
            var variableNameProperty = variableProperty.FindPropertyRelative("variableName");
            var useGetComponentProperty = variableProperty.FindPropertyRelative("useGetComponent");
            var type = variableType.Split(' ')[1];

            if (type == typeof(SharedInt).FullName)
            {
                valueProperty.intValue =
                    EditorGUILayout.IntField(variableNameProperty.stringValue, valueProperty.intValue);
            }
            else if (type == typeof(SharedFloat).FullName)
            {
                valueProperty.floatValue =
                    EditorGUILayout.FloatField(variableNameProperty.stringValue, valueProperty.floatValue);
            }
            else if (type == typeof(SharedTransform).FullName)
            {
                DrawObjectField(variableNameProperty, valueProperty, typeof(Transform));
            }
            else if (type == typeof(SharedCollider).FullName)
            {
                DrawObjectField(variableNameProperty, valueProperty, typeof(Collider));
            }
            else if (type == typeof(SharedColliderArray).FullName)
            {
                DrawArrayFoldout(variableNameProperty, valueProperty, typeof(Collider));
            }
            else if (type == typeof(SharedLayerMask).FullName)
            {
                var layerMaskValue = new LayerMask { value = valueProperty.intValue };
                layerMaskValue =
                    EditorGUILayoutExtensions.LayerMaskField(variableNameProperty.stringValue, layerMaskValue);
                valueProperty.intValue = layerMaskValue.value;
            }
            else if (type == typeof(SharedVector3).FullName)
            {
                valueProperty.vector3Value =
                    EditorGUILayout.Vector3Field(variableNameProperty.stringValue, valueProperty.vector3Value);
            }
            else if (type == typeof(SharedTransformArray).FullName)
            {
                DrawArrayFoldout(variableNameProperty, valueProperty, typeof(Transform));
            }
            else if (type == typeof(SharedBool).FullName)
            {
                valueProperty.boolValue =
                    EditorGUILayout.Toggle(variableNameProperty.stringValue, valueProperty.boolValue);
            }
            else if (type == typeof(SharedColor).FullName)
            {
                valueProperty.colorValue =
                    EditorGUILayout.ColorField(variableNameProperty.stringValue, valueProperty.colorValue);
            }
            else if (type == typeof(SharedGameObject).FullName)
            {
                DrawObjectField(variableNameProperty, valueProperty, typeof(GameObject));
            }
            else if (type == typeof(SharedGameObjectList).FullName)
            {
                DrawArrayFoldout(variableNameProperty, valueProperty, typeof(GameObject));
            }
            else if (type == typeof(SharedMaterial).FullName)
            {
                DrawObjectField(variableNameProperty, valueProperty, typeof(Material));
            }
            else if (type == typeof(SharedNavMeshAgent).FullName)
            {
                DrawObjectField(variableNameProperty, valueProperty, typeof(NavMeshAgent));
            }
            else if (type == typeof(SharedQuaternion).FullName)
            {
                var eulerAngles = valueProperty.quaternionValue.eulerAngles;
                var newEulerAngles = EditorGUILayout.Vector3Field(variableNameProperty.stringValue, eulerAngles);
                if (newEulerAngles != eulerAngles)
                {
                    valueProperty.quaternionValue = Quaternion.Euler(newEulerAngles);
                }
            }
            else if (type == typeof(SharedRect).FullName)
            {
                valueProperty.rectValue =
                    EditorGUILayout.RectField(variableNameProperty.stringValue, valueProperty.rectValue);
            }
            else if (type == typeof(SharedString).FullName)
            {
                valueProperty.stringValue =
                    EditorGUILayout.TextField(variableNameProperty.stringValue, valueProperty.stringValue);
            }
            else if (type == typeof(SharedVector2).FullName)
            {
                valueProperty.vector2Value =
                    EditorGUILayout.Vector2Field(variableNameProperty.stringValue, valueProperty.vector2Value);
            }
            else if (type == typeof(SharedVector2Int).FullName)
            {
                valueProperty.vector2IntValue = EditorGUILayout.Vector2IntField(variableNameProperty.stringValue,
                    valueProperty.vector2IntValue);
            }
            else if (type == typeof(SharedVector3Int).FullName)
            {
                valueProperty.vector3IntValue = EditorGUILayout.Vector3IntField(variableNameProperty.stringValue,
                    valueProperty.vector3IntValue);
            }

            // Add the Use GetComponent toggle
            var newUseGetComponent = EditorGUILayout.Toggle("Use GetComponent", useGetComponentProperty.boolValue);
            if (newUseGetComponent != useGetComponentProperty.boolValue)
            {
                useGetComponentProperty.boolValue = newUseGetComponent;
                EditorUtility.SetDirty(variableProperty.serializedObject.targetObject);
            }
        }

        private static void DrawObjectField(SerializedProperty variableNameProperty, SerializedProperty valueProperty,
            Type objectType)
        {
            valueProperty.objectReferenceValue = EditorGUILayout.ObjectField(variableNameProperty.stringValue,
                valueProperty.objectReferenceValue, objectType, true);
        }

        // 배열 폴드아웃을 그리는 함수
        private void DrawArrayFoldout(SerializedProperty variableNameProperty, SerializedProperty valueProperty,
            Type elementType)
        {
            var arrayFoldoutKey = $"{_foldoutKey}_{variableNameProperty.stringValue}_Array";
            var arrayFoldout = EditorPrefs.GetBool(arrayFoldoutKey, false);

            EditorGUILayout.BeginHorizontal();
            arrayFoldout = EditorGUILayout.Foldout(arrayFoldout, variableNameProperty.stringValue, true);
            EditorPrefs.SetBool(arrayFoldoutKey, arrayFoldout);

            valueProperty.arraySize =
                Mathf.Max(0, EditorGUILayout.IntField(valueProperty.arraySize, GUILayout.Width(50)));
            EditorGUILayout.EndHorizontal();

            if (arrayFoldout)
            {
                EditorGUI.indentLevel++;
                if (valueProperty.isArray)
                {
                    for (var i = 0; i < valueProperty.arraySize; i++)
                    {
                        var elementProperty = valueProperty.GetArrayElementAtIndex(i);
                        elementProperty.objectReferenceValue = EditorGUILayout.ObjectField($"Element {i}",
                            elementProperty.objectReferenceValue, elementType, true);
                    }

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add Element"))
                    {
                        valueProperty.arraySize++;
                    }

                    if (GUILayout.Button("Remove Element"))
                    {
                        if (valueProperty.arraySize > 0)
                        {
                            valueProperty.arraySize--;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
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

        // 변수 타입을 표시하는 함수
        private static SharedVariableType DisplayTypeName(SerializedProperty variableProperty)
        {
            var variableType = variableProperty.managedReferenceFullTypename;
            var type = variableType.Split(' ')[1];
            var sharedVariableType = SharedVariableType.Int;

            if (type == typeof(SharedInt).FullName)
            {
                sharedVariableType = SharedVariableType.Int;
            }
            else if (type == typeof(SharedFloat).FullName)
            {
                sharedVariableType = SharedVariableType.Float;
            }
            else if (type == typeof(SharedTransform).FullName)
            {
                sharedVariableType = SharedVariableType.Transform;
            }
            else if (type == typeof(SharedCollider).FullName)
            {
                sharedVariableType = SharedVariableType.Collider;
            }
            else if (type == typeof(SharedColliderArray).FullName)
            {
                sharedVariableType = SharedVariableType.ColliderArray;
            }
            else if (type == typeof(SharedLayerMask).FullName)
            {
                sharedVariableType = SharedVariableType.LayerMask;
            }
            else if (type == typeof(SharedVector3).FullName)
            {
                sharedVariableType = SharedVariableType.Vector3;
            }
            else if (type == typeof(SharedTransformArray).FullName)
            {
                sharedVariableType = SharedVariableType.TransformArray;
            }
            else if (type == typeof(SharedBool).FullName)
            {
                sharedVariableType = SharedVariableType.Bool;
            }
            else if (type == typeof(SharedColor).FullName)
            {
                sharedVariableType = SharedVariableType.Color;
            }
            else if (type == typeof(SharedGameObject).FullName)
            {
                sharedVariableType = SharedVariableType.GameObject;
            }
            else if (type == typeof(SharedGameObjectList).FullName)
            {
                sharedVariableType = SharedVariableType.GameObjectList;
            }
            else if (type == typeof(SharedMaterial).FullName)
            {
                sharedVariableType = SharedVariableType.Material;
            }
            else if (type == typeof(SharedNavMeshAgent).FullName)
            {
                sharedVariableType = SharedVariableType.NavMeshAgent;
            }
            else if (type == typeof(SharedQuaternion).FullName)
            {
                sharedVariableType = SharedVariableType.Quaternion;
            }
            else if (type == typeof(SharedRect).FullName)
            {
                sharedVariableType = SharedVariableType.Rect;
            }
            else if (type == typeof(SharedString).FullName)
            {
                sharedVariableType = SharedVariableType.String;
            }
            else if (type == typeof(SharedVector2).FullName)
            {
                sharedVariableType = SharedVariableType.Vector2;
            }
            else if (type == typeof(SharedVector2Int).FullName)
            {
                sharedVariableType = SharedVariableType.Vector2Int;
            }
            else if (type == typeof(SharedVector3Int).FullName)
            {
                sharedVariableType = SharedVariableType.Vector3Int;
            }

            return sharedVariableType;
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
                DrawSharedVariableField(node, kvp);
            }
        }

        private void DrawSharedVariableField(Node node, KeyValuePair<string, SharedVariableBase> kvp)
        {
            // 각 필드의 이름과 해당 변수를 출력할 수 있는 UI를 생성
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(kvp.Key, GUILayout.MinWidth(100));

            var variableStyle = new GUIStyle(GUI.skin.label);
            if (string.IsNullOrEmpty(kvp.Value.VariableName))
            {
                variableStyle.normal.textColor = new Color(1.0f, 0.5f, 0f);
                variableStyle.fontStyle = FontStyle.Bold;
            }

            // 현재 변수 이름 및 모든 변수 이름 목록을 가져오기
            var variableNames = node.SharedData.Variables
                .Where(v => v.GetType() == kvp.Value.GetType())
                .Select(v => v.VariableName)
                .ToList();

            // "(None)" 옵션 추가
            variableNames.Insert(0, "(None)");

            // 현재 변수 이름에 해당하는 인덱스를 찾기
            var currentIndex = string.IsNullOrEmpty(kvp.Value.VariableName)
                ? 0
                : variableNames.IndexOf(kvp.Value.VariableName);

            // 드롭다운 메뉴 스타일 설정
            var popupStyle = new GUIStyle(EditorStyles.popup);
            if (currentIndex == 0)
            {
                popupStyle.normal.textColor = new Color(1.0f, 0.5f, 0f);
                popupStyle.fontStyle = FontStyle.Bold;
            }

            // 드롭다운 메뉴 생성
            var selectedIndex = EditorGUILayout.Popup(currentIndex, variableNames.ToArray(), popupStyle,
                GUILayout.MinWidth(100));
            if (selectedIndex != currentIndex)
            {
                // 선택한 변수 이름으로 업데이트
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

            // 변수 값을 표시하도록 설정되어 있으면 변수 값 필드를 그리는 함수를 호출
            if (_showValues)
            {
                EditorGUI.indentLevel++;
                DrawSharedVariableValueFields(kvp.Value);
                EditorGUI.indentLevel--;
            }
        }

        // Shared 변수 값을 그리는 함수
        private void DrawSharedVariableValueFields(SharedVariableBase variable)
        {
            const string labelPrefix = "ㄴ ";
            const string valueLabel = labelPrefix + "Value";

            switch (variable)
            {
                case SharedInt sharedInt:
                    sharedInt.Value = EditorGUILayout.IntField(valueLabel, sharedInt.Value);
                    break;
                case SharedFloat sharedFloat:
                    sharedFloat.Value = EditorGUILayout.FloatField(valueLabel, sharedFloat.Value);
                    break;
                case SharedTransform sharedTransform:
                    sharedTransform.Value =
                        (Transform)EditorGUILayout.ObjectField(valueLabel, sharedTransform.Value, typeof(Transform),
                            true);
                    break;
                case SharedCollider sharedCollider:
                    sharedCollider.Value =
                        (Collider)EditorGUILayout.ObjectField(valueLabel, sharedCollider.Value, typeof(Collider), true);
                    break;
                case SharedColliderArray sharedColliderArray:
                    DrawArrayField(sharedColliderArray);
                    break;
                case SharedLayerMask sharedLayerMask:
                    sharedLayerMask.Value = EditorGUILayout.LayerField(valueLabel, sharedLayerMask.Value);
                    break;
                case SharedVector3 sharedVector3:
                    sharedVector3.Value = EditorGUILayout.Vector3Field(valueLabel, sharedVector3.Value);
                    break;
                case SharedTransformArray sharedTransformArray:
                    DrawArrayField(sharedTransformArray);
                    break;
                case SharedBool sharedBool:
                    sharedBool.Value = EditorGUILayout.Toggle(valueLabel, sharedBool.Value);
                    break;
                case SharedColor sharedColor:
                    sharedColor.Value = EditorGUILayout.ColorField(valueLabel, sharedColor.Value);
                    break;
                case SharedGameObject sharedGameObject:
                    sharedGameObject.Value =
                        (GameObject)EditorGUILayout.ObjectField(valueLabel, sharedGameObject.Value, typeof(GameObject),
                            true);
                    break;
                case SharedGameObjectList sharedGameObjectList:
                    DrawListField(sharedGameObjectList);
                    break;
                case SharedMaterial sharedMaterial:
                    sharedMaterial.Value =
                        (Material)EditorGUILayout.ObjectField(valueLabel, sharedMaterial.Value, typeof(Material), true);
                    break;
                case SharedNavMeshAgent sharedNavMeshAgent:
                    sharedNavMeshAgent.Value =
                        (NavMeshAgent)EditorGUILayout.ObjectField(valueLabel, sharedNavMeshAgent.Value,
                            typeof(NavMeshAgent), true);
                    break;
                case SharedQuaternion sharedQuaternion:
                    {
                        var eulerAngles = sharedQuaternion.Value.eulerAngles;
                        var newEulerAngles = EditorGUILayout.Vector3Field(valueLabel, eulerAngles);
                        if (newEulerAngles != eulerAngles)
                        {
                            sharedQuaternion.Value = Quaternion.Euler(newEulerAngles);
                        }

                        break;
                    }
                case SharedRect sharedRect:
                    sharedRect.Value = EditorGUILayout.RectField(valueLabel, sharedRect.Value);
                    break;
                case SharedString sharedString:
                    sharedString.Value = EditorGUILayout.TextField(valueLabel, sharedString.Value);
                    break;
                case SharedVector2 sharedVector2:
                    sharedVector2.Value = EditorGUILayout.Vector2Field(valueLabel, sharedVector2.Value);
                    break;
                case SharedVector2Int sharedVector2Int:
                    sharedVector2Int.Value = EditorGUILayout.Vector2IntField(valueLabel, sharedVector2Int.Value);
                    break;
                case SharedVector3Int sharedVector3Int:
                    sharedVector3Int.Value = EditorGUILayout.Vector3IntField(valueLabel, sharedVector3Int.Value);
                    break;
                default:
                    EditorGUILayout.LabelField("Unsupported SharedVariable type");
                    break;
            }
        }

        // 배열 필드를 그리는 함수
        private void DrawArrayField<T>(SharedVariable<T[]> sharedVariableArray)
        {
            var array = sharedVariableArray.Value ?? Array.Empty<T>();
            var newSize = EditorGUILayout.IntField("Size", array.Length);

            if (newSize < 0) newSize = 0;
            if (newSize != array.Length)
            {
                var newArray = new T[newSize];
                for (var i = 0; i < Mathf.Min(newSize, array.Length); i++)
                {
                    newArray[i] = array[i];
                }

                sharedVariableArray.Value = newArray;
                array = newArray;
            }

            _arrayFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_arrayFoldout, "Array Elements");
            if (_arrayFoldout)
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < array.Length; i++)
                {
                    array[i] = (T)DrawFieldForType(typeof(T), array[i], $"Element {i}");
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // 리스트 필드를 그리는 함수
        private void DrawListField<T>(SharedVariable<List<T>> sharedVariableList) where T : Object
        {
            var list = sharedVariableList.Value ?? new List<T>();

            EditorGUILayout.BeginHorizontal();
            _arrayFoldout =
                EditorGUILayout.BeginFoldoutHeaderGroup(_arrayFoldout, $"List Elements [{list.Count}]");

            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                list.Add(null);
            }

            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
                if (list.Count > 0)
                {
                    list.RemoveAt(list.Count - 1);
                }
            }

            if (GUILayout.Button("0", GUILayout.Width(30)))
            {
                list.Clear();
            }

            EditorGUILayout.EndHorizontal();

            if (_arrayFoldout)
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < list.Count; i++)
                {
                    list[i] = (T)EditorGUILayout.ObjectField($"Element {i}", list[i], typeof(T), true);
                }

                EditorGUI.indentLevel--;
            }

            sharedVariableList.Value = list;

            EditorGUILayout.EndFoldoutHeaderGroup();
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