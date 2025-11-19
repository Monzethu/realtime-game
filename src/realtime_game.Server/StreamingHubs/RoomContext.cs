using Cysharp.Runtime.Multicast;
using realtime_game.Shared.Interfaces.StreamingHubs;

namespace Server.StreamingHubs
{
    public class RoomContext : IDisposable
    {
        public Guid Id { get; } // ルームID
        public string Name { get; } // ルーム名
        public IMulticastSyncGroup<Guid, IRoomHubReceiver> Group { get; } // グループ
        public Dictionary<Guid, RoomUserData> RoomUserDataList { get; } =
            new Dictionary<Guid, RoomUserData>(); // ユーザデータ一覧

        //その他、ルームのデータとして保存したいものをフィールドに追加していく

        // コンストラクタ
        public RoomContext(IMulticastGroupProvider groupProvider, string roomName)
        {
            Id = Guid.NewGuid();    //ルーム毎のデータにIDを付けておく
            Name = roomName;        //ルーム名をフィールドに保存

            if (groupProvider == null)
            {
                throw new ArgumentNullException(nameof(groupProvider));
            }

            // グループを作成してプロパティに代入
            Group = groupProvider.GetOrAddSynchronousGroup<Guid, IRoomHubReceiver>(roomName);
        }

        public void Dispose()
        {
            Group.Dispose();
        }
    }
}
