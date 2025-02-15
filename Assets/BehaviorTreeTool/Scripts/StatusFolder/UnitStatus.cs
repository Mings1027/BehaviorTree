using UnityEngine;
using UnityEngine.Serialization;

namespace Tree
{
    [CreateAssetMenu]
    public class UnitStatus : ScriptableObject
    {
        public int MaxHealth => maxHealth;
        public int MaxMana => maxMana;
        public int MaxDefense => maxDefense;
        public int BaseDamage => baseDamage;

        [SerializeField] private int maxHealth;
        [SerializeField] private int maxMana;
        [SerializeField] private int maxDefense;
        [SerializeField] private int baseDamage;
    }
}