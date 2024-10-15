using Tree;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviorTreeTool.Editor
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>
        {
        }

        private UnityEditor.Editor _editor;

        public void ShowTreeEditor(BehaviorTreeTab behaviorTreeTab)
        {
            Clear();
            if (_editor) Object.DestroyImmediate(_editor);
            if (behaviorTreeTab == null) return;

            _editor = UnityEditor.Editor.CreateEditor(behaviorTreeTab);
            var container = new IMGUIContainer(() =>
            {
                if (_editor.target) _editor.OnInspectorGUI();
            });
            Add(container);
        }
    }
}