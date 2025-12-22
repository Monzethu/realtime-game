using MagicOnion.Server.Hubs;
using realtime_game.Server.Models.Contexts;
using realtime_game.Shared.Models.Entities;
using realtime_game.Shared.Interfaces.StreamingHubs;
using Shared.Interfaces.StreamingHubs;
using UnityEngine;
using Cysharp.Runtime.Multicast;

namespace Server.StreamingHubs
{
    public class RoomHub(RoomContextRepository roomContextRepository) : StreamingHubBase<IRoomHub, IRoomHubReceiver>, IRoomHub
    {
        private RoomContextRepository roomContextRepos;
        private RoomContext roomContext;

        // ルームに接続
        public async Task<JoinedUser[]> JoinAsync(string roomName, int userId)
        {
            // 同時に生成しないように排他制御
            lock (roomContextRepos)
            {
                // 指定の名前のルームがあるかどうかを確認
                this.roomContext = roomContextRepos.GetContext(roomName);
                if (this.roomContext == null)
                { // 無かったら生成
                    this.roomContext = roomContextRepos.CreateContext(roomName);
                    Console.WriteLine("ルームが生成されました");
                }
            }

            // ルームに参加 & ルームを保持
            this.roomContext.Group.Add(this.ConnectionId, Client);

            // DBからユーザー情報取得
            GameDbContext context = new GameDbContext();
            User user = context.Users.Where(user => user.Id == userId).First();

            // 入室済みユーザーのデータを作成
            var joinedUser = new JoinedUser();
            joinedUser.ConnectionId = this.ConnectionId;
            joinedUser.UserData = user;

            // ルームコンテキストにユーザー情報を登録
            var roomUserData = new RoomUserData() { JoinedUser = joinedUser };
            this.roomContext.RoomUserDataList[ConnectionId] = roomUserData;
            Console.WriteLine($"ルームに参加しました。ID：{roomUserData.JoinedUser.UserData.Id}名前：{roomUserData.JoinedUser.UserData.Name}");

            // 自分以外のルーム参加者全員に、ユーザーの入室通知を送信
            this.roomContext.Group.Except([this.ConnectionId]).OnJoin(joinedUser);

            // 入室リクエストをしたユーザーに、参加者の情報をリストで返す
            return this.roomContext.RoomUserDataList.Select(
                f => f.Value.JoinedUser).ToArray();
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
            this.roomContext.Group.Except([ this.ConnectionId ])
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
    }
}
