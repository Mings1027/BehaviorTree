using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTree.Editor
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits>
        {
        }

        private UnityEditor.Editor _editor;

        public InspectorView()
        {
        }

        internal void UpdateSelection(NodeView nodeView)
        {
            Clear();
            if (_editor)
            {
                Object.DestroyImmediate(_editor);
            }

            _editor = UnityEditor.Editor.CreateEditor(nodeView.Node);
            IMGUIContainer container = new IMGUIContainer(() =>
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