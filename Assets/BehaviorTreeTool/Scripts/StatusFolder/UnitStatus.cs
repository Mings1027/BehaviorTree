using UnityEngine;

namespace Tree
{
    [CreateAssetMenu]
    public class UnitStatus : ScriptableObject
    {
        public int Health => health;

        [SerializeField] private int health;
        
        public Color damageColor;
    }
}