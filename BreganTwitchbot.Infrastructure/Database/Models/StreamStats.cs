using System.ComponentModel.DataAnnotations;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class StreamStats
    {
        [Key]
        public required long StreamId { get; set; }

        public double AvgViewCount { get; set; }

        public long PeakViewerCount { get; set; }

        public required long BitsDonated { get; set; }

        public required long CommandsSent { get; set; }

        public required long DiscordRanksEarnt { get; set; }

        public required long StartingFollowerCount { get; set; }

        public required long EndingFollowerCount { get; set; }

        public required long StartingSubscriberCount { get; set; }

        public required long EndingSubscriberCount { get; set; }

        public required long MessagesReceived { get; set; }

        public required long NewFollowers { get; set; }

        public required long NewSubscribers { get; set; }

        public required long PointsGainedSubscribing { get; set; }

        public required long PointsGainedWatching { get; set; }

        public required long PointsGambled { get; set; }

        public required long PointsLost { get; set; }

        public required long PointsWon { get; set; }

        public required long SongRequestsBlacklisted { get; set; }

        public required long SongRequestsLiked { get; set; }

        public required long SongRequestsSent { get; set; }

        public required long TotalBans { get; set; }

        public required long TotalTimeouts { get; set; }

        public required long NewGiftedSubs { get; set; }

        public required long UniquePeople { get; set; }

        public TimeSpan Uptime { get; set; }

        public required DateTime StreamStarted { get; set; }

        public DateTime StreamEnded { get; set; }

        public required long GiftedPoints { get; set; }

        public required long TotalSpins { get; set; }

        public required long KappaWins { get; set; }

        public required long ForeheadWins { get; set; }

        public required long LulWins { get; set; }

        public required long SmorcWins { get; set; }

        public required long JackpotWins { get; set; }

        public required long TotalUsersClaimed { get; set; }

        public required long TotalPointsClaimed { get; set; }

        public required long AmountOfUsersReset { get; set; }

        public required long AmountOfRewardsRedeemed { get; set; }

        public required long RewardRedeemCost { get; set; }

        public required long AmountOfDiscordUsersJoined { get; set; }
    }
}