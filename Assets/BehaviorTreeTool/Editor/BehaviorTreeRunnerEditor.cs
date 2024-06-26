using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using BehaviorTreeTool.Scripts.TreeUtil;
using BehaviorTreeTool.Editor;

[CustomEditor(typeof(BehaviorTreeRunner))]
public class BehaviorTreeRunnerEditor : Editor
{
    private SerializedProperty _enableVariablesProperty;
    private SerializedProperty _behaviorTreeProperty;
    private SerializedProperty _variablesProperty;

    private readonly Dictionary<string, bool> _foldoutStates = new();

    private void OnEnable()
    {
        _enableVariablesProperty = serializedObject.FindProperty("enableVariables");
        _behaviorTreeProperty = serializedObject.FindProperty("behaviorTree");
        _variablesProperty = serializedObject.FindProperty("variables");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var treeRunner = (BehaviorTreeRunner)target;

        DrawBehaviorTreeField(treeRunner);
        DrawEnableVariablesField(treeRunner);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawBehaviorTreeField(BehaviorTreeRunner treeRunner)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(_behaviorTreeProperty, new GUIContent("Behavior Tree"));
        if (GUILayout.Button("Open", GUILayout.Width(50)))
        {
            // Open behavior tree logic
            BehaviorTreeEditor.OpenWithTree(treeRunner.Tree);
        }
        EditorGUILayout.EndHorizontal();

        var currentBehaviorTree = (BehaviorTree)_behaviorTreeProperty.objectReferenceValue;
        if (currentBehaviorTree != treeRunner.Tree)
        {
            treeRunner.Tree = currentBehaviorTree;
            serializedObject.ApplyModifiedProperties();  // 변경 사항을 즉시 반영합니다.
            treeRunner.UpdateVariables();
        }
    }

    private void DrawEnableVariablesField(BehaviorTreeRunner treeRunner)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_enableVariablesProperty);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            treeRunner.EnableVariables = _enableVariablesProperty.boolValue;
        }

        if (treeRunner.EnableVariables)
        {
            DrawVariables(treeRunner);
        }
    }

    private void DrawVariables(BehaviorTreeRunner treeRunner)
    {
        var treeKey = treeRunner.GetInstanceID().ToString();
        _foldoutStates.TryAdd(treeKey, true);

        var isFolded = EditorGUILayout.Foldout(_foldoutStates[treeKey], "Variables", true);
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
        var variableName = variableProperty.FindPropertyRelative("variableName").stringValue;
        var variableType = (SharedVariableType)variableProperty.FindPropertyRelative("variableType").enumValueIndex;
        var valueProperty = variableProperty.FindPropertyRelative("value");
        var propertyPath = variableProperty.propertyPath;

        if (TreeUtility.IsCollectionVariable(variableType))
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