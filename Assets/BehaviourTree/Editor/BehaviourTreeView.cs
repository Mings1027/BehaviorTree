using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Node = BehaviourTree.Scripts.Runtime.Node;

namespace BehaviourTree.Editor
{
    public class BehaviourTreeView : GraphView
    {
        public Action<NodeView> OnNodeSelected { get; set; }

        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits>
        {
        }

        private BehaviourTree.Scripts.Runtime.BehaviourTree _tree;
        private readonly BehaviourTreeSettings _settings;

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
                templateFile = BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateConditionNode,
                defaultFileName = "NewConditionNode.cs", subFolder = "Conditions"
            },
            new()
            {
                templateFile = BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateActionNode,
                defaultFileName = "NewActionNode.cs", subFolder = "Actions"
            },
            new()
            {
                templateFile = BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateCompositeNode,
                defaultFileName = "NewCompositeNode.cs", subFolder = "Composites"
            },
            new()
            {
                templateFile = BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateDecoratorNode,
                defaultFileName = "NewDecoratorNode.cs", subFolder = "Decorators"
            },
        };

        public BehaviourTreeView()
        {
            _settings = BehaviourTreeSettings.GetOrCreateSettings();

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new DoubleClickSelection());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = _settings.behaviourTreeStyle;
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;

            RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
            RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
        }

        private void OnUndoRedo()
        {
            RefreshTree();
            AssetDatabase.SaveAssets();
        }

        private void OnDragUpdatedEvent(DragUpdatedEvent evt)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }

        private void OnDragPerformEvent(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();
            var sharedData = DragAndDrop.objectReferences.OfType<SharedData>().FirstOrDefault();
            if (sharedData != null)
            {
                var mousePosition =
                    (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer,
                        evt.localMousePosition);
                var graphElement = GetElementAtPosition(mousePosition);
                if (graphElement is NodeView nodeView)
                {
                    nodeView.node.SharedData = sharedData;
                    if (nodeView.node is RootNode rootNode)
                    {
                        rootNode.SharedData = sharedData;
                        ApplySharedDataToChildren(rootNode);
                    }

                    RefreshTree();
                }
            }
        }

        private void ApplySharedDataToChildren(Node rootNode)
        {
            if (rootNode is RootNode root)
            {
                TraverseChildrenAndSetSharedData(root, root.SharedData);
            }
        }

        private void TraverseChildrenAndSetSharedData(Node node, SharedData sharedData)
        {
            if (node is CompositeNode compositeNode)
            {
                foreach (var child in compositeNode.Children)
                {
                    child.SharedData = sharedData;
                    TraverseChildrenAndSetSharedData(child, sharedData);
                }
            }
            else if (node is DecoratorNode decoratorNode)
            {
                if (decoratorNode.Child != null)
                {
                    decoratorNode.Child.SharedData = sharedData;
                    TraverseChildrenAndSetSharedData(decoratorNode.Child, sharedData);
                }
            }
            else if (node is RootNode rootNode)
            {
                if (rootNode.Child != null)
                {
                    rootNode.Child.SharedData = sharedData;
                    TraverseChildrenAndSetSharedData(rootNode.Child, sharedData);
                }
            }
        }

        private GraphElement GetElementAtPosition(Vector2 position)
        {
            return contentViewContainer.Children().OfType<GraphElement>()
                .FirstOrDefault(e => e.worldBound.Contains(position));
        }

        public NodeView FindNodeView(Node node)
        {
            return GetNodeByGuid(node.guid) as NodeView;
        }

        internal void PopulateView(BehaviourTree.Scripts.Runtime.BehaviourTree tree)
        {
            _tree = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            if (tree.RootNode == null)
            {
                tree.RootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
                EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }

            // Creates node view
            tree.Nodes.ForEach(CreateNodeView);

            // Create edges
            tree.Nodes.ForEach(n =>
            {
                var children = BehaviourTree.Scripts.Runtime.BehaviourTree.GetChildren(n);
                children.ForEach(c =>
                {
                    var parentView = FindNodeView(n);
                    var childView = FindNodeView(c);

                    var edge = parentView.output.ConnectTo(childView.input);
                    AddElement(edge);
                });
            });
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            graphViewChange.elementsToRemove?.ForEach(elem =>
            {
                if (elem is NodeView nodeView)
                {
                    _tree.DeleteNode(nodeView.node);
                }

                if (elem is Edge edge)
                {
                    var parentView = edge.output.node as NodeView;
                    var childView = edge.input.node as NodeView;
                    _tree.RemoveChild(parentView.node, childView.node);
                }
            });

            graphViewChange.edgesToCreate?.ForEach(edge =>
            {
                var parentView = edge.output.node as NodeView;
                var childView = edge.input.node as NodeView;
                _tree.AddChild(parentView.node, childView.node);
            });

            nodes.ForEach(n =>
            {
                if (n is NodeView view) view.SortChildren();
            });

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // New script functions
            evt.menu.AppendAction("Create Script.../New Condition Node", _ => CreateNewScript(_scriptFileAssets[0]));
            evt.menu.AppendAction($"Create Script.../New Action Node", _ => CreateNewScript(_scriptFileAssets[1]));
            evt.menu.AppendAction($"Create Script.../New Composite Node", _ => CreateNewScript(_scriptFileAssets[2]));
            evt.menu.AppendAction($"Create Script.../New Decorator Node", _ => CreateNewScript(_scriptFileAssets[3]));
            evt.menu.AppendSeparator();

            var nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            {
                var types = TypeCache.GetTypesDerivedFrom<ConditionNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Condition]/{type.Name}", _ => CreateNode(type, nodePosition));
                }
            }
            {
                var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Action]/{type.Name}", _ => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Composite]/{type.Name}", _ => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Decorator]/{type.Name}", _ => CreateNode(type, nodePosition));
                }
            }
        }

        private void SelectFolder(string path)
        {
            // https://forum.unity.com/threads/selecting-a-folder-in-the-project-via-button-in-editor-window.355357/
            // Check the path has no '/' at the end, if it does remove it,
            // Obviously in this example it doesn't but it might
            // if your getting the path some other way.

            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            // Load object
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
        }

        private void CreateNewScript(ScriptTemplate template)
        {
            SelectFolder($"{_settings.newNodeBasePath}/{template.subFolder}");
            var templatePath = AssetDatabase.GetAssetPath(template.templateFile);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, template.defaultFileName);
        }

        private void CreateNode(Type type, Vector2 position)
        {
            var node = _tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node);
        }

        public void CreateNode(Type type)
        {
            Vector2 position = GetViewCenter();
            var node = _tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node);
        }

        private Vector2 GetViewCenter()
        {
            var viewCenter = contentViewContainer
                .WorldToLocal(contentViewContainer.parent.worldBound.center);
            return viewCenter;
        }

        private void CreateNodeView(Node node)
        {
            var nodeView = new NodeView(node)
            {
                onNodeSelected = OnNodeSelected
            };
            AddElement(nodeView);
        }

        public void RefreshTree()
        {
            PopulateView(_tree);
        }

        public void UpdateNodeStates()
        {
            nodes.ForEach(n =>
            {
                if (n is NodeView view) view.UpdateState();
            });
        }
    }
}