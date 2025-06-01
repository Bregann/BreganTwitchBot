using BreganTwitchBot.Domain.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Database.Context
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<AiBookData> AiBookData { get; set; }
        public DbSet<Birthday> Birthdays { get; set; }
        public DbSet<Blacklist> Blacklist { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChannelConfig> ChannelConfig { get; set; }
        public DbSet<ChannelMessages> ChannelMessages { get; set; }
        public DbSet<ChannelRank> ChannelRanks { get; set; }
        public DbSet<ChannelUser> ChannelUsers { get; set; }
        public DbSet<ChannelUserData> ChannelUserData { get; set; }
        public DbSet<ChannelUserGambleStats> ChannelUserGambleStats { get; set; }
        public DbSet<ChannelUserRankProgress> ChannelUserRankProgress { get; set; }
        public DbSet<ChannelUserStats> ChannelUserStats { get; set; }
        public DbSet<ChannelUserWatchtime> ChannelUserWatchtime { get; set; }
        public DbSet<CustomCommand> CustomCommands { get; set; }
        public DbSet<DiscordDailyPoints> DiscordDailyPoints { get; set; }
        public DbSet<DiscordLinkRequests> DiscordLinkRequests { get; set; }
        public DbSet<DiscordSpinStats> DiscordSpinStats { get; set; }
        public DbSet<DiscordUserStats> DiscordUserStats { get; set; }
        public DbSet<StreamViewCount> StreamViewCounts { get; set; }
        public DbSet<Subathon> Subathons { get; set; }
        public DbSet<TwitchDailyPoints> TwitchDailyPoints { get; set; }
        public DbSet<TwitchSlotMachineStats> TwitchSlotMachineStats { get; set; }
        public DbSet<TwitchStreamStats> TwitchStreamStats { get; set; }
        public DbSet<UniqueViewers> UniqueViewers { get; set; }

        public DbSet<EnvironmentalSetting> EnvironmentalSettings { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;
    }
}
