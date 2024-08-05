// using UnityEditor;
// using UnityEngine;
//
// public class UndoTracker : EditorWindow
// {
//     [MenuItem("Undo Tracker/Undo Tracker Window")]
//     public static void ShowWindow()
//     {
//         GetWindow<UndoTracker>("Undo Tracker");
//     }
//
//     private void OnEnable()
//     {
//         Undo.undoRedoPerformed += OnUndoRedoPerformed;
//     }
//
//     private void OnDisable()
//     {
//         Undo.undoRedoPerformed -= OnUndoRedoPerformed;
//     }
//
//     private void OnUndoRedoPerformed()
//     {
//         Debug.Log("Undo/Redo performed");
//         // 여기서 더 많은 정보를 로그에 남길 수 있습니다.
//     }
//
//     private void OnGUI()
//     {
//         if (GUILayout.Button("Perform Test Action"))
//         {
//             PerformTestAction();
//         }
//     }
//
//     private void PerformTestAction()
//     {
//         Undo.RecordObject(this, "Test Action");
//         // 이 부분에 원하는 액션을 수행합니다.
//         Debug.Log("Test action performed");
//     }
// }