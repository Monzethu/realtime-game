using UnityEngine;
using System.Collections;

public class PlayerHP : Damageable
{
    [SerializeField] private Vector3 respawnPosition = new Vector3(0, 0, 0);

    private PlayerContoroller controller;
    private PlayerPOV pov;

    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<PlayerContoroller>();
        pov = GetComponent<PlayerPOV>();
    }

    protected override void Die()
    {
        Debug.Log($"{gameObject.name} died");

        // 操作停止
        if (controller != null) controller.enabled = false;
        if (pov != null) pov.enabled = false;

        // 2秒後にリスポーン
        StartCoroutine(RespawnAfterDelay(2f));
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // HP回復
        ResetHP();

        // スポーン位置
        if (GameController.Instance != null)
            transform.position = GameController.Instance.GetSpawnPosition();
        else
            transform.position = respawnPosition;

        transform.rotation = Quaternion.identity;

        // 操作復帰
        if (controller != null) controller.enabled = true;
        if (pov != null) pov.enabled = true;
    }
}
