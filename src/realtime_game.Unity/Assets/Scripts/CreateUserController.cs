using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class CreateUserController : MonoBehaviour
{
    [SerializeField] private InputField inputName;
    [SerializeField] private InputField inputPassword;
    [SerializeField] private string nextSceneName = "LobbyScene";

    private bool isProcessing;

    public async void OnRegisterClicked()
    {
        if (isProcessing) return;
        isProcessing = true;

        string name = inputName.text;
        string password = inputPassword.text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("名前またはパスワードが空です");
            isProcessing = false;
            return;
        }

        var userModel = UserModel.Instance;

        bool success = await userModel.RegistUserAsync(name, password);

        if (success)
        {
            Debug.Log("ユーザー登録成功");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("ユーザー登録失敗");
            isProcessing = false;
        }
    }

    // 戻るボタン用（任意）
    public void OnBackClicked()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
