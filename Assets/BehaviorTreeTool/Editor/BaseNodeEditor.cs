using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Tree;

namespace BehaviorTreeTool.Editor
{
    [CustomEditor(typeof(BaseNode), true)]
    public class BaseNodeEditor : UnityEditor.Editor
    {
        private Vector2 _inspectorScrollPos;

        private Texture2D _downArrowTexture;
        private Texture2D _rightArrowTexture;

        private void Awake()
        {
            _downArrowTexture = TreeUtility.LoadTexture("Assets/BehaviorTreeTool/Sprites/Arrow Simple Down.png");
            _rightArrowTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BehaviorTreeTool/Sprites/Arrow Simple Right.png");
        }

        private void OnEnable()
        {
            BehaviorTreeView.IsNodeSelected = true;
        }

        #region DrawInspectorTab

        public void DrawInspectorTab()
        {
            if (!BehaviorTreeView.IsNodeSelected)
            {
                EditorGUILayout.LabelField("No node selected. Please select a node.");
                return;
            }

            var node = (BaseNode)target;
            DrawDescriptionField();
            TreeUtility.DrawHorizontalLine(Color.gray);

            _inspectorScrollPos = EditorGUILayout.BeginScrollView(_inspectorScrollPos);
            DrawSharedVariableFields(node);
            TreeUtility.DrawHorizontalLine(Color.gray);
            DrawNodeVariables(node);
            TreeUtility.DrawHorizontalLine(Color.gray);
            EditorGUILayout.EndScrollView();
        }

        private void DrawDescriptionField()
        {
            var descriptionProperty = serializedObject.FindProperty("description");
            if (descriptionProperty != null)
            {
                EditorGUILayout.PropertyField(descriptionProperty, true);
            }
        }

        private void DrawSharedVariableFields(BaseNode node)
        {
            var fields = node.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => typeof(SharedVariableBase).IsAssignableFrom(field.FieldType))
                .Select(field =>
                    new KeyValuePair<string, SharedVariableBase>(field.Name, (SharedVariableBase)field.GetValue(node)))
                .ToList();

            var boldLabelStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15 };
            EditorGUILayout.LabelField("Shared Variables", boldLabelStyle);

            if (fields.Count <= 0)
            {
                EditorGUILayout.LabelField("This node has no shared variables.",
                    new GUIStyle { fontSize = 14, normal = { textColor = Color.white } });
                return;
            }

            // Calculate the maximum width of all labels
            var maxWidth = fields.Select(kvp =>
            {
                var so = new SerializedObject(node);
                var property = so.FindProperty(kvp.Key);
                var displayName = property != null ? property.displayName : ObjectNames.NicifyVariableName(kvp.Key);
                var labelStyle = new GUIStyle(GUI.skin.label);
                return labelStyle.CalcSize(new GUIContent(displayName)).x;
            }).Max();

            for (var i = 0; i < fields.Count; i++)
            {
                DrawSharedVariableField(node, fields[i], maxWidth);
            }
        }

        private void DrawSharedVariableField(BaseNode node, KeyValuePair<string, SharedVariableBase> kvp,
            float labelWidth)
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                normal = { background = TreeUtility.MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f, 1.0f)) },
                hover = { background = TreeUtility.MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 1.0f)) },
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(4, 4, 2, 2)
            };

            EditorGUILayout.BeginVertical(style);
            EditorGUILayout.BeginHorizontal();

            var variableNames = BehaviorTreeEditor.Tree.SharedData.Variables
                .Where(v => v.GetType() == kvp.Value.GetType())
                .Select(v => v.VariableName)
                .ToList();

            variableNames.Insert(0, "(None)");

            var currentIndex = string.IsNullOrEmpty(kvp.Value.VariableName)
                ? 0
                : variableNames.IndexOf(kvp.Value.VariableName);

            // Add a foldout button to show/hide the variable value
            var foldout = EditorPrefs.GetBool($"{kvp.Key}Foldout", false);

            if (currentIndex != 0 && Application.isPlaying)
            {
                var arrowTexture = foldout ? _downArrowTexture : _rightArrowTexture;
                if (GUILayout.Button(arrowTexture, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    foldout = !foldout;
                }

                EditorPrefs.SetBool($"{kvp.Key}Foldout", foldout);
            }

            var so = new SerializedObject(node);
            var property = so.FindProperty(kvp.Key);
            var displayName = property != null ? property.displayName : ObjectNames.NicifyVariableName(kvp.Key);

            EditorGUILayout.LabelField(displayName, GUILayout.Width(labelWidth));

            var selectedIndex = EditorGUILayout.Popup(currentIndex, variableNames.ToArray(), GUILayout.Width(150));
            if (selectedIndex != currentIndex)
            {
                UpdateVariableSelection(kvp.Value, variableNames, selectedIndex);
                EditorUtility.SetDirty(node);
            }

            EditorGUILayout.EndHorizontal();

            if (foldout)
            {
                if (currentIndex != 0 && Application.isPlaying)
                {
                    EditorGUI.indentLevel++;
                    TreeUtility.DrawSharedVariableValueField(kvp.Value, "Value");
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.EndVertical();
        }

        private static void UpdateVariableSelection(SharedVariableBase variable,
            IReadOnlyList<string> variableNames, int selectedIndex)
        {
            if (selectedIndex == 0)
            {
                variable.VariableName = string.Empty;
                variable.SetValue(null);
            }
            else
            {
                var selectedVariableName = variableNames[selectedIndex];
                var variables = BehaviorTreeEditor.Tree.SharedData.Variables;
                for (var i = 0; i < variables.Count; i++)
                {
                    if (variables[i].VariableName == selectedVariableName)
                    {
                        variable.VariableName = variables[i].VariableName;
                        variable.VariableType = variables[i].VariableType;
                        break;
                    }
                }
            }
        }

        private void DrawNodeVariables(BaseNode node)
        {
            var boldLabelStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15 };
            EditorGUILayout.LabelField("Node Variables", boldLabelStyle);

            var array = node.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // Check if the OnDrawGizmos method is overridden
            var hasDrawGizmosOverride =
                node.GetType()
                    .GetMethod("OnDrawGizmos", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.DeclaringType != typeof(BaseNode);

            for (var i = 0; i < array.Length; i++)
            {
                var field = array[i];
                if (typeof(SharedVariableBase).IsAssignableFrom(field.FieldType) ||
                    field.IsDefined(typeof(HideInInspector), false) ||
                    field.Name == "drawGizmos" && !hasDrawGizmosOverride)
                {
                    continue;
                }

                var property = serializedObject.FindProperty(field.Name);
                if (property != null)
                {
                    var displayName = ObjectNames.NicifyVariableName(field.Name);
                    EditorGUILayout.PropertyField(property, new GUIContent(displayName), true);
                }
            }
        }

        #endregion
    }
}