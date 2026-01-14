using System;
using System.Collections;
using UnityEngine;

public class PlayerContoroller : MonoBehaviour
{
    // ===== �l�b�g���[�NID =====
    public Guid ConnectionId { get; private set; }

    public void SetConnectionId(Guid id)
    {
        ConnectionId = id;
    }

    // ===== �ړ� =====
    Rigidbody rb;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpPower = 5f;

    bool isGround;
    [SerializeField] public Camera cam;

    private FloatingJoystick joystick;

    // ===== HP =====
    [SerializeField] private int maxHp = 50;
    private int hp;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hp = maxHp;
        isDead = false;
    }

    private void Start()
    {
        joystick = GameObject.Find("Floating Joystick")?.GetComponent<FloatingJoystick>();
    }

    private void Update()
    {
        if (isDead) return;

        Move();
        Jump();
    }

    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float jh = joystick != null ? joystick.Horizontal : 0f;
        float jv = joystick != null ? joystick.Vertical : 0f;

        float moveX = (h + jh) * moveSpeed * Time.deltaTime;
        float moveZ = (v + jv) * moveSpeed * Time.deltaTime;

        transform.Translate(moveX, 0, moveZ);
    }

    private void Jump()
    {
        if (isGround && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("floor"))
            isGround = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("floor"))
            isGround = false;
    }

    // ===== ��e���� =====
    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        var bullet = other.GetComponent<BulletManager>();
        if (bullet == null) return;

        // �����̒e�͖���
        if (bullet.ShooterId == ConnectionId)
        {
            Debug.Log("�����̒e�ɓ��������i�����j");
            return;
        }

        Debug.Log(
            $"��e�I �������l:{bullet.ShooterId} �� ���������l:{ConnectionId}"
        );

        // �� �_���[�W
        hp -= 2;
        Debug.Log($"HP: {hp}");

        // �� �e�͑������i���d�v�j
        Destroy(bullet.gameObject);

        if (hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{ConnectionId} died");

        // �����~
        enabled = false;
        if (cam != null) cam.enabled = false;

        StartCoroutine(RespawnAfterDelay(3f));
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        hp = maxHp;
        isDead = false;

        // �X�|�[���ʒu
        if (GameDirector.Instance != null)
            transform.position = GameDirector.Instance.GetSpawnPosition();
        else
            transform.position = Vector3.zero;

        rb.linearVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;

        // ���앜�A
        enabled = true;
        if (cam != null) cam.enabled = true;

        Debug.Log("���X�|�[������");
    }
}
