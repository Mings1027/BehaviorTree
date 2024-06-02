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
        public static BehaviorTree tree;
        public static string treeName;
        public BehaviorTreeView TreeView { get; private set; }
        private InspectorView _inspectorView;
        // private ToolbarMenu _toolbarMenu;
        private TextField _treeNameField;
        private TextField _locationPathField;
        // private Button _createNewTreeButton;
        private VisualElement _overlay;
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

            // Import UXML
            var visualTree = _settings.BehaviorTreeXml;
            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = _settings.BehaviorTreeStyle;
            root.styleSheets.Add(styleSheet);

            // Main treeview
            TreeView = root.Q<BehaviorTreeView>();
            TreeView.OnNodeSelected = OnNodeSelectionChanged;

            // Inspector View
            _inspectorView = root.Q<InspectorView>();

            // Variables View

            // Toolbar assets menu
            // _toolbarMenu = root.Q<ToolbarMenu>();
            // var behaviorTrees = LoadAssets<BehaviorTree>();
            // behaviorTrees.ForEach(tree =>
            // {
            //     _toolbarMenu.menu.AppendAction($"{tree.name}", (a) => { Selection.activeObject = tree; });
            // });
            // _toolbarMenu.menu.AppendSeparator();
            // _toolbarMenu.menu.AppendAction("New Tree...", (a) => CreateNewTree("NewBehaviorTree"));

            // New Tree Dialog
            _treeNameField = root.Q<TextField>("TreeName");
            _locationPathField = root.Q<TextField>("LocationPath");
            _overlay = root.Q<VisualElement>("Overlay");
            // _createNewTreeButton = root.Q<Button>("CreateButton");
            // _createNewTreeButton.clicked += () => CreateNewTree(_treeNameField.value);

            if (_overlay != null)
            {
                _overlay.style.visibility = Visibility.Hidden;
            }

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

        private void OnEnable()
        {
            // EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            // EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            EditorApplication.quitting -= SaveTree;
            EditorApplication.quitting += SaveTree;
        }

        private void OnDisable()
        {
            // EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            SaveTree();
        }

        // private void OnPlayModeStateChanged(PlayModeStateChange obj)
        // {
        //     switch (obj)
        //     {
        //         case PlayModeStateChange.EnteredEditMode:
        //             LoadTree();
        //             break;
        //         case PlayModeStateChange.ExitingEditMode:
        //             SaveTree();
        //             break;
        //         case PlayModeStateChange.EnteredPlayMode:
        //             SaveTree();
        //             break;
        //         case PlayModeStateChange.ExitingPlayMode:
        //             SaveTree();
        //             break;
        //     }
        // }

        private void OnSelectionChange()
        {
            EditorApplication.delayCall += () =>
            {
                var tree = Selection.activeObject as BehaviorTree;
                if (!tree)
                {
                    if (Selection.activeGameObject)
                    {
                        var runner = Selection.activeGameObject.GetComponent<BehaviorTreeRunner>();
                        if (runner)
                        {
                            tree = runner.Tree;
                        }
                    }
                }

                SelectTree(tree);
            };
        }

        private void SelectTree(BehaviorTree newTree)
        {
            if (TreeView == null || !newTree)
            {
                return;
            }

            tree = newTree;

            if (_overlay != null)
            {
                _overlay.style.visibility = Visibility.Hidden;
            }

            TreeView.PopulateView();

            // Save the selected tree name to EditorPrefs
            treeName = tree.name;

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

        private void CreateNewTree(string assetName)
        {
            var path = System.IO.Path.Combine(_locationPathField.value, $"{assetName}.asset");
            var tree = CreateInstance<BehaviorTree>();
            tree.name = _treeNameField.value;
            AssetDatabase.CreateAsset(tree, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = tree;
            EditorGUIUtility.PingObject(tree);
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