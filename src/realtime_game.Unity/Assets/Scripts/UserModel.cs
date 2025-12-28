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

    // ===== 登録ユーザーID =====
    private int userId;
    public int UserId => userId;

    private void Awake()
    {
        // Singleton 保証
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
    public async UniTask<bool> RegistUserAsync(string name)
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        var client = MagicOnionClient.Create<IUserService>(channel);

        try
        {
            userId = await client.RegistUserAsync(name);
            Debug.Log($"User registered. UserId = {userId}");

            // ★ 登録成功したら保存
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
    // ユーザーID保存
    // =========================
    public void SaveUserData()
    {
        PlayerPrefs.SetInt(USER_ID_KEY, userId);
        PlayerPrefs.Save();
        Debug.Log($"UserId saved: {userId}");
    }

    // =========================
    // ユーザーID読込
    // =========================
    public bool LoadUserData()
    {
        if (!PlayerPrefs.HasKey(USER_ID_KEY))
        {
            Debug.Log("UserId not found");
            return false;
        }

        userId = PlayerPrefs.GetInt(USER_ID_KEY);
        Debug.Log($"UserId loaded: {userId}");
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
            return await client.UpdateUserNameAsync(id, newName);
        }
        catch (RpcException e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    // =========================
    // ユーザーIDを直接セット（InputID用）
    // =========================
    public void SetUserId(int id)
    {
        userId = id;
        Debug.Log($"UserId manually set: {userId}");
    }

}
