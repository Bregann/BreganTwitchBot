using BreganTwitchbot.Data.Models;
using BreganTwitchBot.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Blacklist> Blacklist { get; set; }
        public DbSet<Commands> Commands { get; set; }
        public DbSet<Config> Config { get; set; }
        public DbSet<DiscordLinkRequests> DiscordLinkRequests { get; set; }
        public DbSet<SlotMachine> SlotMachine { get; set; }
        public DbSet<StreamStats> StreamStats { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<StreamViewCount> StreamViewCount { get; set; }
        public DbSet<UniqueViewers> UniqueViewers { get; set; }

#if DEBUG
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=localhost;Database=twitchbot;Username=twitchbot;Password=pass");
#else
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=localhost;Database=twitchbot;Username=twitchbot;Password=pass");
#endif
    }
}