using DataControl.ObjectKeyControl;
using UnityEngine;

namespace PoolObjectControl
{
    [DisallowMultipleComponent]
    public class PoolObject : MonoBehaviour
    {
        public PoolObjectKey poolObjKey { get; set; }

        private void OnDisable()
        {
            PoolObjectManager.ReturnToPool(gameObject, poolObjKey);
        }
    }
}