using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using Pathfinding;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviourTree.Editor
{
    [CustomEditor(typeof(Node), true)]
    public class NodeEditor : UnityEditor.Editor
    {
        private string _searchQuery = "";

        private readonly string[] _tabTitles = { "Tasks", "Variables", "Inspector" };
        private Vector2 _scrollPos;
        private string _variableName = "";
        private SharedVariableType _selectedType;

        private static bool _arrayFoldout;
        private static int _selectedTab;
        private static bool _showValues;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DisplayTreeName();
            DisplayNodeHeader();
            DisplayTabs();
            DrawNoneSharedVariables();
            serializedObject.ApplyModifiedProperties();
        }

        private void DisplayTreeName()
        {
            // Display the selected BehaviourTree name
            EditorGUILayout.LabelField("Behaviour Tree:", BehaviourTreeEditor.TreeName, EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
        }

        private void DisplayNodeHeader()
        {
            var node = (Node)target;
            var treeName = node.name; // Replace with the actual way to get the tree name
            var nodeType = GetNodeTypeName(node.GetType());

            var treeNameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fixedHeight = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField($"{nodeType} - {treeName}", treeNameStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space(10);
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
            EditorGUILayout.Space(10);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_selectedTab)
            {
                case 0:
                    DrawTasksTab();
                    DrawSeparator();
                    break;
                case 1:
                    DrawVariablesTab();
                    DrawSeparator();
                    break;
                case 2:
                    DrawInspectorTab();
                    DrawSeparator();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        // 작업 탭을 그리는 함수
        private void DrawTasksTab()
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
            EditorGUILayout.Space(10);

            var actionNodeTypes = TypeCache.GetTypesDerivedFrom<ActionNode>()
                .Where(t => t.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));
            var compositeNodeTypes = TypeCache.GetTypesDerivedFrom<CompositeNode>()
                .Where(t => t.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));
            var conditionNodeTypes = TypeCache.GetTypesDerivedFrom<ConditionNode>()
                .Where(t => t.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));
            var decoratorNodeTypes = TypeCache.GetTypesDerivedFrom<DecoratorNode>()
                .Where(t => t.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));

            var actionFoldout = EditorPrefs.GetBool("ActionNodesFoldout", true);
            var compositeFoldout = EditorPrefs.GetBool("CompositeNodesFoldout", true);
            var conditionFoldout = EditorPrefs.GetBool("ConditionNodesFoldout", true);
            var decoratorFoldout = EditorPrefs.GetBool("DecoratorNodesFoldout", true);

            var folderTitleStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };

            actionFoldout = EditorGUILayout.Foldout(actionFoldout, "Action Nodes", true, folderTitleStyle);
            EditorPrefs.SetBool("ActionNodesFoldout", actionFoldout);
            if (actionFoldout)
            {
                EditorGUI.indentLevel++;
                foreach (var type in actionNodeTypes)
                {
                    if (GUILayout.Button(type.Name))
                    {
                        CreateNode(type);
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(2);

            compositeFoldout = EditorGUILayout.Foldout(compositeFoldout, "Composite Nodes", true, folderTitleStyle);
            EditorPrefs.SetBool("CompositeNodesFoldout", compositeFoldout);
            if (compositeFoldout)
            {
                EditorGUI.indentLevel++;
                foreach (var type in compositeNodeTypes)
                {
                    if (GUILayout.Button(type.Name))
                    {
                        CreateNode(type);
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(2);

            conditionFoldout = EditorGUILayout.Foldout(conditionFoldout, "Condition Nodes", true, folderTitleStyle);
            EditorPrefs.SetBool("ConditionNodesFoldout", conditionFoldout);
            if (conditionFoldout)
            {
                EditorGUI.indentLevel++;
                foreach (var type in conditionNodeTypes)
                {
                    if (GUILayout.Button(type.Name))
                    {
                        CreateNode(type);
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(2);

            decoratorFoldout = EditorGUILayout.Foldout(decoratorFoldout, "Decorator Nodes", true, folderTitleStyle);
            EditorPrefs.SetBool("DecoratorNodesFoldout", decoratorFoldout);
            if (decoratorFoldout)
            {
                EditorGUI.indentLevel++;
                foreach (var type in decoratorNodeTypes)
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
            var window = EditorWindow.GetWindow<BehaviourTreeEditor>();
            var treeView = window.TreeView;
            treeView?.CreateNode(type);
        }

        // 변수 탭을 그리는 함수
        private void DrawVariablesTab()
        {
            var sharedData = (SharedData)serializedObject.FindProperty("sharedData").objectReferenceValue;
            if (sharedData == null)
            {
                var style = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(1.0f, 0.5f, 0f) }
                };

                EditorGUILayout.LabelField("Please assign a SharedData object to the Root node.", style);

                // var helpBoxRect = GUILayoutUtility.GetLastRect();
                // GUI.Box(helpBoxRect, GUIContent.none, style);
                return;
            }

            GUILayout.Label("Add New Shared Variable", EditorStyles.boldLabel);
            _variableName = EditorGUILayout.TextField("Name", _variableName);
            EditorGUILayout.BeginHorizontal();
            _selectedType = (SharedVariableType)EditorGUILayout.EnumPopup("Type", _selectedType);

            if (GUILayout.Button("Add", GUILayout.Width(50)))
            {
                AddVariable(_selectedType, _variableName, sharedData);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            var editor = (SharedDataEditor)CreateEditor(sharedData);
            editor.OnInspectorGUI();
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

            SharedVariableBase newVariable = type switch
            {
                SharedVariableType.Int => new SharedInt { VariableName = variableName },
                SharedVariableType.Float => new SharedFloat { VariableName = variableName },
                SharedVariableType.AIPath => new SharedAIPath { VariableName = variableName },
                SharedVariableType.Transform => new SharedTransform { VariableName = variableName },
                SharedVariableType.Collider => new SharedCollider { VariableName = variableName },
                SharedVariableType.ColliderArray => new SharedColliderArray { VariableName = variableName },
                SharedVariableType.LayerMask => new SharedLayerMask { VariableName = variableName },
                SharedVariableType.Vector3 => new SharedVector3 { VariableName = variableName },
                SharedVariableType.TransformArray => new SharedTransformArray { VariableName = variableName },
                _ => null
            };

            if (newVariable != null)
            {
                sharedData.AddVariable(newVariable);

                EditorUtility.SetDirty(sharedData);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogError("새 변수를 생성하지 못했습니다");
            }

            _variableName = "";
        }

        // 변수 이름 중복을 확인하는 함수
        private bool IsVariableNameDuplicate(string variableName, SharedData sharedData)
        {
            foreach (var variable in sharedData.Variables)
            {
                if (variable.VariableName == variableName)
                {
                    return true;
                }
            }

            return false;
        }

        // 인스펙터 탭을 그리는 함수
        private void DrawInspectorTab()
        {
            var node = (Node)target;

            DrawDescriptionField();
            DrawSharedDataField(node);
            DrawSharedVariableFields(node);
            DrawNonSharedVariableFields(node);
        }

        // 설명 필드를 그리는 함수
        private void DrawDescriptionField()
        {
            EditorGUILayout.Space();
            var descriptionProperty = serializedObject.FindProperty("description");
            if (descriptionProperty != null)
            {
                EditorGUILayout.PropertyField(descriptionProperty, true);
            }
        }

        // 공유 데이터 필드를 그리는 함수
        private void DrawSharedDataField(Node node)
        {
            EditorGUILayout.Space();
            var sharedDataProperty = serializedObject.FindProperty("sharedData");
            EditorGUILayout.PropertyField(sharedDataProperty, new GUIContent("Shared Data"));

            if (sharedDataProperty.objectReferenceValue != null)
            {
                node.SetSharedData((SharedData)sharedDataProperty.objectReferenceValue);
            }

            DrawSeparator();
        }

        // 공유 변수를 그리는 함수
        private void DrawSharedVariableFields(Node node)
        {
            if (node.SharedData == null)
            {
                var style = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(1.0f, 0.5f, 0f) }
                };

                EditorGUILayout.LabelField("Please assign a SharedData object to the Root node.", style);

                var helpBoxRect = GUILayoutUtility.GetLastRect();
                GUI.Box(helpBoxRect, GUIContent.none, style);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shared Variables", EditorStyles.boldLabel);
            _showValues = EditorGUILayout.Toggle("Show Values", _showValues);
            EditorGUILayout.Space();

            foreach (var kvp in node.GetType()
                         .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         .Where(field => typeof(SharedVariableBase).IsAssignableFrom(field.FieldType))
                         .Select(field =>
                             new KeyValuePair<string, SharedVariableBase>(field.Name,
                                 (SharedVariableBase)field.GetValue(node))))
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(kvp.Key, GUILayout.MinWidth(100));

                EditorGUILayout.BeginHorizontal("box");

                var variableStyle = new GUIStyle(GUI.skin.label);
                if (string.IsNullOrEmpty(kvp.Value.VariableName))
                {
                    variableStyle.normal.textColor = new Color(1.0f, 0.5f, 0f);
                    variableStyle.fontStyle = FontStyle.Bold;
                }

                EditorGUILayout.LabelField(
                    string.IsNullOrEmpty(kvp.Value.VariableName) ? "(None)" : kvp.Value.VariableName,
                    variableStyle, GUILayout.MinWidth(100));
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("\u25cf", GUILayout.Width(25)))
                {
                    ShowAssignMenu(node, kvp.Value);
                }

                EditorGUILayout.EndHorizontal();
                if (_showValues)
                {
                    DrawSharedVariableValueFields(kvp.Value);
                }

                EditorGUILayout.EndVertical();
            }

            DrawSeparator();
        }

        // 할당 메뉴를 보여주는 함수
        private void ShowAssignMenu(Node node, SharedVariableBase variable)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("None"), false, () =>
            {
                variable.VariableName = string.Empty;
                variable.SetValue(null);
                // if (variable is IValue valueContainer)
                // {
                //     valueContainer.SetValue(null);
                // }

                EditorUtility.SetDirty(node);
            });

            foreach (var sharedVariable in node.SharedData.Variables)
            {
                if (sharedVariable.GetType() == variable.GetType())
                {
                    menu.AddItem(new GUIContent(sharedVariable.VariableName), false, () =>
                    {
                        variable.VariableName = sharedVariable.VariableName;
                        variable.SetValue(sharedVariable.GetValue());
                        // if (variable is IValue valueContainer &&
                        //     sharedVariable is IValue sharedValueContainer)
                        // {
                        //     valueContainer.SetValue(sharedValueContainer.GetValue());
                        // }

                        EditorUtility.SetDirty(node);
                    });
                }
            }

            menu.ShowAsContext();
        }

        private void DrawSharedVariableValueFields(SharedVariableBase variable)
        {
            switch (variable)
            {
                case SharedInt sharedInt:
                    sharedInt.Value = EditorGUILayout.IntField("Value", sharedInt.Value);
                    break;
                case SharedFloat sharedFloat:
                    sharedFloat.Value = EditorGUILayout.FloatField("Value", sharedFloat.Value);
                    break;
                case SharedAIPath sharedAIPath:
                    sharedAIPath.Value =
                        (AIPath)EditorGUILayout.ObjectField("Value", sharedAIPath.Value, typeof(AIPath), true);
                    break;
                case SharedTransform sharedTransform:
                    sharedTransform.Value =
                        (Transform)EditorGUILayout.ObjectField("Value", sharedTransform.Value, typeof(Transform), true);
                    break;
                case SharedCollider sharedCollider:
                    sharedCollider.Value =
                        (Collider)EditorGUILayout.ObjectField("Value", sharedCollider.Value, typeof(Collider), true);
                    break;
                case SharedColliderArray sharedColliderArray:
                    DrawArrayField(sharedColliderArray);
                    break;
                case SharedLayerMask sharedLayerMask:
                    sharedLayerMask.Value = EditorGUILayout.LayerField("Value", sharedLayerMask.Value);
                    break;
                case SharedVector3 sharedVector3:
                    sharedVector3.Value = EditorGUILayout.Vector3Field("Value", sharedVector3.Value);
                    break;
                case SharedTransformArray sharedTransformArray:
                    DrawArrayField(sharedTransformArray);
                    break;
                default:
                    EditorGUILayout.LabelField("Unsupported SharedVariable type");
                    break;
            }
        }

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

            if (type == typeof(AIPath))
            {
                return EditorGUILayout.ObjectField(label, (AIPath)value, typeof(AIPath), true);
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
                // Note: Drawing nested arrays is more complex, this is a placeholder.
                EditorGUILayout.LabelField(label, "Nested arrays not supported in custom editor.");
                return value;
            }

            EditorGUILayout.LabelField(label, "Unsupported type");
            return value;
        }

        // 비공유 변수를 그리는 함수
        private void DrawNonSharedVariableFields(Node node)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Non-Shared Variables", EditorStyles.boldLabel);
            EditorGUILayout.Space();

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

        // 구분선을 그리는 함수
        private void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.6f, 0.6f, 0.6f, 1));
            EditorGUILayout.Space();
        }

        // 노드 타입 이름을 가져오는 함수
        private static string GetNodeTypeName(Type type)
        {
            if (typeof(ActionNode).IsAssignableFrom(type))
            {
                return "Action";
            }

            if (typeof(CompositeNode).IsAssignableFrom(type))
            {
                return "Composite";
            }

            if (typeof(ConditionNode).IsAssignableFrom(type))
            {
                return "Condition";
            }

            if (typeof(DecoratorNode).IsAssignableFrom(type))
            {
                return "Decorator";
            }

            if (typeof(RootNode).IsAssignableFrom(type))
            {
                return "Root";
            }

            return "Unknown";
        }

        private void DrawNoneSharedVariables()
        {
            var tree = BehaviourTreeEditor._tree;

            if (tree == null)
            {
                return;
            }

            var noneSharedVariables = new List<string>();

            BehaviourTree.Scripts.Runtime.BehaviourTree.Traverse(tree.RootNode, node =>
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

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Assign names in the Inspector tab.", style);
                EditorGUILayout.Space();

                // 텍스트 스타일 지정
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

                // 표의 헤더 그리기
                EditorGUILayout.BeginHorizontal("box");
                GUILayout.Label("Node Name", headerStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label("Variable Name", headerStyle, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                // noneSharedVariables 항목들을 표 형태로 그리기
                foreach (var noneSharedVariable in noneSharedVariables)
                {
                    var parts = noneSharedVariable.Split(new[] { " - " }, System.StringSplitOptions.None);
                    var nodeName = parts[0];
                    var variableName = parts[1];

                    EditorGUILayout.BeginHorizontal("box");
                    GUILayout.Label(nodeName, nodeNameStyle, GUILayout.ExpandWidth(true));
                    GUILayout.Label(variableName, variableNameStyle, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();
                }

                DrawSeparator();
            }
        }
    }
}