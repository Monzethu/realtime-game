using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicOnion;
using Shared.Interfaces.StreamingHubs;

namespace realtime_game.Shared.Interfaces.StreamingHubs
{
    /// <summary>
    /// サーバーからクライアントへの通知関連
    /// </summary>
    public interface IRoomHubReceiver
    {
        // [クライアントに実装]
        // [サーバーから呼び出す]

        // ユーザーの入室通知
        void OnJoin(JoinedUser user);

        // ユーザー退出通知
        void OnLeave(JoinedUser user);
    }

}
