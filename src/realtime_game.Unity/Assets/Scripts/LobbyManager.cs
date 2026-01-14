using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System;

public class LobbyManager : MonoBehaviour
{
    public UserModel userModel;

    public Guid myConnectionId;
    public string userName = "TestPlayer";
    public string roomId = "Room_001";

    private async void Start()
    {
        // 1. ユーザー登録（名前だけ）
        bool success = await userModel.RegistUserAsync(userName);
        if (!success)
        {
            Debug.LogError("ユーザー登録失敗");
            return;
        }

        // 2. Room接続（ここで ConnectionId が確定）
        await GameController.Instance.RoomModel.ConnectAsync();

        myConnectionId = GameController.Instance.RoomModel.ConnectionId;

        // 3. Player生成
        GameController.Instance.CreatePlayer(myConnectionId);

        // 4. Room参加
        await GameController.Instance.RoomModel.JoinAsync(roomId);

        Debug.Log($"Joined Room {roomId} with ConnectionId {myConnectionId}");
    }

    public void OnClickStartGame()
    {
        SceneManager.LoadScene("BattleScene");
    }
}
