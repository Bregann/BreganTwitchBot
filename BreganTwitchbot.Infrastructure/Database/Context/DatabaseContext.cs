using BreganTwitchBot.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Infrastructure.Database.Context
{
    public class DatabaseContext : DbContext
    {
        private static readonly string _connectionString = Environment.GetEnvironmentVariable("BTBConnString");

        public DbSet<Blacklist> Blacklist { get; set; }
        public DbSet<Commands> Commands { get; set; }
        public DbSet<Config> Config { get; set; }
        public DbSet<DiscordLinkRequests> DiscordLinkRequests { get; set; }
        public DbSet<SlotMachine> SlotMachine { get; set; }
        public DbSet<StreamStats> StreamStats { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<StreamViewCount> StreamViewCount { get; set; }
        public DbSet<UniqueViewers> UniqueViewers { get; set; }
        public DbSet<Subathon> Subathon { get; set; }
        public DbSet<Birthdays> Birthdays { get; set; }
        public DbSet<RankBeggar> RankBeggar { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(_connectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Seed in the data
            modelBuilder.Entity<Config>().HasData(new Config
            {
                BroadcasterName = "",
                DailyPointsCollectingAllowed = false,
                DiscordAPIKey = "",
                DiscordBanRole = 0,
                DiscordCommandsChannelID = 0,
                DiscordEventChannelID = 0,
                DiscordGeneralChannel = 0,
                DiscordGiveawayChannelID = 0,
                DiscordGuildID = 0,
                DiscordGuildOwner = 0,
                DiscordLinkingChannelID = 0,
                DiscordRankUpAnnouncementChannelID = 0,
                DiscordReactionRoleChannelID = 0,
                DiscordSocksChannelID = 0,
                DiscordStreamAnnouncementChannelID = 0,
                PinnedStreamDate = DateTime.UtcNow.AddDays(-1),
                LastDailyPointsAllowed = DateTime.UtcNow.AddDays(-1),
                TwitchAPIClientID = "",
                BotOAuth = "",
                BroadcasterOAuth = "",
                StreamAnnounced = false,
                TwitchChannelID = "",
                TwitchAPISecret = "",
                BotName = "",
                BroadcasterRefresh = "",
                PinnedStreamMessage = "",
                PinnedStreamMessageId = 0,
                PointsName = "",
                PrestigeCap = 0,
                SubathonTime = TimeSpan.FromSeconds(0),
                ProjectMonitorApiKey = "",
                HFConnectionString = "",
                TwitchBotApiKey = "",
                TwitchBotApiRefresh = "",
                BotChannelId = ""
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