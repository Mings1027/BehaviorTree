using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Tree;
using Object = UnityEngine.Object;

namespace BehaviorTreeTool.Editor
{
    public class BehaviorTreeEditor : EditorWindow
    {
        private static BehaviorTree _tree;
        public static BehaviorTree SelectedTree => _tree;
        
        public BehaviorTreeView TreeView { get; private set; }
        private static BehaviorTreeTab _behaviorTreeTab;
        private InspectorView _inspectorView;
        private ToolbarMenu _toolbarMenu;
        private BehaviorTreeSettings _settings;
        private string _lastRuntimeTreeName;
        private string _lastSelectedNodeGuid;
        private Label _treeInfoLabel;

        [MenuItem("BehaviorTree/BehaviorTreeEditor")]
        private static void OpenWindow()
        {
            var wnd = GetWindow<BehaviorTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviorTreeEditor");
            wnd.minSize = new Vector2(1000, 750);
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    EditorApplication.delayCall += () =>
                    {
                        var selectedObject = Selection.activeGameObject;
                        if (selectedObject != null)
                        {
                            if (selectedObject.TryGetComponent<BehaviorTreeRunner>(out var runner))
                            {
                                if (runner.Tree != null)
                                {
                                    SelectTree(runner.Tree);
                                }
                            }
                        }
                    };
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    _tree = GetCurrentTree();
                    _lastRuntimeTreeName = _tree?.name;
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    if (!string.IsNullOrEmpty(_lastRuntimeTreeName))
                    {
                        // 저장해둔 트리 이름으로 에셋 찾기
                        var guids = AssetDatabase.FindAssets($"t:{_lastRuntimeTreeName}");
                        foreach (var guid in guids)
                        {
                            var path = AssetDatabase.GUIDToAssetPath(guid);
                            var tree = AssetDatabase.LoadAssetAtPath<BehaviorTree>(path);
                            if (tree != null && tree.name == _lastRuntimeTreeName)
                            {
                                SelectTree(tree);
                                break;
                            }
                        }
                    }
                    break;
            }
        }

        public static void OpenWithTree(BehaviorTree newTree)
        {
            OpenWindow();
            var wnd = GetWindow<BehaviorTreeEditor>();
            wnd.SelectTree(newTree);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is BehaviorTree tree)
            {
                OpenWithTree(tree);
                return true;
            }

            return false;
        }

        public void CreateGUI()
        {
            // Debug.Log("CreateGUI started");

            _settings = BehaviorTreeSettings.GetOrCreateSettings();
            // Debug.Log($"Settings loaded: {_settings != null}");

            var root = rootVisualElement;
            // Debug.Log($"Root element exists: {root != null}");

            var visualTree = _settings.BehaviorTreeXml;
            // Debug.Log($"Visual Tree loaded: {visualTree != null}");

            if (visualTree == null)
            {
                Debug.LogError("BehaviorTreeXml is null. Please check the UXML file path in BehaviorTreeSettings.");
                return;
            }

            visualTree.CloneTree(root);
            // Debug.Log("Tree cloned to root");

            var styleSheet = _settings.BehaviorTreeStyle;
            // Debug.Log($"StyleSheet loaded: {styleSheet != null}");

            if (styleSheet == null)
            {
                Debug.LogError("BehaviorTreeStyle is null. Please check the stylesheet path in BehaviorTreeSettings.");
                return;
            }

            root.styleSheets.Add(styleSheet);

            TreeView = root.Q<BehaviorTreeView>("behaviorTreeView");
            // Debug.Log($"TreeView found: {TreeView != null}");

            if (TreeView == null) return;

            TreeView.OnNodeSelected = OnNodeSelectionChanged;

            _inspectorView = root.Q<InspectorView>();
            // Debug.Log($"InspectorView found: {_inspectorView != null}");

            if (_inspectorView == null)
            {
                Debug.LogError("InspectorView is null. Please check if InspectorView is defined in the UXML file.");
                return;
            }

            _behaviorTreeTab = CreateInstance<BehaviorTreeTab>();
            _inspectorView.ShowTreeEditor(_behaviorTreeTab);

            _toolbarMenu = root.Q<ToolbarMenu>();
            PopulateToolbarMenu();

            _treeInfoLabel = root.Q<Label>("treeInfoLabel");

            _tree = Selection.activeObject as BehaviorTree;
            // Debug.Log($"Active tree found: {_tree != null}");

            if (_tree == null) return;
            SelectTree(_tree);
            // Debug.Log("Tree selection completed");
        }

        private static BehaviorTree GetCurrentTree()
        {
            var selectedObject = Selection.activeGameObject;
            if (selectedObject != null && selectedObject.TryGetComponent(out BehaviorTreeRunner runner))
            {
                return runner.Tree;
            }

            return null;
        }

        private void PopulateToolbarMenu()
        {
            var behaviorTrees = LoadAssets<BehaviorTree>();
            foreach (var behaviorTree in behaviorTrees)
            {
                _toolbarMenu.menu.AppendAction(behaviorTree.name, _ => Selection.activeObject = behaviorTree);
            }
        }

        private List<T> LoadAssets<T>() where T : Object
        {
            var assetIds = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            var assets = new List<T>(assetIds.Length);

            foreach (var assetId in assetIds)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetId);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }

        private void OnSelectionChange()
        {
            var newTree = Selection.activeObject as BehaviorTree;
            if (!newTree && Selection.activeGameObject != null &&
                Selection.activeGameObject.TryGetComponent(out BehaviorTreeRunner behaviorTreeRunner))
            {
                newTree = behaviorTreeRunner.Tree;
            }

            if (newTree)
            {
                SelectTree(newTree);
            }
        }

        private void SelectTree(BehaviorTree newTree)
        {
            if (_behaviorTreeTab == null)
            {
                _behaviorTreeTab = CreateInstance<BehaviorTreeTab>();
                _inspectorView.ShowTreeEditor(_behaviorTreeTab);
            }

            _tree = newTree;

            TreeView.PopulateView(_tree);

            var nodeToSelect = TreeView.FindNodeView(_tree.RootNode);

            BehaviorTreeTabEditor.SelectedSharedDataEditor =
                (SharedDataEditor)UnityEditor.Editor.CreateEditor(_tree.SharedData);

            if (!string.IsNullOrEmpty(_lastSelectedNodeGuid))
            {
                var lastSelectedNode = _tree.Nodes.Find(n => n.guid == _lastSelectedNodeGuid);
                if (lastSelectedNode != null)
                {
                    nodeToSelect = TreeView.FindNodeView(lastSelectedNode);
                }
            }

            if (nodeToSelect != null)
            {
                TreeView.SelectNodeView(nodeToSelect);
            }
        }

        private void OnNodeSelectionChanged(NodeView nodeView)
        {
            BehaviorTreeTabEditor.SelectedNodeEditor = (BaseNodeEditor)UnityEditor.Editor.CreateEditor(nodeView.Node);

            _lastSelectedNodeGuid = nodeView.Node.guid;
            _treeInfoLabel.text = $"Tree: {_tree.name} | Node: {nodeView.Node.name}";
        }

        private void OnInspectorUpdate()
        {
            TreeView?.UpdateNodeStates();
        }
    }
}