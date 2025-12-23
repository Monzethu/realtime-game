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

        // 位置・回転をクライアントに通知する
        void OnMove(Guid connectionId, Vector3 pos, Quaternion rot);

        // 撃った情報を送る
        void OnShoot(Guid shooterId, Vector3 pos, Quaternion rot, Vector3 velocity);

        void OnStartGame(); // クライアントでシーン遷移

        void OnPlayerReadyStatusChanged(Guid connectionId, bool isReady);
    }
}
