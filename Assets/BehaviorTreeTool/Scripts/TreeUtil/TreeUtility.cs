using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BehaviorTreeTool.Scripts.TreeUtil
{
    public static class TreeUtility
    {
        private static readonly Dictionary<string, bool> ArrayFoldouts = new();
        private static readonly Dictionary<string, bool> ListFoldouts = new();

        private static readonly Dictionary<SharedVariableType, Type> VariableTypeMap = new();
        private static readonly Dictionary<Type, SharedVariableType> TypeToEnumMap = new();

        static TreeUtility()
        {
            RegisterSharedVariableTypes();
        }

        private static void RegisterSharedVariableTypes()
        {
            var baseType = typeof(SharedVariableBase);
            var assembly = baseType.Assembly;

            var sharedVariableTypes = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(baseType) && !t.IsAbstract)
                .ToList();

            foreach (var type in sharedVariableTypes)
            {
                var enumName = type.Name.Replace("Shared", "");
                if (Enum.TryParse(enumName, out SharedVariableType variableType))
                {
                    VariableTypeMap[variableType] = type;
                    TypeToEnumMap[type] = variableType;
                }
            }
        }

        public static SharedVariableBase CreateSharedVariable(string variableName, SharedVariableType variableType)
        {
            if (VariableTypeMap.TryGetValue(variableType, out var type))
            {
                var instance = (SharedVariableBase)Activator.CreateInstance(type);
                instance.VariableName = variableName;
#if UNITY_EDITOR
                instance.VariableType = variableType;
#endif
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
            else if (fieldType == typeof(int))
            {
                return EditorGUILayout.IntField(label, (int)value);
            }
            else if (fieldType == typeof(float))
            {
                return EditorGUILayout.FloatField(label, (float)value);
            }
            else if (fieldType == typeof(double))
            {
                return EditorGUILayout.DoubleField(label, (double)value);
            }
            else if (fieldType == typeof(string))
            {
                return EditorGUILayout.TextField(label, (string)value);
            }
            else if (fieldType == typeof(Color))
            {
                return EditorGUILayout.ColorField(label, (Color)value);
            }
            else if (fieldType == typeof(AnimationCurve))
            {
                return EditorGUILayout.CurveField(label, (AnimationCurve)value);
            }
            else if (fieldType == typeof(Bounds))
            {
                return EditorGUILayout.BoundsField(label, (Bounds)value);
            }
            else if (fieldType == typeof(Rect))
            {
                return EditorGUILayout.RectField(label, (Rect)value);
            }
            else if (fieldType == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(label, (Vector2)value);
            }
            else if (fieldType == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(label, (Vector3)value);
            }
            else if (fieldType == typeof(Vector4))
            {
                return EditorGUILayout.Vector4Field(label, (Vector4)value);
            }
            else if (fieldType == typeof(Quaternion))
            {
                return Quaternion.Euler(EditorGUILayout.Vector3Field(label, ((Quaternion)value).eulerAngles));
            }
            else if (fieldType.IsEnum)
            {
                return EditorGUILayout.EnumPopup(label, (Enum)value);
            }
            else if (fieldType == typeof(LayerMask))
            {
                return EditorGUILayout.LayerField(label, (LayerMask)value);
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                return EditorGUILayout.ObjectField(label, (UnityEngine.Object)value, fieldType, true);
            }
            else if (fieldType.IsArray)
            {
                return DrawArrayField((Array)value, label, fieldType.GetElementType());
            }
            else if (typeof(IList).IsAssignableFrom(fieldType))
            {
                return DrawListField((IList)value, label, fieldType.GetGenericArguments()[0]);
            }

            EditorGUILayout.LabelField($"Unsupported type: {fieldType}");
            return value;
        }

        public static Array DrawArrayField(Array array, string label, Type elementType)
        {
            EditorGUILayout.BeginHorizontal();

            if (!string.IsNullOrEmpty(label))
            {
                ArrayFoldouts.TryAdd(label, false);

                ArrayFoldouts[label] = EditorGUILayout.Foldout(ArrayFoldouts[label], $"{label}", true);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(1);

            if (ArrayFoldouts.ContainsKey(label) && ArrayFoldouts[label])
            {
                EditorGUI.indentLevel++;

                // Initialize array if it's null
                if (array == null)
                {
                    array = Array.CreateInstance(elementType, 0);
                }

                int newSize = Mathf.Max(0, EditorGUILayout.IntField("Size", array.Length));
                if (newSize != array.Length)
                {
                    Array newArray = Array.CreateInstance(elementType, newSize);
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

        public static IList DrawListField(IList list, string label, Type elementType)
        {
            if (!string.IsNullOrEmpty(label))
            {
                ListFoldouts.TryAdd(label, false);

                EditorGUILayout.BeginHorizontal();

                ListFoldouts[label] = EditorGUILayout.Foldout(ListFoldouts[label], $"{label}", true);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(1);

            if (ListFoldouts.ContainsKey(label) && ListFoldouts[label])
            {
                EditorGUI.indentLevel++;

                if (list == null)
                {
                    list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                }

                int newSize = Mathf.Max(0, EditorGUILayout.IntField("Size", list.Count));
                if (newSize != list.Count)
                {
                    var newList = (IList)Activator.CreateInstance(list.GetType());
                    for (int i = 0; i < newSize; i++)
                    {
                        if (i < list.Count)
                        {
                            newList.Add(list[i]);
                        }
                        else
                        {
                            newList.Add(Activator.CreateInstance(elementType));
                        }
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
    }
}
