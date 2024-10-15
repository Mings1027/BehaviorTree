using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Tree;
using Object = UnityEngine.Object;

namespace BehaviorTreeTool.Editor
{
    public class BehaviorTreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BehaviorTreeView, UxmlTraits> { }

        public Action<NodeView> OnNodeSelected { get; set; }
        public static bool IsNodeSelected { get; set; }

        private readonly List<NodeView> _nodeViewPool = new();
        private BehaviorTree _tree;
        private readonly BehaviorTreeSettings _settings;
        private Vector2 _nodePosition;

        private TaskSearchWindow _taskSearchWindow;
        private Vector2 _lastMousePosition;

        private struct ScriptTemplate
        {
            public TextAsset templateFile;
            public string defaultFileName;
            public string subFolder;
        }

        private readonly ScriptTemplate[] _scriptFileAssets =
        {
            new()
            {
                templateFile = BehaviorTreeSettings.GetOrCreateSettings().ScriptTemplateActionNode,
                defaultFileName = "NewActionNode.cs", subFolder = "Actions"
            },
            new()
            {
                templateFile = BehaviorTreeSettings.GetOrCreateSettings().ScriptTemplateCompositeNode,
                defaultFileName = "NewCompositeNode.cs", subFolder = "Composites"
            },
            new()
            {
                templateFile = BehaviorTreeSettings.GetOrCreateSettings().ScriptTemplateConditionNode,
                defaultFileName = "NewConditionNode.cs", subFolder = "Conditions"
            },
            new()
            {
                templateFile = BehaviorTreeSettings.GetOrCreateSettings().ScriptTemplateDecoratorNode,
                defaultFileName = "NewDecoratorNode.cs", subFolder = "Decorators"
            },
        };

        public BehaviorTreeView()
        {
            _settings = BehaviorTreeSettings.GetOrCreateSettings();
            InitGraphView();
            InitializeStyleSheets();
            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            OpenTreeDelay().Forget();
        }

        private async UniTaskVoid OpenTreeDelay()
        {
            await UniTask.Yield();
            if (Selection.activeGameObject != null &&
                Selection.activeGameObject.TryGetComponent(out BehaviorTreeRunner runner))
            {
                BehaviorTreeEditor.OpenWithTree(runner.Tree);
            }

            PopulateView();
        }

        private void InitGraphView()
        {
            Insert(0, new GridBackground());
            AddManipulators();
            RegisterEventCallbacks();
        }

        private void InitializeStyleSheets()
        {
            styleSheets.Add(_settings.BehaviorTreeStyle);
        }

        private void AddManipulators()
        {
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new DoubleClickSelection());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        private void RegisterEventCallbacks()
        {
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        public NodeView FindNodeView(BaseNode node)
        {
            return GetNodeByGuid(node.guid) as NodeView;
        }

        public void SelectNodeView(NodeView nodeView)
        {
            ClearSelection();
            AddToSelection(nodeView);
        }

        private void OnUndoRedo()
        {
            RefreshView();
            AssetDatabase.SaveAssets();
        }

        private void RefreshView()
        {
            if (_tree == null) return;

            if (_tree.RootNode == null)
            {
                _tree.SetRootNode(_tree.CreateNode(typeof(RootNode)) as RootNode);
                EditorUtility.SetDirty(_tree);
                AssetDatabase.SaveAssets();
            }

            if (_tree.SharedData == null)
            {
                _tree.CreateSharedData();
                EditorUtility.SetDirty(_tree);
                AssetDatabase.SaveAssets();
            }

            graphViewChanged -= OnGraphViewChanged;
            ClearGraphView();
            RecreateNodeViews();
            RecreateEdges();
            graphViewChanged += OnGraphViewChanged;
        }

        private void ClearGraphView()
        {
            var list = graphElements.ToList();
            for (var i = 0; i < list.Count; i++)
            {
                var element = list[i];
                if (element is NodeView nodeView)
                {
                    nodeView.Hide();
                }
                else
                {
                    RemoveElement(element);
                }
            }
        }

        private void RecreateNodeViews()
        {
            for (var i = 0; i < _tree.Nodes.Count; i++)
            {
                var node = _tree.Nodes[i];
                if (node) CreateNodeView(node, i);
            }
        }

        private void RecreateEdges()
        {
            for (var i = 0; i < _tree.Nodes.Count; i++)
            {
                var node = _tree.Nodes[i];
                var children = BehaviorTree.GetChildren(node);
                for (var i1 = 0; i1 < children.Count; i1++)
                {
                    var child = children[i1];
                    var parentView = FindNodeView(node);
                    var childView = FindNodeView(child);

                    if (parentView != null && childView != null)
                    {
                        var edge = parentView.Output.ConnectTo(childView.Input);
                        AddElement(edge);
                    }
                }
            }
        }

        internal void PopulateView()
        {
            _tree = BehaviorTreeEditor.Tree;
            RefreshView();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            HandleElementsToRemove(graphViewChange.elementsToRemove);
            HandleEdgesToCreate(graphViewChange.edgesToCreate);

            foreach (var view in nodes.OfType<NodeView>())
            {
                view.SortChildren();
                view.UpdateState();
            }

            SaveChangesIfAny(graphViewChange);

            return graphViewChange;
        }

        private void HandleElementsToRemove(List<GraphElement> elementsToRemove)
        {
            if (elementsToRemove == null) return;

            for (var i = 0; i < elementsToRemove.Count; i++)
            {
                var element = elementsToRemove[i];
                if (element is NodeView nodeView)
                {
                    HandleNodeRemoval(nodeView);
                }
                else if (element is Edge edge)
                {
                    HandleEdgeRemoval(edge);
                }
            }
        }

        private void HandleNodeRemoval(NodeView nodeView)
        {
            Undo.RegisterCompleteObjectUndo(_tree, "Delete Node");
            _nodeViewPool.Remove(nodeView);
            _tree.DeleteNode(nodeView.Node);

            foreach (var connectedEdge in edges.Where(edge =>
                         edge.output.node == nodeView || edge.input.node == nodeView).ToList())
            {
                connectedEdge.output.Disconnect(connectedEdge);
                connectedEdge.input.Disconnect(connectedEdge);
                RemoveElement(connectedEdge);
            }
        }

        private void HandleEdgeRemoval(Edge edge)
        {
            if (edge.output.node is NodeView parentView && edge.input.node is NodeView childView)
            {
                Undo.RegisterCompleteObjectUndo(_tree, "Remove Edge");
                BehaviorTree.RemoveChild(parentView.Node, childView.Node);
            }
        }

        private void HandleEdgesToCreate(List<Edge> edgesToCreate)
        {
            if (edgesToCreate == null) return;

            for (var i = 0; i < edgesToCreate.Count; i++)
            {
                var edge = edgesToCreate[i];
                if (edge.output.node is NodeView parentView && edge.input.node is NodeView childView)
                {
                    Undo.RegisterCompleteObjectUndo(_tree, "Add Edge");
                    BehaviorTree.AddChild(parentView.Node, childView.Node);
                }
            }
        }

        private void SaveChangesIfAny(GraphViewChange graphViewChange)
        {
            if ((graphViewChange.elementsToRemove != null && graphViewChange.elementsToRemove.Any()) ||
                (graphViewChange.edgesToCreate != null && graphViewChange.edgesToCreate.Any()))
            {
                EditorUtility.SetDirty(_tree);
                AssetDatabase.SaveAssets();
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var clickedElement = GetElementAtMousePosition(evt.localMousePosition);
            if (clickedElement is NodeView)
            {
                evt.menu.AppendAction("Duplicate", _ => DuplicateSelectedNodes());
                evt.menu.AppendAction("Delete", _ => DeleteSelectedNodes());
                evt.menu.AppendAction("Open In IDE", _ => OpenBaseNode());
            }
            else
            {
                AddScriptCreationMenuItems(evt);
                AddNodeCreationMenuItems(evt, GetMousePosition(evt));
            }
        }

        private void AddScriptCreationMenuItems(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Create Script.../New Action Node", _ => CreateNewScript(_scriptFileAssets[0]));
            evt.menu.AppendAction("Create Script.../New Composite Node", _ => CreateNewScript(_scriptFileAssets[1]));
            evt.menu.AppendAction("Create Script.../New Condition Node", _ => CreateNewScript(_scriptFileAssets[2]));
            evt.menu.AppendAction("Create Script.../New Decorator Node", _ => CreateNewScript(_scriptFileAssets[3]));
            evt.menu.AppendSeparator();
        }

        private Vector2 GetMousePosition(ContextualMenuPopulateEvent evt)
        {
            _nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            return _nodePosition;
        }

        private void AddNodeCreationMenuItems(ContextualMenuPopulateEvent evt, Vector2 nodePosition)
        {
            AddNodeCreationMenuItems(evt, nodePosition, typeof(ActionNode), "[Action]");
            AddNodeCreationMenuItems(evt, nodePosition, typeof(CompositeNode), "[Composite]");
            AddNodeCreationMenuItems(evt, nodePosition, typeof(ConditionNode), "[Condition]");
            AddNodeCreationMenuItems(evt, nodePosition, typeof(DecoratorNode), "[Decorator]");
        }

        private void AddNodeCreationMenuItems(ContextualMenuPopulateEvent evt, Vector2 nodePosition, Type baseType,
                                              string menuPath)
        {
            var types = TypeCache.GetTypesDerivedFrom(baseType);
            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                evt.menu.AppendAction($"{menuPath}/{type.Name}", _ => CreateNode(type, nodePosition));
            }
        }

        private GraphElement GetElementAtMousePosition(Vector2 mousePosition)
        {
            var worldMousePosition = this.LocalToWorld(mousePosition);
            return this.Query<GraphElement>().ToList().FirstOrDefault(e => e.worldBound.Contains(worldMousePosition));
        }

        private void DuplicateSelectedNodes()
        {
            if (selection.FirstOrDefault() is NodeView selectedNode)
            {
                var duplicateNode = _tree.CreateNode(selectedNode.Node.GetType());
                duplicateNode.position = selectedNode.Node.position + new Vector2(20, 20);

                CreateNodeView(duplicateNode, _tree.Nodes.IndexOf(duplicateNode));
                EditorUtility.SetDirty(_tree);
                AssetDatabase.SaveAssets();
            }
        }

        private void DeleteSelectedNodes()
        {
            if (selection.FirstOrDefault() is NodeView selectedNode)
            {
                Undo.RegisterCompleteObjectUndo(_tree, "Delete Node");

                var connectedEdges = edges.Where(edge =>
                    edge.output.node == selectedNode || edge.input.node == selectedNode).ToList();

                for (var i = 0; i < connectedEdges.Count; i++)
                {
                    var edge = connectedEdges[i];
                    var parentNode = (NodeView)edge.output.node;
                    var childNode = (NodeView)edge.input.node;
                    Undo.RecordObject(parentNode.Node, "Delete Edge");
                    Undo.RecordObject(childNode.Node, "Delete Edge");
                }

                _tree.DeleteNode(selectedNode.Node);
                RemoveElement(selectedNode);

                for (var i = 0; i < connectedEdges.Count; i++)
                {
                    var connectedEdge = connectedEdges[i];
                    connectedEdge.output.Disconnect(connectedEdge);
                    connectedEdge.input.Disconnect(connectedEdge);
                    RemoveElement(connectedEdge);
                }

                EditorUtility.SetDirty(_tree);
                AssetDatabase.SaveAssets();
            }
        }

        private void OpenBaseNode()
        {
            if (selection.FirstOrDefault() is NodeView selectedNode)
            {
                if (selectedNode.Node != null)
                {
                    var script = MonoScript.FromScriptableObject(selectedNode.Node);
                    if (script != null)
                    {
                        AssetDatabase.OpenAsset(script);
                    }
                    // var assetPath = AssetDatabase.GetAssetPath(script);
                    // if (!string.IsNullOrEmpty(assetPath))
                    // {
                    //     var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    //     AssetDatabase.OpenAsset(asset);
                    // }
                }
            }
        }

        private static void SelectFolder(string path)
        {
            if (path.EndsWith("/"))
                path = path.Substring(0, path.Length - 1);

            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        private void CreateNewScript(ScriptTemplate template)
        {
            SelectFolder($"{_settings.NewNodeBasePath}/{template.subFolder}");
            var templatePath = AssetDatabase.GetAssetPath(template.templateFile);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, template.defaultFileName);
        }

        public void CreateNode(Type type, Vector2 position)
        {
            var node = _tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node, _tree.Nodes.IndexOf(node));
        }

        public void CreateNode(Type type)
        {
            var position = GetViewCenter();
            var node = _tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node, _tree.Nodes.IndexOf(node));
        }

        private Vector2 GetViewCenter()
        {
            return contentViewContainer.WorldToLocal(contentViewContainer.parent.worldBound.center);
        }

        private NodeView GetOrCreateNodeView(BaseNode node, int index)
        {
            NodeView nodeView;
            if (index < _nodeViewPool.Count)
            {
                nodeView = _nodeViewPool[index];
                nodeView.SetNode(node);
            }
            else
            {
                nodeView = new NodeView(node)
                {
                    OnNodeSelectAction = OnNodeSelected
                };
                _nodeViewPool.Add(nodeView);
            }

            return nodeView;
        }

        private void CreateNodeView(BaseNode node, int index)
        {
            var nodeView = GetOrCreateNodeView(node, index);
            if (!Contains(nodeView))
            {
                AddElement(nodeView);
            }

            nodeView.style.display = DisplayStyle.Flex;
        }

        public void UpdateNodeStates()
        {
            foreach (var view in nodes.OfType<NodeView>())
            {
                view.UpdateState();
            }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Space)
            {
                ShowTasksSearchWindow();
            }
            else if (evt.keyCode == KeyCode.F && selection.OfType<NodeView>().Any() && !IsNodeSelected)
            {
                FrameSelection(selection.OfType<NodeView>().First());
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                ClearSelection();
                IsNodeSelected = false;
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            _lastMousePosition = evt.mousePosition;
        }

        private void ShowTasksSearchWindow()
        {
            if (_taskSearchWindow == null)
            {
                _taskSearchWindow = ScriptableObject.CreateInstance<TaskSearchWindow>();
                _taskSearchWindow.Initialize(this);
            }

            var localMousePosition = contentViewContainer.WorldToLocal(_lastMousePosition);
            _taskSearchWindow.SetPosition(localMousePosition);
            var screenMousePosition = GUIUtility.GUIToScreenPoint(_lastMousePosition);
            SearchWindow.Open(new SearchWindowContext(screenMousePosition), _taskSearchWindow);
        }

        private void FrameSelection(NodeView nodeView)
        {
            var position = nodeView.GetPosition();
            UpdateViewTransform(position.position, _nodePosition);
        }
    }

    public class TaskSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private BehaviorTreeView _treeView;
        private Vector2 _position;

        public void Initialize(BehaviorTreeView treeView)
        {
            _treeView = treeView;
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node")),
                new SearchTreeGroupEntry(new GUIContent("Actions"), 1),
                new SearchTreeGroupEntry(new GUIContent("Composites"), 1),
                new SearchTreeGroupEntry(new GUIContent("Conditions"), 1),
                new SearchTreeGroupEntry(new GUIContent("Decorators"), 1)
            };

            AddNodeEntries<ActionNode>(tree, "Actions", 2);
            AddNodeEntries<CompositeNode>(tree, "Composites", 2);
            AddNodeEntries<ConditionNode>(tree, "Conditions", 2);
            AddNodeEntries<DecoratorNode>(tree, "Decorators", 2);

            return tree;
        }

        private void AddNodeEntries<T>(List<SearchTreeEntry> tree, string groupName, int level) where T : BaseNode
        {
            var nodeTypes = TypeCache.GetTypesDerivedFrom<T>();
            for (var i = 0; i < nodeTypes.Count; i++)
            {
                var type = nodeTypes[i];
                var entry = new SearchTreeEntry(new GUIContent(type.Name))
                {
                    level = level,
                    userData = type
                };

                var groupEntry = tree.Find(e => e.name == groupName) as SearchTreeGroupEntry;
                var groupIndex = tree.IndexOf(groupEntry) + 1;

                tree.Insert(groupIndex, entry);
            }
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (entry.userData is Type type)
            {
                _treeView.CreateNode(type, _position);
                return true;
            }

            return false;
        }
    }
}