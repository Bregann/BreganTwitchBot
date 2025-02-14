using BreganTwitchBot.Domain.Data.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Data.Database.Context
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChannelConfig> ChannelConfig { get; set; }
        public DbSet<ChannelUser> ChannelUsers { get; set; }
        public DbSet<EnvironmentalSetting> EnvironmentalSettings { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;
    }
}
