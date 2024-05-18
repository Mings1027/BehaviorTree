using System;
using System.Collections.Generic;
using BehaviourTree.Scripts.TreeSharedData;
using Pathfinding;
using UnityEditor;
using UnityEngine;

namespace BehaviourTree.Editor
{
    [CustomEditor(typeof(SharedData))]
    public class SharedDataEditor : UnityEditor.Editor
    {
        private SerializedProperty _variablesProperty;
        private bool[] _foldouts;
        private string _foldoutKey;

        private void OnEnable()
        {
            _variablesProperty = serializedObject.FindProperty("variables");
            _foldoutKey = $"{target.GetInstanceID()}_SharedDataFoldouts";
            _foldouts = LoadFoldoutStates(_foldoutKey, _variablesProperty.arraySize);

            if (_foldouts.Length != _variablesProperty.arraySize)
            {
                Array.Resize(ref _foldouts, _variablesProperty.arraySize);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawVariablesList();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawVariablesList()
        {
            GUILayout.Label("Variables", EditorStyles.boldLabel);

            if (_variablesProperty is { isArray: true })
            {
                if (_foldouts.Length != _variablesProperty.arraySize)
                {
                    Array.Resize(ref _foldouts, _variablesProperty.arraySize);
                }

                for (var i = 0; i < _variablesProperty.arraySize; i++)
                {
                    var variableProperty = _variablesProperty.GetArrayElementAtIndex(i);
                    if (variableProperty is { managedReferenceValue: not null })
                    {
                        var variableNameProperty = variableProperty.FindPropertyRelative("variableName");
                        EditorGUILayout.BeginHorizontal();

                        _foldouts[i] = EditorGUILayout.Foldout(_foldouts[i],
                            $"{variableNameProperty.stringValue} ({DisplayTypeName(variableProperty)})", true);

                        if (GUILayout.Button("\u25b2", GUILayout.Width(40)))
                        {
                            MoveVariableUp(i);
                        }

                        if (GUILayout.Button("\u25bc", GUILayout.Width(40)))
                        {
                            MoveVariableDown(i);
                        }

                        if (GUILayout.Button("X", GUILayout.Width(30)))
                        {
                            DeleteVariable(variableNameProperty.stringValue, i);
                            serializedObject.ApplyModifiedProperties();
                            SaveFoldoutStates(_foldoutKey, _foldouts);
                            EditorGUILayout.EndHorizontal();
                            break;
                        }

                        EditorGUILayout.EndHorizontal();

                        if (_foldouts[i])
                        {
                            EditorGUILayout.BeginVertical("box");
                            DrawVariableNameAndType(variableProperty);
                            EditorGUILayout.Space();
                            DrawVariableFields(variableProperty);
                            EditorGUILayout.EndVertical();
                        }

                        if (i < _variablesProperty.arraySize - 1)
                        {
                            DrawHorizontalLine(Color.gray);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("_variablesProperty는 null이거나 배열이 아닙니다.");
            }

            SaveFoldoutStates(_foldoutKey, _foldouts);
        }

        private void MoveVariableUp(int index)
        {
            if (index > 0)
            {
                _variablesProperty.MoveArrayElement(index, index - 1);
            }
        }

        private void MoveVariableDown(int index)
        {
            if (index < _variablesProperty.arraySize - 1)
            {
                _variablesProperty.MoveArrayElement(index, index + 1);
            }
        }

        private void DrawVariableNameAndType(SerializedProperty variableProperty)
        {
            var variableNameProperty = variableProperty.FindPropertyRelative("variableName");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", GUILayout.Width(50));
            EditorGUILayout.TextField(variableNameProperty.stringValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type", GUILayout.Width(50));
            EditorGUILayout.EnumPopup(DisplayTypeName(variableProperty));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawVariableFields(SerializedProperty variableProperty)
        {
            var variableType = variableProperty.managedReferenceFullTypename;
            var valueProperty = variableProperty.FindPropertyRelative("_value");
            var variableNameProperty = variableProperty.FindPropertyRelative("variableName");
            var type = variableType.Split(' ')[1];

            if (type == typeof(SharedInt).FullName)
            {
                valueProperty.intValue =
                    EditorGUILayout.IntField(variableNameProperty.stringValue, valueProperty.intValue);
            }
            else if (type == typeof(SharedFloat).FullName)
            {
                valueProperty.floatValue =
                    EditorGUILayout.FloatField(variableNameProperty.stringValue, valueProperty.floatValue);
            }
            else if (type == typeof(SharedAIPath).FullName)
            {
                valueProperty.objectReferenceValue = EditorGUILayout.ObjectField(variableNameProperty.stringValue,
                    valueProperty.objectReferenceValue, typeof(AIPath), true);
            }
            else if (type == typeof(SharedTransform).FullName)
            {
                valueProperty.objectReferenceValue = EditorGUILayout.ObjectField(variableNameProperty.stringValue,
                    valueProperty.objectReferenceValue, typeof(Transform), true);
            }
            else if (type == typeof(SharedCollider).FullName)
            {
                valueProperty.objectReferenceValue = EditorGUILayout.ObjectField(variableNameProperty.stringValue,
                    valueProperty.objectReferenceValue, typeof(Collider), true);
            }
            else if (type == typeof(SharedColliderArray).FullName)
            {
                DrawArrayFoldout(variableNameProperty, valueProperty, typeof(Collider));
            }
            else if (type == typeof(SharedLayerMask).FullName)
            {
                var layerMaskValue = new LayerMask { value = valueProperty.intValue };
                layerMaskValue =
                    EditorGUILayoutExtensions.LayerMaskField(variableNameProperty.stringValue, layerMaskValue);
                valueProperty.intValue = layerMaskValue.value;
            }
            else if (type == typeof(SharedVector3).FullName)
            {
                valueProperty.vector3Value =
                    EditorGUILayout.Vector3Field(variableNameProperty.stringValue, valueProperty.vector3Value);
            }
            else if (type == typeof(SharedTransformArray).FullName)
            {
                DrawArrayFoldout(variableNameProperty, valueProperty, typeof(Transform));
            }
        }

        private void DrawArrayFoldout(SerializedProperty variableNameProperty, SerializedProperty valueProperty,
            Type elementType)
        {
            var arrayFoldoutKey = $"{_foldoutKey}_{variableNameProperty.stringValue}_Array";
            var arrayFoldout = EditorPrefs.GetBool(arrayFoldoutKey, false);

            EditorGUILayout.BeginHorizontal();
            arrayFoldout = EditorGUILayout.Foldout(arrayFoldout, variableNameProperty.stringValue, true);
            EditorPrefs.SetBool(arrayFoldoutKey, arrayFoldout);

            valueProperty.arraySize =
                Mathf.Max(0, EditorGUILayout.IntField(valueProperty.arraySize, GUILayout.Width(50)));
            EditorGUILayout.EndHorizontal();

            if (arrayFoldout)
            {
                EditorGUI.indentLevel++;
                if (valueProperty.isArray)
                {
                    for (var i = 0; i < valueProperty.arraySize; i++)
                    {
                        var elementProperty = valueProperty.GetArrayElementAtIndex(i);
                        elementProperty.objectReferenceValue = EditorGUILayout.ObjectField($"Element {i}",
                            elementProperty.objectReferenceValue, elementType, true);
                    }

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add Element"))
                    {
                        valueProperty.arraySize++;
                    }

                    if (GUILayout.Button("Remove Element"))
                    {
                        if (valueProperty.arraySize > 0)
                        {
                            valueProperty.arraySize--;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DeleteVariable(string variableName, int index)
        {
            var sharedData = (SharedData)target;

            serializedObject.Update();
            sharedData.RemoveVariable(variableName);
            _variablesProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();

            Array.Resize(ref _foldouts, _variablesProperty.arraySize);
        }

        private static void DrawHorizontalLine(Color color, int thickness = 1, int padding = 10)
        {
            var rect = EditorGUILayout.GetControlRect(false, thickness + padding * 2);
            rect.height = thickness;
            rect.y += padding / 2f;
            EditorGUI.DrawRect(rect, color);
        }

        private static void SaveFoldoutStates(string foldoutKey, IReadOnlyList<bool> foldouts)
        {
            for (var i = 0; i < foldouts.Count; i++)
            {
                EditorPrefs.SetBool($"{foldoutKey}_{i}", foldouts[i]);
            }
        }

        private static bool[] LoadFoldoutStates(string foldoutKey, int size)
        {
            var foldouts = new bool[size];
            for (var i = 0; i < size; i++)
            {
                foldouts[i] = EditorPrefs.GetBool($"{foldoutKey}_{i}", false);
            }

            return foldouts;
        }

        private static SharedVariableType DisplayTypeName(SerializedProperty variableProperty)
        {
            var variableType = variableProperty.managedReferenceFullTypename;
            var type = variableType.Split(' ')[1];
            var sharedVariableType = SharedVariableType.Int;

            if (type == typeof(SharedInt).FullName)
            {
                sharedVariableType = SharedVariableType.Int;
            }
            else if (type == typeof(SharedFloat).FullName)
            {
                sharedVariableType = SharedVariableType.Float;
            }
            else if (type == typeof(SharedAIPath).FullName)
            {
                sharedVariableType = SharedVariableType.AIPath;
            }
            else if (type == typeof(SharedTransform).FullName)
            {
                sharedVariableType = SharedVariableType.Transform;
            }
            else if (type == typeof(SharedCollider).FullName)
            {
                sharedVariableType = SharedVariableType.Collider;
            }
            else if (type == typeof(SharedColliderArray).FullName)
            {
                sharedVariableType = SharedVariableType.ColliderArray;
            }
            else if (type == typeof(SharedLayerMask).FullName)
            {
                sharedVariableType = SharedVariableType.LayerMask;
            }
            else if (type == typeof(SharedVector3).FullName)
            {
                sharedVariableType = SharedVariableType.Vector3;
            }
            else if (type == typeof(SharedTransformArray).FullName)
            {
                sharedVariableType = SharedVariableType.TransformArray;
            }

            return sharedVariableType;
        }
    }

    public static class EditorGUILayoutExtensions
    {
        public static LayerMask LayerMaskField(string label, LayerMask selected)
        {
            var layers = new List<string>();
            var layerNumbers = new List<int>();

            for (var i = 0; i < 32; i++)
            {
                var layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }

            var maskWithoutEmpty = 0;
            for (var i = 0; i < layerNumbers.Count; i++)
            {
                if ((selected & (1 << layerNumbers[i])) > 0)
                    maskWithoutEmpty |= 1 << i;
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());

            var mask = 0;
            for (var i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= 1 << layerNumbers[i];
            }

            selected.value = mask;
            return selected;
        }
    }
}
