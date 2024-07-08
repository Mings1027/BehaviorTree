using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviorTreeTool.Editor
{
    public class BehaviorTreeEditor : EditorWindow
    {
        public static BehaviorTree tree { get; private set; }
        public static string treeName;
        public BehaviorTreeView TreeView { get; private set; }
        private InspectorView _inspectorView;
        private ToolbarMenu _toolbarMenu;
        private BehaviorTreeSettings _settings;
        private static string lastSelectedNodeGuid;
        private Label _treeInfoLabel;

        [MenuItem("BehaviorTree/BehaviorTreeEditor ...")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<BehaviorTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviorTreeEditor");
            wnd.minSize = new Vector2(800, 600);
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
            if (Selection.activeObject is BehaviorTree)
            {
                OpenWindow();
                return true;
            }

            return false;
        }

        public void CreateGUI()
        {
            _settings = BehaviorTreeSettings.GetOrCreateSettings();

            var root = rootVisualElement;

            var visualTree = _settings.BehaviorTreeXml;
            if (visualTree == null)
            {
                Debug.LogError("BehaviorTreeXml is null. Please check the UXML file path in BehaviorTreeSettings.");
                return;
            }

            visualTree.CloneTree(root);

            var styleSheet = _settings.BehaviorTreeStyle;
            if (styleSheet == null)
            {
                Debug.LogError("BehaviorTreeStyle is null. Please check the stylesheet path in BehaviorTreeSettings.");
                return;
            }

            root.styleSheets.Add(styleSheet);

            TreeView = root.Q<BehaviorTreeView>("behaviorTreeView");
            if (TreeView == null) return;

            TreeView.OnNodeSelected = OnNodeSelectionChanged;

            _inspectorView = root.Q<InspectorView>();
            if (_inspectorView == null)
            {
                Debug.LogError("InspectorView is null. Please check if InspectorView is defined in the UXML file.");
                return;
            }

            _toolbarMenu = root.Q<ToolbarMenu>();
            PopulateToolbarMenu();

            _treeInfoLabel = root.Q<Label>("treeInfoLabel");

            if (tree == null)
            {
                OnSelectionChange();
            }
            else
            {
                SelectTree(tree);
            }

            LoadTree();
        }

        private void PopulateToolbarMenu()
        {
            var behaviorTrees = LoadAssets<BehaviorTree>();
            foreach (var behaviorTree in behaviorTrees)
            {
                _toolbarMenu.menu.AppendAction(behaviorTree.name, action => Selection.activeObject = behaviorTree);
            }
        }

        private void OnEnable()
        {
            EditorApplication.quitting -= SaveTree;
            EditorApplication.quitting += SaveTree;
        }

        private void OnDisable()
        {
            EditorApplication.quitting -= SaveTree;
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

            if (!newTree && Selection.activeGameObject != null && Selection.activeGameObject.TryGetComponent(out BehaviorTreeRunner behaviorTreeRunner))
            {
                newTree = behaviorTreeRunner.Tree;
            }

            SelectTree(newTree);
        }

        private void SelectTree(BehaviorTree newTree)
        {
            if (TreeView == null || newTree == null || newTree == tree) return;

            tree = newTree;
            treeName = tree.name;

            EnsureRootNode();

            TreeView.PopulateView();

            NodeView nodeToSelect = TreeView.FindNodeView(tree.RootNode);

            if (!string.IsNullOrEmpty(lastSelectedNodeGuid))
            {
                var lastSelectedNode = tree.Nodes.Find(n => n.guid == lastSelectedNodeGuid);
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

        private void EnsureRootNode()
        {
            if (tree.RootNode != null) return;

            var rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
            var sharedData = CreateInstance<SharedData>();

            if (rootNode != null)
            {
                AssetDatabase.AddObjectToAsset(sharedData, tree);
                rootNode.SharedData = sharedData;
                rootNode.SharedData.name = "SharedData";
                tree.SetRootNode(rootNode);
            }
        }

        private void OnNodeSelectionChanged(NodeView nodeView)
        {
            _inspectorView.UpdateSelection(nodeView);
            lastSelectedNodeGuid = nodeView.Node.guid;
            _treeInfoLabel.text = $"Tree: {tree.name} | Node: {nodeView.Node.name}";
        }

        private void OnInspectorUpdate()
        {
            TreeView?.UpdateNodeStates();
        }

        private void LoadTree()
        {
            var treePath = EditorPrefs.GetString("SelectedBehaviorTreePath");
            var loadedTree = AssetDatabase.LoadAssetAtPath<BehaviorTree>(treePath);
            if (loadedTree != null)
            {
                SelectTree(loadedTree);
                TreeView.LoadTree();
            }
        }

        private void SaveTree()
        {
            if (tree == null) return;

            EditorPrefs.SetString("SelectedBehaviorTreePath", AssetDatabase.GetAssetPath(tree));
            TreeView.SaveTree();
        }
    }
}
