using System;
using Pathfinding;
using UnityEngine;

namespace BehaviourTree.Scripts.TreeSharedData
{
    public enum SharedVariableType
    {
        Int,
        Float,
        AIPath,
        Transform,
        Collider,
        ColliderArray,
        LayerMask,
        Vector3,
        TransformArray
    }

    public interface IValueContainer
    {
        object GetValue();
        void SetValue(object value);
    }

    [Serializable]
    public class SharedVariableBase : IValueContainer
    {
        public string variableName;

        public virtual object GetValue()
        {
            return null;
        }

        public virtual void SetValue(object value)
        {
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
            _value = value switch
            {
                T typedValue => typedValue,
                null => default,
                _ => _value
            };
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
    public class SharedFloat : SharedVariable<float>
    {
        public static implicit operator SharedFloat(float value)
        {
            return new SharedFloat { Value = value };
        }
    }

    [Serializable]
    public class SharedAIPath : SharedVariable<AIPath>
    {
        public static implicit operator SharedAIPath(AIPath value)
        {
            return new SharedAIPath { Value = value };
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
    public class SharedCollider : SharedVariable<Collider>
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
    public class SharedLayerMask : SharedVariable<LayerMask>
    {
        public static implicit operator SharedLayerMask(LayerMask value)
        {
            return new SharedLayerMask { Value = value };
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
    public class SharedTransformArray : SharedVariable<Transform[]>
    {
        public static implicit operator SharedTransformArray(Transform[] value)
        {
            return new SharedTransformArray { Value = value };
        }
    }
}