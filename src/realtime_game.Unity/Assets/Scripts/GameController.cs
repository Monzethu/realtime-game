using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    // GuidÇ≈ä«óù
    public Dictionary<Guid, GameObject> players = new Dictionary<Guid, GameObject>();

    public RoomModel RoomModel { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        RoomModel = new GameObject("RoomModel").AddComponent<RoomModel>();
        DontDestroyOnLoad(RoomModel.gameObject);
    }

    public void CreatePlayer(Guid connectionId)
    {
        if (players.ContainsKey(connectionId) && players[connectionId] != null)
            return;

        GameObject player = Instantiate(playerPrefab, GetSpawnPosition(), Quaternion.identity);
        DontDestroyOnLoad(player);

        var pc = player.GetComponent<PlayerContoroller>();
        if (pc != null)
        {
            pc.SetConnectionId(connectionId);
        }

        players[connectionId] = player;
        Debug.Log($"Player {connectionId} created.");
    }

    public void EnsurePlayerExists(Guid connectionId)
    {
        if (!players.ContainsKey(connectionId) || players[connectionId] == null)
        {
            CreatePlayer(connectionId);
        }
    }

    public Vector3 GetSpawnPosition()
    {
        return new Vector3(
            UnityEngine.Random.Range(-5f, 5f),
            1f,
            UnityEngine.Random.Range(-5f, 5f)
        );
    }
}
