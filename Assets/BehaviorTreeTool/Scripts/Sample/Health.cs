using System.Threading.Tasks;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth;
    private Collider collider;
    private Renderer objectRenderer;
    private Color originColor;
    private int curHealth;
    [SerializeField] private Color damageColor;

    private void Awake()
    {
        collider = GetComponent<Collider>();
        objectRenderer = GetComponent<Renderer>();
        originColor = objectRenderer.material.color;
    }

    private void OnEnable()
    {
        collider.enabled = true;
        curHealth = maxHealth;
    }

    public void Damage(int amount)
    {
        curHealth -= amount;
        ChangeColor();
        if (curHealth <= 0)
        {
            collider.enabled = false;
            gameObject.SetActive(false);
        }
    }

    private async void ChangeColor()
    {
        objectRenderer.material.color = damageColor;
        await Task.Delay(200);
        objectRenderer.material.color = originColor;
    }
}