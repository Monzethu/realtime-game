using MagicOnion.Server;
using MagicOnion;
using realtime_game.Shared.Interfaces.Services;
using realtime_game.Shared.Models.Entities;
using realtime_game.Server.Models.Contexts;
using Microsoft.EntityFrameworkCore;

public class UserService : ServiceBase<IUserService>, IUserService
{
    public async UnaryResult<int> RegistUserAsync(string name)
    {
        using var context = new GameDbContext();
        //バリデーションチェック(名前登録済みかどうか)
        if (context.Users.Count() > 0 &&
              context.Users.Where(user => user.Name == name).Count() > 0)
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "");
        }
        //テーブルにレコードを追加
        User user = new User();
        user.Name = name;
        user.Token = "";
        user.Created_at = DateTime.Now;
        user.Updated_at = DateTime.Now;
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    public async UnaryResult<User> GetUserByIdAsync(int id)
    {
        using var context = new GameDbContext();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.NotFound, "ユーザーが存在しません。");
        }

        return user;
    }

    public async UnaryResult<User[]> GetAllUsersAsync()
    {
        using var context = new GameDbContext();

        return await context.Users.ToArrayAsync();
    }

    public async UnaryResult<bool> UpdateUserNameAsync(int id, string newName)
    {
        using var context = new GameDbContext();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.NotFound, "ユーザーが存在しません。");
        }

        user.Name = newName;
        user.Updated_at = DateTime.Now;

        await context.SaveChangesAsync();
        return true;
    }
}
