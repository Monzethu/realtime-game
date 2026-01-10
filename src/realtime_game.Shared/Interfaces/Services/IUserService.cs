using MagicOnion;
using realtime_game.Shared.Models.Entities;

namespace realtime_game.Shared.Interfaces.Services
{
    public interface IUserService : IService<IUserService>
    {
        // ユーザー登録（名前 + パスワード）
        UnaryResult<int> RegistUserAsync(string name, string password);

        // ログイン（名前 + パスワード）
        UnaryResult<User> LoginUserAsync(string name, string password);

        // ID指定でユーザー取得
        UnaryResult<User> GetUserByIdAsync(int id);

        // ユーザー一覧取得
        UnaryResult<User[]> GetAllUsersAsync();

        // ユーザー名更新
        UnaryResult<bool> UpdateUserNameAsync(int id, string newName);
    }
}
