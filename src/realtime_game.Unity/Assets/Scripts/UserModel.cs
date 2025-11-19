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
    private int userId;// 登録ユーザーID

    // ユーザー登録
    public async UniTask<bool> RegistUserAsync(string name)
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        var client = MagicOnionClient.Create<IUserService>(channel);

        try
        {
            userId = await client.RegistUserAsync(name); // OK
            return true;
        }
        catch (RpcException e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    // ID からユーザー取得
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

    // 全ユーザー取得
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

    // ユーザー名更新
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
}
