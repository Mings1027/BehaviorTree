using System;
using InterfaceFolder;
using UnityEngine;

namespace Utilities
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int initHealth;
        private int current;

        private void Start()
        {
            current = initHealth;
        }

        public void Damage(int amount)
        {
            current -= amount;
            if (current <= 0)
            {
                Debug.Log("Deaaaaaaaaaaaad");
                gameObject.SetActive(false);
            }
        }
    }
}