using UnityEngine;
using System;
using System.Collections.Generic;
using Shared.Interfaces.StreamingHubs;

public class BattleGameDirector : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletsParent;

    private RoomModel room;
    private Dictionary<Guid, GameObject> players = new();

    private void Start()
    {
        room = RoomModel.Instance;

        if (room == null)
        {
            Debug.LogError("RoomModel not found");
            return;
        }

        room.OnJoinedUser += OnJoinedUser;
        room.OnLeftUser += OnLeftUser;
        room.OnMoveCharacter += OnMoveCharacter;
        room.OnBulletReceived += OnBulletReceived;
    }

    private void OnDestroy()
    {
        if (room == null) return;

        room.OnJoinedUser -= OnJoinedUser;
        room.OnLeftUser -= OnLeftUser;
        room.OnMoveCharacter -= OnMoveCharacter;
        room.OnBulletReceived -= OnBulletReceived;
    }

    private void OnJoinedUser(JoinedUser user)
    {
        bool isLocal = user.ConnectionId == room.ConnectionId;
        SpawnPlayer(user.ConnectionId, isLocal);
    }

    private void SpawnPlayer(Guid id, bool isLocal)
    {
        if (players.ContainsKey(id)) return;

        GameObject player = Instantiate(playerPrefab, RandomSpawn(), Quaternion.identity);

        if (!isLocal)
        {
            var controller = player.GetComponent<PlayerContoroller>();
            if (controller != null) controller.enabled = false;

            var pov = player.GetComponent<PlayerPOV>();
            if (pov != null) pov.enabled = false;

            var shooting = player.GetComponent<Shooting>();
            if (shooting != null) shooting.enabled = false;
        }
        else
        {
            var shooting = player.GetComponent<Shooting>();
            if (shooting != null)
            {
                shooting.SetRoomModel(room);
            }
        }


        players[id] = player;
    }

    private void OnLeftUser(Guid id)
    {
        if (!players.TryGetValue(id, out var player)) return;
        Destroy(player);
        players.Remove(id);
    }

    private void OnMoveCharacter(Guid id, Vector3 pos, Quaternion rot)
    {
        if (!players.TryGetValue(id, out var player)) return;
        player.transform.SetPositionAndRotation(pos, rot);
    }

    private void OnBulletReceived(Guid shooterId, Vector3 pos, Quaternion rot, Vector3 velocity)
    {
        GameObject bullet = Instantiate(bulletPrefab, pos, rot, bulletsParent);

        var rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = velocity; // ← ここ重要
        }

        Destroy(bullet, 3f);
    }

    private Vector3 RandomSpawn()
    {
        return new Vector3(
            UnityEngine.Random.Range(-5f, 5f),
            1f,
            UnityEngine.Random.Range(-5f, 5f)
        );
    }
}
