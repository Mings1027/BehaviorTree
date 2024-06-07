using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviorTreeTool.Editor
{
    public class BehaviorTreeEditor : EditorWindow
    {
        public static BehaviorTree tree;
        public static string treeName;
        public BehaviorTreeView TreeView { get; private set; }
        private InspectorView _inspectorView;
        private BehaviorTreeSettings _settings;

        [MenuItem("BehaviorTree/BehaviorTreeEditor ...")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<BehaviorTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviorTreeEditor");
            wnd.minSize = new Vector2(800, 600);
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

        private List<T> LoadAssets<T>() where T : Object
        {
            var assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var assets = new List<T>();
            foreach (var assetId in assetIds)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetId);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }

            return assets;
        }

        public void CreateGUI()
        {
            _settings = BehaviorTreeSettings.GetOrCreateSettings();

            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            // Import UXML if it has not been cloned yet
            var visualTree = _settings.BehaviorTreeXml;
            if (visualTree == null)
            {
                Debug.LogError("BehaviorTreeXml is null. Please check the UXML file path in BehaviorTreeSettings.");
                return;
            }

            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = _settings.BehaviorTreeStyle;
            if (styleSheet == null)
            {
                Debug.LogError("BehaviorTreeStyle is null. Please check the stylesheet path in BehaviorTreeSettings.");
                return;
            }

            root.styleSheets.Add(styleSheet);

            // Main treeview
            TreeView = root.Q<BehaviorTreeView>("behaviorTreeView");
            if (TreeView == null)
            {
                Debug.LogError("TreeView is null. Please check if BehaviorTreeView is defined in the UXML file.");

                Debug.Log("Root children:");
                foreach (var element in root.Children())
                {
                    Debug.Log(element.GetType().Name);
                    if (element is TemplateContainer templateContainer)
                    {
                        Debug.Log("TemplateContainer children:");
                        foreach (var child in templateContainer.Children())
                        {
                            Debug.Log(child.GetType().Name + " " + child.name);
                        }
                    }
                }

                return;
            }

            TreeView.OnNodeSelected = OnNodeSelectionChanged;

            // Inspector View
            _inspectorView = root.Q<InspectorView>();
            if (_inspectorView == null)
            {
                Debug.LogError("InspectorView is null. Please check if InspectorView is defined in the UXML file.");
                return;
            }

            if (tree == null)
            {
                OnSelectionChange();
            }
            else
            {
                SelectTree(tree);
            }

            // 트리를 로드
            LoadTree();
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

        private void OnSelectionChange()
        {
            // EditorApplication.delayCall += () =>
            {
                var tree = Selection.activeObject as BehaviorTree;
                if (!tree)
                {
                    if (Selection.activeGameObject)
                    {
                        if (Selection.activeGameObject.TryGetComponent(out BehaviorTreeRunner behaviorTreeRunner))
                        {
                            tree = behaviorTreeRunner.Tree;
                        }
                    }
                }

                SelectTree(tree);
            }
            ;
        }

        private void SelectTree(BehaviorTree newTree)
        {
            if (TreeView == null || newTree == null) return;

            var currentSelectedNodeGuid = TreeView.SelectedNodeView?.Node.guid;
            var selectSameNode = tree != null && tree.name == newTree.name;

            tree = newTree;
            if (!tree.RootNode)
            {
                var rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
                tree.SetRootNode(rootNode);
            }

            TreeView.PopulateView();

            // Save the selected tree name to EditorPrefs
            treeName = tree.name;

            // 마지막 커밋전엔 이거 if 자체가 없었고 관련된 BehaviorTreeView에 함수랑 프로퍼티도 없엇음
            if (tree.RootNode != null)
            {
                if (selectSameNode && !string.IsNullOrEmpty(currentSelectedNodeGuid))
                {
                    var selectedNodeView = TreeView.FindNodeViewByGuid(currentSelectedNodeGuid);
                    if (selectedNodeView != null)
                    {
                        TreeView.SelectNodeView(selectedNodeView);
                    }
                    else
                    {
                        // If the node does not exist in the new tree, select the root node
                        OnNodeSelectionChanged(TreeView.FindNodeView(tree.RootNode));
                    }
                }
                else
                {
                    // If no node was previously selected, select the root node
                    OnNodeSelectionChanged(TreeView.FindNodeView(tree.RootNode));
                }
            }

            EditorApplication.delayCall += () => { TreeView.FrameAll(); };
        }

        private void OnNodeSelectionChanged(NodeView nodeView)
        {
            _inspectorView.UpdateSelection(nodeView);
        }

        private void OnInspectorUpdate()
        {
            TreeView?.UpdateNodeStates();
        }

        private void LoadTree()
        {
            var treePath = EditorPrefs.GetString("SelectedBehaviorTreePath");
            tree = AssetDatabase.LoadAssetAtPath<BehaviorTree>(treePath);
            if (tree != null)
            {
                SelectTree(tree);
                TreeView.LoadTree();
            }
        }

        private void SaveTree()
        {
            if (tree != null)
            {
                EditorPrefs.SetString("SelectedBehaviorTreePath", AssetDatabase.GetAssetPath(tree));
                TreeView.SaveTree();
            }
        }
    }
}