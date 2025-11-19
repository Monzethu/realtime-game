using MagicOnion;
using realtime_game.Shared.Interfaces.StreamingHubs;
using System;
using System.Threading.Tasks;
namespace Shared.Interfaces.StreamingHubs
{
    /// <summary>
    /// クライアントから呼び出す処理を実装するクラス用インターフェース
    /// </summary>
    public interface IRoomHub : IStreamingHub<IRoomHub, IRoomHubReceiver>
    {
        // [サーバーに実装]
        // [クライアントから呼び出す]

        // ユーザー入室
        Task<JoinedUser[]> JoinAsync(string roomName, int userId);

        Task<Guid> GetConnectionId();

        // ユーザー退出
        Task LeaveAsync();
    }
}
