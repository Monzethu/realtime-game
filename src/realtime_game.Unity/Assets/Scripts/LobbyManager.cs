using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    [SerializeField] private string roomId = "test";
    [SerializeField] private int userId = 1;

    public async void JoinRoom()
    {
        await RoomModel.Instance.JoinAsync(roomId, userId);
        SceneManager.LoadScene("BattleScene");
    }
}
