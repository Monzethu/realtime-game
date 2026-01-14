//using UnityEngine;
//using System.Collections;

//public class PlayerHP : Damageable
//{
//    [SerializeField] private Vector3 respawnPosition = new Vector3(0, 1, 0); // 高さ1に変更

//    private PlayerContoroller controller;
//    private PlayerPOV pov;

//    protected override void Awake()
//    {
//        base.Awake();
//        controller = GetComponent<PlayerContoroller>();
//        pov = GetComponent<PlayerPOV>();
//    }

//    // 弾から呼ぶ用
//    public void TakeBulletDamage()
//    {
//        TakeDamage(2); // 弾は固定で2ダメージ
//    }

//    protected override void Die()
//    {
//        Debug.Log($"{gameObject.name} died");

//        // 自分だけ操作停止
//        if (IsLocalPlayer())
//        {
//            if (controller != null) controller.enabled = false;
//            if (pov != null) pov.enabled = false;

//            StartCoroutine(RespawnAfterDelay(3f)); // 3秒後リスポーン
//        }

//        // 他プレイヤーは見た目だけ
//        else
//        {
//            // TODO: 死亡エフェクトなど
//        }
//    }

//    private IEnumerator RespawnAfterDelay(float delay)
//    {
//        yield return new WaitForSeconds(delay);

//        ResetHP();

//        Vector3 spawnPos = respawnPosition;
//        if (GameDirector.Instance != null)
//            spawnPos = GameDirector.Instance.GetSpawnPosition();

//        transform.position = spawnPos;
//        transform.rotation = Quaternion.identity;

//        if (controller != null) controller.enabled = true;
//        if (pov != null) pov.enabled = true;
//    }

//    private bool IsLocalPlayer()
//    {
//        return GameDirector.Instance != null &&
//               GameDirector.Instance.MyUserId == GetUserId();
//    }

//    private int GetUserId()
//    {
//        var pc = GetComponent<PlayerContoroller>();
//       return pc != null ? pc.UserID : -1;
//    }
//}
