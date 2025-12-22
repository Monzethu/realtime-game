using DG.Tweening;
using Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;

// Playerの移動
public class PlayerContoroller : MonoBehaviour
{
    Rigidbody rb;
    
    [SerializeField] private float moveSpeed = 5f;           // 移動速度
    float jumpPower=5f;    // ジャンプ力

    bool isGround;        // 地面に着地しているかどうかのフラグ変数

    [SerializeField] public Camera cam;
    
    FloatingJoystick joystick;

    // HPの実装

    private void Awake()
    {
        isGround = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb= GetComponent<Rigidbody>();

        // ジョイスティックの情報を取得
        joystick = GameObject.Find("Floating Joystick").GetComponent<FloatingJoystick> ();

    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Jump();
    }

    // 移動
    private void Move()
    {
        // WASD / キーボード入力
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // ジョイスティック入力
        float jh = joystick != null ? joystick.Horizontal : 0f;
        float jv = joystick != null ? joystick.Vertical : 0f;

        // 入力を足す
        float moveX = (h + jh) * moveSpeed * Time.deltaTime;
        float moveZ = (v + jv) * moveSpeed * Time.deltaTime;

        // 実際の移動
        transform.Translate(moveX, 0, moveZ);
    }


    void Jump()
    {
        if (isGround)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.AddForce(Vector3.up*jumpPower, ForceMode.Impulse);
            }
        }
    }

    // 地面についたら（Floorについてたら）
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("floor"))
        {
            isGround=true;
        }
    }

    // 地面からはなれたら（Floorからはなれたら）
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("floor"))
        {
            isGround=false;
        }
    }
}

