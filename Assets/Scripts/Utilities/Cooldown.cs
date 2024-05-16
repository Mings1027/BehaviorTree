using System;
using UnityEngine;

namespace Utilities
{
    [Serializable]
    public class Cooldown
    {
        private float _nextFireTime;

        public float cooldownTime { get; set; }
        public bool isCoolingDown => Time.time < _nextFireTime;
        public void StartCooldown() => _nextFireTime = Time.time + cooldownTime;
    }
}