using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using MagicOnion;
using realtime_game.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;

public class RoomModel : BaseModel, IRoomHubReceiver
{
    private GrpcChannelx channel;
    private IRoomHub roomHub;

    //　接続ID
    public Guid ConnectionId { get; set; }

    //　ユーザー接続通知
    public Action<JoinedUser> OnJoinedUser { get; set; }

    //　ユーザー退出通知
    public Action<JoinedUser> OnLeftUser { get; set; }


    //　MagicOnion接続処理
    public async UniTask ConnectAsync()
    {
        channel = GrpcChannelx.ForAddress(ServerURL);
        roomHub = await StreamingHubClient.
             ConnectAsync<IRoomHub, IRoomHubReceiver>(channel, this);
        this.ConnectionId = await roomHub.GetConnectionId();
    }

    //　MagicOnion切断処理
    public async UniTask DisconnectAsync()
    {
        if (roomHub != null) await roomHub.DisposeAsync();
        if (channel != null) await channel.ShutdownAsync();
        roomHub = null; channel = null;
    }

    //　破棄処理 
    async void OnDestroy()
    {
        DisconnectAsync();
    }

    // 入室
    public async UniTask<JoinedUser[]> JoinAsync(string roomName, int userId)
    {
        // サーバーから全ユーザーの情報を受け取る
        JoinedUser[] users = await roomHub.JoinAsync(roomName, userId);

        // 受け取った全ユーザーに対してイベント発火
        foreach (var user in users)
        {
            OnJoinedUser?.Invoke(user);
        }

        // ★ 呼び出し元に返す
        return users;
    }


    //　入室通知 (IRoomHubReceiverインタフェースの実装)
    public void OnJoin(JoinedUser user)
    {
        if (OnJoinedUser != null)
        {
            OnJoinedUser(user);
        }
    }

    // ルーム退出
    public async UniTask LeaveAsync()
    {
        if (roomHub != null)
        {
            await roomHub.LeaveAsync();   // ★ サーバー側の LeaveAsync を呼ぶ
        }
    }

    //　退出通知
    public void OnLeave(JoinedUser user)
    {
        if (OnLeftUser != null)
        {
            OnLeftUser(user);
        }
    }

}

