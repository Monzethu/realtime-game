using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] protected int maxHp = 100;
    protected int currentHp;

    protected virtual void Awake()
    {
        currentHp = maxHp;
    }

    public virtual void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} died");
    }

    public virtual void ResetHP()
    {
        currentHp = maxHp;
    }
}
