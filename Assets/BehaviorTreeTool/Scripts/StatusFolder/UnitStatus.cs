using UnityEngine;

[CreateAssetMenu]
public class UnitStatus : ScriptableObject
{
    public int Health
    {
        get => health;
        set => health = value;
    }

    [SerializeField] private int health;
}
