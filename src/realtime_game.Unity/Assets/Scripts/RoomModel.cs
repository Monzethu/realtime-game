using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using MagicOnion;
using realtime_game.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;
using System.Threading.Tasks;
using Grpc.Core;

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

    // 弾の発射
    public Action<Guid, Vector3, Quaternion, Vector3> OnBulletReceived { get; set; }

    // プレイヤーReady状態通知
    public Action<Guid, bool> OnPlayerReadyStatusChangedReceived { get; set; }

    // ゲーム開始通知
    public Action OnStartGameReceived { get; set; }

    // ゲーム開始失敗通知（エラー理由）
    public Action<string> OnStartGameError { get; set; }


    // ルームに接続してるかどうか
    public bool IsJoined { get; private set; }

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
        IsJoined = false;

        if (roomHub != null) await roomHub.DisposeAsync();
        if (channel != null) await channel.ShutdownAsync();
        roomHub = null; channel = null;
    }

    //　破棄処理 
    async void OnDestroy()
    {
        DisconnectAsync();
    }

    #region Join/Leave
    //　入室
    public async UniTask JoinAsync(string roomName, int userId)
    {
        JoinedUser[] users = await roomHub.JoinAsync(roomName, userId);

        IsJoined = true;

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
        IsJoined = false;
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
    #endregion

    #region Move/Shoot
    //位置・回転を送信する
    public async UniTask MoveAsync(Vector3 position, Quaternion rotation)
    {
        await roomHub.MoveAsync(position, rotation);
    }

    // 他プレイヤーの移動通知
    public void OnMove(Guid connectionId, Vector3 position, Quaternion rotation)
    {
        if (OnMoveCharacter != null)
        {
            OnMoveCharacter(connectionId, position, rotation);
        }
    }

    // サーバーに自分が撃った弾の情報を送信
    public async UniTask ShootAsync(Vector3 pos, Quaternion rot, Vector3 velocity)
    {
        await roomHub.ShootAsync(pos, rot, velocity);
    }

    // サーバーから他プレイヤーの弾情報を受信
    public void OnShoot(Guid shooterId, Vector3 pos, Quaternion rot, Vector3 velocity)
    {
        if (OnBulletReceived != null)
        {
            OnBulletReceived(shooterId, pos, rot, velocity);
        }
    }
    #endregion

    #region Ready/Start
    // サーバーにReady状態を送信
    public async UniTask SetReadyAsync(bool ready)
    {
        if (roomHub == null || !IsJoined) return;
        await roomHub.SetReadyAsync(ready);
    }

    // サーバーにゲーム開始要求（ホスト用）
    public async UniTask StartGameAsync()
    {
        try
        {
            await roomHub.StartGameAsync();
        }
        catch (RpcException ex)
        {
            OnStartGameError?.Invoke(ex.Status.Detail);
        }
    }


    // サーバーからReady状態通知を受信
    public void OnPlayerReadyStatusChanged(Guid connectionId, bool isReady)
    {
        OnPlayerReadyStatusChangedReceived?.Invoke(connectionId, isReady);
    }

    // サーバーからゲーム開始通知を受信
    public void OnStartGame()
    {
        OnStartGameReceived?.Invoke();
    }
    #endregion
}
