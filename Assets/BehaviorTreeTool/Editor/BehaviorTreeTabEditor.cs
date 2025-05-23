using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tree;
using UnityEditor;
using UnityEngine;

namespace Tree
{
    public class BehaviorTreeTab : ScriptableObject { }
}

namespace BehaviorTreeTool.Editor
{
    [CustomEditor(typeof(BehaviorTreeTab))]
    public class BehaviorTreeTabEditor : UnityEditor.Editor
    {
        private string _searchQuery = "";
        private Vector2 _taskScrollPos;

        public static BaseNodeEditor SelectedNodeEditor;
        public static SharedDataEditor SelectedSharedDataEditor;
        private const string Uncategorized = "Uncategorized";
        private int _selectedTab;
        private readonly string[] _tabTitles = { "Tasks", "Variables", "Inspector" };

        private SerializedProperty _sharedDataProperty;

        public override void OnInspectorGUI()
        {
            DisplayTabs();
        }

        private void DisplayTabs()
        {
            serializedObject.Update();
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
                    DrawSharedDataTab();
                    break;
                case 2:
                    DrawInspectorTab();
                    break;
            }

            CheckUnassignVariableName();
            serializedObject.ApplyModifiedProperties();
        }

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

        private readonly Dictionary<Type, Dictionary<string, bool>> _categoryFoldouts = new()
        {
            { typeof(ActionNode), new Dictionary<string, bool>() },
            { typeof(CompositeNode), new Dictionary<string, bool>() },
            { typeof(ConditionNode), new Dictionary<string, bool>() },
            { typeof(DecoratorNode), new Dictionary<string, bool>() }
        };

        private void DrawNodeTypeButtons<T>() where T : BaseNode
        {
            var nodeTypes = TypeCache.GetTypesDerivedFrom<T>()
                                     .Where(t => t.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase))
                                     .ToArray();

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

                var nodeCategoryFoldouts = _categoryFoldouts[typeof(T)];
                
                // Group nodes by their category, separating categorized from uncategorized
                var categorizedNodes = nodeTypes
                                       .Where(type => type.GetCustomAttribute<NodeCategoryAttribute>() != null)
                                       .GroupBy(type => type.GetCustomAttribute<NodeCategoryAttribute>().Category)
                                       .OrderBy(group => group.Key) // Sort categories alphabetically
                                       .ToList();

                // List categorized nodes
                for (var i = 0; i < categorizedNodes.Count; i++)
                {
                    var category = categorizedNodes[i];
                    var categoryName = category.Key;
                    nodeCategoryFoldouts.TryAdd(categoryName, true);

                    nodeCategoryFoldouts[categoryName] = EditorGUILayout.Foldout(nodeCategoryFoldouts[categoryName],
                        categoryName, true, folderTitleStyle);

                    if (nodeCategoryFoldouts[categoryName])
                    {
                        EditorGUI.indentLevel++;

                        foreach (var item in category)
                        {
                            if (GUILayout.Button(item.Name))
                            {
                                CreateNode(item);
                            }
                        }

                        EditorGUI.indentLevel--;
                    }
                }

                // Now list uncategorized nodes
                var uncategorizedNodes = nodeTypes
                                         .Where(type => type.GetCustomAttribute<NodeCategoryAttribute>() == null)
                                         .ToArray();

                if (uncategorizedNodes.Length > 0)
                {
                    var uncategorizedKey = $"{typeof(T).Name}_Uncategorized";
                    nodeCategoryFoldouts.TryAdd(uncategorizedKey, true);

                    nodeCategoryFoldouts[uncategorizedKey] = EditorGUILayout.Foldout(nodeCategoryFoldouts[uncategorizedKey],
                        Uncategorized, true, folderTitleStyle);
                    if (nodeCategoryFoldouts[uncategorizedKey])
                    {
                        EditorGUI.indentLevel++;

                        foreach (var item in uncategorizedNodes)
                        {
                            if (GUILayout.Button(item.Name))
                            {
                                CreateNode(item);
                            }
                        }

                        EditorGUI.indentLevel--;
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

        private void DrawSharedDataTab()
        {
            SelectedSharedDataEditor.DrawSharedDataTab();
        }

        private void DrawInspectorTab()
        {
            SelectedNodeEditor.DrawInspectorTab();
        }

        private void CheckUnassignVariableName()
        {
            var tree = BehaviorTreeEditor.SelectedTree;

            if (!tree) return;

            var noneSharedVariables = new List<string>();

            BehaviorTree.Traverse(tree.RootNode, node =>
            {
                var nodeType = node.GetType();
                var sharedVariableFields = nodeType
                                           .GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                      BindingFlags.Instance)
                                           .Where(field =>
                                               typeof(SharedVariableBase).IsAssignableFrom(field.FieldType));

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

                var nodeNameStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

                for (var i = 0; i < noneSharedVariables.Count; i++)
                {
                    var noneSharedVariable = noneSharedVariables[i];
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
            var tree = BehaviorTreeEditor.SelectedTree;
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