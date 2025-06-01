using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Database.Context
{
    public class PostgresqlContext : AppDbContext
    {
        public PostgresqlContext(DbContextOptions<PostgresqlContext> options) : base(options) { }
    }
}
