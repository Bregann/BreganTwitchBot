using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Data.Database.Context
{
    public class PostgresqlContext : AppDbContext
    {
        public PostgresqlContext(DbContextOptions<PostgresqlContext> options) : base(options) { }
    }
}
