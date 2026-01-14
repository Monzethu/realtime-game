using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // LobbySceneで新規生成したPlayerをBattleSceneで再生成
    // もしくはゲームスタートした時点でユーザー登録

    public static GameController Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Game Rule")]
    [SerializeField] private float matchTime = 30f;

    [Header("UI")]
    [SerializeField] private GameObject resultPanel; // Inspectorで紐付け

    private float timer;
    private bool isGameRunning;

    // プレイヤー管理（必要に応じてBattleSceneで既存Playerを引き継ぐ）
    public Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    void Start()
    {
        timer = matchTime;
        isGameRunning = true;

        if (resultPanel != null)
            resultPanel.SetActive(false);
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

    // 試合終了
    private void EndGame()
    {
        isGameRunning = false;
        Debug.Log("Match End");

        if (resultPanel != null)
            resultPanel.SetActive(true);
    }

    // スポーン位置（固定座標 or ランダム座標）
    public Vector3 GetSpawnPosition()
    {
        return new Vector3(
            Random.Range(-5f, 5f),
            0f,
            Random.Range(-5f, 5f)
        );
    }
}
