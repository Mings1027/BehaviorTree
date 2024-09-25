using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Tree
{
    public class Health : MonoBehaviour, IDamageable
    {
        private Collider _col;
        private Renderer _objectRenderer;
        private Color _originColor;
        private int _curHealth;

        [SerializeField] private UnitStatus unitStatus;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _objectRenderer = GetComponent<Renderer>();
            _originColor = _objectRenderer.material.color;
        }

        private void OnEnable()
        {
            _col.enabled = true;
            _curHealth = unitStatus.Health;
        }

        public void Damage(int amount)
        {
            _curHealth -= amount;
            ChangeColor().Forget();
            if (_curHealth <= 0)
            {
                _col.enabled = false;
                gameObject.SetActive(false);
            }
        }

        private async UniTaskVoid ChangeColor()
        {
            _objectRenderer.material.color = unitStatus.damageColor;
            await UniTask.Delay(100, cancellationToken: destroyCancellationToken);
            _objectRenderer.material.color = _originColor;
        }
    }
}