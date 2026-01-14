using System;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    // ===== 同期用 =====
    public Guid BulletId;      // 弾の一意ID
    public Guid ShooterId;     // 撃った人

    // ===== 設定 =====
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float ignoreTime = 0.1f; // ★ 追加：生成直後は無視

    private float timer;
    private bool isDestroyed;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDestroyed) return;

        // ★ 生成直後は誰にも当たらない
        if (timer < ignoreTime) return;

        var player = other.GetComponentInParent<PlayerContoroller>();
        if (player == null) return;

        // Player 側で処理するので、ここでは消すだけ
        isDestroyed = true;
        Destroy(gameObject);
    }
}
