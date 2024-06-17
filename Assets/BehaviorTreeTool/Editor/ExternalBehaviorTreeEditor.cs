using System.Collections.Generic;
using BehaviorTreeTool.Scripts.TreeUtil;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviorTreeTool.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ExternalBehaviorTreeRunner))]
    public class ExternalBehaviorTreeEditor : UnityEditor.Editor
    {
        private SerializedProperty _externalTreeProperty;
        private SerializedProperty _variablesProperty;

        // 폴드 상태를 저장하는 딕셔너리
        private readonly Dictionary<string, bool> _foldoutStates = new();

        private void OnEnable()
        {
            _externalTreeProperty = serializedObject.FindProperty("behaviorTree");
            _variablesProperty = serializedObject.FindProperty("variables");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_externalTreeProperty);

            var treeRunner = (ExternalBehaviorTreeRunner)target;
            ExternalTreePropertyChange(treeRunner);

            // 폴드 상태가 저장되지 않은 경우 초기화
            var treeKey = treeRunner.GetInstanceID().ToString();
            _foldoutStates.TryAdd(treeKey, true);

            DrawVariables(treeRunner, _foldoutStates[treeKey]);

            serializedObject.ApplyModifiedProperties();
        }

        private void ExternalTreePropertyChange(ExternalBehaviorTreeRunner treeRunner)
        {
            // externalTreeProperty가 변경되었는지 감지
            if (_externalTreeProperty.objectReferenceValue != treeRunner.ExternalBehaviorTree)
            {
                treeRunner.ExternalBehaviorTree = (ExternalBehaviorTree)_externalTreeProperty.objectReferenceValue;
                serializedObject.Update();
            }
        }

        private void DrawVariables(ExternalBehaviorTreeRunner treeRunner, bool isFolded)
        {
            isFolded = EditorGUILayout.Foldout(isFolded, "Variables", true);
            _foldoutStates[treeRunner.GetInstanceID().ToString()] = isFolded;

            if (isFolded)
            {
                if (_variablesProperty != null)
                {
                    if (treeRunner.ExternalBehaviorTree != null)
                    {
                        for (var i = 0; i < _variablesProperty.arraySize; i++)
                        {
                            var variableProperty = _variablesProperty.GetArrayElementAtIndex(i);
                            DrawSharedVariableField(variableProperty);
                        }
                    }
                    else
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField("Assign a Behavior Tree");
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }

        private void DrawSharedVariableField(SerializedProperty variableProperty)
        {
            var variableType = (SharedVariableType)variableProperty.FindPropertyRelative("variableType").enumValueIndex;
            var valueProperty = variableProperty.FindPropertyRelative("value");
            var variableName = variableProperty.FindPropertyRelative("variableName").stringValue;
            var propertyPath = variableProperty.propertyPath;

            if (IsCollectionVariable(variableType))
            {
                EditorGUI.indentLevel++;
                DrawCollectionVariable(variableType, variableName, valueProperty, propertyPath);
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(variableName, GUILayout.Width(110));

                TreeUtility.DrawSharedVariableValue(variableType, valueProperty);

                EditorGUILayout.EndHorizontal();
            }

            TreeUtility.DrawHorizontalLine(Color.gray);
        }

        private static bool IsCollectionVariable(SharedVariableType variableType)
        {
            return variableType is SharedVariableType.ColliderArray or SharedVariableType.TransformArray
                or SharedVariableType.GameObjectList;
        }

        private void DrawCollectionVariable(SharedVariableType variableType, string variableName,
            SerializedProperty valueProperty, string propertyPath)
        {
            _foldoutStates.TryAdd(propertyPath, true);

            var isFolded = EditorGUILayout.Foldout(_foldoutStates[propertyPath], variableName, true);
            _foldoutStates[propertyPath] = isFolded;

            if (isFolded)
            {
                EditorGUI.indentLevel++;
                TreeUtility.DrawCollectionField(variableType, valueProperty);
                EditorGUI.indentLevel--;
            }
        }
    }
}