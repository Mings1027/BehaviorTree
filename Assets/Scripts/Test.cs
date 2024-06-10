using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour
{
    public List<Transform> TransformList => transformList;
    [SerializeField] private List<Transform> transformList;
}

[CustomEditor(typeof(Test))]
public class TestEditor: Editor
{
    public override void OnInspectorGUI()
    {
        var test = (Test)target;
        EditorGUILayout.LabelField("Transform List", EditorStyles.boldLabel);

        if (test.TransformList != null)
        {
            for (int i = 0; i < test.TransformList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                // Transform 객체를 ObjectField로 표시
                test.TransformList[i] = (Transform)EditorGUILayout.ObjectField(test.TransformList[i], typeof(Transform), true);

                // 항목 제거 버튼
                if (GUILayout.Button("Remove"))
                {
                    test.TransformList.RemoveAt(i);
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }

        // 새로운 Transform 객체 추가 버튼
        if (GUILayout.Button("Add Transform"))
        {
            test.TransformList.Add(null);
        }

        // 변경 사항 저장
        if (GUI.changed)
        {
            EditorUtility.SetDirty(test);
        }       
    }
}