using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviorTreeTool.Editor
{
    public class NodePort : Port
    {
        // GITHUB:UnityCsReference-master\UnityCsReference-master\Modules\GraphViewEditor\Elements\Port.cs
        private class DefaultEdgeConnectorListener : IEdgeConnectorListener
        {
            private readonly GraphViewChange _graphViewChange;
            private readonly List<Edge> _edgesToCreate;
            private readonly List<GraphElement> _edgesToDelete;

            public DefaultEdgeConnectorListener()
            {
                _edgesToCreate = new List<Edge>();
                _edgesToDelete = new List<GraphElement>();

                _graphViewChange.edgesToCreate = _edgesToCreate;
            }

            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                _edgesToCreate.Clear();
                _edgesToCreate.Add(edge);

                // We can't just add these edges to delete to the m_GraphViewChange
                // because we want the proper deletion code in GraphView to also
                // be called. Of course, that code (in DeleteElements) also
                // sends a GraphViewChange.
                _edgesToDelete.Clear();
                if (edge.input.capacity == Capacity.Single)
                    foreach (Edge edgeToDelete in edge.input.connections)
                        if (edgeToDelete != edge)
                            _edgesToDelete.Add(edgeToDelete);
                if (edge.output.capacity == Capacity.Single)
                    foreach (Edge edgeToDelete in edge.output.connections)
                        if (edgeToDelete != edge)
                            _edgesToDelete.Add(edgeToDelete);
                if (_edgesToDelete.Count > 0)
                    graphView.DeleteElements(_edgesToDelete);

                var edgesToCreate = _edgesToCreate;
                if (graphView.graphViewChanged != null)
                {
                    edgesToCreate = graphView.graphViewChanged(_graphViewChange).edgesToCreate;
                }

                for (int i = 0; i < edgesToCreate.Count; i++)
                {
                    Edge e = edgesToCreate[i];
                    graphView.AddElement(e);
                    edge.input.Connect(e);
                    edge.output.Connect(e);
                }
            }
        }

        public NodePort(Direction direction, Capacity capacity) : base(Orientation.Vertical, direction, capacity,
            typeof(bool))
        {
            var connectorListener = new DefaultEdgeConnectorListener();
            m_EdgeConnector = new EdgeConnector<Edge>(connectorListener);
            this.AddManipulator(m_EdgeConnector);
            style.width = 100;
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            Rect rect = new Rect(0, 0, layout.width, layout.height);
            return rect.Contains(localPoint);
        }
    }
}
