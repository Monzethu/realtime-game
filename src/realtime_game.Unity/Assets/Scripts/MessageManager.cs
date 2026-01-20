using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }

    [SerializeField] private Text messageText;

    private void Awake()
    {
        //Debug.Log("MessageManager Awake");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sceneをまたいでも保持
        }
        else
        {
            Destroy(gameObject);
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// メッセージ表示
    /// </summary>
    /// <param name="message">表示するテキスト</param>
    /// <param name="duration">表示時間（秒）</param>
    public void ShowMessage(string message, float duration = 3f)
    {
        if (messageText == null) return;

        messageText.text = message;
        messageText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), duration);
    }

    private void HideMessage()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }
}
