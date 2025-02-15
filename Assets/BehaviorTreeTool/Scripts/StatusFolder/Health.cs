using UnityEngine;

namespace Tree
{
    [RequireComponent(typeof(DamageComponent))]
    public class Health : MonoBehaviour, IDamageable
    {
        private Collider _col;
        private int _curHealth;
        private int _curMana;
        private int _curDefense;

        [SerializeField] private UnitStatus unitStatus;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            GetComponent<DamageComponent>().SetUnitStatus(unitStatus);
        }

        private void OnEnable()
        {
            _col.enabled = true;
            _curHealth = unitStatus.MaxHealth;
        }

        public void Damage(int amount)
        {
            _curHealth -= amount;
            if (_curHealth <= 0)
            {
                _col.enabled = false;
                gameObject.SetActive(false);
            }
        }

        public void RecoveryHealth(int percent)
        {
            _curHealth += _curHealth * percent / 100;
            if (_curHealth > unitStatus.MaxHealth)
            {
                _curHealth = unitStatus.MaxHealth;
            }
        }

        public void RecoveryMana(int percent)
        {
            _curMana += _curMana * percent / 100;
            if (_curMana > unitStatus.MaxMana)
            {
                _curMana = unitStatus.MaxMana;
            }
        }

        public void AddDefense(int percent)
        {
            if (_curDefense > 0)
            {
                _curDefense += _curDefense * percent / 100;
            }
            else
            {
                _curDefense = unitStatus.MaxDefense;
            }
        }

        public void SetDefense(int defense)
        {
            _curDefense = defense;
        }
    }
}