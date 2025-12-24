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
    public class RoomHub(RoomContextRepository roomContextRepository) : StreamingHubBase<IRoomHub, IRoomHubReceiver>, IRoomHub
    {
        private RoomContextRepository roomContextRepos;
        private RoomContext roomContext;

        public async Task<JoinedUser[]> JoinAsync(string roomName, int userId)
        {
            lock (roomContextRepos)
            {
                this.roomContext = roomContextRepos.GetContext(roomName);
                if (this.roomContext == null)
                {
                    this.roomContext = roomContextRepos.CreateContext(roomName);
                    Console.WriteLine("ルームが生成されました");
                }
            }

            // グループに参加
            this.roomContext.Group.Add(this.ConnectionId, Client);

            // DBからユーザー情報取得
            GameDbContext context = new GameDbContext();
            User user = context.Users.First(u => u.Id == userId);

            // JoinOrder 決定
            int joinOrder = this.roomContext.RoomUserDataList.Count;

            // JoinedUser 作成
            var joinedUser = new JoinedUser
            {
                ConnectionId = this.ConnectionId,
                UserData = user,
                JoinOrder = joinOrder
            };

            // RoomUserData 作成して登録
            var roomUserData = new RoomUserData
            {
                JoinedUser = joinedUser,
                IsReady = false
            };

            this.roomContext.RoomUserDataList[this.ConnectionId] = roomUserData;

            Console.WriteLine(
                $"Join: UserId={user.Id}, Name={user.Name}, JoinOrder={joinOrder}"
            );

            // 自分以外に入室通知
            this.roomContext.Group
                .Except([this.ConnectionId])
                .OnJoin(joinedUser);

            // 現在の参加者一覧を返す
            return this.roomContext.RoomUserDataList
                .Select(x => x.Value.JoinedUser)
                .ToArray();
        }



        // 接続時の処理
        protected override ValueTask OnConnected()
        {
            roomContextRepos = roomContextRepository;
            return default;
        }

        // 切断時の処理
        protected override ValueTask OnDisconnected()
        {
            if (roomContext != null)
            {
                LeaveAsync();
            }
            return CompletedTask;
        }


        // 接続ID取得
        public Task<Guid> GetConnectionId()
        {
            return Task.FromResult<Guid>(this.ConnectionId);
        }


        // ルームから退出
        public Task LeaveAsync()
        {
            //　退室したことを全メンバーに通知
            this.roomContext.Group.All.OnLeave(this.ConnectionId);
            Console.WriteLine($"ルームから退出しました。ID：{roomContext.RoomUserDataList[ConnectionId].JoinedUser.UserData.Id}名前：{roomContext.RoomUserDataList[ConnectionId].JoinedUser.UserData.Name}");

            //　ルーム内のメンバーから自分を削除
            this.roomContext.Group.Remove(this.ConnectionId);

            //　ルームデータから退室したユーザーを削除
            this.roomContext.RoomUserDataList.Remove(this.ConnectionId);

            // ルーム内にユーザーが一人もいなければルーム削除
            if (this.roomContext.RoomUserDataList.Count == 0)
            {
                roomContextRepos.RemoveContext(this.roomContext.Name);
                Console.WriteLine("ルームが削除されました");
            }

            return Task.CompletedTask;
        }

        // 移動
        public Task MoveAsync(Vector3 pos, Quaternion rot)
        {
            // 位置情報を記録
            //this.roomContext.RoomUserDataList[this.ConnectionId].pos = pos;

            var userData = this.roomContext.RoomUserDataList[this.ConnectionId];
            userData.Position = pos;
            userData.Rotation = rot;

            // 自分以外の全メンバーに通知
            this.roomContext.Group.Except([this.ConnectionId])
                .OnMove(this.ConnectionId, pos, rot);

            return Task.CompletedTask;
        }

        public Task ShootAsync(Vector3 pos, Quaternion rot, Vector3 velocity)
        {
            this.roomContext.Group
                .Except(this.ConnectionId)
                .OnShoot(this.ConnectionId, pos, rot, velocity);

            return Task.CompletedTask;
        }

        public Task SetReadyAsync(bool ready)
        {
            if (roomContext == null) return Task.CompletedTask;

            roomContext.RoomUserDataList[ConnectionId].IsReady = ready;

            // 必要に応じて全員にReady状態を通知する
            roomContext.Group.All.OnPlayerReadyStatusChanged(ConnectionId, ready);

            return Task.CompletedTask;
        }

        public Task StartGameAsync()
        {
            var myData = roomContext.RoomUserDataList[this.ConnectionId];

            // ホストチェック（JoinOrder 0 = ホスト）
            if (myData.JoinedUser.JoinOrder != 0)
            {
                throw new RpcException(
                    new Status(StatusCode.PermissionDenied, "NOT_HOST")
                );
            }

            // 全員Readyチェック
            if (!roomContext.RoomUserDataList.Values.All(u => u.IsReady))
            {
                throw new RpcException(
                    new Status(StatusCode.FailedPrecondition, "NOT_ALL_READY")
                );
            }

            // 全員にゲーム開始通知
            roomContext.Group.All.OnStartGame();

            return Task.CompletedTask;
        }
    }
}