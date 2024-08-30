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
        public static BehaviorTree Tree { get; private set; }
        public BehaviorTreeView TreeView { get; private set; }

        private static BehaviorTreeTab _behaviorTreeTab;
        private InspectorView _inspectorView;
        private ToolbarMenu _toolbarMenu;
        private BehaviorTreeSettings _settings;
        private string _lastSelectedNodeGuid;
        private Label _treeInfoLabel;

        [MenuItem("BehaviorTree/BehaviorTreeEditor")]
        private static void OpenWindow()
        {
            var wnd = GetWindow<BehaviorTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviorTreeEditor");
            wnd.minSize = new Vector2(1000, 750);
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

        // private void OnDisable()
        // {
        //     SelectTree(tree);
        // }

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

            _behaviorTreeTab = CreateInstance<BehaviorTreeTab>();
            _inspectorView.ShowTreeEditor(_behaviorTreeTab);

            _toolbarMenu = root.Q<ToolbarMenu>();
            PopulateToolbarMenu();

            _treeInfoLabel = root.Q<Label>("treeInfoLabel");

            Tree = Selection.activeObject as BehaviorTree;
            if (Tree == null) return;
            SelectTree(Tree);
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

            Tree = newTree;
            TreeInit();

            TreeView.PopulateView();

            var nodeToSelect = TreeView.FindNodeView(Tree.RootNode);

            BehaviorTreeTabEditor.SelectedSharedDataEditor =
                (SharedDataEditor)UnityEditor.Editor.CreateEditor(Tree.SharedData);

            if (!string.IsNullOrEmpty(_lastSelectedNodeGuid))
            {
                var lastSelectedNode = Tree.Nodes.Find(n => n.guid == _lastSelectedNodeGuid);
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

        private void TreeInit()
        {
            if (Tree.RootNode != null) return;
            var rootNode = Tree.CreateNode(typeof(RootNode)) as RootNode;
            var sharedData = CreateInstance<SharedData>();
            if (rootNode != null)
            {
                AssetDatabase.AddObjectToAsset(sharedData, Tree);
                Tree.SharedData = sharedData;
                Tree.SharedData.name = "SharedData";
                Tree.SetRootNode(rootNode);
            }
        }

        private void OnNodeSelectionChanged(NodeView nodeView)
        {
            BehaviorTreeTabEditor.SelectedNodeEditor = (BaseNodeEditor)UnityEditor.Editor.CreateEditor(nodeView.Node);

            _lastSelectedNodeGuid = nodeView.Node.guid;
            _treeInfoLabel.text = $"Tree: {Tree.name} | Node: {nodeView.Node.name}";
        }

        private void OnInspectorUpdate()
        {
            TreeView?.UpdateNodeStates();
        }
    }
}