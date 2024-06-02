using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviorTreeTool.Editor
{
    public class BehaviorTreeView : GraphView
    {
        public Action<NodeView> OnNodeSelected { get; set; }
        private TaskSearchWindow _taskSearchWindow; // 캐싱된 TaskSearchWindow 인스턴스
        private Vector2 _lastMousePosition; // 마지막 마우스 위치 저장

        public new class UxmlFactory : UxmlFactory<BehaviorTreeView, UxmlTraits> { }

        private BehaviorTree _tree;
        private readonly BehaviorTreeSettings _settings;
        private Vector2 _nodePosition;

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

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new DoubleClickSelection());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = _settings.BehaviorTreeStyle;
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed = OnUndoRedo;

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        private void OnUndoRedo()
        {
            PopulateView();
            AssetDatabase.SaveAssets();
        }

        public NodeView FindNodeView(Node node)
        {
            return GetNodeByGuid(node.guid) as NodeView;
        }

        internal void PopulateView()
        {
            _tree = BehaviorTreeEditor.tree;
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;
            if (AssetDatabase.Contains(_tree))
            {
                if (_tree.RootNode == null)
                {
                    _tree.RootNode = _tree.CreateNode(typeof(RootNode)) as RootNode;
                    EditorUtility.SetDirty(_tree);
                    AssetDatabase.SaveAssets();
                }
            }

            // Creates node view
            _tree.Nodes.ForEach(CreateNodeView);

            // Create edges
            for (var i = 0; i < _tree.Nodes.Count; i++)
            {
                var n = _tree.Nodes[i];
                var children = BehaviorTree.GetChildren(n);
                for (var j = 0; j < children.Count; j++)
                {
                    var c = children[j];
                    var parentView = FindNodeView(n);
                    var childView = FindNodeView(c);

                    var edge = parentView.Output.ConnectTo(childView.Input);
                    AddElement(edge);
                }
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            graphViewChange.elementsToRemove?.ForEach(elem =>
            {
                if (elem is NodeView nodeView)
                {
                    _tree.DeleteNode(nodeView.Node);
                }

                if (elem is Edge edge)
                {
                    if (edge.output.node is NodeView parentView && edge.input.node is NodeView childView)
                        BehaviorTree.RemoveChild(parentView.Node, childView.Node);
                }
            });

            graphViewChange.edgesToCreate?.ForEach(edge =>
            {
                if (edge.output.node is NodeView parentView && edge.input.node is NodeView childView)
                    BehaviorTree.AddChild(parentView.Node, childView.Node);
            });

            foreach (var n in nodes)
            {
                if (n is NodeView view) view.SortChildren();
            }

            return graphViewChange;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // New script functions
            evt.menu.AppendAction("Create Script.../New Action Node", _ => CreateNewScript(_scriptFileAssets[0]));
            evt.menu.AppendAction("Create Script.../New Composite Node", _ => CreateNewScript(_scriptFileAssets[1]));
            evt.menu.AppendAction("Create Script.../New Condition Node", _ => CreateNewScript(_scriptFileAssets[2]));
            evt.menu.AppendAction("Create Script.../New Decorator Node", _ => CreateNewScript(_scriptFileAssets[3]));
            evt.menu.AppendSeparator();

            _nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            AddNodeCreationMenuItems(evt, _nodePosition, typeof(ActionNode), "[Action]");
            AddNodeCreationMenuItems(evt, _nodePosition, typeof(CompositeNode), "[Composite]");
            AddNodeCreationMenuItems(evt, _nodePosition, typeof(ConditionNode), "[Condition]");
            AddNodeCreationMenuItems(evt, _nodePosition, typeof(DecoratorNode), "[Decorator]");
        }

        private void AddNodeCreationMenuItems(ContextualMenuPopulateEvent evt, Vector2 nodePosition, Type baseType,
            string menuPath)
        {
            var types = TypeCache.GetTypesDerivedFrom(baseType);
            foreach (var type in types)
            {
                evt.menu.AppendAction($"{menuPath}/{type.Name}", _ => CreateNode(type, nodePosition));
            }
        }

        private void SelectFolder(string path)
        {
            // 경로가 '/'로 끝나는 경우, '/'를 제거하여 경로를 수정
            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            // 주어진 경로에 있는 Asset을 불러옴
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            // 불러온 Asset을 선택 상태로 만듦
            Selection.activeObject = obj;
            // 에디터에서 해당 Asset을 하이라이트 (핑)함
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
            CreateNodeView(node);
        }

        public void CreateNode(Type type)
        {
            var position = GetViewCenter();
            var node = _tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node);
        }

        private Vector2 GetViewCenter()
        {
            var viewCenter = contentViewContainer.WorldToLocal(contentViewContainer.parent.worldBound.center);
            return viewCenter;
        }

        private void CreateNodeView(Node node)
        {
            var nodeView = new NodeView(node)
            {
                OnNodeSelected = OnNodeSelected
            };
            AddElement(nodeView);
        }

        public void UpdateNodeStates()
        {
            foreach (var n in nodes)
            {
                if (n is NodeView view) view.UpdateState();
            }
        }

        public void SaveTree()
        {
            if (_tree != null)
            {
                EditorUtility.SetDirty(_tree);
                AssetDatabase.SaveAssets();
            }
        }

        public void LoadTree()
        {
            if (_tree != null)
            {
                PopulateView();
            }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Space)
            {
                ShowTasksSearchWindow();
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
    }

    // Search Window Class
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

        private void AddNodeEntries<T>(List<SearchTreeEntry> tree, string groupName, int level) where T : Node
        {
            var nodeTypes = TypeCache.GetTypesDerivedFrom<T>();
            foreach (var type in nodeTypes)
            {
                var entry = new SearchTreeEntry(new GUIContent(type.Name))
                {
                    level = level,
                    userData = type
                };

                // Find the correct group to add this entry to
                var groupEntry = tree.Find(e => e.name == groupName) as SearchTreeGroupEntry;
                var groupIndex = tree.IndexOf(groupEntry) + 1;

                tree.Insert(groupIndex, entry);
            }
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var type = entry.userData as Type;
            if (type != null)
            {
                // 노드 생성 위치를 현재 마우스 위치로 설정
                _treeView.CreateNode(type, _position);
                return true;
            }

            return false;
        }
    }
}
