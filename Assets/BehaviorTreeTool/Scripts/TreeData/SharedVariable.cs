using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public enum SharedVariableType
{
    AIPath,
    Bool,
    Collider,
    ColliderArray,
    Color,
    Float,
    GameObject,
    GameObjectList,
    Int,
    LayerMask,
    Material,
    NavMeshAgent,
    Quaternion,
    Rect,
    String,
    Transform,
    TransformArray,
    Vector2,
    Vector2Int,
    Vector3,
    Vector3Int
}

public interface IComponentObject
{
    bool UseGetComponent { get; set; }
}

public interface IBehaviorTree
{
    BehaviorTree Tree { get; }
    void TreeUpdate();
}

[Serializable]
public class SharedVariableBase
{
    [SerializeField] private string variableName;
    [SerializeField] private SharedVariableType variableType;

    public string VariableName
    {
        get => variableName;
        set => variableName = value;
    }

    public SharedVariableType VariableType
    {
        get => variableType;
        set => variableType = value;
    }

    public virtual object GetValue()
    {
        return null;
    }

    public virtual void SetValue(object value) { }

    public virtual SharedVariableBase Clone()
    {
        var clone = (SharedVariableBase)MemberwiseClone();
        return clone;
    }
}

[Serializable]
public class SharedVariable<T> : SharedVariableBase
{
    [SerializeField] private T _value;

    public T Value
    {
        get => _value;
        set => _value = value;
    }

    public override object GetValue()
    {
        return _value;
    }

    public override void SetValue(object value)
    {
        Value = value switch
        {
            T typedValue => typedValue,
            null => default,
            _ => _value
        };
    }

    public override SharedVariableBase Clone()
    {
        var clone = (SharedVariable<T>)base.Clone();
        clone._value = _value;
        return clone;
    }
}

public class SharedVariableComponentObject<T> : SharedVariable<T>, IComponentObject where T : Object
{
    [SerializeField] private bool useGetComponent;

    public bool UseGetComponent
    {
        get => useGetComponent;
        set => useGetComponent = value;
    }
}

[Serializable]
public class SharedBool : SharedVariable<bool>
{
    public static implicit operator SharedBool(bool value)
    {
        return new SharedBool { Value = value };
    }
}

[Serializable]
public class SharedCollider : SharedVariableComponentObject<Collider>
{
    public static implicit operator SharedCollider(Collider value)
    {
        return new SharedCollider { Value = value };
    }
}

[Serializable]
public class SharedColliderArray : SharedVariable<Collider[]>
{
    public static implicit operator SharedColliderArray(Collider[] value)
    {
        return new SharedColliderArray { Value = value };
    }
}

[Serializable]
public class SharedColor : SharedVariable<Color>
{
    public static implicit operator SharedColor(Color value)
    {
        return new SharedColor { Value = value };
    }
}

[Serializable]
public class SharedFloat : SharedVariable<float>
{
    public static implicit operator SharedFloat(float value)
    {
        return new SharedFloat { Value = value };
    }
}

[Serializable]
public class SharedGameComponentObject : SharedVariableComponentObject<GameObject>
{
    public static implicit operator SharedGameComponentObject(GameObject value)
    {
        return new SharedGameComponentObject { Value = value };
    }
}

[Serializable]
public class SharedGameObjectList : SharedVariable<List<GameObject>>
{
    public static implicit operator SharedGameObjectList(List<GameObject> value)
    {
        return new SharedGameObjectList { Value = value };
    }
}

[Serializable]
public class SharedInt : SharedVariable<int>
{
    public static implicit operator SharedInt(int value)
    {
        return new SharedInt { Value = value };
    }
}

[Serializable]
public class SharedLayerMask : SharedVariable<LayerMask>
{
    public static implicit operator SharedLayerMask(LayerMask value)
    {
        return new SharedLayerMask { Value = value };
    }
}

[Serializable]
public class SharedMaterial : SharedVariable<Material>
{
    public static implicit operator SharedMaterial(Material value)
    {
        return new SharedMaterial { Value = value };
    }
}

[Serializable]
public class SharedNavMeshAgent : SharedVariableComponentObject<NavMeshAgent>
{
    public static implicit operator SharedNavMeshAgent(NavMeshAgent value)
    {
        return new SharedNavMeshAgent { Value = value };
    }
}

[Serializable]
public class SharedQuaternion : SharedVariable<Quaternion>
{
    public static implicit operator SharedQuaternion(Quaternion value)
    {
        return new SharedQuaternion { Value = value };
    }
}

[Serializable]
public class SharedRect : SharedVariable<Rect>
{
    public static implicit operator SharedRect(Rect value)
    {
        return new SharedRect { Value = value };
    }
}

[Serializable]
public class SharedString : SharedVariable<string>
{
    public static implicit operator SharedString(string value)
    {
        return new SharedString { Value = value };
    }
}

[Serializable]
public class SharedTransform : SharedVariable<Transform>
{
    public static implicit operator SharedTransform(Transform value)
    {
        return new SharedTransform { Value = value };
    }
}

[Serializable]
public class SharedTransformArray : SharedVariable<Transform[]>
{
    public static implicit operator SharedTransformArray(Transform[] value)
    {
        return new SharedTransformArray { Value = value };
    }
}

[Serializable]
public class SharedVector2 : SharedVariable<Vector2>
{
    public static implicit operator SharedVector2(Vector2 value)
    {
        return new SharedVector2 { Value = value };
    }
}

[Serializable]
public class SharedVector2Int : SharedVariable<Vector2Int>
{
    public static implicit operator SharedVector2Int(Vector2Int value)
    {
        return new SharedVector2Int { Value = value };
    }
}

[Serializable]
public class SharedVector3 : SharedVariable<Vector3>
{
    public static implicit operator SharedVector3(Vector3 value)
    {
        return new SharedVector3 { Value = value };
    }
}

[Serializable]
public class SharedVector3Int : SharedVariable<Vector3Int>
{
    public static implicit operator SharedVector3Int(Vector3Int value)
    {
        return new SharedVector3Int { Value = value };
    }
}

[Serializable]
public class SharedCustomClass : SharedVariable<CustomClass>
{
    public static implicit operator SharedCustomClass(CustomClass value)
    {
        return new SharedCustomClass { Value = value };
    }
}

[Serializable]
public class CustomClass
{
    public string name;
    public Transform myPos;
    public int height;
}