using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class StreamStats
    {
        [Key]
        [Required]
        public long StreamId { get; set; }

        [Required]
        public double AvgViewCount { get; set; }

        [Required]
        public long PeakViewerCount { get; set; }

        [Required]
        public long BitsDonated { get; set; }

        [Required]
        public long CommandsSent { get; set; }

        [Required]
        public long DiscordRanksEarnt { get; set; }

        [Required]
        public long StartingFollowerCount { get; set; }

        [Required]
        public long EndingFollowerCount { get; set; }

        [Required]
        public long StartingSubscriberCount { get; set; }

        [Required]
        public long EndingSubscriberCount { get; set; }

        [Required]
        public long MessagesReceived { get; set; }

        [Required]
        public long NewFollowers { get; set; }

        [Required]
        public long NewSubscribers { get; set; }

        [Required]
        public long PointsGainedSubscribing { get; set; }

        [Required]
        public long PointsGainedWatching { get; set; }

        [Required]
        public long PointsGambled { get; set; }

        [Required]
        public long PointsLost { get; set; }

        [Required]
        public long PointsWon { get; set; }

        [Required]
        public long SongRequestsBlacklisted { get; set; }

        [Required]
        public long SongRequestsLiked { get; set; }

        [Required]
        public long SongRequestsSent { get; set; }

        [Required]
        public long TotalBans { get; set; }

        [Required]
        public long TotalTimeouts { get; set; }

        [Required]
        public long NewGiftedSubs { get; set; }

        [Required]
        public long UniquePeople { get; set; }

        [Required]
        public TimeSpan Uptime { get; set; }

        [Required]
        public DateTime StreamStarted { get; set; }

        [Required]
        public DateTime StreamEnded { get; set; }

        [Required]
        public long GiftedPoints { get; set; }

        [Required]
        public long TotalSpins { get; set; }

        [Required]
        public long KappaWins { get; set; }

        [Required]
        public long ForeheadWins { get; set; }

        [Required]
        public long LulWins { get; set; }

        [Required]
        public long SmorcWins { get; set; }

        [Required]
        public long JackpotWins { get; set; }

        [Required]
        public long TotalUsersClaimed { get; set; }

        [Required]
        public long TotalPointsClaimed { get; set; }

        [Required]
        public long AmountOfUsersReset { get; set; }

        [Required]
        public long AmountOfRewardsRedeemed { get; set; }

        [Required]
        public long RewardRedeemCost { get; set; }

        [Required]
        public long AmountOfDiscordUsersJoined { get; set; }
    }
}