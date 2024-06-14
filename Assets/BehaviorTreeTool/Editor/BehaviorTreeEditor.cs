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
            if (TreeView == null) return;

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
            var tree = Selection.activeObject as BehaviorTree;
            if (!tree)
            {
                if (Selection.activeGameObject)
                {
                    if (Selection.activeGameObject.TryGetComponent(out BehaviorTreeRunner behaviorTree))
                    {
                        tree = behaviorTree.Tree;
                    }
                    else if (Selection.activeGameObject.TryGetComponent(out ExternalBehaviorTreeRunner externalTree))
                    {
                        tree = externalTree.ExternalBehaviorTree;
                    }
                }
            }

            SelectTree(tree);
        }

        private void SelectTree(BehaviorTree newTree)
        {
            if (TreeView == null || newTree == null) return;
            if (tree == newTree) return;

            tree = newTree;
            treeName = tree.name;

            if (!tree.RootNode)
            {
                var rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
                var sharedData = CreateInstance<SharedData>();
                AssetDatabase.AddObjectToAsset(sharedData, tree);
                if (rootNode != null)
                {
                    rootNode.SharedData = sharedData;
                    rootNode.SharedData.name = "SharedData";
                    tree.SetRootNode(rootNode);
                }
            }

            TreeView.PopulateView();

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