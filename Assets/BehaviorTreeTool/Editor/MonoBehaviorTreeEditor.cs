using UnityEditor;
using UnityEngine;
using BehaviorTreeTool.Scripts.TreeUtil;
using UnityEngine.AI;
using System.Collections.Generic;

[CustomEditor(typeof(MonoBehaviorTree))]
public class MonoBehaviorTreeEditor : Editor
{
    private SerializedProperty externalTreeProperty;
    private SerializedProperty variablesProperty;
    private Texture2D _removeTexture;

    // 폴드 상태를 저장하는 딕셔너리
    private Dictionary<int, bool> foldoutStates = new();

    private void OnEnable()
    {
        externalTreeProperty = serializedObject.FindProperty("behaviorTree");
        variablesProperty = serializedObject.FindProperty("variables");
        _removeTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BehaviorTreeTool/Sprites/Remove.png");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(externalTreeProperty);

        var tree = (MonoBehaviorTree)target;

        // externalTreeProperty가 변경되었는지 감지
        if (externalTreeProperty.objectReferenceValue != tree.ExternalBehaviorTree)
        {
            tree.ExternalBehaviorTree = (ExternalBehaviorTree)externalTreeProperty.objectReferenceValue;
            serializedObject.Update();
        }

        // 폴드 상태가 저장되지 않은 경우 초기화
        if (!foldoutStates.ContainsKey(tree.GetInstanceID()))
        {
            foldoutStates[tree.GetInstanceID()] = true;
        }

        DrawVariables(tree, foldoutStates[tree.GetInstanceID()]);
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawVariables(MonoBehaviorTree tree, bool isFolded)
    {
        isFolded = EditorGUILayout.Foldout(isFolded, "Variables", true);

        // 폴드 상태 저장
        foldoutStates[tree.GetInstanceID()] = isFolded;

        if (isFolded)
        {
            if (variablesProperty != null)
            {
                for (int i = 0; i < variablesProperty.arraySize; i++)
                {
                    var variableProperty = variablesProperty.GetArrayElementAtIndex(i);
                    DrawSharedVariableField(variableProperty, i);
                }
            }
        }
    }

    private void DrawSharedVariableField(SerializedProperty variableProperty, int index)
    {
        var variableType = (SharedVariableType)variableProperty.FindPropertyRelative("variableType").enumValueIndex;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(variableProperty.FindPropertyRelative("variableName").stringValue,
            GUILayout.Width(110));

        SerializedProperty valueProperty = variableProperty.FindPropertyRelative("_value");
        DrawSharedVariableValue(variableType, valueProperty);

        if (GUILayout.Button(new GUIContent(_removeTexture), GUILayout.Width(20), GUILayout.Height(20)))
        {
            RemoveVariable(index);
        }
        EditorGUILayout.EndHorizontal();
        TreeUtility.DrawHorizontalLine(Color.gray);
    }

    private void DrawSharedVariableValue(SharedVariableType variableType, SerializedProperty valueProperty)
    {
        switch (variableType)
        {
            case SharedVariableType.AIPath:
                EditorGUILayout.LabelField("AIPath type not supported");
                break;
            case SharedVariableType.Bool:
                valueProperty.boolValue = EditorGUILayout.Toggle(valueProperty.boolValue);
                break;
            case SharedVariableType.Collider:
                valueProperty.objectReferenceValue =
                    EditorGUILayout.ObjectField(valueProperty.objectReferenceValue, typeof(Collider), true);
                break;
            case SharedVariableType.ColliderArray:
                EditorGUILayout.LabelField("ColliderArray type not supported");
                break;
            case SharedVariableType.Color:
                valueProperty.colorValue = EditorGUILayout.ColorField(valueProperty.colorValue);
                break;
            case SharedVariableType.Float:
                valueProperty.floatValue = EditorGUILayout.FloatField(valueProperty.floatValue);
                break;
            case SharedVariableType.GameObject:
                valueProperty.objectReferenceValue =
                    EditorGUILayout.ObjectField(valueProperty.objectReferenceValue, typeof(GameObject), true);
                break;
            case SharedVariableType.GameObjectList:
                EditorGUILayout.LabelField("GameObjectList type not supported");
                break;
            case SharedVariableType.Int:
                valueProperty.intValue = EditorGUILayout.IntField(valueProperty.intValue);
                break;
            case SharedVariableType.LayerMask:
                valueProperty.intValue = EditorGUILayout.LayerField(valueProperty.intValue);
                break;
            case SharedVariableType.Material:
                valueProperty.objectReferenceValue =
                    EditorGUILayout.ObjectField(valueProperty.objectReferenceValue, typeof(Material), true);
                break;
            case SharedVariableType.NavMeshAgent:
                valueProperty.objectReferenceValue =
                    EditorGUILayout.ObjectField(valueProperty.objectReferenceValue, typeof(NavMeshAgent), true);
                break;
            case SharedVariableType.Quaternion:
                var eulerValue = valueProperty.quaternionValue.eulerAngles;
                eulerValue = EditorGUILayout.Vector3Field("", eulerValue);
                valueProperty.quaternionValue = Quaternion.Euler(eulerValue);
                break;
            case SharedVariableType.Rect:
                valueProperty.rectValue = EditorGUILayout.RectField(valueProperty.rectValue);
                break;
            case SharedVariableType.String:
                valueProperty.stringValue = EditorGUILayout.TextField(valueProperty.stringValue);
                break;
            case SharedVariableType.Transform:
                valueProperty.objectReferenceValue =
                    EditorGUILayout.ObjectField(valueProperty.objectReferenceValue, typeof(Transform), true);
                break;
            case SharedVariableType.TransformArray:
                EditorGUILayout.LabelField("TransformArray type not supported");
                break;
            case SharedVariableType.Vector2:
                valueProperty.vector2Value = EditorGUILayout.Vector2Field("", valueProperty.vector2Value);
                break;
            case SharedVariableType.Vector2Int:
                valueProperty.vector2IntValue = EditorGUILayout.Vector2IntField("", valueProperty.vector2IntValue);
                break;
            case SharedVariableType.Vector3:
                valueProperty.vector3Value = EditorGUILayout.Vector3Field("", valueProperty.vector3Value);
                break;
            case SharedVariableType.Vector3Int:
                valueProperty.vector3IntValue = EditorGUILayout.Vector3IntField("", valueProperty.vector3IntValue);
                break;
            default:
                EditorGUILayout.LabelField("Unsupported SharedVariable type");
                break;
        }
    }

    private void RemoveVariable(int index)
    {
        var variableName = variablesProperty.GetArrayElementAtIndex(index).FindPropertyRelative("variableName")
            .stringValue;
        if (EditorUtility.DisplayDialog("Remove Variable",
                $"Are you sure you want to remove the variable '{variableName}'?", "Yes", "No"))
        {
            variablesProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}
