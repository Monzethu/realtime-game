using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameDirector : MonoBehaviour
{
    [SerializeField] private GameObject characterPrefab; // Playerプレハブ
    private GameObject character;

    [SerializeField] private Text messageText;
    [SerializeField] private InputField roomNameInput;
    [SerializeField] private InputField userIdInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startButton;

    private int myUserId;
    private bool isJoin;
    private float timer;
    private bool isShowMouseCursor;

    public static GameDirector Instance { get; private set; }

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        HideMouseCursor();
        isShowMouseCursor = false;
    }

    private void Start()
    {
        // Playerが既にいる場合は二重生成しない
        if (PlayerRoot.Instance != null)
        {
            character = PlayerRoot.Instance.gameObject;
        }
        else
        {
            character = Instantiate(characterPrefab);
            character.AddComponent<PlayerRoot>(); // PlayerRootを追加
        }

        // UIボタン登録
        joinButton.onClick.AddListener(OnJoinButtonPressed);
        leaveButton.onClick.AddListener(OnLeaveButtonPressed);
        readyButton.onClick.AddListener(OnReadyClicked);
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void Update()
    {
        // カーソル表示切り替え
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isShowMouseCursor = !isShowMouseCursor;
            if (isShowMouseCursor) ShowMouseCursor();
            else HideMouseCursor();
        }

        timer += Time.deltaTime;
    }

    public void HideMouseCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowMouseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnJoinButtonPressed()
    {
        if (string.IsNullOrWhiteSpace(roomNameInput.text)) return;
        myUserId = int.Parse(userIdInput.text);

        // TODO: サーバー接続処理
        isJoin = true;
        ShowMessage("ルームに入室しました");
    }

    private void OnLeaveButtonPressed()
    {
        isJoin = false;
        ShowMessage("ルームを退出しました");
    }

    private void OnReadyClicked()
    {
        if (!isJoin) return;
        ShowMessage("Ready状態にしました");
    }

    private void OnStartClicked()
    {
        if (!isJoin) return;
        ShowMessage("ゲームスタート！");

        // シーン切り替え
        SceneManager.LoadScene("ButtleScene");
    }

    public void ShowMessage(string message)
    {
        if (messageText == null) return;

        messageText.text = message;
        messageText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), 3f);
    }

    private void HideMessage()
    {
        if (messageText == null) return;
        messageText.gameObject.SetActive(false);
    }
}
