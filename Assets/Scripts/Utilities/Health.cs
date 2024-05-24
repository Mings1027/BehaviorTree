using System;
using InterfaceFolder;
using UnityEngine;

namespace Utilities
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int initHealth;
        private Collider _collider;
        private int current;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            _collider.enabled = true;
        }

        private void Start()
        {
            current = initHealth;
        }

        public void Damage(int amount)
        {
            current -= amount;
            if (current <= 0)
            {
                _collider.enabled = false;
                gameObject.SetActive(false);
            }
        }
    }
}