using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using MagicOnion;
using realtime_game.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;
using Grpc.Core;

public class RoomModel : BaseModel, IRoomHubReceiver
{
    private GrpcChannelx channel;
    private IRoomHub roomHub;

    // 接続ID
    public Guid ConnectionId { get; set; }

    // 現在入室しているルーム名
    public string CurrentRoomName { get; private set; } = "";

    // イベント
    public Action<JoinedUser> OnJoinedUser { get; set; }
    public Action<Guid> OnLeftUser { get; set; }
    public Action<Guid, Vector3, Quaternion> OnMoveCharacter { get; set; }
    public Action<Guid, Vector3, Quaternion, Vector3> OnBulletReceived { get; set; }
    public Action<Guid, bool> OnPlayerReadyStatusChangedReceived { get; set; }
    public Action OnStartGameReceived { get; set; }
    public Action<string> OnStartGameError { get; set; }

    public bool IsJoined { get; private set; }

    public async UniTask ConnectAsync()
    {
        channel = GrpcChannelx.ForAddress(ServerURL);
        roomHub = await StreamingHubClient.ConnectAsync<IRoomHub, IRoomHubReceiver>(channel, this);
        ConnectionId = await roomHub.GetConnectionId();
    }

    public async UniTask DisconnectAsync()
    {
        IsJoined = false;
        if (roomHub != null) await roomHub.DisposeAsync();
        if (channel != null) await channel.ShutdownAsync();
        roomHub = null;
        channel = null;
    }

    async void OnDestroy()
    {
        await DisconnectAsync();
    }

    #region Join/Leave
    public async UniTask JoinAsync(string roomName)
    {
        CurrentRoomName = roomName;
        JoinedUser[] users = await roomHub.JoinAsync(roomName);
        IsJoined = true;

        foreach (var user in users)
        {
            OnJoinedUser?.Invoke(user);
        }
    }



    public void OnJoin(JoinedUser user)
    {
        OnJoinedUser?.Invoke(user);
    }

    public async UniTask LeaveAsync()
    {
        await roomHub.LeaveAsync();
        IsJoined = false;
        CurrentRoomName = ""; // ←退室時にリセット
        Debug.Log("退室完了");
    }

    public void OnLeave(Guid connectionId)
    {
        OnLeftUser?.Invoke(connectionId);
    }
    #endregion

    #region Move/Shoot
    public async UniTask MoveAsync(Vector3 position, Quaternion rotation)
    {
        await roomHub.MoveAsync(position, rotation);
    }

    public void OnMove(Guid connectionId, Vector3 position, Quaternion rotation)
    {
        OnMoveCharacter?.Invoke(connectionId, position, rotation);
    }

    public async UniTask ShootAsync(Vector3 pos, Quaternion rot, Vector3 velocity)
    {
        await roomHub.ShootAsync(pos, rot, velocity);
    }

    public void OnShoot(Guid shooterId, Vector3 pos, Quaternion rot, Vector3 velocity)
    {
        OnBulletReceived?.Invoke(shooterId, pos, rot, velocity);
    }
    #endregion

    #region Ready/Start
    public async UniTask SetReadyAsync(bool ready)
    {
        if (roomHub == null || !IsJoined) return;
        await roomHub.SetReadyAsync(ready);
    }

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

    public void OnPlayerReadyStatusChanged(Guid connectionId, bool isReady)
    {
        OnPlayerReadyStatusChangedReceived?.Invoke(connectionId, isReady);
    }

    public void OnStartGame()
    {
        OnStartGameReceived?.Invoke();
    }
    #endregion
}
