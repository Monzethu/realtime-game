using Cysharp.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Client;
using MagicOnion;
using realtime_game.Shared.Interfaces.Services;
using realtime_game.Shared.Models.Entities;
using UnityEngine;
using System;

public class UserModel : BaseModel
{
    // ===== Singleton =====
    private static UserModel instance;
    public static UserModel Instance => instance;

    // ===== PlayerPrefs Key =====
    private const string USER_ID_KEY = "USER_ID";
    private const string USER_NAME_KEY = "USER_NAME";
    private const string USER_TOKEN_KEY = "USER_TOKEN";

    // ===== User Data =====
    private int userId;
    private string userName;
    private string token;

    public int UserId => userId;
    public string UserName => userName;
    public string Token => token;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // =========================
    // ログイン
    // =========================
    public async UniTask<bool> LoginUserAsync(string name, string password)
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        var client = MagicOnionClient.Create<IUserService>(channel);

        try
        {
            var user = await client.LoginUserAsync(name, password);

            userId = user.Id;
            userName = user.Name;
            token = user.Token;

            SaveUserData();

            Debug.Log($"[Login] Success id={userId}, name={userName}, token={token}");
            return true;
        }
        catch (RpcException e)
        {
            Debug.LogError($"[Login] Failed: {e.Status.Detail}");
            return false;
        }
    }

    // =========================
    // ユーザー登録（必要なら）
    // =========================
    public async UniTask<bool> RegistUserAsync(string name, string password)
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        var client = MagicOnionClient.Create<IUserService>(channel);

        try
        {
            userId = await client.RegistUserAsync(name, password);
            userName = name;

            Debug.Log($"[Register] Success id={userId}");
            return true;
        }
        catch (RpcException e)
        {
            Debug.LogError($"[Register] Failed: {e.Status.Detail}");
            return false;
        }
    }

    // =========================
    // 保存
    // =========================
    public void SaveUserData()
    {
        PlayerPrefs.SetInt(USER_ID_KEY, userId);
        PlayerPrefs.SetString(USER_NAME_KEY, userName);
        PlayerPrefs.SetString(USER_TOKEN_KEY, token);
        PlayerPrefs.Save();

        Debug.Log($"[Save] {userId}, {userName}, {token}");
    }

    // =========================
    // 読み込み
    // =========================
    public bool LoadUserData()
    {
        if (!PlayerPrefs.HasKey(USER_ID_KEY) ||
            !PlayerPrefs.HasKey(USER_NAME_KEY) ||
            !PlayerPrefs.HasKey(USER_TOKEN_KEY))
        {
            Debug.Log("[Load] No user data");
            return false;
        }

        userId = PlayerPrefs.GetInt(USER_ID_KEY);
        userName = PlayerPrefs.GetString(USER_NAME_KEY);
        token = PlayerPrefs.GetString(USER_TOKEN_KEY);

        Debug.Log($"[Load] {userId}, {userName}, {token}");
        return true;
    }

    // =========================
    // デバッグ用（強制クリア）
    // =========================
    public void ClearUserData()
    {
        PlayerPrefs.DeleteKey(USER_ID_KEY);
        PlayerPrefs.DeleteKey(USER_NAME_KEY);
        PlayerPrefs.DeleteKey(USER_TOKEN_KEY);
        PlayerPrefs.Save();

        userId = 0;
        userName = null;
        token = null;

        Debug.Log("[Clear] User data cleared");
    }
}
