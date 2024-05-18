using UnityEditor;
using UnityEngine.UIElements;

namespace BehaviourTree.Editor
{
    public class DoubleClickSelection : MouseManipulator
    {
        private double _time = EditorApplication.timeSinceStartup;
        private readonly double _doubleClickDuration = 0.3;

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            var graphView = target as BehaviourTreeView;
            if (graphView == null)
                return;

            var duration = EditorApplication.timeSinceStartup - _time;
            if (duration < _doubleClickDuration)
            {
                SelectChildren(evt);
            }

            _time = EditorApplication.timeSinceStartup;
        }

        private void SelectChildren(MouseDownEvent evt)
        {
            var graphView = target as BehaviourTreeView;
            if (graphView == null)
                return;

            if (!CanStopManipulation(evt))
                return;

            var clickedElement = evt.target as NodeView;
            if (clickedElement == null)
            {
                var ve = evt.target as VisualElement;
                clickedElement = ve.GetFirstAncestorOfType<NodeView>();
                if (clickedElement == null)
                    return;
            }

            // Add children to selection so the root element can be moved
            Scripts.Runtime.BehaviourTree.Traverse(clickedElement.node, node =>
            {
                var view = graphView.FindNodeView(node);
                graphView.AddToSelection(view);
            });
        }
    }
}