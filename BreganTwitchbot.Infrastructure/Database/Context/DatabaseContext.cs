using BreganTwitchBot.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Infrastructure.Database.Context
{
    public class DatabaseContext : DbContext
    {
        private static readonly string _connectionString = Environment.GetEnvironmentVariable("QBConnString")!;

        public DbSet<Blacklist> Blacklist { get; set; }
        public DbSet<Commands> Commands { get; set; }
        public DbSet<Config> Config { get; set; }
        public DbSet<SlotMachine> SlotMachine { get; set; }
        public DbSet<StreamStats> StreamStats { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<StreamViewCount> StreamViewCount { get; set; }
        public DbSet<UniqueViewers> UniqueViewers { get; set; }
        public DbSet<Subathon> Subathon { get; set; }
        public DbSet<DailyPoints> DailyPoints { get; set; }
        public DbSet<UserGambleStats> UserGambleStats { get; set; }
        public DbSet<Watchtime> Watchtime { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(_connectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("twitchbot_new");

            modelBuilder.Entity<Users>()
                .HasOne(b => b.DailyPoints)
                .WithOne(i => i.User)
                .HasForeignKey<DailyPoints>(b => b.TwitchUserId);

            modelBuilder.Entity<Users>()
                .HasOne(b => b.UserGambleStats)
                .WithOne(i => i.User)
                .HasForeignKey<UserGambleStats>(b => b.TwitchUserId);

            modelBuilder.Entity<Users>()
                .HasOne(b => b.Watchtime)
                .WithOne(i => i.User)
                .HasForeignKey<Watchtime>(b => b.TwitchUserId);

            //Seed in the data
            modelBuilder.Entity<Config>().HasData(new Config
            {
                BroadcasterName = "",
                DailyPointsCollectingAllowed = false,
                LastDailyPointsAllowed = DateTime.UtcNow.AddDays(-1),
                TwitchAPIClientID = "",
                BotOAuth = "",
                BroadcasterOAuth = "",
                StreamAnnounced = false,
                TwitchChannelID = "",
                TwitchAPISecret = "",
                BotName = "",
                BroadcasterRefresh = "",
                PointsName = "",
                PrestigeCap = 0,
                SubathonTime = TimeSpan.FromSeconds(0),
                ProjectMonitorApiKey = "",
                HFConnectionString = "",
                TwitchBotApiKey = "",
                TwitchBotApiRefresh = "",
                BotChannelId = "",
                SubathonActive = false,
                HangfireUsername = "",
                HangfirePassword = "",
                StreamHappenedThisWeek = false
            });

            modelBuilder.Entity<SlotMachine>().HasData(new SlotMachine
            {
                StreamName = "",
                JackpotAmount = 0,
                CheeseWins = 0,
                CherriesWins = 0,
                CucumberWins = 0,
                DiscordTotalSpins = 0,
                EggplantWins = 0,
                GrapesWins = 0,
                JackpotWins = 0,
                PineappleWins = 0,
                SmorcWins = 0,
                Tier1Wins = 0,
                Tier2Wins = 0,
                Tier3Wins = 0,
                TotalSpins = 0
            });
        }
    }
}