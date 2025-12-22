using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField] Slider hpslider;

    private void Start()
    {
        UpdateHP(10);
    }

    public void UpdateHP(int hp)
    {
        hpslider.value = hp;
    }
}
