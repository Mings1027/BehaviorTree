using UnityEditor;

namespace BehaviorTreeTool.Editor
{
    public class BehaviorTreeAssetModificationProcessor : AssetModificationProcessor
    {
        // 자산 삭제 전에 호출되는 메서드
        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            // 자산을 가져옴
            var asset = AssetDatabase.LoadAssetAtPath<BehaviorTree>(assetPath);

            // 자산이 BehaviorTree인지 확인
            if (asset != null)
            {
                // 삭제 확인 대화 상자를 표시
                if (EditorUtility.DisplayDialog("Delete Behavior Tree",
                        $"Are you sure you want to delete the behavior tree '{asset.name}'?", "Yes", "No"))
                {
                    // 사용자가 삭제를 취소한 경우 삭제를 취소
                    return AssetDeleteResult.DidNotDelete;
                }
            }

            // 자산을 삭제할 수 있도록 허용
            return AssetDeleteResult.DidDelete;
        }
    }
}

