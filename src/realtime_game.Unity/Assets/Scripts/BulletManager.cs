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

        // Damageable を探す（Player想定）
        var damageable = collision.gameObject.GetComponentInParent<Damageable>();
        if (damageable == null) return;

        // 自分が撃った弾が自分に当たった → 無視
        var player = collision.gameObject.GetComponentInParent<PlayerIdentity>();
        if (player != null && player.ConnectionId == ShooterId) return;

        isDestroyed = true;

        // ★ HPを減らす
        damageable.TakeDamage(10);

        Destroy(gameObject);
    }


    void NotifyHit(Guid hitPlayerId)
    {
        // 後で RoomModel / RoomHub 経由でサーバーに送る
        Debug.Log($"Hit! Bullet:{BulletId} Shooter:{ShooterId} Target:{hitPlayerId}");
    }
}
