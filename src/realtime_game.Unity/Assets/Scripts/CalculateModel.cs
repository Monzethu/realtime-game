using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using MagicOnion;
using realtime_game.Shared.Interfaces.Services;
using UnityEngine;

public class CalculateModel : MonoBehaviour
{
    const string ServerURL = "http://localhost:5244";
    async void Start()
    {
        int result = await Mul(100, 323);
        Debug.Log(result);

        int[] array = new int[2] { 100,200 };
        int result2 = await SumAll(array);
        Debug.Log(result2);

        int[] result3 = await CalcForOperation(10,20);
        for(int i = 0; i < result3.Length; i++)
        {
            Debug.Log(result3[i]);
        }

        Number number = new Number();
        number.x= 0.5f;
        number.y = 1.5f;
        number.z = 2.5f;
        float result4 = await SumAllNumber(number);
        Debug.Log(result4);
    }

    public async UniTask<int> Mul(int x, int y)
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        var client = MagicOnionClient.Create<ICalculateService>(channel);
        var result = await client.MulAsync(x, y);
        return result;
    }

    public async UniTask<int> SumAll(int[] numList)
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        var client = MagicOnionClient.Create<ICalculateService>(channel);
        var result = await client.SumAllAsync(numList);
        return result;
    }

    public async UniTask<int[]> CalcForOperation(int x, int y)
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        var client = MagicOnionClient.Create<ICalculateService>(channel);
        var result = await client.CalcForOperationAsync(x, y);
        return result;
    }

    public async UniTask<float> SumAllNumber(Number numData)
    {
        var channel = GrpcChannelx.ForAddress(ServerURL);
        var client = MagicOnionClient.Create<ICalculateService>(channel);
        var result = await client.SumAllNumberAsync(numData);
        return result;
    }
}
