using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] protected int maxHp = 100;
    protected int currentHp;

    protected virtual void Awake()
    {
        currentHp = maxHp;
    }

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);

        Debug.Log($"{gameObject.name} HP: {currentHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// HPが0になったときの処理（派生クラスで上書き）
    /// </summary>
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} Died");
    }
}
