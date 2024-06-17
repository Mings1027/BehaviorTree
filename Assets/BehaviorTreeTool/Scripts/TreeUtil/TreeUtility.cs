using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BehaviorTreeTool.Scripts.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace BehaviorTreeTool.Scripts.TreeUtil
{
    public static class TreeUtility
    {
        private static readonly Texture2D PlusTexture =
            AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BehaviorTreeTool/Sprites/Plus.png");

        private static readonly Texture2D MinusTexture =
            AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BehaviorTreeTool/Sprites/Minus.png");

        private static readonly Dictionary<string, bool> ArrayFoldouts = new();
        private static readonly Dictionary<string, bool> ListFoldouts = new();

        private static readonly Dictionary<Type, string> NodeTypeNames = new()
        {
            { typeof(ActionNode), "Action" },
            { typeof(CompositeNode), "Composite" },
            { typeof(ConditionNode), "Condition" },
            { typeof(DecoratorNode), "Decorator" },
            { typeof(RootNode), "Root" }
        };

        // Shared 변수를 생성하는 함수
        public static SharedVariableBase CreateSharedVariable(string variableName, SharedVariableType variableType)
        {
            return variableType switch
            {
                SharedVariableType.Bool
                    => new SharedBool { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Collider
                    => new SharedCollider { VariableName = variableName, VariableType = variableType },
                SharedVariableType.ColliderArray
                    => new SharedColliderArray { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Color
                    => new SharedColor { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Float
                    => new SharedFloat { VariableName = variableName, VariableType = variableType },
                SharedVariableType.GameObject
                    => new SharedGameObject { VariableName = variableName, VariableType = variableType },
                SharedVariableType.GameObjectList
                    => new SharedGameObjectList { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Int
                    => new SharedInt { VariableName = variableName, VariableType = variableType },
                SharedVariableType.LayerMask
                    => new SharedLayerMask { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Material
                    => new SharedMaterial { VariableName = variableName, VariableType = variableType },
                SharedVariableType.NavMeshAgent
                    => new SharedNavMeshAgent { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Quaternion
                    => new SharedQuaternion { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Rect
                    => new SharedRect { VariableName = variableName, VariableType = variableType },
                SharedVariableType.String
                    => new SharedString { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Transform
                    => new SharedTransform { VariableName = variableName, VariableType = variableType },
                SharedVariableType.TransformArray
                    => new SharedTransformArray { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Vector2
                    => new SharedVector2 { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Vector2Int
                    => new SharedVector2Int { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Vector3
                    => new SharedVector3 { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Vector3Int
                    => new SharedVector3Int { VariableName = variableName, VariableType = variableType },
                _ => null
            };
        }


        public static void DrawSharedVariableValueField(SharedVariableBase variable, string valueLabel)
        {
            switch (variable)
            {
                case SharedBool sharedBool:
                    sharedBool.SetValue(EditorGUILayout.Toggle(valueLabel, sharedBool.Value));
                    break;
                case SharedCollider sharedCollider:
                    sharedCollider.SetValue((Collider)EditorGUILayout.ObjectField(valueLabel, sharedCollider.Value, typeof(Collider), true));
                    break;
                case SharedColliderArray sharedColliderArray:
                    DrawArrayField(sharedColliderArray);
                    break;
                case SharedColor sharedColor:
                    sharedColor.SetValue(EditorGUILayout.ColorField(valueLabel, sharedColor.Value));
                    break;
                case SharedFloat sharedFloat:
                    sharedFloat.SetValue(EditorGUILayout.FloatField(valueLabel, sharedFloat.Value));
                    break;
                case SharedGameObject sharedGameObject:
                    sharedGameObject.SetValue((GameObject)EditorGUILayout.ObjectField(valueLabel, sharedGameObject.Value, typeof(GameObject), true));
                    break;
                case SharedGameObjectList sharedGameObjectList:
                    DrawListField(sharedGameObjectList);
                    break;
                case SharedInt sharedInt:
                    sharedInt.SetValue(EditorGUILayout.IntField(valueLabel, sharedInt.Value));
                    break;
                case SharedLayerMask sharedLayerMask:
                    sharedLayerMask.SetValue((LayerMask)EditorGUILayout.LayerField(valueLabel, sharedLayerMask.Value));
                    break;
                case SharedMaterial sharedMaterial:
                    sharedMaterial.SetValue((Material)EditorGUILayout.ObjectField(valueLabel, sharedMaterial.Value, typeof(Material), true));
                    break;
                case SharedNavMeshAgent sharedNavMeshAgent:
                    sharedNavMeshAgent.SetValue((NavMeshAgent)EditorGUILayout.ObjectField(valueLabel, sharedNavMeshAgent.Value, typeof(NavMeshAgent), true));
                    break;
                case SharedQuaternion sharedQuaternion:
                    sharedQuaternion.SetValue(Quaternion.Euler(EditorGUILayout.Vector3Field(valueLabel, sharedQuaternion.Value.eulerAngles)));
                    break;
                case SharedRect sharedRect:
                    sharedRect.SetValue(EditorGUILayout.RectField(valueLabel, sharedRect.Value));
                    break;
                case SharedString sharedString:
                    sharedString.SetValue(EditorGUILayout.TextField(valueLabel, sharedString.Value));
                    break;
                case SharedTransform sharedTransform:
                    sharedTransform.SetValue((Transform)EditorGUILayout.ObjectField(valueLabel, sharedTransform.Value, typeof(Transform), true));
                    break;
                case SharedTransformArray sharedTransformArray:
                    DrawArrayField(sharedTransformArray);
                    break;
                case SharedVector2 sharedVector2:
                    sharedVector2.SetValue(EditorGUILayout.Vector2Field(valueLabel, sharedVector2.Value));
                    break;
                case SharedVector2Int sharedVector2Int:
                    sharedVector2Int.SetValue(EditorGUILayout.Vector2IntField(valueLabel, sharedVector2Int.Value));
                    break;
                case SharedVector3 sharedVector3:
                    sharedVector3.SetValue(EditorGUILayout.Vector3Field(valueLabel, sharedVector3.Value));
                    break;
                case SharedVector3Int sharedVector3Int:
                    sharedVector3Int.SetValue(EditorGUILayout.Vector3IntField(valueLabel, sharedVector3Int.Value));
                    break;
                default:
                    EditorGUILayout.LabelField("Unsupported SharedVariable type");
                    break;
            }
        }

        // 타입에 맞는 필드를 그리는 함수
        private static object DrawFieldForType(Type type, object value, string label)
        {
            if (type == typeof(int))
            {
                return EditorGUILayout.IntField(label, (int)value);
            }

            if (type == typeof(float))
            {
                return EditorGUILayout.FloatField(label, (float)value);
            }

            if (type == typeof(Transform))
            {
                return EditorGUILayout.ObjectField(label, (Transform)value, typeof(Transform), true);
            }

            if (type == typeof(Collider))
            {
                return EditorGUILayout.ObjectField(label, (Collider)value, typeof(Collider), true);
            }

            if (type == typeof(LayerMask))
            {
                return EditorGUILayout.LayerField(label, (LayerMask)value);
            }

            if (type == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(label, (Vector3)value);
            }

            if (type == typeof(Transform[]))
            {
                EditorGUILayout.LabelField(label, "Nested arrays not supported in custom editor.");
                return value;
            }

            EditorGUILayout.LabelField(label, "Unsupported type");
            return value;
        }

        private static void DrawArrayField<T>(SharedVariable<T[]> sharedVariableArray)
        {
            var array = sharedVariableArray.Value ?? Array.Empty<T>();

            EditorGUILayout.BeginHorizontal();

            // if (!string.IsNullOrEmpty(sharedVariableArray.VariableName))
            {
                ArrayFoldouts.TryAdd(sharedVariableArray.VariableName, false);

                ArrayFoldouts[sharedVariableArray.VariableName] =
                    EditorGUILayout.Foldout(ArrayFoldouts[sharedVariableArray.VariableName], $"Array Elements", true);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(1);

            if (/* !string.IsNullOrEmpty(sharedVariableArray.VariableName) && */
                ArrayFoldouts.ContainsKey(sharedVariableArray.VariableName) &&
                ArrayFoldouts[sharedVariableArray.VariableName])
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < array.Length; i++)
                {
                    var newValue = (T)DrawFieldForType(typeof(T), array[i], $"Element {i}");
                    if (!EqualityComparer<T>.Default.Equals(array[i], newValue))
                    {
                        array[i] = newValue;
                        sharedVariableArray.SetValue(array);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawListField<T>(SharedVariable<List<T>> sharedVariableList) where T : Object
        {
            var list = sharedVariableList.Value ?? new List<T>();

            if (!string.IsNullOrEmpty(sharedVariableList.VariableName))
            {
                ListFoldouts.TryAdd(sharedVariableList.VariableName, false);

                EditorGUILayout.BeginHorizontal();

                ListFoldouts[sharedVariableList.VariableName] =
                    EditorGUILayout.Foldout(ListFoldouts[sharedVariableList.VariableName], $"List Elements", true);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(1);

            if (!string.IsNullOrEmpty(sharedVariableList.VariableName) &&
                ListFoldouts.ContainsKey(sharedVariableList.VariableName) &&
                ListFoldouts[sharedVariableList.VariableName])
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < list.Count; i++)
                {
                    var newValue = (T)EditorGUILayout.ObjectField($"Element {i}", list[i], typeof(T), true);
                    if (list[i] != newValue)
                    {
                        list[i] = newValue;
                        sharedVariableList.SetValue(list);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        public static void DrawArrayField<T>(SerializedProperty arrayProperty) where T : Object
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

        public static void DrawListField<T>(SerializedProperty listProperty) where T : Object
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

        // 가로선을 그리는 함수
        public static void DrawHorizontalLine(Color color, int thickness = 1)
        {
            var rect = EditorGUILayout.GetControlRect(false, thickness);
            EditorGUI.DrawRect(rect, color);
        }

        public static Texture2D LoadTexture(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // 노드 타입 이름을 가져오는 함수
        public static string GetNodeTypeName(Type type)
        {
            return NodeTypeNames.FirstOrDefault(nodeType => nodeType.Key.IsAssignableFrom(type)).Value ?? "Unknown";
        }

        public static void DrawSharedVariableValue(SharedVariableType variableType, SerializedProperty valueProperty)
        {
            switch (variableType)
            {
                case SharedVariableType.AIPath:
                    DrawAIPathField(valueProperty);
                    break;
                case SharedVariableType.Animator:
                    DrawAnimatorField(valueProperty);
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

        public static void DrawCollectionField(SharedVariableType variableType, SerializedProperty collectionProperty)
        {
            switch (variableType)
            {
                case SharedVariableType.ColliderArray:
                    DrawArrayField<Collider>(collectionProperty);
                    break;
                case SharedVariableType.GameObjectList:
                    DrawListField<GameObject>(collectionProperty);
                    break;
                case SharedVariableType.TransformArray:
                    DrawArrayField<Transform>(collectionProperty);
                    break;
            }
        }

        private static void DrawAIPathField(SerializedProperty valueProperty)
        {
            EditorGUILayout.LabelField("AIPath type not supported");
        }

        private static void DrawAnimatorField(SerializedProperty valueProperty)
        {
            valueProperty.objectReferenceValue =
                EditorGUILayout.ObjectField(valueProperty.objectReferenceValue, typeof(Animator), true);
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



        public static Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}