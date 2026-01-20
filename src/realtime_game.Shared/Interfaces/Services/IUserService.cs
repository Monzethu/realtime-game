using MagicOnion;
using realtime_game.Shared.Models.Entities;

namespace realtime_game.Shared.Interfaces.Services
{
    public interface IUserService : IService<IUserService>
    {
        UnaryResult<User> LoginUserAsync(string name, string password);
        UnaryResult<int> RegistUserAsync(string name, string password);

        //UnaryResult<User> GetUserByNameAsync(string name);

        UnaryResult<User> GetUserByIdAsync(int id);

        UnaryResult<User[]> GetAllUsersAsync();

        UnaryResult<bool> UpdateUserNameAsync(int id, string newName);
    }

}
