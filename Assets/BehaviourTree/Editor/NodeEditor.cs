using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviourTree.Editor
{
    [CustomEditor(typeof(Node), true)]
    public class NodeEditor : UnityEditor.Editor
    {
        private string _searchQuery = "";
        private int _selectedTab;
        private readonly string[] _tabTitles = { "Tasks", "Variables", "Inspector" };
        private Vector2 _scrollPos;

        private const string SelectedTabKey = "NodeEditor_SelectedTab";

        // Add Variable Name as a field to preserve its value
        private string _variableName = "";
        private SharedVariableType _selectedType;

        private void OnEnable()
        {
            _selectedTab = EditorPrefs.GetInt(SelectedTabKey, 0);
        }

        private void OnDisable()
        {
            EditorPrefs.SetInt(SelectedTabKey, _selectedTab);
        }

        public override void OnInspectorGUI()
        {
            DisplayTreeName();
            DisplayNodeHeader();
            DisplayTabs();
            DrawNoneSharedVariables();
        }

        private void DisplayTreeName()
        {
            // Display the selected BehaviourTree name
            string selectedTreeName = EditorPrefs.GetString("SelectedBehaviourTreeName", "No Tree Selected");
            EditorGUILayout.LabelField("Behaviour Tree:", selectedTreeName, EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
        }

        private void DisplayNodeHeader()
        {
            var node = (Node)target;
            string treeName = node.name; // Replace with the actual way to get the tree name
            string nodeType = GetNodeTypeName(node.GetType());

            GUIStyle treeNameStyle = new GUIStyle(GUI.skin.label)
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
            GUIStyle tabStyle = new GUIStyle(GUI.skin.button)
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
            GUIStyle searchFieldStyle = new GUIStyle(GUI.skin.textField)
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

            bool actionFoldout = EditorPrefs.GetBool("ActionNodesFoldout", true);
            bool compositeFoldout = EditorPrefs.GetBool("CompositeNodesFoldout", true);
            bool conditionFoldout = EditorPrefs.GetBool("ConditionNodesFoldout", true);
            bool decoratorFoldout = EditorPrefs.GetBool("DecoratorNodesFoldout", true);

            GUIStyle folderTitleStyle = new GUIStyle(EditorStyles.foldout)
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
                GUIStyle style = new GUIStyle(GUI.skin.box)
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
                serializedObject.Update();
                sharedData.AddVariable(newVariable);
                serializedObject.ApplyModifiedProperties();

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
            serializedObject.Update();

            var node = (Node)target;

            DrawDescriptionField();
            DrawSharedDataField(node);
            DrawSharedVariableFields(node);
            DrawNonSharedVariableFields(node);

            serializedObject.ApplyModifiedProperties();
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
                GUIStyle style = new GUIStyle(GUI.skin.box)
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
                if (GUILayout.Button("\u25cf", GUILayout.Width(20)))
                {
                    ShowAssignMenu(node, kvp.Value);
                }

                EditorGUILayout.EndHorizontal();
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
                if (variable is IValueContainer valueContainer)
                {
                    valueContainer.SetValue(null);
                }

                EditorUtility.SetDirty(node);
            });

            foreach (var sharedVariable in node.SharedData.Variables)
            {
                if (sharedVariable.GetType() == variable.GetType())
                {
                    menu.AddItem(new GUIContent(sharedVariable.VariableName), false, () =>
                    {
                        variable.VariableName = sharedVariable.VariableName;
                        if (variable is IValueContainer valueContainer &&
                            sharedVariable is IValueContainer sharedValueContainer)
                        {
                            valueContainer.SetValue(sharedValueContainer.GetValue());
                        }

                        EditorUtility.SetDirty(node);
                    });
                }
            }

            menu.ShowAsContext();
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

            List<string> noneSharedVariables = new List<string>();

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
                GUIStyle style = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(1.0f, 0.5f, 0f) }
                };

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "Create variables in the Variables tab and then find the nodes below. Assign names in the Inspector tab.",
                    style);
                EditorGUILayout.Space();

                // 텍스트 스타일 지정
                GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

                GUIStyle nodeNameStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

                GUIStyle variableNameStyle = new GUIStyle(GUI.skin.label)
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
                    string[] parts = noneSharedVariable.Split(new[] { " - " }, System.StringSplitOptions.None);
                    string nodeName = parts[0];
                    string variableName = parts[1];

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