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
    private bool isShowMouseCursor;

    // =========================
    // Unity Lifecycle
    // =========================
    private async void Start()
    {
        roomModel = GetComponent<RoomModel>();
        userModel = UserModel.Instance;

        // RoomModel Events
        roomModel.OnJoinedUser += OnJoinedUser;
        roomModel.OnLeftUser += OnLeftUser;
        roomModel.OnPlayerReadyStatusChangedReceived += OnReadyStatusChanged;
        roomModel.OnStartGameReceived += OnStartGameReceived;
        roomModel.OnStartGameError += OnStartGameError;

        // 接続
        LoadingManager.Show();
        await roomModel.ConnectAsync();
        LoadingManager.Hide();

        // UI
        joinButton.onClick.AddListener(OnJoinClicked);
        leaveButton.onClick.AddListener(OnLeaveClicked);
        readyButton.onClick.AddListener(OnReadyClicked);
        startButton.onClick.AddListener(OnStartClicked);

        startButton.interactable = false;

        HideMouseCursor();
        isShowMouseCursor = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isShowMouseCursor = !isShowMouseCursor;
            if (isShowMouseCursor) ShowMouseCursor();
            else HideMouseCursor();
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

        await roomModel.JoinAsync(roomNameInput.text, userModel.UserId);
        isJoined = true;

        ShowMessage("ルームに参加しました");
    }

    private async void OnLeaveClicked()
    {
        if (!isJoined) return;

        if (myCharacter != null)
            Destroy(myCharacter);

        foreach (var obj in otherCharacters.Values)
            Destroy(obj);

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
        Debug.Log($"OnJoinedUser 呼ばれた: {user.ConnectionId}");

        bool isLocal = user.ConnectionId == roomModel.ConnectionId;

        GameObject player = Instantiate(characterPrefab);

        var controller = player.GetComponent<PlayerContoroller>();
        var pov = player.GetComponent<PlayerPOV>();
        var shooting = player.GetComponentInChildren<Shooting>();

        // ★ Local / Remote 制御
        controller.enabled = isLocal;
        pov.enabled = isLocal;
        shooting.enabled = isLocal;

        if (isLocal)
        {
            myCharacter = player;
            myJoinedUser = user;

            shooting.SetRoomModel(roomModel);

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
        }
        else
        {
            otherCharacters[user.ConnectionId] = player;
        }
    }

    private void OnLeftUser(Guid connectionId)
    {
        if (!otherCharacters.TryGetValue(connectionId, out var player)) return;

        Destroy(player);
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
        if (!isJoined || myJoinedUser == null) return;

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
    private void HideMouseCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ShowMouseCursor()
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
