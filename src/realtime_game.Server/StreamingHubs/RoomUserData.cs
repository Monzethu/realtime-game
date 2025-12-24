using Shared.Interfaces.StreamingHubs;
using UnityEngine;

namespace Server.StreamingHubs
{
    // ルーム内のユーザー単体の情報
    public class RoomUserData
    {
        public JoinedUser JoinedUser;

        // 座標
        public Vector3 Position;
        
        // 回転
        public Quaternion Rotation;

        // 準備完了フラグ
        public bool IsReady = false;

        public int JoinOrder;
    }
}

