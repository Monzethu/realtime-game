using MagicOnion;
using MagicOnion.Server;
using realtime_game.Shared.Interfaces.Services;

public class CalclateService : ServiceBase<ICalculateService>, ICalculateService
{
    // 『乗算API』二つの整数を引数で受け取り乗算値を返す
    public async UnaryResult<int> MulAsync(int x, int y)
    {
        Console.WriteLine("Received:" + x + "," + y);
        return x * y;
    }

    // 受け取った配列の値の合計を返す
    public async UnaryResult<int> SumAllAsync(int[] numList)
    {
        if (numList == null || numList.Length == 0)
            return 0;
        int sum = 0;
        foreach (var n in numList)
        {
            sum += n;
        }
        return sum;
    }
    // x + yを[0]に、x - yを[1]に、x * yを[2]に、x / yを[3]に入れて配列で返す
    public async UnaryResult<int[]> CalcForOperationAsync(int x, int y)
    {
        int[] result = new int[4];
        result[0] = x + y;
        result[1] = x - y;
        result[2] = x * y;
        result[3] = (y != 0) ? x / y : 0;
        return result;
    }
    // 小数の値3つをフィールドに持つNumberクラスを渡して、3つの値の合計値を返す
    public async UnaryResult<float> SumAllNumberAsync(Number numData)
    {
        if (numData == null)
            return 0f;
        float sum = numData.x + numData.y + numData.z;
        return sum;
    }
}
