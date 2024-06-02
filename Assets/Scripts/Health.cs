using UnityEngine;

public class Health : MonoBehaviour , IDamageable
{
    [SerializeField] private int maxHealth;
    private Collider collider;
    private int curHealth;

    private void Awake()
    {
        curHealth = maxHealth;
        collider = GetComponent<Collider>();
    }

    public void Damage(int amount)
    {
        curHealth -= amount;
        if (curHealth <= 0)
        {
            Debug.Log("Dead!!!");
            collider.enabled = false;
            gameObject.SetActive(false);
        }
    }
}