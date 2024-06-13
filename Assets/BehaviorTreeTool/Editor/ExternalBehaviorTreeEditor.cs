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
                bool useGetComponent = false;
                if (variableProperty.managedReferenceValue is IComponentObject componentObject)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Use GetComponent", GUILayout.Width(120));
                    useGetComponent = EditorGUILayout.Toggle(componentObject.UseGetComponent);
                    componentObject.UseGetComponent = useGetComponent;
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(variableName, GUILayout.Width(110));

                // 필드 비활성화
                EditorGUI.BeginDisabledGroup(useGetComponent);
                DrawSharedVariableValue(variableType, valueProperty);
                EditorGUI.EndDisabledGroup();

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
                DrawCollectionField(variableType, valueProperty);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawSharedVariableValue(SharedVariableType variableType, SerializedProperty valueProperty)
        {
            switch (variableType)
            {
                case SharedVariableType.AIPath:
                    DrawAIPathField();
                    break;
                case SharedVariableType.Bool:
                    DrawBoolField(valueProperty);
                    break;
                case SharedVariableType.Collider:
                    DrawColliderField(valueProperty);
                    break;
                case SharedVariableType.Color:
                    DrawColorField(valueProperty);
                    break;
                case SharedVariableType.Float:
                    DrawFloatField(valueProperty);
                    break;
                case SharedVariableType.GameObject:
                    DrawGameObjectField(valueProperty);
                    break;
                case SharedVariableType.Int:
                    DrawIntField(valueProperty);
                    break;
                case SharedVariableType.LayerMask:
                    DrawLayerMaskField(valueProperty);
                    break;
                case SharedVariableType.Material:
                    DrawMaterialField(valueProperty);
                    break;
                case SharedVariableType.NavMeshAgent:
                    DrawNavMeshAgentField(valueProperty);
                    break;
                case SharedVariableType.Quaternion:
                    DrawQuaternionField(valueProperty);
                    break;
                case SharedVariableType.Rect:
                    DrawRectField(valueProperty);
                    break;
                case SharedVariableType.String:
                    DrawStringField(valueProperty);
                    break;
                case SharedVariableType.Transform:
                    DrawTransformField(valueProperty);
                    break;
                case SharedVariableType.Vector2:
                    DrawVector2Field(valueProperty);
                    break;
                case SharedVariableType.Vector2Int:
                    DrawVector2IntField(valueProperty);
                    break;
                case SharedVariableType.Vector3:
                    DrawVector3Field(valueProperty);
                    break;
                case SharedVariableType.Vector3Int:
                    DrawVector3IntField(valueProperty);
                    break;
                default:
                    EditorGUILayout.LabelField("Unsupported SharedVariable type");
                    break;
            }
        }

        private void DrawCollectionField(SharedVariableType variableType, SerializedProperty collectionProperty)
        {
            switch (variableType)
            {
                case SharedVariableType.ColliderArray:
                    DrawArrayField<Collider>(collectionProperty);
                    break;
                case SharedVariableType.TransformArray:
                    DrawArrayField<Transform>(collectionProperty);
                    break;
                case SharedVariableType.GameObjectList:
                    DrawListField<GameObject>(collectionProperty);
                    break;
            }
        }

        private static void DrawAIPathField()
        {
            EditorGUILayout.LabelField("AIPath type not supported");
        }

        private static void DrawBoolField(SerializedProperty valueProperty)
        {
            valueProperty.boolValue = EditorGUILayout.Toggle(valueProperty.boolValue);
        }

        private static void DrawColliderField(SerializedProperty valueProperty)
        {
            valueProperty.objectReferenceValue =
                EditorGUILayout.ObjectField(valueProperty.objectReferenceValue, typeof(Collider), true);
        }

        private static void DrawColorField(SerializedProperty valueProperty)
        {
            valueProperty.colorValue = EditorGUILayout.ColorField(valueProperty.colorValue);
        }

        private static void DrawFloatField(SerializedProperty valueProperty)
        {
            valueProperty.floatValue = EditorGUILayout.FloatField(valueProperty.floatValue);
        }

        private static void DrawGameObjectField(SerializedProperty valueProperty)
        {
            valueProperty.objectReferenceValue =
                EditorGUILayout.ObjectField(valueProperty.objectReferenceValue, typeof(GameObject), true);
        }

        private static void DrawIntField(SerializedProperty valueProperty)
        {
            valueProperty.intValue = EditorGUILayout.IntField(valueProperty.intValue);
        }

        private static void DrawLayerMaskField(SerializedProperty valueProperty)
        {
            valueProperty.intValue = EditorGUILayout.LayerField(valueProperty.intValue);
        }

        private static void DrawMaterialField(SerializedProperty valueProperty)
        {
            valueProperty.objectReferenceValue =
                EditorGUILayout.ObjectField(valueProperty.objectReferenceValue, typeof(Material), true);
        }

        private static void DrawNavMeshAgentField(SerializedProperty valueProperty)
        {
            valueProperty.objectReferenceValue =
                EditorGUILayout.ObjectField(valueProperty.objectReferenceValue, typeof(NavMeshAgent), true);
        }

        private static void DrawQuaternionField(SerializedProperty valueProperty)
        {
            var eulerValue = valueProperty.quaternionValue.eulerAngles;
            eulerValue = EditorGUILayout.Vector3Field("", eulerValue);
            valueProperty.quaternionValue = Quaternion.Euler(eulerValue);
        }

        private static void DrawRectField(SerializedProperty valueProperty)
        {
            valueProperty.rectValue = EditorGUILayout.RectField(valueProperty.rectValue);
        }

        private static void DrawStringField(SerializedProperty valueProperty)
        {
            valueProperty.stringValue = EditorGUILayout.TextField(valueProperty.stringValue);
        }

        private static void DrawTransformField(SerializedProperty valueProperty)
        {
            valueProperty.objectReferenceValue =
                EditorGUILayout.ObjectField(valueProperty.objectReferenceValue, typeof(Transform), true);
        }

        private static void DrawVector2Field(SerializedProperty valueProperty)
        {
            valueProperty.vector2Value = EditorGUILayout.Vector2Field("", valueProperty.vector2Value);
        }

        private static void DrawVector2IntField(SerializedProperty valueProperty)
        {
            valueProperty.vector2IntValue = EditorGUILayout.Vector2IntField("", valueProperty.vector2IntValue);
        }

        private static void DrawVector3Field(SerializedProperty valueProperty)
        {
            valueProperty.vector3Value = EditorGUILayout.Vector3Field("", valueProperty.vector3Value);
        }

        private static void DrawVector3IntField(SerializedProperty valueProperty)
        {
            valueProperty.vector3IntValue = EditorGUILayout.Vector3IntField("", valueProperty.vector3IntValue);
        }

        private static void DrawArrayField<T>(SerializedProperty arrayProperty) where T : Object
        {
            EditorGUILayout.BeginVertical();
            var arraySize = arrayProperty.arraySize;
            arraySize = EditorGUILayout.IntField("Size", arraySize);

            if (arraySize != arrayProperty.arraySize)
            {
                while (arraySize > arrayProperty.arraySize)
                    arrayProperty.InsertArrayElementAtIndex(arrayProperty.arraySize);
                while (arraySize < arrayProperty.arraySize)
                    arrayProperty.DeleteArrayElementAtIndex(arrayProperty.arraySize - 1);
            }

            EditorGUI.indentLevel++;
            for (var i = 0; i < arrayProperty.arraySize; i++)
            {
                var elementProperty = arrayProperty.GetArrayElementAtIndex(i);
                elementProperty.objectReferenceValue = EditorGUILayout.ObjectField($"Element {i}",
                    elementProperty.objectReferenceValue, typeof(T), true);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private static void DrawListField<T>(SerializedProperty listProperty) where T : Object
        {
            EditorGUILayout.BeginVertical();
            var listSize = listProperty.arraySize;
            listSize = EditorGUILayout.IntField("Size", listSize);

            if (listSize != listProperty.arraySize)
            {
                while (listSize > listProperty.arraySize)
                    listProperty.InsertArrayElementAtIndex(listProperty.arraySize);
                while (listSize < listProperty.arraySize)
                    listProperty.DeleteArrayElementAtIndex(listProperty.arraySize - 1);
            }

            EditorGUI.indentLevel++;
            for (var i = 0; i < listProperty.arraySize; i++)
            {
                var elementProperty = listProperty.GetArrayElementAtIndex(i);
                elementProperty.objectReferenceValue = EditorGUILayout.ObjectField($"Element {i}",
                    elementProperty.objectReferenceValue, typeof(T), true);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    }
}