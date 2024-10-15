using System;
using Cysharp.Threading.Tasks;
using DataControl.ObjectKeyControl;
using PoolObjectControl;
using UnityEngine;

namespace Tree
{
    public class Health : MonoBehaviour, IDamageable
    {
        private Collider _col;
        private int _curHealth;

        [SerializeField] private UnitStatus unitStatus;
        [SerializeField] private PoolObjectKey damageEffect;

        private void Awake()
        {
            _col = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            _col.enabled = true;
            _curHealth = unitStatus.Health;
        }

        public void Damage(int amount)
        {
            _curHealth -= amount;
            // PlayDamageEffect();
            if (_curHealth <= 0)
            {
                _col.enabled = false;
                gameObject.SetActive(false);
            }
        }

        private void PlayDamageEffect()
        {
            PoolObjectManager.Get(damageEffect, transform.position, Quaternion.identity);
            // await UniTask.Delay(1000, cancellationToken: destroyCancellationToken);
        }
    }
}