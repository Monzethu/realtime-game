using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;

public class GameController : MonoBehaviour
{
    // LobbySceneで新規生成したPlayerをBattleSceneで再利用

    public static GameController Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Game Rule")]
    [SerializeField] private float matchTime = 30f;

    [Header("UI")]
    [SerializeField] private GameObject resultPanel;

    private float timer;
    private bool isGameRunning;

    // プレイヤー管理
    public Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        timer = matchTime;
        isGameRunning = true;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        // ★ 既存Playerを探して登録（Lobby → Battle 対応）
        RegisterExistingPlayer();
    }

    void Update()
    {
        if (!isGameRunning) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            EndGame();
        }
    }

    // ★ Lobby用：Playerがいなければ生成
    public GameObject SpawnPlayerIfNeeded(string playerId)
    {
        if (players.ContainsKey(playerId))
        {
            return players[playerId];
        }

        GameObject player = FindObjectOfType<PlayerRoot>()?.gameObject;

        if (player == null)
        {
            player = Instantiate(
                playerPrefab,
                GetSpawnPosition(),
                Quaternion.identity
            );
        }

        players[playerId] = player;
        return player;
    }

    // ★ BattleScene用：既存Playerを登録するだけ
    private void RegisterExistingPlayer()
    {
        var player = FindObjectOfType<PlayerRoot>();
        if (player != null)
        {
            players["LocalPlayer"] = player.gameObject;
        }
    }

    // 試合終了
    private void EndGame()
    {
        isGameRunning = false;
        Debug.Log("Match End");

        if (resultPanel != null)
            resultPanel.SetActive(true);
    }

    // スポーン位置
    public Vector3 GetSpawnPosition()
    {
        return new Vector3(
            Random.Range(-5f, 5f),
            0f,
            Random.Range(-5f, 5f)
        );
    }
}
