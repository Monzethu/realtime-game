using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    [SerializeField] private GameObject Loading;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Loading.SetActive(false);
    }

    public static void Show()
    {
        if (Instance == null) return;
        Instance.Loading.SetActive(true);
    }

    public static void Hide()
    {
        if (Instance == null) return;
        Instance.Loading.SetActive(false);
    }
}
