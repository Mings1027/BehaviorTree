using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Tree;

namespace BehaviorTreeTool.Editor
{
    [CustomEditor(typeof(BaseNode), true)]
    public class BaseNodeEditor : UnityEditor.Editor
    {
        private string _searchQuery = "";
        private readonly string[] _tabTitles = { "Tasks", "Variables", "Inspector" };
        private Vector2 _taskScrollPos;
        private Vector2 _inspectorScrollPos;

        private SerializedProperty _sharedDataProperty;
        private UnityEditor.Editor _sharedDataEditor;

        private Texture2D _downArrowTexture;
        private Texture2D _rightArrowTexture;

        private static int _selectedTab;

        private void OnEnable()
        {
            _downArrowTexture = TreeUtility.LoadTexture("Assets/BehaviorTreeTool/Sprites/Arrow Simple Down.png");
            _rightArrowTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BehaviorTreeTool/Sprites/Arrow Simple Right.png");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Initialize properties if they haven't been initialized yet
            if (_sharedDataProperty == null)
            {
                InitializeProperties();
            }

            DisplayTabs();
            CheckUnassignVariableName();

            serializedObject.ApplyModifiedProperties();
        }

        private void InitializeProperties()
        {
            _sharedDataProperty = serializedObject.FindProperty("sharedData");

            if (_sharedDataProperty == null)
            {
                Debug.LogError("Failed to find 'sharedData' property. Make sure it exists in the Node class.");
            }
            else if (_sharedDataProperty.objectReferenceValue != null)
            {
                _sharedDataEditor = CreateEditor(_sharedDataProperty.objectReferenceValue);
            }
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

        private void DrawNodeTypeButtons<T>() where T : BaseNode
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
            if (_sharedDataProperty.objectReferenceValue != null && _sharedDataEditor != null)
            {
                _sharedDataEditor.OnInspectorGUI();
            }
        }

        #endregion

        #region DrawInspectorTab

        private void DrawInspectorTab()
        {
            var node = (BaseNode)target;

            DrawDescriptionField();
            TreeUtility.DrawHorizontalLine(Color.gray);

            _inspectorScrollPos = EditorGUILayout.BeginScrollView(_inspectorScrollPos);
            DrawSharedVariableFields(node);
            TreeUtility.DrawHorizontalLine(Color.gray);
            DrawNodeVariables(node);
            TreeUtility.DrawHorizontalLine(Color.gray);
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

        private void DrawSharedVariableFields(BaseNode node)
        {
            var fields = node.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => typeof(SharedVariableBase).IsAssignableFrom(field.FieldType))
                .Select(field =>
                    new KeyValuePair<string, SharedVariableBase>(field.Name, (SharedVariableBase)field.GetValue(node)))
                .ToList();

            if (fields.Count <= 0)
            {
                var style = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15 };
                EditorGUILayout.LabelField("This node has no Shared Variables", style);
                return;
            }

            var boldLabelStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15 };
            EditorGUILayout.LabelField("Shared Variables", boldLabelStyle);

            // Calculate the maximum width of all labels
            var maxWidth = fields.Select(kvp =>
            {
                var serializedObject = new SerializedObject(node);
                var property = serializedObject.FindProperty(kvp.Key);
                var displayName = property != null ? property.displayName : ObjectNames.NicifyVariableName(kvp.Key);
                var labelStyle = new GUIStyle(GUI.skin.label);
                return labelStyle.CalcSize(new GUIContent(displayName)).x;
            }).Max();

            for (int i = 0; i < fields.Count; i++)
            {
                DrawSharedVariableField(node, fields[i], maxWidth);
                TreeUtility.DrawHorizontalLine(Color.gray);
            }
        }

        private void DrawSharedVariableField(BaseNode node, KeyValuePair<string, SharedVariableBase> kvp, float labelWidth)
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                normal = { background = TreeUtility.MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 1.0f)) },
                hover = { background = TreeUtility.MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 1.0f)) },
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(4, 4, 2, 2)
            };

            EditorGUILayout.BeginVertical(style);
            EditorGUILayout.BeginHorizontal();

            var variableNames = node.SharedData.Variables
                .Where(v => v.GetType() == kvp.Value.GetType())
                .Select(v => v.VariableName)
                .ToList();

            variableNames.Insert(0, "(None)");

            var currentIndex = string.IsNullOrEmpty(kvp.Value.VariableName)
                ? 0
                : variableNames.IndexOf(kvp.Value.VariableName);

            // Add a foldout button to show/hide the variable value
            var foldout = EditorPrefs.GetBool($"{kvp.Key}Foldout", false);

            if (currentIndex == 0 || Application.isPlaying)
            {
                var arrowTexture = foldout ? _downArrowTexture : _rightArrowTexture;
                if (GUILayout.Button(arrowTexture, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    foldout = !foldout;
                }
                EditorPrefs.SetBool($"{kvp.Key}Foldout", foldout);
            }

            var serializedObject = new SerializedObject(node);
            var property = serializedObject.FindProperty(kvp.Key);
            var displayName = property != null ? property.displayName : ObjectNames.NicifyVariableName(kvp.Key);

            EditorGUILayout.LabelField(displayName, GUILayout.Width(labelWidth));

            var selectedIndex = EditorGUILayout.Popup(currentIndex, variableNames.ToArray(), GUILayout.Width(150));
            if (selectedIndex != currentIndex)
            {
                UpdateVariableSelection(node, kvp.Value, variableNames, selectedIndex);
                EditorUtility.SetDirty(node);
            }

            EditorGUILayout.EndHorizontal();

            if (foldout)
            {
                if (currentIndex == 0 || Application.isPlaying)
                {
                    EditorGUI.indentLevel++;
                    TreeUtility.DrawSharedVariableValueField(kvp.Value, "Value");
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void UpdateVariableSelection(BaseNode node, SharedVariableBase variable,
            IReadOnlyList<string> variableNames, int selectedIndex)
        {
            if (selectedIndex == 0)
            {
                variable.VariableName = string.Empty;
                variable.SetValue(null);
            }
            else
            {
                var selectedVariableName = variableNames[selectedIndex];
                var variables = node.SharedData.Variables;
                for (int i = 0; i < variables.Count; i++)
                {
                    if (variables[i].VariableName == selectedVariableName)
                    {
                        variable.VariableName = variables[i].VariableName;
                        variable.VariableType = variables[i].VariableType;
                        break;
                    }
                }
            }
        }


        private void DrawNodeVariables(BaseNode node)
        {
            var boldLabelStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15 };
            EditorGUILayout.LabelField("Node Variables", boldLabelStyle);

            var array = node.GetType()
                         .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // Check if the OnDrawGizmos method is overridden
            bool hasDrawGizmosOverride = node.GetType().GetMethod("OnDrawGizmos",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).DeclaringType != typeof(BaseNode);

            for (int i = 0; i < array.Length; i++)
            {
                FieldInfo field = array[i];
                if (typeof(SharedVariableBase).IsAssignableFrom(field.FieldType) ||
                    field.IsDefined(typeof(HideInInspector), false) ||
                    field.Name == "drawGizmos" && !hasDrawGizmosOverride)
                {
                    continue;
                }

                var property = serializedObject.FindProperty(field.Name);
                if (property != null)
                {
                    var displayName = ObjectNames.NicifyVariableName(field.Name);
                    EditorGUILayout.PropertyField(property, new GUIContent(displayName), true);
                }
            }
        }

        #endregion

        private void CheckUnassignVariableName()
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
                        noneSharedVariables.Add(node.name);
                        break;
                    }
                }
            });
            if (noneSharedVariables.Count > 0)
            {
                var style = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                    normal = { textColor = new Color(1.0f, 0.5f, 0f) }
                };

                EditorGUILayout.LabelField("Set the variable name in the Inspector tab.", style);

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

                for (int i = 0; i < noneSharedVariables.Count; i++)
                {
                    string noneSharedVariable = noneSharedVariables[i];
                    var parts = noneSharedVariable.Split(new[] { " - " }, StringSplitOptions.None);
                    var nodeName = parts[0];

                    EditorGUILayout.BeginHorizontal("box");
                    GUILayout.Label(nodeName, nodeNameStyle);
                    if (GUILayout.Button("Select", GUILayout.Width(50)))
                    {
                        _selectedTab = 2;
                        SelectNodeByName(nodeName);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void SelectNodeByName(string nodeName)
        {
            var tree = BehaviorTreeEditor.tree;
            if (tree == null) return;

            var node = tree.Nodes.Find(n => n.name == nodeName);
            if (node == null) return;

            var editorWindow = EditorWindow.GetWindow<BehaviorTreeEditor>();
            var treeView = editorWindow.TreeView;
            var nodeView = treeView?.FindNodeView(node);
            if (nodeView != null)
            {
                treeView.SelectNodeView(nodeView);
                editorWindow.Repaint();
            }
        }
    }
}