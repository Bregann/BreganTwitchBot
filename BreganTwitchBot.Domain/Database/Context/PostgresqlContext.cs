using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Database.Context
{
    public class PostgresqlContext(DbContextOptions<PostgresqlContext> options) : AppDbContext(options)
    {
    }
}
