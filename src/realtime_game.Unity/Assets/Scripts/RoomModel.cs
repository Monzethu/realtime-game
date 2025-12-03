using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using MagicOnion;
using realtime_game.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;
using System.Threading.Tasks;

public class RoomModel : BaseModel, IRoomHubReceiver
{
    private GrpcChannelx channel;
    private IRoomHub roomHub;

    //　接続ID
    public Guid ConnectionId { get; set; }

    //　ユーザー接続通知
    public Action<JoinedUser> OnJoinedUser { get; set; }

    // ユーザー切断通知
    public Action<Guid> OnLeftUser { get; set; }

    // ユーザー位置情報
    public Action<Guid, Vector3, Quaternion> OnMoveCharacter { get; set; }


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


    //　入室
    public async UniTask JoinAsync(string roomName, int userId)
    {
        JoinedUser[] users = await roomHub.JoinAsync(roomName, userId);

        if (OnJoinedUser != null)
        {
            foreach (var user in users)
            {
                OnJoinedUser(user);
            }
        }
    }

    //　入室通知 (IRoomHubReceiverインタフェースの実装)
    public void OnJoin(JoinedUser user)
    {
        if (OnJoinedUser != null)
        {
            OnJoinedUser(user);
        }
    }

    // 退室
    public async UniTask LeaveAsync()
    {
        await roomHub.LeaveAsync();
        Debug.Log("退室完了");
    }

    // 退室通知 (IRoomHubReceiverインタフェースの実装)
    public void OnLeave(Guid connectionId)
    {
        if (OnLeftUser != null)
        {
            OnLeftUser(connectionId);
        }
    }

    //位置・回転を送信する
    public　async UniTask MoveAsync(Vector3 position, Quaternion rotation)
    {
        // サーバーの関数呼び出し
        await roomHub.MoveAsync(position, rotation);
    }

    public void OnMove(Guid connectionId, Vector3 position, Quaternion rotation)
    {
        if (OnMoveCharacter != null)
        {
            OnMoveCharacter(connectionId, position, rotation);
        }
    }
}
