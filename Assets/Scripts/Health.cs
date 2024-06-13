using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth;
    private Collider collider;
    private int curHealth;

    private void Awake()
    {
        collider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        collider.enabled = true;
        curHealth = maxHealth;
    }

    public void Damage(int amount)
    {
        curHealth -= amount;
        if (curHealth <= 0)
        {
            collider.enabled = false;
            gameObject.SetActive(false);
        }
    }
}