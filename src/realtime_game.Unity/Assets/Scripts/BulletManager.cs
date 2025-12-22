//using UnityEngine;

//// 弾の当たり判定（Playerに当たったら弾を消す）
//public class BulletManager : MonoBehaviour
//{
//    private void OnCollisionEnter(Collision bullet)
//    {
//        // Playerタグを判定して弾を消す
//        if (bullet.gameObject.CompareTag("Player"))
//        {
//            // 弾だけ削除（衝突した相手は消さない）
//            Destroy(gameObject);
//        }
//    }
//}

using System;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    // ===== 同期用 =====
    public Guid BulletId;      // 弾の一意ID
    public Guid ShooterId;     // 撃った人

    // ===== 設定 =====
    [SerializeField] private float lifeTime = 3f;

    private bool isDestroyed;

    void Start()
    {
        // 一定時間で消える（念のため）
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed) return;

        // Player 以外は無視
        if (!collision.gameObject.CompareTag("Player")) return;

        var player = collision.gameObject.GetComponent<PlayerIdentity>();
        if (player == null) return;

        // 自分が撃った弾が自分に当たった → 無視
        if (player.ConnectionId == ShooterId) return;

        // ヒット処理
        isDestroyed = true;

        // ここで「弾が当たった」ことを通知する
        NotifyHit(player.ConnectionId);

        Destroy(gameObject);
    }

    void NotifyHit(Guid hitPlayerId)
    {
        // 後で RoomModel / RoomHub 経由でサーバーに送る
        Debug.Log($"Hit! Bullet:{BulletId} Shooter:{ShooterId} Target:{hitPlayerId}");
    }
}
