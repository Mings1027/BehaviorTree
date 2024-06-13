using System;
using UnityEngine;

[CreateAssetMenu]
public class Test : ScriptableObject
{
    [SerializeField] private TestInt testInt;
}

[Serializable]
public class TestGeneric<T>
{
    public T Value
    {
        get => value;
        set => this.value = value;
    }

    [SerializeField] private T value;
}

[Serializable]
public class TestInt : TestGeneric<int>
{
    public static implicit operator TestInt(int value)
    {
        return new TestInt { Value = value };
    }
}