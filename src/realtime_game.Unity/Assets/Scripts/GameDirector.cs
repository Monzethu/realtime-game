using Cysharp.Threading.Tasks;
using realtime_game.Shared.Models.Entities;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    [SerializeField] GameObject characterPrefab;
    Dictionary<Guid, GameObject> characterList = new Dictionary<Guid, GameObject>();

    RoomModel roomModel;
    UserModel userModel;

    int myUserId = 1;
    User myself;

    [SerializeField] InputField roomNameInput;
    [SerializeField] Button joinButton;
    [SerializeField] Button leaveButton;

    async void Start()
    {
        roomModel = GetComponent<RoomModel>();
        userModel = GetComponent<UserModel>();

        //ユーザーが入室した時にOnJoinedUserメソッドを実行するよう、モデルに登録しておく
        roomModel.OnJoinedUser += this.OnJoinedUser;
        // ユーザーが退室した時にOnLeftUserメソッドを実行できるよう、モデルに登録しておく
        roomModel.OnLeftUser += this.OnLeftUser;
        // ユーザーが退室した時にOnLeftUserAllメソッドを実行できるよう、モデルに登録しておく
        roomModel.OnLeftUserAll += this.OnLeftUserAll;
        //// ユーザーが移動・回転したときにOnMoveCharacterメソッドを実行できるよう、モデルに登録しておく
        //roomModel.OnMoveCharacter += OnMoveCharacter;


        //接続
        Debug.Log("ConnectAsync 開始");
        await roomModel.ConnectAsync();
        Debug.Log("ConnectAsync 完了");
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

        // ボタン登録
        joinButton.onClick.AddListener(OnJoinButtonPressed);
        leaveButton.onClick.AddListener(OnLeaveButtonPressed);
    }

    // Join ボタン
    private void OnJoinButtonPressed()
    {
        Debug.Log("Joinボタンが押された！");

        if (string.IsNullOrWhiteSpace(roomNameInput.text))
        {
            Debug.Log("ルーム名が空です");
            return;
        }

        JoinRoom(roomNameInput.text);
    }

    // Leave ボタン
    private void OnLeaveButtonPressed()
    {
        LeaveRoom();
    }

    // 入室
    public async void JoinRoom(string roomName)
    {
        Debug.Log("JoinRoom 呼ばれた: " + roomName);

        try
        {
            Debug.Log("JoinAsync 開始");
            await roomModel.JoinAsync(roomName, 1);
            Debug.Log("JoinAsync 完了");
        }
        catch (Exception e)
        {
            Debug.Log("JoinAsync 失敗");
            Debug.LogException(e);
        }
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
        if (characterList.ContainsKey(user.ConnectionId))
        {
            return;
        }

        GameObject characterObject = Instantiate(characterPrefab);  //インスタンス生成
        characterObject.transform.position = new Vector3(0, 0, 0); // 配置位置設定
        characterObject.name = "Player_" + user.UserData.Id;

        // 自分なら操作できるようにする
        if (user.UserData.Id == myUserId)
        {
            characterObject.AddComponent<PlayerMover>();
        }

        characterList[user.ConnectionId] = characterObject;  //フィールドで保持
    }

    // 退室処理
    public async void LeaveRoom()
    {
        if (roomNameInput == null || string.IsNullOrWhiteSpace(roomNameInput.text))
        {
            Debug.Log("ルーム名が空です");
            // ルーム名が入力されていない場合は何もしない
            return;
        }

        // 自分がルームを抜けるので、自分以外のオブジェクトを削除
        List<Guid> connectionIdList = characterList.Keys.ToList();
        foreach (Guid connectionId in connectionIdList)
        {
            // 自分のPlayerは削除しない
            if (characterList[connectionId].GetComponent<PlayerMover>() != null)
                continue;

            Destroy(characterList[connectionId]);
            characterList.Remove(connectionId);
        }

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

    // 自分が退室した時の処理
    private void OnLeftUserAll()
    {
        // 自分以外のオブジェクトを削除する
        List<Guid> connectionIdList = characterList.Keys.ToList();
        foreach (Guid connectionId in connectionIdList)
        {
            // 自分のPlayerは削除しない
            if (characterList[connectionId].GetComponent<PlayerMover>() != null)
                continue;

            // 一人分の退室処理
            OnLeftUser(connectionId);
        }
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

    //void OnMoveCharacter(接続ID, 位置, 回転)
    //{
    //    // characterListから対象のGameObjectを取得
    //    // 位置・回転を反映


    //}

}
