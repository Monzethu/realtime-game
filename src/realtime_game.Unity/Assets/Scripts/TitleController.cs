using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class TitleController : MonoBehaviour
{
    [SerializeField] private InputField inputUserId;
    [SerializeField] private string nextSceneName = "LobbyScene";

    private bool isProcessing;

    public async void OnStartClicked()
    {
        if (isProcessing) return;
        isProcessing = true;

        var userModel = UserModel.Instance;

        // すでに保存済みならそれを使う
        if (userModel.LoadUserData())
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        // InputID が入っているならそれを使う（デバッグ用）
        if (!string.IsNullOrEmpty(inputUserId.text)
            && int.TryParse(inputUserId.text, out int inputId))
        {
            userModel.SetUserId(inputId);
            userModel.SaveUserData();
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        // 完全新規ユーザー登録
        bool success = await userModel.RegistUserAsync("Player");

        if (success)
        {
            userModel.SaveUserData();
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("ユーザー登録失敗");
            isProcessing = false;
        }
    }
}
