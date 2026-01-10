using UnityEngine;

public class PlayerRoot : MonoBehaviour
{
    public static PlayerRoot Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ƒV[ƒ“Ø‚è‘Ö‚¦‚Ä‚àÁ‚³‚È‚¢
        }
        else
        {
            Destroy(gameObject); // “ñd¶¬–h~
        }
    }
}
