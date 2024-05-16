using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using BehaviourTree.Scripts.Runtime;
using BehaviourTree.Scripts.TreeSharedData;

namespace BehaviourTree.Editor
{
    [CustomEditor(typeof(Node), true)]
    public class NodeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Node node = (Node)target;

            DrawDescriptionField();
            DrawSharedDataField(node);
            DrawSharedVariableFields(node);
            DrawNonSharedVariableFields(node);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDescriptionField()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            SerializedProperty descriptionProperty = serializedObject.FindProperty("description");
            if (descriptionProperty != null)
            {
                EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("Description"), true);
            }

            // DrawSeparator();
        }

        private void DrawSharedDataField(Node node)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shared Data", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            SerializedProperty sharedDataProperty = serializedObject.FindProperty("sharedData");
            EditorGUILayout.PropertyField(sharedDataProperty, new GUIContent("Shared Data"));

            if (sharedDataProperty.objectReferenceValue != null)
            {
                node.SetSharedData((SharedData)sharedDataProperty.objectReferenceValue);
            }

            DrawSeparator();
        }

        private void DrawSharedVariableFields(Node node)
        {
            if (node.sharedData == null)
            {
                EditorGUILayout.HelpBox("Please assign a SharedData object.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shared Variables", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            foreach (var kvp in node.GetType()
                         .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         .Where(field => typeof(SharedVariableBase).IsAssignableFrom(field.FieldType))
                         .Select(field =>
                             new KeyValuePair<string, SharedVariableBase>(field.Name,
                                 (SharedVariableBase)field.GetValue(node))))
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(kvp.Key, GUILayout.MinWidth(100));

                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField(string.IsNullOrEmpty(kvp.Value.variableName) ? "(None)" : kvp.Value.variableName, GUILayout.MinWidth(100));
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("\u25cf", GUILayout.Width(20)))
                {
                    ShowAssignMenu(node, kvp.Value);
                }

                EditorGUILayout.EndHorizontal();

                // var property = serializedObject.FindProperty(kvp.Key);
                // if (property != null)
                // {
                //     EditorGUILayout.PropertyField(property, GUIContent.none, true);
                // }
                // else
                // {
                //     DrawFieldValue(kvp.Value);
                // }

                EditorGUILayout.EndVertical();
            }

            DrawSeparator();
        }

        private void DrawNonSharedVariableFields(Node node)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Non-Shared Variables", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            foreach (var field in node.GetType()
                         .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (typeof(SharedVariableBase).IsAssignableFrom(field.FieldType) ||
                    field.IsDefined(typeof(HideInInspector), false))
                {
                    continue;
                }

                var property = serializedObject.FindProperty(field.Name);
                if (property != null)
                {
                    EditorGUILayout.PropertyField(property, new GUIContent(field.Name), true);
                }
                else if (field.IsPublic || field.IsDefined(typeof(SerializeField), false))
                {
                    DrawField(field, node);
                }
            }
        }

        private static void DrawField(FieldInfo field, Node node)
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

        private void ShowAssignMenu(Node node, SharedVariableBase variable)
        {
            var menu = new GenericMenu();

            // Add "None" option as the first item
            menu.AddItem(new GUIContent("None"), false, () =>
            {
                variable.variableName = string.Empty; // Set variableName to an empty string
                if (variable is IValueContainer valueContainer)
                {
                    valueContainer.SetValue(null); // Set value to null
                }

                EditorUtility.SetDirty(node);
            });

            foreach (var sharedVariable in node.sharedData.Variables)
            {
                if (sharedVariable.GetType() == variable.GetType())
                {
                    menu.AddItem(new GUIContent(sharedVariable.variableName), false, () =>
                    {
                        variable.variableName = sharedVariable.variableName; // Update variableName
                        if (variable is IValueContainer valueContainer &&
                            sharedVariable is IValueContainer sharedValueContainer)
                        {
                            valueContainer.SetValue(sharedValueContainer.GetValue()); // Update value
                        }

                        EditorUtility.SetDirty(node);
                    });
                }
            }

            menu.ShowAsContext();
        }

        private void DrawSeparator()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.6f, 0.6f, 0.6f, 1));
            EditorGUILayout.Space();
        }
    }
}