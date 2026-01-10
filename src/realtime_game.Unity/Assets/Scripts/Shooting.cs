using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shotSpeed = 30f;

    [SerializeField] private int maxBullet = 30;
    private int bulletAmount;
    private float shotInterval;

    private Transform bulletsParent;
    private Transform cameraTransform;

    private RoomModel roomModel;

    private void Awake()
    {
        bulletAmount = maxBullet;
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        bulletsParent = GameObject.Find("BulletsParent")?.transform;
    }

    public void SetRoomModel(RoomModel model)
    {
        // 二重登録防止
        if (roomModel != null)
        {
            roomModel.OnBulletReceived -= OnOtherPlayerShoot;
        }

        roomModel = model;
        roomModel.OnBulletReceived += OnOtherPlayerShoot;
    }

    private void OnDestroy()
    {
        // 破棄時に必ず解除
        if (roomModel != null)
        {
            roomModel.OnBulletReceived -= OnOtherPlayerShoot;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            shotInterval += Time.deltaTime;

            if (shotInterval >= 0.05f && bulletAmount > 0)
            {
                bulletAmount--;
                shotInterval = 0f;
                ShootLocal();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            bulletAmount = maxBullet;
        }
    }

    /// <summary>
    /// 自分の弾をローカル生成 → サーバーに通知
    /// </summary>
    private void ShootLocal()
    {
        if (cameraTransform == null || bulletsParent == null)
            return;

        Vector3 spawnPos = cameraTransform.position + cameraTransform.forward * 0.5f;
        Quaternion rot = Quaternion.Euler(
            cameraTransform.eulerAngles.x,
            cameraTransform.eulerAngles.y,
            0f
        );
        Vector3 velocity = cameraTransform.forward * shotSpeed;

        // 自分の弾はローカルで生成
        SpawnBullet(spawnPos, rot, velocity);

        // 参加中なら同期
        if (roomModel != null && roomModel.IsJoined)
        {
            roomModel.ShootAsync(spawnPos, rot, velocity).Forget();
        }
    }

    /// <summary>
    /// 他プレイヤーの弾を受信して生成
    /// </summary>
    private void OnOtherPlayerShoot(Guid shooterId, Vector3 pos, Quaternion rot, Vector3 velocity)
    {
        if (roomModel == null) return;

        // ★ 重要：自分の弾は無視
        if (shooterId == roomModel.ConnectionId) return;

        SpawnBullet(pos, rot, velocity);
    }

    private void SpawnBullet(Vector3 pos, Quaternion rot, Vector3 velocity)
    {
        if (bulletsParent == null) return;

        GameObject bullet = Instantiate(bulletPrefab, pos, rot, bulletsParent);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = velocity;
        Destroy(bullet, 3f);
    }
}
