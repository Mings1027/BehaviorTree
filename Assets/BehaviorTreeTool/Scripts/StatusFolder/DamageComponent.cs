using UnityEngine;

namespace Tree
{
    public class DamageComponent : MonoBehaviour
    {
        private int _curDamage;
        private UnitStatus _unitStatus;
        public int CurDamage => _curDamage;

        public void SetUnitStatus(UnitStatus unitStatus)
        {
            _unitStatus = unitStatus;
            _curDamage = _unitStatus.BaseDamage;
        }

        public void BuffDamage(int percent)
        {
            _curDamage += _curDamage * percent / 100;
        }

        public void ResetDamage()
        {
            _curDamage = _unitStatus.BaseDamage;
        }
    }
}