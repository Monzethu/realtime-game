using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    [Header("HP Settings")]
    [SerializeField] private int maxHp = 100;
    private int currentHp;

    [Header("UI")]
    [SerializeField] private Slider hpSlider;

    private void Awake()
    {
        currentHp = maxHp;

        if (hpSlider != null)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("HP Slider が設定されていません");
        }
    }

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        UpdateUI();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 回復
    /// </summary>
    public void Heal(int value)
    {
        currentHp += value;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (hpSlider != null)
        {
            hpSlider.value = (float)currentHp / maxHp;
        }
    }

    private void Die()
    {
        Debug.Log("Player Dead");

        // 一旦動けなくする例
        var controller = GetComponent<PlayerContoroller>();
        if (controller != null)
        {
            controller.enabled = false;
        }
    }

    /// <summary>
    /// デバッグ用
    /// </summary>
    public int GetCurrentHP()
    {
        return currentHp;
    }
}
