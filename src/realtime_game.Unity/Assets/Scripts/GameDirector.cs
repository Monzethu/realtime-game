using Cysharp.Threading.Tasks;
using realtime_game.Shared.Models.Entities;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class GameDirector : MonoBehaviour
{
    [SerializeField] GameObject characterPrefab;
    public GameObject character;
    public Dictionary<Guid, GameObject> characterList = new Dictionary<Guid, GameObject>();

    public RoomModel roomModel;
    public UserModel userModel;

    public int myUserId;
    User myself;

    [Header("Lobby UI")]
    [SerializeField] InputField roomNameInput;
    [SerializeField] InputField userIdInput;
    [SerializeField] Button joinButton;
    [SerializeField] Button leaveButton;
    [SerializeField] Button readyButton;
    [SerializeField] Button startButton;

    [SerializeField] private GameObject bulletPrefab;

    [Header("Message UI")]
    [SerializeField] Text messageText;

    bool isJoin;
    float timer;
    bool isShowMouseCursor;
    JoinedUser myJoinedUser;

    // --- 公開プロパティ ---
    public int MyUserId => myUserId;
    public RoomModel Room => roomModel;
    public GameObject Character => character;

    public static GameDirector Instance { get; private set; }

    private void Awake()
    {
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

        // メッセージUIは最初非表示
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    async void Start()
    {
        roomModel = GetComponent<RoomModel>();
        userModel = GetComponent<UserModel>();

        InitPlayerIfNeeded();

        // イベント登録
        roomModel.OnJoinedUser += OnJoinedUser;
        roomModel.OnLeftUser += OnLeftUser;
        roomModel.OnMoveCharacter += OnMoveCharacter;
        roomModel.OnBulletReceived += OnBulletReceived;
        roomModel.OnStartGameReceived += OnStartGameReceived;
        roomModel.OnPlayerReadyStatusChangedReceived += OnPlayerReadyStatusChangedReceived;
        roomModel.OnStartGameError += OnStartGameError;

        // LobbyScene では接続がまだなら Connect
        if (!roomModel.IsJoined)
        {
            Debug.Log("ConnectAsync 開始");
            await roomModel.ConnectAsync();
            Debug.Log("ConnectAsync 完了");
        }

        // ボタン登録
        if (joinButton != null) joinButton.onClick.AddListener(OnJoinButtonPressed);
        if (leaveButton != null) leaveButton.onClick.AddListener(OnLeaveButtonPressed);
        if (readyButton != null) readyButton.onClick.AddListener(OnReadyClicked);
        if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
    }

    async void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 0.1f && isJoin && character != null)
        {
            timer = 0;
            await roomModel.MoveAsync(character.transform.position, character.transform.rotation);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isShowMouseCursor = !isShowMouseCursor;
            if (isShowMouseCursor) ShowMouseCursor();
            else HideMouseCursor();
        }
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

    async void OnJoinButtonPressed()
    {
        if (string.IsNullOrWhiteSpace(roomNameInput.text)) return;

        myUserId = int.Parse(userIdInput.text);

        try
        {
            myself = await userModel.GetUserByIdAsync(myUserId);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        try
        {
            await roomModel.JoinAsync(roomNameInput.text); // ★ここ重要
            isJoin = true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }


    private void OnLeaveButtonPressed() => LeaveRoom();
    async void OnReadyClicked() { if (!isJoin) return; await roomModel.SetReadyAsync(true); }
    async void OnStartClicked() { if (!isJoin) return; await roomModel.StartGameAsync(); }

    private void OnStartGameReceived()
    {
        Debug.Log("ゲームスタート！UIを非表示にしてバトル開始");

        // ゲーム開始時にInputFieldとボタンをまとめて非表示
        if (roomNameInput) roomNameInput.gameObject.SetActive(false);
        if (userIdInput) userIdInput.gameObject.SetActive(false);
        if (joinButton) joinButton.gameObject.SetActive(false);
        if (leaveButton) leaveButton.gameObject.SetActive(false);
        if (readyButton) readyButton.gameObject.SetActive(false);
        if (startButton) startButton.gameObject.SetActive(false);

        // カメラ有効化
        var pc = character.GetComponent<PlayerContoroller>();
        if (pc != null && pc.cam != null)
            pc.cam.enabled = true;
    }

    private void OnPlayerReadyStatusChangedReceived(Guid connectionId, bool isReady)
    {
        Debug.Log($"Player {connectionId} Ready: {isReady}");
    }

    public void OnJoinedUser(JoinedUser user)
    {
        if (user.UserData.Id == myUserId)
        {
            myJoinedUser = user;
            if (startButton) startButton.interactable = myJoinedUser.JoinOrder == 0;

            // ルームに入った直後にホストか待機かをUIで表示
            if (myJoinedUser.JoinOrder == 0)
                ShowMessage("あなたはホストです！");
            else
                ShowMessage("ホストの開始を待っています…");

            return;
        }

        if (characterList.ContainsKey(user.ConnectionId)) return;

        GameObject other = Instantiate(characterPrefab);
        other.GetComponent<PlayerContoroller>().cam.depth = -10;
        other.GetComponent<PlayerContoroller>().enabled = false;
        other.GetComponent<PlayerPOV>().enabled = false;
        characterList[user.ConnectionId] = other;
    }

    public async void LeaveRoom()
    {
        foreach (Guid id in characterList.Keys.ToArray())
        {
            Destroy(characterList[id]);
            characterList.Remove(id);
        }

        isJoin = false;
        await roomModel.LeaveAsync();
    }

    private void OnLeftUser(Guid connectionId)
    {
        if (!characterList.ContainsKey(connectionId)) return;
        Destroy(characterList[connectionId]);
        characterList.Remove(connectionId);
    }

    public void OnMoveCharacter(Guid connectionId, Vector3 pos, Quaternion rot)
    {
        if (!characterList.ContainsKey(connectionId)) return;
        var obj = characterList[connectionId].transform;
        obj.DOKill();
        obj.rotation = rot;
        obj.DOMove(pos, 0.1f).SetEase(Ease.Linear);
    }

    public void InitPlayerIfNeeded()
    {
        if (character == null)
        {
            character = Instantiate(characterPrefab);
            character.SetActive(true);

            var shooting = character.GetComponentInChildren<Shooting>();
            if (shooting != null && roomModel != null)
                shooting.SetRoomModel(roomModel);

            var pc = character.GetComponent<PlayerContoroller>();
            if (pc != null && pc.cam != null)
                pc.cam.enabled = true;
        }
    }

    void OnBulletReceived(Guid shooterId, Vector3 pos, Quaternion rot, Vector3 vel)
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab is not assigned");
            return;
        }

        var bullet = Instantiate(bulletPrefab, pos, rot);
        var bm = bullet.GetComponent<BulletManager>();
        bm.ShooterId = shooterId;
    }

    void OnStartGameError(string errorCode)
    {
        switch (errorCode)
        {
            case "NOT_HOST": ShowMessage("あなたはホストではありません！"); break;
            case "NOT_ALL_READY": ShowMessage("全員の準備が終わっていません"); break;
            default: ShowMessage("開始できませんでした"); break;
        }
    }

    void ShowMessage(string message)
    {
        if (messageText == null) return;
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), 3f);
    }

    public void HideMessage()
    {
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    public Vector3 GetSpawnPosition()
    {
        // 適当な位置にスポーン（後でランダムや指定座標に変更可）
        return new Vector3(UnityEngine.Random.Range(-5f, 5f), 1f, UnityEngine.Random.Range(-5f, 5f));
    }

}
