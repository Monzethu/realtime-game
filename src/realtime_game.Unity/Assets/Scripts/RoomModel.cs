using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using MagicOnion;
using Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;
using Grpc.Core;
using realtime_game.Shared.Interfaces.StreamingHubs;

public class RoomModel : BaseModel, IRoomHubReceiver
{
    public static RoomModel Instance { get; private set; }

    private GrpcChannelx channel;
    private IRoomHub roomHub;

    public Guid ConnectionId { get; private set; }

    public Action<JoinedUser> OnJoinedUser;
    public Action<Guid> OnLeftUser;
    public Action<Guid, Vector3, Quaternion> OnMoveCharacter;
    public Action<Guid, Vector3, Quaternion, Vector3> OnBulletReceived;

    public Action<Guid, bool> OnPlayerReadyStatusChangedReceived;
    public Action OnStartGameReceived;
    public Action<string> OnStartGameError;

    public bool IsJoined { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // =========================
    // Connect / Disconnect
    // =========================

    public async UniTask ConnectAsync()
    {
        if (roomHub != null) return;

        channel = GrpcChannelx.ForAddress(ServerURL);
        roomHub = await StreamingHubClient
            .ConnectAsync<IRoomHub, IRoomHubReceiver>(channel, this);

        ConnectionId = await roomHub.GetConnectionId();
    }

    public async UniTask DisconnectAsync()
    {
        IsJoined = false;

        if (roomHub != null)
        {
            await roomHub.DisposeAsync();
            roomHub = null;
        }

        if (channel != null)
        {
            await channel.ShutdownAsync();
            channel = null;
        }
    }

    private async void OnDestroy()
    {
        await DisconnectAsync();
    }

    // =========================
    // Join / Leave
    // =========================

    public async UniTask JoinAsync(string roomName, int userId)
    {
        if (IsJoined) return;

        await ConnectAsync();

        JoinedUser[] users = await roomHub.JoinAsync(roomName, userId);
        IsJoined = true;

        foreach (var user in users)
        {
            OnJoinedUser?.Invoke(user);
        }
    }

    public async UniTask LeaveAsync()
    {
        if (!IsJoined) return;

        await roomHub.LeaveAsync();
        IsJoined = false;
    }

    public void OnJoin(JoinedUser user)
    {
        OnJoinedUser?.Invoke(user);
    }

    public void OnLeave(Guid connectionId)
    {
        OnLeftUser?.Invoke(connectionId);
    }

    // =========================
    // Move / Shoot
    // =========================

    public async UniTask MoveAsync(Vector3 position, Quaternion rotation)
    {
        if (!IsJoined) return;
        await roomHub.MoveAsync(position, rotation);
    }

    public void OnMove(Guid connectionId, Vector3 position, Quaternion rotation)
    {
        OnMoveCharacter?.Invoke(connectionId, position, rotation);
    }

    public async UniTask ShootAsync(Vector3 pos, Quaternion rot, Vector3 velocity)
    {
        if (!IsJoined) return;
        await roomHub.ShootAsync(pos, rot, velocity);
    }

    public void OnShoot(Guid shooterId, Vector3 pos, Quaternion rot, Vector3 velocity)
    {
        OnBulletReceived?.Invoke(shooterId, pos, rot, velocity);
    }

    // =========================
    // Ready / Start
    // =========================

    public async UniTask SetReadyAsync(bool ready)
    {
        if (!IsJoined) return;
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
}
