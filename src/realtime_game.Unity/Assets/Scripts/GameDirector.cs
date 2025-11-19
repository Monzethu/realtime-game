using Cysharp.Threading.Tasks;
using realtime_game.Shared.Models.Entities;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    [SerializeField] GameObject characterPrefab;
    Dictionary<Guid, GameObject> characterList = new Dictionary<Guid, GameObject>();

    RoomModel roomModel;
    UserModel userModel;

    int myUserId = 1;// 自分のユーザーID（この端末はみんなID1）
    User myself;// 自分のユーザー情報を保持

    [SerializeField] InputField roomNameInput;
    [SerializeField] Button joinButton;
    [SerializeField] Button leaveButton;


    async void Start()
    {
        roomModel=GetComponent<RoomModel>();
        userModel=GetComponent<UserModel>();

        //ユーザーが入室した時にOnJoinedUserメソッドを実行するよう、モデルに登録しておく
        roomModel.OnJoinedUser += this.OnJoinedUser;

        // ユーザー退出
        roomModel.OnLeftUser += this.OnLeftUser;


        //接続
        await roomModel.ConnectAsync();

        // Join ボタンを押したときの処理
        joinButton.onClick.AddListener(OnJoinButtonPressed);

        // leave ボタンを押したときの処理
        leaveButton.onClick.AddListener(OnLeaveButtonPressed);

        try
        {
            // ユーザー情報を取得
            myself = await userModel.GetUserByIdAsync(myUserId);
        }
        catch (Exception e)
        {
            Debug.Log("GetUser failed");
            Debug.LogException(e);
        }
    }

    public async UniTaskVoid JoinRoom(string roomName)
    {
        try
        {
            var joinedUsers = await roomModel.JoinAsync(roomName, myUserId);

            Debug.Log("---- Joined Room ----");

            foreach (var user in joinedUsers)
            {
                // ★ 自分のキャラはここで生成する
                if (user.UserData.Id == myUserId)
                {
                    if (!characterList.ContainsKey(user.ConnectionId))
                    {
                        GameObject myObject = Instantiate(characterPrefab);
                        myObject.transform.position = new Vector3(0, 0, 0);
                        characterList[user.ConnectionId] = myObject;

                        Debug.Log($"ConnectionId: {user.ConnectionId} UserId: {user.UserData.Id} UserName: {user.UserData.Name}");
                    }
                }
            }
            Debug.Log("----------------------");
        }
        catch (Exception e)
        {
            Debug.Log("Join failed");
            Debug.LogException(e);
        }
    }


    //ユーザーが入室した時の処理
    private void OnJoinedUser(JoinedUser user)
    {
        // すでに表示済みのユーザーは追加しない
        if (characterList.ContainsKey(user.ConnectionId))
        {
            return;
        }

        // 自分は追加しない
        if (user.UserData.Id == myUserId)
        {
            return;
        }

        GameObject characterObject = Instantiate(characterPrefab);  //インスタンス生成
        characterObject.transform.position = new Vector3(0, 0, 0); // 配置位置設定
        characterList[user.ConnectionId] = characterObject;  //フィールドで保持
    }

    private void OnLeftUser(JoinedUser user)
    {
        if (characterList.ContainsKey(user.ConnectionId))
        {
            Destroy(characterList[user.ConnectionId]);
            characterList.Remove(user.ConnectionId);
            Debug.Log($"{user.UserData.Name} が退出しました");
        }
    }


    // Joinボタン押下時の処理
    private void OnJoinButtonPressed()
    {
        //ルーム名未入力なら何もしない（ミッション要件）
        if (string.IsNullOrWhiteSpace(roomNameInput.text))
        {
            Debug.Log("ルーム名が空です");
            return;
        }

        JoinRoom(roomNameInput.text).Forget();
    }

    private void OnLeaveButtonPressed()
    {
        LeaveRoom().Forget();
    }

    private async UniTaskVoid LeaveRoom()
    {
        try
        {
            await roomModel.LeaveAsync();
            Debug.Log("部屋から退出しました");

            // ★ 自分のキャラを削除
            if (characterList.TryGetValue(roomModel.ConnectionId, out var obj))
            {
                Destroy(obj);
                characterList.Remove(roomModel.ConnectionId);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Leave failed");
            Debug.LogException(e);
        }
    }

}

