using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class Users
    {
        [Key]
        public required string TwitchUserId { get; set; }

        public required string Username { get; set; }

        public required ulong DiscordUserId { get; set; }

        public required long Points { get; set; }

        public required int TotalMessages { get; set; }

        public required bool InStream { get; set; }

        public required DateTime LastSeenDate { get; set; }

        public required bool IsSub { get; set; }

        public required bool IsSuperMod { get; set; }

        public required int GiftedSubsThisMonth { get; set; }

        public required int BitsDonatedThisMonth { get; set; }

        public required int MarblesWins { get; set; }

        public required int BossesDone { get; set; }

        public required long BossesPointsWon { get; set; }

        public required int TimeoutStrikes { get; set; }

        public required int WarnStrikes { get; set; }

        public required DailyPoints DailyPoints { get; set; }

        public required DiscordUserStats DiscordUserStats { get; set; }

        public required UserGambleStats UserGambleStats { get; set; }

        public required Watchtime Watchtime { get; set; }
    }
}