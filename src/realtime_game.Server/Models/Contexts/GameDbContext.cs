using Microsoft.EntityFrameworkCore;
using realtime_game.Shared.Models.Entities;

namespace realtime_game.Server.Models.Contexts
{
    public class GameDbContext : DbContext
    {
        public DbSet<User>Users { get; set; }// テーブル（エンティティ）追加したらここに追加。

        readonly string connectionString = "server=localhost;database=realtime_game;user=jobi;password=jobi;";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8,0)));
        }
    }
}
