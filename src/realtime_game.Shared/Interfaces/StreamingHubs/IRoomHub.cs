using MagicOnion;
using realtime_game.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using System;
using System.Threading.Tasks;
using UnityEngine;

public interface IRoomHub : IStreamingHub<IRoomHub, IRoomHubReceiver>
{
    Task<JoinedUser[]> JoinAsync(string roomName);
    Task LeaveAsync();
    Task<Guid> GetConnectionId();
    Task MoveAsync(Vector3 pos, Quaternion rot);
    Task ShootAsync(Vector3 pos, Quaternion rot, Vector3 velocity);
    Task SetReadyAsync(bool ready);
    Task StartGameAsync();
}
