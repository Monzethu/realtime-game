using Cysharp.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Client;
using realtime_game.Shared.Interfaces.Services;
using UnityEngine;
using realtime_game.Shared.Models.Entities;
using System;
using MagicOnion;

public class UserModel : BaseModel
{
    public int UserId { get; private set; } // ìoò^ÉÜÅ[ÉUÅ[ID

    public async UniTask<bool> RegistUserAsync(string name)
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        var client = MagicOnionClient.Create<IUserService>(channel);

        try
        {
            UserId = await client.RegistUserAsync(name);
            return true;
        }
        catch (RpcException e)
        {
            Debug.LogError(e);
            return false;
        }
    }

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
}
