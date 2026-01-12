using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Battle")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletsParent;

    [Header("Game Rule")]
    [SerializeField] private float matchTime = 30f;

    [Header("Spawn")]
    [SerializeField] private Vector3[] spawnPoints;

    [Header("UI")]
    [SerializeField] private GameObject resultPanel;

    private float timer;
    private bool isGameRunning;

    public Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        timer = matchTime;
        isGameRunning = true;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        RegisterExistingPlayer();
    }

    private void Update()
    {
        if (!isGameRunning) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            EndGame();
        }
    }

    /// <summary>
    /// ★ これが無かった
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int index = Random.Range(0, spawnPoints.Length);
            return spawnPoints[index];
        }

        // 保険（スポーンポイント未設定時）
        return new Vector3(
            Random.Range(-5f, 5f),
            0f,
            Random.Range(-5f, 5f)
        );
    }

    private void RegisterExistingPlayer()
    {
        var player = FindObjectOfType<PlayerRoot>();
        if (player == null)
        {
            //Debug.LogError("[GameController] PlayerRoot not found");
            return;
        }

        players["LocalPlayer"] = player.gameObject;

        var shooting = player.GetComponentInChildren<Shooting>();
        if (shooting == null)
        {
            //Debug.LogError("[GameController] Shooting not found");
            return;
        }

        var roomModel = GameDirector.Instance?.GetComponent<RoomModel>();
        if (roomModel == null)
        {
            //Debug.LogError("[GameController] RoomModel not found");
            return;
        }

        shooting.Initialize(roomModel, bulletPrefab, bulletsParent);
    }

    private void EndGame()
    {
        isGameRunning = false;
        Debug.Log("Match End");

        if (resultPanel != null)
            resultPanel.SetActive(true);
    }
}
