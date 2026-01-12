using UnityEngine;
using System.Collections;

public class PlayerHP : Damageable
{
    [SerializeField] private Vector3 respawnPosition = Vector3.zero;
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private float invincibleTime = 1.5f;

    private PlayerContoroller controller;
    private PlayerPOV pov;
    private Collider[] colliders;

    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<PlayerContoroller>();
        pov = GetComponent<PlayerPOV>();
        colliders = GetComponentsInChildren<Collider>();
    }

    protected override void Die()
    {
        Debug.Log($"{gameObject.name} died");

        // “ñd€–S–h~
        if (!gameObject.activeInHierarchy) return;

        // ‘€ì’â~
        if (controller != null) controller.enabled = false;
        if (pov != null) pov.enabled = false;

        // “–‚½‚è”»’èOFF
        SetColliders(false);

        StartCoroutine(RespawnAfterDelay(respawnDelay));
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // HP‰ñ•œ
        ResetHP();

        // ƒXƒ|[ƒ“ˆÊ’u
        if (GameController.Instance != null)
            transform.position = GameController.Instance.GetSpawnPosition();
        else
            transform.position = respawnPosition;

        transform.rotation = Quaternion.identity;

        // ‘€ì•œ‹A
        if (controller != null) controller.enabled = true;
        if (pov != null) pov.enabled = true;

        // –³“GŠÔ
        yield return new WaitForSeconds(invincibleTime);

        SetColliders(true);
    }

    private void SetColliders(bool enabled)
    {
        foreach (var col in colliders)
        {
            col.enabled = enabled;
        }
    }
}
