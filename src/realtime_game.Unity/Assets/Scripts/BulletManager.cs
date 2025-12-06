using UnityEngine;

// 弾の当たり判定（Playerに当たったら弾を消す）
public class BulletManager : MonoBehaviour
{
    private void OnCollisionEnter(Collision bullet)
    {
        // Playerタグを判定して弾を消す
        if (bullet.gameObject.CompareTag("Player"))
        {
            // 弾だけ削除（衝突した相手は消さない）
            Destroy(gameObject);
        }
    }
}
