#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tree
{
    public static class TreeUtility
    {
        private static readonly Dictionary<string, bool> arrayFoldouts = new();
        private static readonly Dictionary<string, bool> listFoldouts = new();

        private static readonly List<Type> VariableTypes = new();
        public const string Shared = "Shared";

        static TreeUtility()
        {
            RegisterSharedVariableTypes();
        }

        private static void RegisterSharedVariableTypes()
        {
            var baseType = typeof(SharedVariableBase);
            var assembly = baseType.Assembly;

            VariableTypes.AddRange(assembly.GetTypes()
                                           .Where(t => t.IsSubclassOf(baseType) && !t.IsAbstract && !t.IsGenericType)
                                           .ToList());
        }

        public static SharedVariableBase CreateSharedVariable(string variableName, Type variableType)
        {
            if (VariableTypes.Contains(variableType))
            {
                var instance = (SharedVariableBase)Activator.CreateInstance(variableType);
                instance.VariableName = variableName;
                instance.VariableType = variableType.Name.Replace(Shared, "");
                return instance;
            }

            return null;
        }

        public static void DrawSharedVariableValueField(SharedVariableBase variable, string valueLabel)
        {
            var valueProperty = variable.GetType().GetProperty("Value");

            if (valueProperty != null)
            {
                var value = valueProperty.GetValue(variable);
                var fieldType = valueProperty.PropertyType;
                value = DrawFieldForType(fieldType, value, valueLabel);
                valueProperty.SetValue(variable, value);
            }
        }

        private static object DrawFieldForType(Type fieldType, object value, string label)
        {
            if (fieldType == typeof(bool))
            {
                return EditorGUILayout.Toggle(label, (bool)value);
            }

            if (fieldType == typeof(int))
            {
                return EditorGUILayout.IntField(label, (int)value);
            }

            if (fieldType == typeof(float))
            {
                return EditorGUILayout.FloatField(label, (float)value);
            }

            if (fieldType == typeof(double))
            {
                return EditorGUILayout.DoubleField(label, (double)value);
            }

            if (fieldType == typeof(long))
            {
                return EditorGUILayout.LongField(label, (long)value);
            }

            if (fieldType == typeof(string))
            {
                return EditorGUILayout.TextField(label, (string)value);
            }

            if (fieldType == typeof(Color))
            {
                return EditorGUILayout.ColorField(label, (Color)value);
            }

            if (fieldType == typeof(AnimationCurve))
            {
                return EditorGUILayout.CurveField(label, (AnimationCurve)value);
            }

            if (fieldType == typeof(Bounds))
            {
                return EditorGUILayout.BoundsField(label, (Bounds)value);
            }

            if (fieldType == typeof(Rect))
            {
                return EditorGUILayout.RectField(label, (Rect)value);
            }

            if (fieldType == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(label, (Vector2)value);
            }

            if (fieldType == typeof(Vector2Int))
            {
                return EditorGUILayout.Vector2IntField(label, (Vector2Int)value);
            }

            if (fieldType == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(label, (Vector3)value);
            }

            if (fieldType == typeof(Vector3Int))
            {
                return EditorGUILayout.Vector3IntField(label, (Vector3Int)value);
            }

            if (fieldType == typeof(Vector4))
            {
                return EditorGUILayout.Vector4Field(label, (Vector4)value);
            }

            if (fieldType == typeof(Quaternion))
            {
                return Quaternion.Euler(EditorGUILayout.Vector3Field(label, ((Quaternion)value).eulerAngles));
            }

            if (fieldType == typeof(LayerMask))
            {
                var layerIndex = LayerMaskToLayer((LayerMask)value);
                var newLayerIndex = EditorGUILayout.LayerField(label, layerIndex);
                return LayerToLayerMask(newLayerIndex);
            }

            if (fieldType.IsEnum)
            {
                return EditorGUILayout.EnumPopup(label, (Enum)value);
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                return EditorGUILayout.ObjectField(label, (UnityEngine.Object)value, fieldType, true);
            }

            if (fieldType.IsArray)
            {
                return DrawArrayField((Array)value, label, fieldType.GetElementType());
            }

            if (typeof(IList).IsAssignableFrom(fieldType))
            {
                return DrawListField((IList)value, label, fieldType.GetGenericArguments()[0]);
            }

            EditorGUILayout.LabelField($"Unsupported type: {fieldType}");
            return value;
        }

        private static Array DrawArrayField(Array array, string label, Type elementType)
        {
            EditorGUILayout.BeginHorizontal();

            if (!string.IsNullOrEmpty(label))
            {
                arrayFoldouts.TryAdd(label, false);

                arrayFoldouts[label] = EditorGUILayout.Foldout(arrayFoldouts[label], $"{label}", true);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(1);

            if (label != null && arrayFoldouts.ContainsKey(label) && arrayFoldouts[label])
            {
                EditorGUI.indentLevel++;

                array ??= Array.CreateInstance(elementType, 0);

                var newSize = Mathf.Max(0, EditorGUILayout.IntField("Size", array.Length));
                if (newSize != array.Length)
                {
                    var newArray = Array.CreateInstance(elementType, newSize);
                    Array.Copy(array, newArray, Math.Min(array.Length, newSize));
                    array = newArray;
                }

                for (var i = 0; i < array.Length; i++)
                {
                    var element = array.GetValue(i);
                    var newValue = DrawFieldForType(elementType, element, $"{label} {i}");
                    if (!EqualityComparer<object>.Default.Equals(element, newValue))
                    {
                        array.SetValue(newValue, i);
                    }
                }

                EditorGUI.indentLevel--;
            }

            return array;
        }

        private static IList DrawListField(IList list, string label, Type elementType)
        {
            EditorGUILayout.BeginHorizontal();

            if (!string.IsNullOrEmpty(label))
            {
                listFoldouts.TryAdd(label, false);

                listFoldouts[label] = EditorGUILayout.Foldout(listFoldouts[label], $"{label}", true);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(1);

            if (label != null && listFoldouts.ContainsKey(label) && listFoldouts[label])
            {
                EditorGUI.indentLevel++;

                list ??= (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

                var newSize = Mathf.Max(0, EditorGUILayout.IntField("Size", list.Count));
                if (newSize != list.Count)
                {
                    var newList = (IList)Activator.CreateInstance(list.GetType());
                    for (var i = 0; i < newSize; i++)
                    {
                        newList.Add(i < list.Count ? list[i] : null);
                    }

                    list = newList;
                }

                for (var i = 0; i < list.Count; i++)
                {
                    var element = list[i];
                    var newValue = DrawFieldForType(elementType, element, $"{label} {i}");
                    if (!EqualityComparer<object>.Default.Equals(element, newValue))
                    {
                        list[i] = newValue;
                    }
                }

                EditorGUI.indentLevel--;
            }

            return list;
        }

        // 가로선을 그리는 함수
        public static void DrawHorizontalLine(Color color, int thickness = 1)
        {
            var rect = EditorGUILayout.GetControlRect(false, thickness);
            EditorGUI.DrawRect(rect, color);
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

        public static Texture2D LoadTexture(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static int LayerMaskToLayer(LayerMask layerMask)
        {
            var mask = layerMask.value;
            for (var i = 0; i < 32; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    return i;
                }
            }

            return 0;
        }

        private static LayerMask LayerToLayerMask(int layerIndex)
        {
            return 1 << layerIndex;
        }
    }
}
#endif