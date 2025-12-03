using Cysharp.Threading.Tasks;
using realtime_game.Shared.Models.Entities;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.TextCore.Text;


public class GameDirector : MonoBehaviour
{
    [SerializeField] GameObject characterPrefab;
    GameObject character;
    Dictionary<Guid, GameObject> characterList = new Dictionary<Guid, GameObject>();

    RoomModel roomModel;
    UserModel userModel;

    int myUserId;
    User myself;

    [SerializeField] InputField roomNameInput;
    [SerializeField] InputField userIdInput;
    [SerializeField] Button joinButton;
    [SerializeField] Button leaveButton;

    bool isJoin;

    float timer;

    async void Start()
    {
        roomModel = GetComponent<RoomModel>();
        userModel = GetComponent<UserModel>();

        character = Instantiate(characterPrefab);
        Debug.Log(character.transform.position);

        isJoin = false;
        timer = 0;

        //ユーザーが入室した時にOnJoinedUserメソッドを実行するよう、モデルに登録しておく
        roomModel.OnJoinedUser += this.OnJoinedUser;
        // ユーザーが退室した時にOnLeftUserメソッドを実行できるよう、モデルに登録しておく
        roomModel.OnLeftUser += this.OnLeftUser;
        // ユーザーが移動・回転したときにOnMoveCharacterメソッドを実行できるよう、モデルに登録しておく
        roomModel.OnMoveCharacter += OnMoveCharacter;


        //接続
        Debug.Log("ConnectAsync 開始");
        await roomModel.ConnectAsync();
        Debug.Log("ConnectAsync 完了");

        // ボタン登録
        joinButton.onClick.AddListener(OnJoinButtonPressed);
        leaveButton.onClick.AddListener(OnLeaveButtonPressed);
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
                await roomModel.MoveAsync(character.transform.position, character.transform.rotation);
            }
        }
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

        
        Debug.Log("JoinRoom 呼ばれた: " + roomNameInput.text);
        try
        {
            // ユーザー情報を取得
            myself = await userModel.GetUserByIdAsync(myUserId);
        }
        catch (Exception e)
        {
            Debug.Log("RegistUser failed");
            Debug.LogException(e);
        }

        // 入室
        try
        {
            Debug.Log("JoinAsync 開始");
            await roomModel.JoinAsync(roomNameInput.text, myUserId);
            Debug.Log("JoinAsync 完了");
            isJoin = true;
        }
        catch (Exception e)
        {
            Debug.Log("JoinAsync 失敗");
            Debug.LogException(e);
        }
    }

    // Leave ボタン
    private void OnLeaveButtonPressed()
    {
        LeaveRoom();
    }

    // ユーザーが入室した時の処理
    private void OnJoinedUser(JoinedUser user)
    {
        Debug.Log("===== ユーザー入室 =====");
        Debug.Log("Connection ID : " + user.ConnectionId);
        Debug.Log("User ID       : " + user.UserData.Id);
        Debug.Log("User Name     : " + user.UserData.Name);
        Debug.Log("=======================");

        // すでに表示済みのユーザーは追加しない
        // 自分は追加しない
        if (characterList.ContainsKey(user.ConnectionId) || user.UserData.Id == myUserId)
        {
            return;
        }

        GameObject characterObject = Instantiate(characterPrefab);  //インスタンス生成
        characterObject.transform.position = new Vector3(0, 0, 0); // 配置位置設定
        characterObject.name = "Player_" + user.UserData.Id;

        characterObject.GetComponent<PlayerContoroller>().enabled = false;

        characterList[user.ConnectionId] = characterObject;  //フィールドで保持
    }

    // 退室処理
    public async void LeaveRoom()
    {
        // 自分以外のオブジェクトを削除
        foreach (Guid connectionId in characterList.Keys.ToArray())
        {
            Destroy(characterList[connectionId]);
            characterList.Remove(connectionId);
        }

        isJoin=false;

        // 退室
        await roomModel.LeaveAsync();
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


    //// 自分以外のユーザーの移動を反映
    //private void OnMoveUser(Guid connectionId, Vector3 pos, Quaternion quaternion)
    //{
    //    // いない人は移動できない
    //    if (!characterList.ContainsKey(connectionId))
    //    {
    //        return;
    //    }

    //    // DOTweenを使うことでなめらかに動く！
    //    //characterList[connectionId].transform.DOMove(pos, 0.1f);
    //    characterList[connectionId].transform.position = pos;
    //}

}
