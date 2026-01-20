using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    void Start()
    {
        if (!UserModel.Instance.LoadUserData())
        {
            Debug.LogError("ñ¢ÉçÉOÉCÉìÇ≈Ç∑");
            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
            return;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
