using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class Titlemanager : MonoBehaviour
{
    [SerializeField] private InputField inputName;
    [SerializeField] private InputField inputPassword;
    [SerializeField] private string nextSceneName = "LobbyScene";

    private bool isProcessing;

    public async void OnLoginClicked()
    {
        Debug.Log("OnLoginClicked 呼ばれた");

        if (isProcessing) return;
        isProcessing = true;

        string name = inputName.text;
        string password = inputPassword.text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
        {
            MessageManager.Instance.ShowMessage("名前とパスワードを入力してください");
            isProcessing = false;
            return;
        }

        MessageManager.Instance.ShowMessage("ログイン中...");

        bool success = await UserModel.Instance.LoginUserAsync(name, password);

        if (success)
        {
            MessageManager.Instance.ShowMessage("ログイン成功！");
            await UniTask.Delay(300);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            MessageManager.Instance.ShowMessage("ログイン失敗");
            isProcessing = false;
        }
    }
}
