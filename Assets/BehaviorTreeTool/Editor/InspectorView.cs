using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviorTreeTool.Editor
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

        private UnityEditor.Editor _editor;

        internal void UpdateSelection(NodeView nodeView)
        {
            Clear();
            if (_editor)
            {
                Object.DestroyImmediate(_editor);
            }

            if (nodeView?.Node == null)
            {
                var nullLabel = new Label("No Node selected or Node is null.");
                Add(nullLabel);
                return;
            }

            _editor = UnityEditor.Editor.CreateEditor(nodeView.Node);
            var container = new IMGUIContainer(() =>
            {
                if (_editor.target)
                {
                    _editor.OnInspectorGUI();
                }
            });
            Add(container);
        }
    }
}