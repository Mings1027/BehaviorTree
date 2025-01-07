using System.Collections.Generic;
using Tree;
using UnityEngine;

public class GridManager : MonoSingleton<GridManager>
{
    private static Dictionary<Vector2Int, List<Transform>> grid = new();
    private static float cellSize = 10f;
    private const int MAX_NEIGHBORS = 9;

    protected override void Awake()
    {
        base.Awake();
        grid.Clear();
        SetCellSize(cellSize);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 그리드 시각화
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        foreach (var kvp in grid)
        {
            if (kvp.Value.Count > 0)
            {
                Vector3 cellCenter = new Vector3(
                    kvp.Key.x * cellSize + cellSize / 2,
                    0,
                    kvp.Key.y * cellSize + cellSize / 2
                );
                Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, 1, cellSize));

                // 오브젝트 수 표시
                UnityEditor.Handles.Label(cellCenter, kvp.Value.Count.ToString());
            }
        }
    }
#endif
    private static Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / cellSize),
            Mathf.FloorToInt(worldPos.z / cellSize)
        );
    }

    public static void UpdateObjectPosition(Transform obj, Vector3 oldPos, Vector3 newPos)
    {
        var oldCell = WorldToGrid(oldPos);
        var newCell = WorldToGrid(newPos);

        if (oldCell != newCell)
        {
            if (grid.ContainsKey(oldCell))
            {
                grid[oldCell].Remove(obj);
            }

            if (!grid.ContainsKey(newCell))
            {
                grid[newCell] = new List<Transform>();
            }

            grid[newCell].Add(obj);
        }
    }

    public static Transform FindNearestTarget(Vector3 position, LayerMask targetLayer)
    {
        var currentCell = WorldToGrid(position);
        var nearestTarget = default(Transform);
        var minDistance = float.MaxValue;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                var checkCell = new Vector2Int(currentCell.x + dx, currentCell.y + dz);
                if (!grid.ContainsKey(checkCell)) continue;

                foreach (var obj in grid[checkCell])
                {
                    if (!obj || ((1 << obj.gameObject.layer) & targetLayer) == 0) continue;

                    var distance = Vector3.SqrMagnitude(position - obj.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestTarget = obj;
                    }
                }
            }
        }

        return nearestTarget;
    }

    public static void RemoveObject(Transform obj, Vector3 position)
    {
        var cell = WorldToGrid(position);
        if (grid.ContainsKey(cell))
        {
            grid[cell].Remove(obj);
            if (grid[cell].Count == 0)
                grid.Remove(cell);
        }
    }

    public static void SetCellSize(float size)
    {
        cellSize = size;
    }

    public static void Clear()
    {
        grid.Clear();
    }
}