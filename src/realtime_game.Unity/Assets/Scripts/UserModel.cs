using Cysharp.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Client;
using MagicOnion;
using realtime_game.Shared.Interfaces.Services;
using UnityEngine;
using realtime_game.Shared.Models.Entities;
using System;

public class UserModel : BaseModel
{
    // ===== Singleton =====
    private static UserModel instance;
    public static UserModel Instance => instance;

    // ===== PlayerPrefs Key =====
    private const string USER_ID_KEY = "USER_ID";
    private const string USER_NAME_KEY = "USER_NAME";

    // ===== 登録ユーザー情報 =====
    private int userId;
    private string userName;
    public int UserId => userId;
    public string UserName => userName;

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
    // ユーザー登録
    // =========================
    public async UniTask<bool> RegistUserAsync(string name, string password)
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        var client = MagicOnionClient.Create<IUserService>(channel);

        try
        {
            userId = await client.RegistUserAsync(name, password);
            userName = name;
            Debug.Log($"User registered. UserId = {userId}");

            SaveUserData();
            return true;
        }
        catch (RpcException e)
        {
            Debug.LogError(e);
            return false;
        }
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

            SaveUserData();
            return true;
        }
        catch (RpcException e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    // =========================
    // ユーザー情報保存
    // =========================
    public void SaveUserData()
    {
        PlayerPrefs.SetInt(USER_ID_KEY, userId);
        PlayerPrefs.SetString(USER_NAME_KEY, userName);
        PlayerPrefs.Save();
        Debug.Log($"User data saved: {userId}, {userName}");
    }

    // =========================
    // ユーザー情報読み込み
    // =========================
    public bool LoadUserData()
    {
        if (!PlayerPrefs.HasKey(USER_ID_KEY) || !PlayerPrefs.HasKey(USER_NAME_KEY))
        {
            Debug.Log("User data not found");
            return false;
        }

        userId = PlayerPrefs.GetInt(USER_ID_KEY);
        userName = PlayerPrefs.GetString(USER_NAME_KEY);
        Debug.Log($"User data loaded: {userId}, {userName}");
        return true;
    }

    // =========================
    // ID からユーザー取得
    // =========================
    public async UniTask<User> GetUserByIdAsync(int id)
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        var client = MagicOnionClient.Create<IUserService>(channel);

        try
        {
            return await client.GetUserByIdAsync(id);
        }
        catch (RpcException e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    // =========================
    // 全ユーザー取得
    // =========================
    public async UniTask<User[]> GetAllUsersAsync()
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        var client = MagicOnionClient.Create<IUserService>(channel);

        try
        {
            return await client.GetAllUsersAsync();
        }
        catch (RpcException e)
        {
            Debug.LogError(e);
            return Array.Empty<User>();
        }
    }

    // =========================
    // ユーザー名更新
    // =========================
    public async UniTask<bool> UpdateUserNameAsync(int id, string newName)
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        var client = MagicOnionClient.Create<IUserService>(channel);

        try
        {
            bool result = await client.UpdateUserNameAsync(id, newName);
            if (result && id == userId)
            {
                userName = newName;
                SaveUserData();
            }
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"RegistUserAsync Error: {e}");
            return false;
        }
    }

    // =========================
    // ユーザーIDを手動セット
    // =========================
    public void SetUserId(int id, string name = "")
    {
        userId = id;
        if (!string.IsNullOrEmpty(name)) userName = name;
        Debug.Log($"UserId manually set: {userId}, Name: {userName}");
    }
}
