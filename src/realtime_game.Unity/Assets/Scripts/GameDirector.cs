using Cysharp.Threading.Tasks;
using realtime_game.Shared.Models.Entities;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// ロビー兼射撃場（Playerは死なない。）
public class GameDirector : MonoBehaviour
{
    [SerializeField] GameObject characterPrefab;
    GameObject character;
    public Dictionary<Guid, GameObject> characterList = new Dictionary<Guid, GameObject>();

    RoomModel roomModel;
    UserModel userModel;

    int myUserId;
    User myself;

    [SerializeField] InputField roomNameInput;
    [SerializeField] InputField userIdInput;
    [SerializeField] Button joinButton;
    [SerializeField] Button leaveButton;
    [SerializeField] Button readyButton;
    [SerializeField] Button startButton;

    bool isJoin;

    float timer;

    private bool isShowMouseCursor;

    JoinedUser myJoinedUser;

    [SerializeField] Text messageText;// 表示するメッセージ

    public static GameDirector Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // LobbyScene → BattleScene で保持
        }
        else
        {
            Destroy(gameObject); // 二重生成防止
        }

        HideMouseCursor();
        isShowMouseCursor = false;
    }

    async void Start()
    {
        roomModel = GetComponent<RoomModel>();
        userModel = GetComponent<UserModel>();

        character = Instantiate(characterPrefab);
        //Debug.Log(character.transform.position);

        var shooting = character.GetComponentInChildren<Shooting>();
        if (shooting != null)
        {
            shooting.SetRoomModel(roomModel);
        }

        isJoin = false;
        timer = 0;

        //ユーザーが入室した時にOnJoinedUserメソッドを実行するよう、モデルに登録しておく
        roomModel.OnJoinedUser += this.OnJoinedUser;
        // ユーザーが退室した時にOnLeftUserメソッドを実行できるよう、モデルに登録しておく
        roomModel.OnLeftUser += this.OnLeftUser;
        // ユーザーが移動・回転したときにOnMoveCharacterメソッドを実行できるよう、モデルに登録しておく
        roomModel.OnMoveCharacter += OnMoveCharacter;

        roomModel.OnStartGameError += OnStartGameError;

        // サーバーからのゲーム開始通知イベントを登録
        roomModel.OnStartGameReceived += OnStartGameReceived;
        // プレイヤーReady状態変更通知イベント
        roomModel.OnPlayerReadyStatusChangedReceived += OnPlayerReadyStatusChangedReceived;

        //接続
        Debug.Log("ConnectAsync 開始");
        LoadingManager.Show();
        await roomModel.ConnectAsync();
        LoadingManager.Hide();
        Debug.Log("ConnectAsync 完了");

        // ボタン登録
        joinButton.onClick.AddListener(OnJoinButtonPressed);
        leaveButton.onClick.AddListener(OnLeaveButtonPressed);
        readyButton.onClick.AddListener(OnReadyClicked); // Readyボタン登録
        startButton.onClick.AddListener(OnStartClicked); // Startボタン登録
    }

    async void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 0.1f)
        {
            if (isJoin)
            {
                timer = 0;

                // 自分の位置と回転をサーバーに送信
                if (character != null)
                {
                    await roomModel.MoveAsync(character.transform.position, character.transform.rotation);
                }
            }
        }

        // Ecapeを押したとき
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isShowMouseCursor = !isShowMouseCursor;
            if (isShowMouseCursor)
            {
                ShowMouseCursor();
                Debug.Log("カーソルを表示");
            }
            else
            {
                HideMouseCursor();
                Debug.Log("カーソルを非表示");
            }
        }
    }

    /// <summary>
    /// カーソル非表示
    /// </summary>
    public void HideMouseCursor()
    {
        // カーソルを画面中央にロックする
        Cursor.lockState = CursorLockMode.Locked;
        // カーソル非表示
        Cursor.visible = false;
    }

    /// <summary>
    /// カーソル表示
    /// </summary>
    public void ShowMouseCursor()
    {
        // カーソルのロックを解除
        Cursor.lockState = CursorLockMode.None;
        // カーソル表示
        Cursor.visible = true;
    }

    // Join ボタン
    async void OnJoinButtonPressed()
    {
        Debug.Log("Joinボタンが押された！");

        if (string.IsNullOrWhiteSpace(roomNameInput.text))
        {
            Debug.Log("ルーム名が空です");
            return;
        }

        myUserId = int.Parse(userIdInput.text);

        LoadingManager.Show();

        try
        {
            // ユーザー情報取得
            myself = await userModel.GetUserByIdAsync(myUserId);

            // 入室
            await roomModel.JoinAsync(roomNameInput.text, myUserId);

            isJoin = true;
        }
        catch (Exception e)
        {
            Debug.Log("Join失敗");
            Debug.LogException(e);
        }
        finally
        {
            LoadingManager.Hide();
        }
    }


    // Leave ボタン
    private void OnLeaveButtonPressed()
    {
        LeaveRoom();
    }

    // Readyボタン押下
    private async void OnReadyClicked()
    {
        if (!isJoin) return;
        await roomModel.SetReadyAsync(true);
    }

    // Startボタン押下
    private async void OnStartClicked()
    {
        if (!isJoin) return;

        LoadingManager.Show();
        await roomModel.StartGameAsync();
        LoadingManager.Hide();
    }


    // サーバーからゲーム開始通知を受け取った
    private void OnStartGameReceived()
    {
        Debug.Log("ゲームスタート！シーン遷移します");
        UnityEngine.SceneManagement.SceneManager.LoadScene("ButtleScene");
    }

    // サーバーからReady状態通知を受け取った
    private void OnPlayerReadyStatusChangedReceived(Guid connectionId, bool isReady)
    {
        Debug.Log($"Player {connectionId} Ready: {isReady}");
        // TODO: UIに反映
    }

    // ユーザーが入室した時の処理
    private void OnJoinedUser(JoinedUser user)
    {
        Debug.Log("===== ユーザー入室 =====");
        Debug.Log($"UserId={user.UserData.Id}, JoinOrder={user.JoinOrder}");
        Debug.Log("=======================");

        // ★ 自分自身だった場合
        if (user.UserData.Id == myUserId)
        {
            myJoinedUser = user;

            // ホスト判定（JoinOrder == 0）
            if (myJoinedUser.JoinOrder == 0)
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

        // ===== 以下は他人用（今までの処理）=====
        if (characterList.ContainsKey(user.ConnectionId))
            return;

        GameObject characterObject = Instantiate(characterPrefab);
        characterObject.GetComponent<PlayerContoroller>().cam.depth = -10;
        characterObject.GetComponent<PlayerContoroller>().enabled = false;
        characterObject.GetComponent<PlayerPOV>().enabled = false;

        characterList[user.ConnectionId] = characterObject;
    }

    // 退室処理
    public async void LeaveRoom()
    {
        LoadingManager.Show();

        foreach (Guid connectionId in characterList.Keys.ToArray())
        {
            Destroy(characterList[connectionId]);
            characterList.Remove(connectionId);
        }

        isJoin = false;

        await roomModel.LeaveAsync();

        LoadingManager.Hide();
    }


    // ユーザーが退室した時の処理
    private void OnLeftUser(Guid connectionId)
    {
        // いない人は退室できない
        if (!characterList.ContainsKey(connectionId))
        {
            return;
        }

        Destroy(characterList[connectionId]); // 対象のオブジェクトを削除
        characterList.Remove(connectionId); // リストから対象のデータを削除
    }

    // 自分以外のユーザーの移動を反映
    void OnMoveCharacter(Guid connectionId, Vector3 pos, Quaternion rotation)
    {
        // いない人は移動できない
        if (!characterList.ContainsKey(connectionId))
        {
            return;
        }

        var obj = characterList[connectionId].transform;

        // 既存Tweenを止める
        obj.DOKill();

        // 回転反映
        obj.rotation = rotation;

        // DOTween で滑らかに移動
        obj.DOMove(pos, 0.1f).SetEase(Ease.Linear);
    }

    // メッセージ関係
    void OnStartGameError(string errorCode)
    {
        switch (errorCode)
        {
            case "NOT_HOST":
                ShowMessage("あなたはホストではありません！");
                break;

            case "NOT_ALL_READY":
                ShowMessage("全員の準備が終わっていません");
                break;

            default:
                ShowMessage("開始できませんでした");
                break;
        }
    }

    void ShowMessage(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);

        // 3秒後に消す
        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), 3f);
    }

    void HideMessage()
    {
        messageText.gameObject.SetActive(false);
    }

}
