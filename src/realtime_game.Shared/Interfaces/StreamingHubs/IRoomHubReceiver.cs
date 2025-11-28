using System;
using MagicOnion;
using Shared.Interfaces.StreamingHubs;
using UnityEngine;

namespace realtime_game.Shared.Interfaces.StreamingHubs
{
    public interface IRoomHubReceiver
    {
        // [クライアントに実装]
        // [サーバーから呼び出す]

        // ユーザーの入室通知
        void OnJoin(JoinedUser user);

        // ユーザーの退室通知
        void OnLeave(Guid connectionId);

        ////位置・回転をクライアントに通知する
        //void OnMove(接続ID, 位置, 回転);
    }
}
