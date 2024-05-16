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
        public Action<NodeView> onNodeSelected;

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
            new ScriptTemplate
            {
                templateFile = BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateConditionNode,
                defaultFileName = "NewConditionNode.cs", subFolder = "Conditions"
            },
            new ScriptTemplate
            {
                templateFile = BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateActionNode,
                defaultFileName = "NewActionNode.cs", subFolder = "Actions"
            },
            new ScriptTemplate
            {
                templateFile = BehaviourTreeSettings.GetOrCreateSettings().scriptTemplateCompositeNode,
                defaultFileName = "NewCompositeNode.cs", subFolder = "Composites"
            },
            new ScriptTemplate
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
                Vector2 mousePosition = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
                var graphElement = GetElementAtPosition(mousePosition);
                if (graphElement is NodeView nodeView)
                {
                    nodeView.node.sharedData = sharedData;
                    if (nodeView.node is RootNode rootNode)
                    {
                        rootNode.sharedData = sharedData;
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
                TraverseChildrenAndSetSharedData(root, root.sharedData);
            }
        }

        private void TraverseChildrenAndSetSharedData(Node node, SharedData sharedData)
        {
            if (node is CompositeNode compositeNode)
            {
                foreach (var child in compositeNode.children)
                {
                    child.sharedData = sharedData;
                    TraverseChildrenAndSetSharedData(child, sharedData);
                }
            }
            else if (node is DecoratorNode decoratorNode)
            {
                if (decoratorNode.child != null)
                {
                    decoratorNode.child.sharedData = sharedData;
                    TraverseChildrenAndSetSharedData(decoratorNode.child, sharedData);
                }
            }
            else if (node is RootNode rootNode)
            {
                if (rootNode.child != null)
                {
                    rootNode.child.sharedData = sharedData;
                    TraverseChildrenAndSetSharedData(rootNode.child, sharedData);
                }
            }
        }

        private GraphElement GetElementAtPosition(Vector2 position)
        {
            return contentViewContainer.Children().OfType<GraphElement>().Where(e => e.worldBound.Contains(position)).FirstOrDefault();
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

            if (tree.rootNode == null)
            {
                tree.rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
                EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }

            // Creates node view
            tree.nodes.ForEach(n => CreateNodeView(n));

            // Create edges
            tree.nodes.ForEach(n =>
            {
                var children = BehaviourTree.Scripts.Runtime.BehaviourTree.GetChildren(n);
                children.ForEach(c =>
                {
                    NodeView parentView = FindNodeView(n);
                    NodeView childView = FindNodeView(c);

                    Edge edge = parentView.output.ConnectTo(childView.input);
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
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    NodeView nodeView = elem as NodeView;
                    if (nodeView != null)
                    {
                        _tree.DeleteNode(nodeView.node);
                    }

                    Edge edge = elem as Edge;
                    if (edge != null)
                    {
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;
                        _tree.RemoveChild(parentView.node, childView.node);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    _tree.AddChild(parentView.node, childView.node);
                });
            }

            nodes.ForEach((n) =>
            {
                NodeView view = n as NodeView;
                view.SortChildren();
            });

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //base.BuildContextualMenu(evt);

            // New script functions
            evt.menu.AppendAction("Create Script.../New Condition Node", (a) => CreateNewScript(_scriptFileAssets[0]));
            evt.menu.AppendAction($"Create Script.../New Action Node", (a) => CreateNewScript(_scriptFileAssets[1]));
            evt.menu.AppendAction($"Create Script.../New Composite Node", (a) => CreateNewScript(_scriptFileAssets[2]));
            evt.menu.AppendAction($"Create Script.../New Decorator Node", (a) => CreateNewScript(_scriptFileAssets[3]));
            evt.menu.AppendSeparator();

            Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            {
                var types = TypeCache.GetTypesDerivedFrom<ConditionNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Condition]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }
            {
                var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Action]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Composite]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
                foreach (var type in types)
                {
                    evt.menu.AppendAction($"[Decorator]/{type.Name}", (a) => CreateNode(type, nodePosition));
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
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

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
            Node node = _tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node);
        }

        private void CreateNodeView(Node node)
        {
            NodeView nodeView = new NodeView(node);
            nodeView.onNodeSelected = onNodeSelected;
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
                NodeView view = n as NodeView;
                view.UpdateState();
            });
        }
    }
}
