using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Tree
{
    public class Health : MonoBehaviour, IDamageable
    {
        private Collider col;
        private Renderer objectRenderer;
        private Color originColor;
        private int curHealth;

        [SerializeField] private UnitStatus unitStatus;
        [SerializeField] private Color damageColor;

        private void Awake()
        {
            col = GetComponent<Collider>();
            objectRenderer = GetComponent<Renderer>();
            originColor = objectRenderer.material.color;
        }

        private void OnEnable()
        {
            col.enabled = true;
            curHealth = unitStatus.Health;
        }

        public void Damage(int amount)
        {
            curHealth -= amount;
            ChangeColor().Forget();
            if (curHealth <= 0)
            {
                col.enabled = false;
                gameObject.SetActive(false);
            }
        }

        private async UniTaskVoid ChangeColor()
        {
            objectRenderer.material.color = damageColor;
            await UniTask.Delay(100, cancellationToken: this.GetCancellationTokenOnDestroy());
            objectRenderer.material.color = originColor;
        }
    }
}