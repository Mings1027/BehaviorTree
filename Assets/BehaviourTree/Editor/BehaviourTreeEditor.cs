using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using UnityEditor.UIElements;

namespace BehaviourTree.Editor
{
    public class BehaviourTreeEditor : EditorWindow
    {
        public BehaviourTreeView TreeView { get; private set; }
        private BehaviourTree.Scripts.Runtime.BehaviourTree _tree;
        private InspectorView _inspectorView;
        private ToolbarMenu _toolbarMenu;
        private TextField _treeNameField;
        private TextField _locationPathField;
        private Button _createNewTreeButton;
        private VisualElement _overlay;
        private BehaviourTreeSettings _settings;
        private ObjectField _sharedDataField;

        [MenuItem("BehaviorTree/BehaviourTreeEditor ...")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
            wnd.minSize = new Vector2(800, 600);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is BehaviourTree.Scripts.Runtime.BehaviourTree)
            {
                OpenWindow();
                return true;
            }

            return false;
        }

        private List<T> LoadAssets<T>() where T : Object
        {
            string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            List<T> assets = new List<T>();
            foreach (var assetId in assetIds)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetId);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }

            return assets;
        }

        public void CreateGUI()
        {
            _settings = BehaviourTreeSettings.GetOrCreateSettings();

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = _settings.behaviourTreeXml;
            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = _settings.behaviourTreeStyle;
            root.styleSheets.Add(styleSheet);

            // Main treeview
            TreeView = root.Q<BehaviourTreeView>();
            TreeView.OnNodeSelected = OnNodeSelectionChanged;

            // Inspector View
            _inspectorView = root.Q<InspectorView>();

            // Shared Data Field
            _sharedDataField = new ObjectField("Shared Data")
            {
                objectType = typeof(SharedData),
                allowSceneObjects = false
            };
            _sharedDataField.RegisterValueChangedCallback(evt =>
            {
                if (_tree != null && _tree.RootNode != null)
                {
                    _tree.RootNode.SharedData = (SharedData)evt.newValue;
                    TreeView.RefreshTree(); // 트리 뷰를 새로고침하여 변경사항 반영
                }
            });

            // Toolbar assets menu
            _toolbarMenu = root.Q<ToolbarMenu>();
            var behaviourTrees = LoadAssets<BehaviourTree.Scripts.Runtime.BehaviourTree>();
            behaviourTrees.ForEach(tree =>
            {
                _toolbarMenu.menu.AppendAction($"{tree.name}", (a) => { Selection.activeObject = tree; });
            });
            _toolbarMenu.menu.AppendSeparator();
            _toolbarMenu.menu.AppendAction("New Tree...", (a) => CreateNewTree("NewBehaviourTree"));

            // New Tree Dialog
            _treeNameField = root.Q<TextField>("TreeName");
            _locationPathField = root.Q<TextField>("LocationPath");
            _overlay = root.Q<VisualElement>("Overlay");
            _createNewTreeButton = root.Q<Button>("CreateButton");
            _createNewTreeButton.clicked += () => CreateNewTree(_treeNameField.value);

            if (_overlay != null)
            {
                _overlay.style.visibility = Visibility.Hidden;
            }

            if (_tree == null)
            {
                OnSelectionChange();
            }
            else
            {
                SelectTree(_tree);
            }
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

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        private void OnSelectionChange()
        {
            EditorApplication.delayCall += () =>
            {
                BehaviourTree.Scripts.Runtime.BehaviourTree tree =
                    Selection.activeObject as BehaviourTree.Scripts.Runtime.BehaviourTree;
                if (!tree)
                {
                    if (Selection.activeGameObject)
                    {
                        BehaviourTreeRunner runner = Selection.activeGameObject.GetComponent<BehaviourTreeRunner>();
                        if (runner)
                        {
                            tree = runner.Tree;
                        }
                    }
                }

                SelectTree(tree);
            };
        }

        private void SelectTree(BehaviourTree.Scripts.Runtime.BehaviourTree newTree)
        {
            if (TreeView == null || !newTree)
            {
                return;
            }

            _tree = newTree;

            if (_overlay != null)
            {
                _overlay.style.visibility = Visibility.Hidden;
            }

            TreeView.PopulateView(_tree);

            // Save the selected tree name to EditorPrefs
            EditorPrefs.SetString("SelectedBehaviourTreeName", _tree.name);

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
            string path = System.IO.Path.Combine(_locationPathField.value, $"{assetName}.asset");
            BehaviourTree.Scripts.Runtime.BehaviourTree tree =
                CreateInstance<BehaviourTree.Scripts.Runtime.BehaviourTree>();
            tree.name = _treeNameField.value;
            AssetDatabase.CreateAsset(tree, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = tree;
            EditorGUIUtility.PingObject(tree);
        }
    }
}