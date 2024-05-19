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

            Undo.undoRedoPerformed = OnUndoRedo;
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
            _tree = BehaviourTreeEditor._tree;
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;
            if (_tree.RootNode == null)
            {
                _tree.RootNode = _tree.CreateNode(typeof(RootNode)) as RootNode;
                EditorUtility.SetDirty(_tree);
                AssetDatabase.SaveAssets();
            }

            // Creates node view
            _tree.Nodes.ForEach(CreateNodeView);

            // Create edges
            _tree.Nodes.ForEach(n =>
            {
                var children = BehaviourTree.Scripts.Runtime.BehaviourTree.GetChildren(n);
                children.ForEach(c =>
                {
                    var parentView = FindNodeView(n);
                    var childView = FindNodeView(c);

                    var edge = parentView.Output.ConnectTo(childView.Input);
                    AddElement(edge);
                });
            });
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
                    var parentView = edge.output.node as NodeView;
                    var childView = edge.input.node as NodeView;
                    _tree.RemoveChild(parentView.Node, childView.Node);
                }
            });

            graphViewChange.edgesToCreate?.ForEach(edge =>
            {
                var parentView = edge.output.node as NodeView;
                var childView = edge.input.node as NodeView;
                _tree.AddChild(parentView.Node, childView.Node);
            });

            nodes.ForEach(n =>
            {
                if (n is NodeView view) view.SortChildren();
            });

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
                OnNodeSelected = OnNodeSelected
            };
            AddElement(nodeView);
        }

        public void UpdateNodeStates()
        {
            nodes.ForEach(n =>
            {
                if (n is NodeView view) view.UpdateState();
            });
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
    }
}