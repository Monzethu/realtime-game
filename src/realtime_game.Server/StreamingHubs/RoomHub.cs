using MagicOnion.Server.Hubs;
using realtime_game.Server.Models.Contexts;
using realtime_game.Shared.Models.Entities;
using realtime_game.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using UnityEngine;
using Cysharp.Runtime.Multicast;
using Grpc.Core;

namespace Server.StreamingHubs
{
    public class RoomHub : StreamingHubBase<IRoomHub, IRoomHubReceiver>, IRoomHub
    {
        private readonly RoomContextRepository roomContextRepos;
        private RoomContext roomContext;

        public RoomHub(RoomContextRepository roomContextRepository)
        {
            roomContextRepos = roomContextRepository;
        }

        public async Task<JoinedUser[]> JoinAsync(string roomName, string token)
        {
            roomContext =
                roomContextRepos.GetContext(roomName)
                ?? roomContextRepos.CreateContext(roomName);

            roomContext.Group.Add(ConnectionId, Client);

            using var context = new GameDbContext();
            var user = context.Users.FirstOrDefault(u => u.Token == token);

            if (user == null)
                throw new RpcException(
                    new Status(StatusCode.PermissionDenied, "INVALID_TOKEN")
                );

            int joinOrder = roomContext.RoomUserDataList.Count;

            var joinedUser = new JoinedUser
            {
                ConnectionId = ConnectionId,
                UserData = user,
                JoinOrder = joinOrder
            };

            roomContext.RoomUserDataList[ConnectionId] = new RoomUserData
            {
                JoinedUser = joinedUser,
                IsReady = false
            };

            roomContext.Group.Except([ConnectionId]).OnJoin(joinedUser);

            return roomContext.RoomUserDataList.Values
                .Select(x => x.JoinedUser)
                .ToArray();
        }

        protected override ValueTask OnDisconnected()
        {
            LeaveAsync();
            return default;
        }

        public Task<Guid> GetConnectionId()
        {
            return Task.FromResult(ConnectionId);
        }

        public Task LeaveAsync()
        {
            if (roomContext == null) return Task.CompletedTask;

            if (!roomContext.RoomUserDataList.TryGetValue(ConnectionId, out var userData))
                return Task.CompletedTask;

            roomContext.Group.All.OnLeave(ConnectionId);

            roomContext.Group.Remove(ConnectionId);
            roomContext.RoomUserDataList.Remove(ConnectionId);

            if (roomContext.RoomUserDataList.Count == 0)
            {
                roomContextRepos.RemoveContext(roomContext.Name);
            }

            return Task.CompletedTask;
        }

        public Task MoveAsync(Vector3 pos, Quaternion rot)
        {
            if (roomContext == null) return Task.CompletedTask;

            var userData = roomContext.RoomUserDataList[ConnectionId];
            userData.Position = pos;
            userData.Rotation = rot;

            roomContext.Group
                .Except([ConnectionId])
                .OnMove(ConnectionId, pos, rot);

            return Task.CompletedTask;
        }

        public Task ShootAsync(Vector3 pos, Quaternion rot, Vector3 velocity)
        {
            roomContext?.Group
                .Except(ConnectionId)
                .OnShoot(ConnectionId, pos, rot, velocity);

            return Task.CompletedTask;
        }

        public Task SetReadyAsync(bool ready)
        {
            if (roomContext == null) return Task.CompletedTask;

            roomContext.RoomUserDataList[ConnectionId].IsReady = ready;
            roomContext.Group.All.OnPlayerReadyStatusChanged(ConnectionId, ready);

            return Task.CompletedTask;
        }

        public Task StartGameAsync()
        {
            var myData = roomContext.RoomUserDataList[ConnectionId];

            if (myData.JoinedUser.JoinOrder != 0)
            {
                throw new RpcException(
                    new Status(StatusCode.PermissionDenied, "NOT_HOST")
                );
            }

            if (!roomContext.RoomUserDataList.Values.All(u => u.IsReady))
            {
                throw new RpcException(
                    new Status(StatusCode.FailedPrecondition, "NOT_ALL_READY")
                );
            }

            roomContext.Group.All.OnStartGame();
            return Task.CompletedTask;
        }
    }
}
