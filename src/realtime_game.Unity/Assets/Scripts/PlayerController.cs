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

    // JoyStickで操作させたい

    private void Awake()
    {
        isGround = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb= GetComponent<Rigidbody>();
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
        // 入力の取得
        float h = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float v = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        transform.Translate(h, 0, v);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("floor"))
        {
            isGround=true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("floor"))
        {
            isGround=false;
        }
    }
}

