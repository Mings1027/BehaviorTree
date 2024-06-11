using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                SharedVariableType.Int => new SharedInt
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Float => new SharedFloat
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Transform => new SharedTransform
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Collider => new SharedCollider
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.ColliderArray => new SharedColliderArray
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.LayerMask => new SharedLayerMask
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Vector3 => new SharedVector3
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.TransformArray => new SharedTransformArray
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Bool => new SharedBool
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Color => new SharedColor
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.GameObject => new SharedGameComponentObject
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.GameObjectList => new SharedGameObjectList
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Material => new SharedMaterial
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.NavMeshAgent => new SharedNavMeshAgent
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Quaternion => new SharedQuaternion
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Rect => new SharedRect
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.String => new SharedString
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Vector2 => new SharedVector2
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Vector2Int => new SharedVector2Int
                    { VariableName = variableName, VariableType = variableType },
                SharedVariableType.Vector3Int => new SharedVector3Int
                    { VariableName = variableName, VariableType = variableType },
                _ => null
            };
        }

        public static SharedVariableType DisplayType(SharedVariableBase variable)
        {
            var sharedType = variable switch
            {
                SharedBool => SharedVariableType.Bool,
                SharedCollider => SharedVariableType.Collider,
                SharedColliderArray => SharedVariableType.ColliderArray,
                SharedColor => SharedVariableType.Color,
                SharedFloat => SharedVariableType.Float,
                SharedGameComponentObject => SharedVariableType.GameObject,
                SharedGameObjectList => SharedVariableType.GameObjectList,
                SharedInt => SharedVariableType.Int,
                SharedLayerMask => SharedVariableType.LayerMask,
                SharedMaterial => SharedVariableType.Material,
                SharedNavMeshAgent => SharedVariableType.NavMeshAgent,
                SharedQuaternion => SharedVariableType.Quaternion,
                SharedRect => SharedVariableType.Rect,
                SharedString => SharedVariableType.String,
                SharedTransform => SharedVariableType.Transform,
                SharedTransformArray => SharedVariableType.TransformArray,
                SharedVector2 => SharedVariableType.Vector2,
                SharedVector2Int => SharedVariableType.Vector2Int,
                SharedVector3 => SharedVariableType.Vector3,
                SharedVector3Int => SharedVariableType.Vector3Int,
                _ => SharedVariableType.Bool
            };
            return sharedType;
        }

        public static void DrawSharedVariableValueField(SharedVariableBase variable, string valueLabel)
        {
            switch (variable)
            {
                case SharedInt sharedInt:
                    var newIntValue = EditorGUILayout.IntField(valueLabel, sharedInt.Value);
                    if (sharedInt.Value != newIntValue)
                    {
                        sharedInt.SetValue(newIntValue);
                    }
                    break;
                case SharedFloat sharedFloat:
                    var newFloatValue = EditorGUILayout.FloatField(valueLabel, sharedFloat.Value);
                    if (!Mathf.Approximately(sharedFloat.Value, newFloatValue))
                    {
                        sharedFloat.SetValue(newFloatValue);
                    }
                    break;
                case SharedTransform sharedTransform:
                    var newTransformValue =
                        (Transform)EditorGUILayout.ObjectField(valueLabel, sharedTransform.Value, typeof(Transform),
                            true);
                    if (sharedTransform.Value != newTransformValue)
                    {
                        sharedTransform.SetValue(newTransformValue);
                    }
                    break;
                case SharedCollider sharedCollider:
                    var newColliderValue =
                        (Collider)EditorGUILayout.ObjectField(valueLabel, sharedCollider.Value, typeof(Collider), true);
                    if (sharedCollider.Value != newColliderValue)
                    {
                        sharedCollider.SetValue(newColliderValue);
                    }
                    break;
                case SharedColliderArray sharedColliderArray:
                    DrawArrayField(sharedColliderArray);
                    break;
                case SharedLayerMask sharedLayerMask:
                    LayerMask newLayerMaskValue = EditorGUILayout.LayerField(valueLabel, sharedLayerMask.Value);
                    if (sharedLayerMask.Value != newLayerMaskValue)
                    {
                        sharedLayerMask.SetValue(newLayerMaskValue);
                    }
                    break;
                case SharedVector3 sharedVector3:
                    var newVector3Value = EditorGUILayout.Vector3Field(valueLabel, sharedVector3.Value);
                    if (sharedVector3.Value != newVector3Value)
                    {
                        sharedVector3.SetValue(newVector3Value);
                    }
                    break;
                case SharedTransformArray sharedTransformArray:
                    DrawArrayField(sharedTransformArray);
                    break;
                case SharedBool sharedBool:
                    var newBoolValue = EditorGUILayout.Toggle(valueLabel, sharedBool.Value);
                    if (sharedBool.Value != newBoolValue)
                    {
                        sharedBool.SetValue(newBoolValue);
                    }
                    break;
                case SharedColor sharedColor:
                    var newColorValue = EditorGUILayout.ColorField(valueLabel, sharedColor.Value);
                    if (sharedColor.Value != newColorValue)
                    {
                        sharedColor.SetValue(newColorValue);
                    }
                    break;
                case SharedGameComponentObject sharedGameObject:
                    var newGameObjectValue =
                        (GameObject)EditorGUILayout.ObjectField(valueLabel, sharedGameObject.Value, typeof(GameObject),
                            true);
                    if (sharedGameObject.Value != newGameObjectValue)
                    {
                        sharedGameObject.SetValue(newGameObjectValue);
                    }
                    break;
                case SharedGameObjectList sharedGameObjectList:
                    DrawListField(sharedGameObjectList);
                    break;
                case SharedMaterial sharedMaterial:
                    var newMaterialValue =
                        (Material)EditorGUILayout.ObjectField(valueLabel, sharedMaterial.Value, typeof(Material), true);
                    if (sharedMaterial.Value != newMaterialValue)
                    {
                        sharedMaterial.SetValue(newMaterialValue);
                    }
                    break;
                case SharedNavMeshAgent sharedNavMeshAgent:
                    var newNavMeshAgentValue = (NavMeshAgent)EditorGUILayout.ObjectField(valueLabel,
                        sharedNavMeshAgent.Value, typeof(NavMeshAgent), true);
                    if (sharedNavMeshAgent.Value != newNavMeshAgentValue)
                    {
                        sharedNavMeshAgent.SetValue(newNavMeshAgentValue);
                    }
                    break;
                case SharedQuaternion sharedQuaternion:
                {
                    var newEulerAngles =
                        EditorGUILayout.Vector3Field(valueLabel, sharedQuaternion.Value.eulerAngles);
                    if (sharedQuaternion.Value.eulerAngles != newEulerAngles)
                    {
                        sharedQuaternion.SetValue(Quaternion.Euler(newEulerAngles));
                    }
                    break;
                }
                case SharedRect sharedRect:
                    var newRectValue = EditorGUILayout.RectField(valueLabel, sharedRect.Value);
                    if (sharedRect.Value != newRectValue)
                    {
                        sharedRect.SetValue(newRectValue);
                    }
                    break;
                case SharedString sharedString:
                    var newStringValue = EditorGUILayout.TextField(valueLabel, sharedString.Value);
                    if (sharedString.Value != newStringValue)
                    {
                        sharedString.SetValue(newStringValue);
                    }
                    break;
                case SharedVector2 sharedVector2:
                    var newVector2Value = EditorGUILayout.Vector2Field(valueLabel, sharedVector2.Value);
                    if (sharedVector2.Value != newVector2Value)
                    {
                        sharedVector2.SetValue(newVector2Value);
                    }
                    break;
                case SharedVector2Int sharedVector2Int:
                    var newVector2IntValue = EditorGUILayout.Vector2IntField(valueLabel, sharedVector2Int.Value);
                    if (sharedVector2Int.Value != newVector2IntValue)
                    {
                        sharedVector2Int.SetValue(newVector2IntValue);
                    }
                    break;
                case SharedVector3Int sharedVector3Int:
                    var newVector3IntValue = EditorGUILayout.Vector3IntField(valueLabel, sharedVector3Int.Value);
                    if (sharedVector3Int.Value != newVector3IntValue)
                    {
                        sharedVector3Int.SetValue(newVector3IntValue);
                    }
                    break;
                default:
                    EditorGUILayout.LabelField("Unsupported SharedVariable type");
                    break;
            }
        }

        private static void DrawArrayField<T>(SharedVariable<T[]> sharedVariableArray)
        {
            var array = sharedVariableArray.Value ?? Array.Empty<T>();

            EditorGUILayout.BeginHorizontal();

            if (!string.IsNullOrEmpty(sharedVariableArray.VariableName))
            {
                ArrayFoldouts.TryAdd(sharedVariableArray.VariableName, false);

                ArrayFoldouts[sharedVariableArray.VariableName] =
                    EditorGUILayout.Foldout(ArrayFoldouts[sharedVariableArray.VariableName],
                        $"Array Elements", true);
            }

            var newSize = EditorGUILayout.IntField(array.Length, GUILayout.Width(50));
            if (GUILayout.Button(new GUIContent(PlusTexture), GUILayout.Width(20), GUILayout.Height(20)))
            {
                newSize++;
            }

            if (GUILayout.Button(new GUIContent(MinusTexture), GUILayout.Width(20), GUILayout.Height(20)))
            {
                newSize--;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(1);

            if (newSize < 0) newSize = 0;
            if (newSize != array.Length)
            {
                var newArray = new T[newSize];
                for (var i = 0; i < Mathf.Min(newSize, array.Length); i++)
                {
                    newArray[i] = array[i];
                }
                sharedVariableArray.SetValue(newArray);
                array = newArray;
            }

            if (!string.IsNullOrEmpty(sharedVariableArray.VariableName) &&
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

        private static void DrawListField<T>(SharedVariable<List<T>> sharedVariableList) where T : Object
        {
            var list = sharedVariableList.Value ?? new List<T>();

            if (!string.IsNullOrEmpty(sharedVariableList.VariableName))
            {
                ListFoldouts.TryAdd(sharedVariableList.VariableName, false);

                EditorGUILayout.BeginHorizontal();

                ListFoldouts[sharedVariableList.VariableName] =
                    EditorGUILayout.Foldout(ListFoldouts[sharedVariableList.VariableName],
                        $"List Elements", true);
            }

            var newSize = EditorGUILayout.IntField(list.Count, GUILayout.Width(50));
            if (GUILayout.Button(new GUIContent(PlusTexture), GUILayout.Width(20), GUILayout.Height(20)))
            {
                newSize++;
            }

            if (GUILayout.Button(new GUIContent(MinusTexture), GUILayout.Width(20), GUILayout.Height(20)))
            {
                newSize--;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(1);

            if (newSize < 0) newSize = 0;
            if (newSize != list.Count)
            {
                var newList = new List<T>(newSize);
                for (var i = 0; i < Mathf.Min(newSize, list.Count); i++)
                {
                    newList.Add(list[i]);
                }
                for (var i = list.Count; i < newSize; i++)
                {
                    newList.Add(null);
                }
                sharedVariableList.SetValue(newList);
                list = newList;
            }

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

        // 필드를 그리는 함수
        public static void DrawField(FieldInfo field, Node node)
        {
            var fieldType = field.FieldType;
            var fieldValue = field.GetValue(node);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(field.Name, GUILayout.Width(EditorGUIUtility.labelWidth));

            if (fieldType == typeof(int))
            {
                field.SetValue(node, EditorGUILayout.IntField((int)fieldValue));
            }
            else if (fieldType == typeof(float))
            {
                field.SetValue(node, EditorGUILayout.FloatField((float)fieldValue));
            }
            else if (fieldType == typeof(string))
            {
                field.SetValue(node, EditorGUILayout.TextField((string)fieldValue));
            }
            else if (fieldType == typeof(bool))
            {
                field.SetValue(node, EditorGUILayout.Toggle((bool)fieldValue));
            }
            else if (fieldType == typeof(Vector3))
            {
                field.SetValue(node, EditorGUILayout.Vector3Field("", (Vector3)fieldValue));
            }
            else if (fieldType == typeof(Color))
            {
                field.SetValue(node, EditorGUILayout.ColorField((Color)fieldValue));
            }
            else if (typeof(Object).IsAssignableFrom(fieldType))
            {
                field.SetValue(node, EditorGUILayout.ObjectField((Object)fieldValue, fieldType, true));
            }
            else
            {
                EditorGUILayout.LabelField($"Unsupported field type: {fieldType}");
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        // 가로선을 그리는 함수
        public static void DrawHorizontalLine(Color color, int thickness = 1)
        {
            var rect = EditorGUILayout.GetControlRect(false, thickness);
            EditorGUI.DrawRect(rect, color);
        }

        // 폴드아웃 상태를 저장하는 함수
        public static void SaveFoldoutStates(string key, bool[] states)
        {
            for (int i = 0; i < states.Length; i++)
            {
                EditorPrefs.SetBool($"{key}_{i}", states[i]);
            }
        }

        public static bool[] LoadFoldoutStates(string key, int size)
        {
            bool[] states = new bool[size];
            for (int i = 0; i < size; i++)
            {
                states[i] = EditorPrefs.GetBool($"{key}_{i}", false);
            }
            return states;
        }

        // 노드 타입 이름을 가져오는 함수
        public static string GetNodeTypeName(Type type)
        {
            return NodeTypeNames.FirstOrDefault(nodeType => nodeType.Key.IsAssignableFrom(type)).Value ?? "Unknown";
        }
    }
}