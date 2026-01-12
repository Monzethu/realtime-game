using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [Header("Battle")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletsParent;
    private RoomModel roomModel;

    [SerializeField] private float shotSpeed = 30f;
    [SerializeField] private int maxBullet = 30;

    private int bulletAmount;
    private float shotInterval;
    private Transform cameraTransform;

    private void Awake()
    {
        bulletAmount = maxBullet;
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    // ★ 新方式（GameController用）
    public void Initialize(RoomModel model, GameObject bullet, Transform parent)
    {
        roomModel = model;
        bulletPrefab = bullet;
        bulletsParent = parent;

        RegisterRoomEvents();
    }

    // ★ 旧方式（BattleGameDirector用）
    public void SetRoomModel(RoomModel model)
    {
        roomModel = model;
        RegisterRoomEvents();
    }

    private void RegisterRoomEvents()
    {
        if (roomModel != null)
        {
            roomModel.OnBulletReceived -= OnOtherPlayerShoot;
            roomModel.OnBulletReceived += OnOtherPlayerShoot;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            shotInterval += Time.deltaTime;

            if (shotInterval >= 0.05f && bulletAmount > 0)
            {
                shotInterval = 0f;
                bulletAmount--;
                ShootLocal();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            bulletAmount = maxBullet;
        }
    }

    private void ShootLocal()
    {
        if (bulletPrefab == null || cameraTransform == null)
            return;

        Vector3 spawnPos = cameraTransform.position + cameraTransform.forward * 0.5f;
        Quaternion rot = Quaternion.LookRotation(cameraTransform.forward);
        Vector3 velocity = cameraTransform.forward * shotSpeed;

        GameObject bullet = Instantiate(
            bulletPrefab,
            spawnPos,
            rot,
            bulletsParent
        );

        if (bullet.TryGetComponent(out Rigidbody rb))
            rb.linearVelocity = velocity;

        Destroy(bullet, 3f);

        if (roomModel != null && roomModel.IsJoined)
        {
            roomModel.ShootAsync(spawnPos, rot, velocity).Forget();
        }
    }

    private void OnOtherPlayerShoot(Guid shooterId, Vector3 pos, Quaternion rot, Vector3 velocity)
    {
        if (roomModel == null) return;
        if (shooterId == roomModel.ConnectionId) return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            pos,
            rot,
            bulletsParent
        );

        if (bullet.TryGetComponent(out Rigidbody rb))
            rb.linearVelocity = velocity;

        Destroy(bullet, 3f);
    }
}
