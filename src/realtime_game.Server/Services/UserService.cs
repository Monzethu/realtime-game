using MagicOnion;
using MagicOnion.Server;
using realtime_game.Shared.Interfaces.Services;
using realtime_game.Shared.Models.Entities;
using realtime_game.Server.Models.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;

public class UserService : ServiceBase<IUserService>, IUserService
{
    // ===========================
    // ユーザー登録
    // ===========================
    public async UnaryResult<int> RegistUserAsync(string name, string password)
    {
        using var context = new GameDbContext();

        // 名前重複チェック
        if (await context.Users.AnyAsync(u => u.Name == name))
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "この名前は既に使われています。");
        }

        // パスワードをハッシュ化
        var hashedPass = PassHash.Hash(password);

        var user = new User
        {
            Name = name,
            Pass = hashedPass,      // サーバー内で保持
            Token = Guid.NewGuid().ToString(),
            Created_at = DateTime.Now,
            Updated_at = DateTime.Now
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.Id;
    }

    // ===========================
    // ログイン
    // ===========================
    public async UnaryResult<User> LoginUserAsync(string name, string password)
    {
        Console.WriteLine($"[RegistUserAsync] name={name}");

        using var context = new GameDbContext();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Name == name);
        if (user == null || !PassHash.Verify(password, user.Pass))
        {
            throw new ReturnStatusException(Grpc.Core.StatusCode.Unauthenticated, "名前またはパスワードが違います。");
        }

        // ログイントークン更新
        user.Token = Guid.NewGuid().ToString();
        user.Updated_at = DateTime.Now;
        await context.SaveChangesAsync();

        return user;
    }

    // ===========================
    // ID指定でユーザー取得
    // ===========================
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

    // ===========================
    // 全ユーザー取得
    // ===========================
    public async UnaryResult<User[]> GetAllUsersAsync()
    {
        using var context = new GameDbContext();
        return await context.Users.ToArrayAsync();
    }

    // ===========================
    // ユーザー名更新
    // ===========================
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

    // ===========================
    // パスワードハッシュ
    // ===========================
    public static class PassHash
    {
        public static string Hash(string password)
        {
            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public static bool Verify(string password, string hashed)
        {
            return Hash(password) == hashed;
        }
    }
}
