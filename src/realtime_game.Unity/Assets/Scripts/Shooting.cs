using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shotSpeed;
    [SerializeField] private int bulletAmount;
    [SerializeField] private int maxBullet;
    private float shotInterval;

    [SerializeField] public Transform bulletsParent;
    [SerializeField] public Transform shootingTransform;
    private Transform cameraTransform;

    private void Awake()
    {
        bulletAmount = maxBullet;
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            shotInterval += Time.deltaTime;

            if (shotInterval >= 0.05f && bulletAmount > 0)
            {
                bulletAmount -= 1;
                shotInterval = 0;

                // 弾の生成（カメラの向いている方向に発射）
                GameObject bullet = Instantiate(bulletPrefab, cameraTransform.position + cameraTransform.forward*0.5f, Quaternion.Euler(cameraTransform.eulerAngles.x, cameraTransform.eulerAngles.y, 0), bulletsParent);
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                bulletRb.AddForce(cameraTransform.forward * shotSpeed);

                //射撃されてから3秒後に銃弾のオブジェクトを破壊する
                Destroy(bullet, 3.0f);
            }

        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            bulletAmount = maxBullet;
        }
    }
}