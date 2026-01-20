using Cysharp.Threading.Tasks;
using realtime_game.Shared.Models.Entities;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameDirector : MonoBehaviour
{
    [SerializeField] GameObject characterPrefab;

    // 自分の Player
    private GameObject myCharacter;

    // 他人の Player
    public Dictionary<Guid, GameObject> otherCharacters = new();

    RoomModel roomModel;
    UserModel userModel;

    bool isJoin;
    float timer;

    JoinedUser myJoinedUser;

    [Header("UI")]
    [SerializeField] InputField roomNameInput;
    [SerializeField] Button joinButton;
    [SerializeField] Button leaveButton;
    [SerializeField] Button readyButton;
    [SerializeField] Button startButton;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    async void Start()
    {
        roomModel = GetComponent<RoomModel>();
        userModel = GetComponent<UserModel>();

        // ★ 自分用 Player を先に生成
        myCharacter = Instantiate(characterPrefab);

        // Shooting に RoomModel を渡す
        var shooting = myCharacter.GetComponentInChildren<Shooting>();
        if (shooting != null)
        {
            shooting.SetRoomModel(roomModel);
        }

        if (string.IsNullOrEmpty(userModel.Token))
        {
            MessageManager.Instance.ShowMessage("ログイン情報がありません");
            Debug.LogError("Token is null or empty");
            return;
        }

        Debug.Log($"Game Start: UserId={userModel.UserId}, Token={userModel.Token}");

        // イベント登録
        roomModel.OnJoinedUser += OnJoinedUser;
        roomModel.OnLeftUser += OnLeftUser;
        roomModel.OnMoveCharacter += OnMoveCharacter;
        roomModel.OnStartGameReceived += OnStartGameReceived;
        roomModel.OnPlayerReadyStatusChangedReceived += OnPlayerReadyStatusChangedReceived;
        roomModel.OnStartGameError += OnStartGameError;

        await roomModel.ConnectAsync();

        joinButton.onClick.AddListener(OnJoinButtonPressed);
        leaveButton.onClick.AddListener(OnLeaveButtonPressed);
        readyButton.onClick.AddListener(OnReadyClicked);
        startButton.onClick.AddListener(OnStartClicked);

        isJoin = false;
        timer = 0f;
    }

    async void Update()
    {
        if (!isJoin || myCharacter == null) return;

        timer += Time.deltaTime;
        if (timer >= 0.1f)
        {
            timer = 0f;
            await roomModel.MoveAsync(
                myCharacter.transform.position,
                myCharacter.transform.rotation
            );
        }
    }

    // ======================
    // Join / Leave
    // ======================

    async void OnJoinButtonPressed()
    {
        if (string.IsNullOrWhiteSpace(roomNameInput.text))
        {
            Debug.Log("ルーム名が空です");
            return;
        }

        // ★ Token は使わない。UserName を渡す
        if (userModel == null || userModel.UserId <= 0)
        {
            Debug.Log("ログインしていません");
            return;
        }

        await roomModel.JoinAsync(roomNameInput.text, userModel.UserName);
        isJoin = true;
    }

    async void OnLeaveButtonPressed()
    {
        foreach (var obj in otherCharacters.Values)
        {
            Destroy(obj);
        }
        otherCharacters.Clear();

        isJoin = false;
        await roomModel.LeaveAsync();
    }

    // ======================
    // Ready / Start
    // ======================

    async void OnReadyClicked()
    {
        if (!isJoin) return;
        await roomModel.SetReadyAsync(true);
    }

    async void OnStartClicked()
    {
        if (!isJoin) return;
        await roomModel.StartGameAsync();
    }

    void OnStartGameReceived()
    {
        Debug.Log("ゲームスタート");
        UnityEngine.SceneManagement.SceneManager.LoadScene("ButtleScene");
    }

    void OnPlayerReadyStatusChangedReceived(Guid id, bool ready)
    {
        Debug.Log($"Player {id} Ready = {ready}");
    }

    // ======================
    // Room Events
    // ======================

    void OnJoinedUser(JoinedUser user)
    {
        Debug.Log($"Join: {user.UserData.Name}, order={user.JoinOrder}");

        // ★ 自分
        if (user.UserData.Id == userModel.UserId)
        {
            myJoinedUser = user;

            // Host 判定
            startButton.interactable = (user.JoinOrder == 0);

            // 自分の Player を有効化
            myCharacter.GetComponent<PlayerContoroller>().enabled = true;
            myCharacter.GetComponent<PlayerPOV>().enabled = true;

            return;
        }

        // ★ 他人
        if (otherCharacters.ContainsKey(user.ConnectionId)) return;

        GameObject other = Instantiate(characterPrefab);

        other.GetComponent<PlayerContoroller>().enabled = false;
        other.GetComponent<PlayerPOV>().enabled = false;

        otherCharacters[user.ConnectionId] = other;
    }

    void OnLeftUser(Guid connectionId)
    {
        if (!otherCharacters.ContainsKey(connectionId)) return;

        Destroy(otherCharacters[connectionId]);
        otherCharacters.Remove(connectionId);
    }

    void OnMoveCharacter(Guid connectionId, Vector3 pos, Quaternion rot)
    {
        if (!otherCharacters.ContainsKey(connectionId)) return;

        var t = otherCharacters[connectionId].transform;
        t.DOKill();
        t.rotation = rot;
        t.DOMove(pos, 0.1f).SetEase(Ease.Linear);
    }

    void OnStartGameError(string error)
    {
        Debug.Log($"StartGameError: {error}");
    }
}
