using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject characterPrefab;

    [Header("UI")]
    [SerializeField] private InputField roomNameInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Text messageText;

    // ===== Model =====
    private RoomModel roomModel;
    private UserModel userModel;
    private JoinedUser myJoinedUser;

    // ===== Player管理 =====
    private GameObject myCharacter;
    private Dictionary<Guid, GameObject> otherCharacters = new();

    private bool isJoined;

    // ===== Cursor =====
    private bool isShowMouseCursor = false;

    // =========================
    // Unity Lifecycle
    // =========================
    private void Awake()
    {
        Debug.Log("LobbyManager Awake 呼ばれた");
    }

    private async void Start()
    {
        Debug.Log("LobbyManager Start 呼ばれた");

        roomModel = GetComponent<RoomModel>();
        userModel = UserModel.Instance;

        // 自分のPlayerは常に生成（射撃場）
        SpawnMyCharacter();

        // RoomModelイベント
        roomModel.OnJoinedUser += OnJoinedUser;
        roomModel.OnLeftUser += OnLeftUser;
        roomModel.OnPlayerReadyStatusChangedReceived += OnReadyStatusChanged;
        roomModel.OnStartGameReceived += OnStartGameReceived;
        roomModel.OnStartGameError += OnStartGameError;


        // 接続
        LoadingManager.Show();   // ← ここに追加
        await roomModel.ConnectAsync();
        LoadingManager.Hide();   // ← ここに追加

        // ボタン
        joinButton.onClick.AddListener(OnJoinClicked);
        leaveButton.onClick.AddListener(OnLeaveClicked);
        readyButton.onClick.AddListener(OnReadyClicked);
        startButton.onClick.AddListener(OnStartClicked);

        startButton.interactable = false;

        // 初期カーソル状態
        HideMouseCursor();
        isShowMouseCursor = false;
    }

    private void Update()
    {
        // ESCでカーソル切り替え
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isShowMouseCursor = !isShowMouseCursor;

            if (isShowMouseCursor)
            {
                ShowMouseCursor();
                Debug.Log("カーソル表示");
            }
            else
            {
                HideMouseCursor();
                Debug.Log("カーソル非表示");
            }
        }
    }

    // =========================
    // Player生成
    // =========================
    private void SpawnMyCharacter()
    {
        Debug.Log("SpawnMyCharacter start");

        myCharacter = Instantiate(characterPrefab);
        Debug.Log("Instantiate OK");

        var controller = myCharacter.GetComponent<PlayerContoroller>();
        Debug.Log("Controller = " + controller);

        var pov = myCharacter.GetComponent<PlayerPOV>();
        Debug.Log("POV = " + pov);

        var shooting = myCharacter.GetComponentInChildren<Shooting>();
        Debug.Log("Shooting = " + shooting);

        if (controller != null) controller.enabled = true;
        if (pov != null) pov.enabled = true;

        if (shooting != null)
        {
            shooting.SetRoomModel(roomModel);
        }
    }

    // =========================
    // Join / Leave
    // =========================
    private async void OnJoinClicked()
    {
        if (isJoined) return;

        if (string.IsNullOrWhiteSpace(roomNameInput.text))
        {
            ShowMessage("ルーム名を入力してください");
            return;
        }

        int userId = userModel.UserId;

        await roomModel.JoinAsync(roomNameInput.text, userId);
        isJoined = true;

        ShowMessage("ルームに参加しました");
    }




    private async void OnLeaveClicked()
    {
        if (!isJoined) return;

        foreach (var obj in otherCharacters.Values)
        {
            Destroy(obj);
        }
        otherCharacters.Clear();

        await roomModel.LeaveAsync();
        isJoined = false;

        startButton.interactable = false;
        ShowMessage("ルームから退出しました");
    }

    // =========================
    // RoomModel Events
    // =========================
    private void OnJoinedUser(JoinedUser user)
    {
        // 自分
        if (user.ConnectionId == roomModel.ConnectionId)
        {
            myJoinedUser = user;

            if (user.JoinOrder == 0)
            {
                startButton.interactable = true;
                ShowMessage("あなたはホストです");
            }
            else
            {
                startButton.interactable = false;
                ShowMessage("ホストの開始を待っています");
            }

            return;
        }

        if (otherCharacters.ContainsKey(user.ConnectionId)) return;

        GameObject other = Instantiate(characterPrefab);
        other.GetComponent<PlayerContoroller>().enabled = false;
        other.GetComponent<PlayerPOV>().enabled = false;

        otherCharacters[user.ConnectionId] = other;
    }


    private void OnLeftUser(Guid connectionId)
    {
        if (!otherCharacters.ContainsKey(connectionId)) return;

        Destroy(otherCharacters[connectionId]);
        otherCharacters.Remove(connectionId);
    }

    // =========================
    // Ready / Start
    // =========================
    private async void OnReadyClicked()
    {
        if (!isJoined) return;
        await roomModel.SetReadyAsync(true);
        ShowMessage("Ready!");
    }

    private async void OnStartClicked()
    {
        if (!isJoined) return;
        if (myJoinedUser == null) return;

        await roomModel.StartGameAsync();
    }


    private void OnReadyStatusChanged(Guid connectionId, bool isReady)
    {
        Debug.Log($"Player {connectionId} Ready = {isReady}");
    }

    private void OnStartGameReceived()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ButtleScene");
    }

    private void OnStartGameError(string errorCode)
    {
        switch (errorCode)
        {
            case "NOT_ALL_READY":
                ShowMessage("全員の準備が終わっていません");
                break;

            case "NOT_HOST":
                ShowMessage("ホストしか開始できません");
                break;

            default:
                ShowMessage("ゲームを開始できませんでした");
                break;
        }
    }


    // =========================
    // Cursor
    // =========================
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

    // =========================
    // UI
    // =========================
    private void ShowMessage(string msg)
    {
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), 2f);
    }

    private void HideMessage()
    {
        messageText.gameObject.SetActive(false);
    }
}
