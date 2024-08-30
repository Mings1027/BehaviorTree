namespace Tree
{
    public enum SharedVariableType
    {
        //The last number is 24
        AIPath = 0,
        Animator = 1,
        Bool = 2,
        Collider = 3,
        ColliderArray = 4,
        ColliderList = 22,
        Color = 5,
        Float = 6,
        GameObject = 7,
        GameObjectArray = 23,
        GameObjectList = 8,
        Int = 9,
        LayerMask = 10,
        Material = 11,
        NavMeshAgent = 12,
        Quaternion = 13,
        Rect = 14,
        String = 15,
        Transform = 16,
        TransformArray = 17,
        TransformList = 24,
        Vector2 = 18,
        Vector2Int = 19,
        Vector3 = 20,
        Vector3Int = 21
    }

    public enum TaskState
    {
        Running,
        Failure,
        Success
    }
}