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

    // ユーザー切断通知
    public Action<Guid> OnLeftUser { get; set; }

    // ユーザー切断通知
    public Action OnLeftUserAll { get; set; }

    //// ユーザー位置情報
    //public Action<位置, 回転> OnMoveCharacter { get; set; }


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
        foreach (var user in users)
        {
            if (OnJoinedUser != null)
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

        // 自分以外のオブジェクトを削除する
        if (OnLeftUserAll != null)
        {
            OnLeftUserAll();
        }
    }

    // 退室通知 (IRoomHubReceiverインタフェースの実装)
    public void OnLeave(Guid connectionId)
    {
        if (OnLeftUser != null)
        {
            OnLeftUser(connectionId);
        }
    }

    // 全員退室通知
    public void OnLeaveAll()
    {
        OnLeftUserAll?.Invoke();
    }

    ////位置・回転を送信する
    //public Task MoveAsync()
    //{
    //    // サーバーの関数呼び出し
    //}

    //void OnMove(接続ID, 位置, 回転)
    //{
    //    OnMoveCharacter(接続ID, 位置, 回転);
    //}

}
