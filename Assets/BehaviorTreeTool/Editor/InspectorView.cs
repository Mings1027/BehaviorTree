using Tree;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviorTreeTool.Editor
{
    [UxmlElement("inspectorView")] // 이름을 소문자로 시작하는 것이 관례입니다
    public partial class InspectorView : VisualElement
    {
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