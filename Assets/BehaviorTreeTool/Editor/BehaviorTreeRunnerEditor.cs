using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using BehaviorTreeTool.Scripts.TreeUtil;
using BehaviorTreeTool.Editor;

[CustomEditor(typeof(BehaviorTreeRunner))]
public class BehaviorTreeRunnerEditor : Editor
{
    private SerializedProperty _initModeProperty;
    private SerializedProperty _behaviorTreeProperty;
    private SerializedProperty _variablesProperty;

    private readonly Dictionary<string, bool> _foldoutStates = new();

    private void OnEnable()
    {
        _initModeProperty = serializedObject.FindProperty("initMode");
        _behaviorTreeProperty = serializedObject.FindProperty("behaviorTree");
        _variablesProperty = serializedObject.FindProperty("variables");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var treeRunner = (BehaviorTreeRunner)target;
        DrawInitMode(treeRunner);
        if (treeRunner.InitMode == InitMode.Preload)
        {
            DrawExternalBehaviorTreeHelpBox();
        }

        DrawBehaviorTreeField(treeRunner);
        CheckChangeTree(treeRunner);
        serializedObject.Update();
        DrawVariables(treeRunner);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawInitMode(BehaviorTreeRunner treeRunner)
    {
        EditorGUILayout.PropertyField(_initModeProperty, new GUIContent("Init Mode"));
        if (treeRunner.InitMode != (InitMode)_initModeProperty.enumValueIndex)
        {
            treeRunner.InitMode = (InitMode)_initModeProperty.enumValueIndex;
            EditorUtility.SetDirty(target);
        }
    }

    private void DrawExternalBehaviorTreeHelpBox()
    {
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;
        var wordWrapStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            richText = true,
            wordWrap = true,
        };
        var message = "Use <b><color=#FFA500>Preload</color></b> if any element in"
            + " the <b><color=#FFA500>Variables</color></b> is a <b><color=#FFA500>Reference</color></b> type"
            + " that needs to be assigned before play.";
        EditorGUILayout.LabelField(message, wordWrapStyle);

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private void DrawBehaviorTreeField(BehaviorTreeRunner treeRunner)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(_behaviorTreeProperty);
        if (GUILayout.Button("Open", GUILayout.Width(50)))
        {
            // Open behavior tree logic
            BehaviorTreeEditor.OpenWithTree(treeRunner.Tree);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void CheckChangeTree(BehaviorTreeRunner treeRunner)
    {
        var currentBehaviorTree = (BehaviorTree)_behaviorTreeProperty.objectReferenceValue;
        if (currentBehaviorTree != treeRunner.Tree)
        {
            treeRunner.Tree = currentBehaviorTree;
        }
    }

    private void DrawVariables(BehaviorTreeRunner treeRunner)
    {
        var treeKey = treeRunner.GetInstanceID().ToString();
        _foldoutStates.TryAdd(treeKey, true);

        bool isFolded = EditorGUILayout.Foldout(_foldoutStates[treeKey], "Variables", true);
        _foldoutStates[treeKey] = isFolded;

        if (isFolded)
        {
            if (_variablesProperty != null)
            {
                if (_variablesProperty.arraySize <= 0)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("There are no variables");
                    EditorGUI.indentLevel--;
                    return;
                }

                for (var i = 0; i < _variablesProperty.arraySize; i++)
                {
                    var variableProperty = _variablesProperty.GetArrayElementAtIndex(i);
                    DrawSharedVariableField(variableProperty);
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
