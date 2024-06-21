using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    private Collider col;
    private Renderer objectRenderer;
    private Color originColor;
    private int curHealth;

    [SerializeField] private int maxHealth;
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
        curHealth = maxHealth;
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