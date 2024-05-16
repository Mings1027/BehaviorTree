using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;
using UnityEditor.UIElements;

namespace BehaviourTree.Editor
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>
        {
        }

        private UnityEditor.Editor _editor;
        private PopupField<string> _sharedDataKeysField;

        public InspectorView()
        {
        }

        internal void UpdateSelection(NodeView nodeView)
        {
            Clear();

            UnityEngine.Object.DestroyImmediate(_editor);

            _editor = UnityEditor.Editor.CreateEditor(nodeView.node);
            IMGUIContainer container = new IMGUIContainer(() =>
            {
                if (_editor && _editor.target)
                {
                    _editor.OnInspectorGUI();
                }
            });
            Add(container);

            // Shared Data Keys Dropdown
            // var node = nodeView.node;
            // if (node.sharedData != null)
            // {
            //     var keys = node.sharedData.Variables.Select(v => v.variableName).ToList();
            //     if (keys.Count > 0)
            //     {
            //         _sharedDataKeysField = new PopupField<string>("Shared Data Keys", keys, 0);
            //         _sharedDataKeysField.RegisterValueChangedCallback(evt =>
            //         {
            //             Debug.Log($"Selected Shared Data Key: {evt.newValue}");
            //             // 추가 로직을 여기에 작성합니다 (필요시)
            //         });
            //         Add(_sharedDataKeysField);
            //     }
            //     else
            //     {
            //         var noKeysLabel = new Label("No keys available in Shared Data.");
            //         Add(noKeysLabel);
            //     }
            // }
        }
    }
}