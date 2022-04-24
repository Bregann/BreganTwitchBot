using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Data.Models
{
    public class StreamStats
    {
        [Key]
        [Column("streamId")]
        public long StreamId { get; set; }

        [Column("avgViewCount")]
        public double AvgViewCount { get; set; }

        [Column("peakViewerCount")]
        public long PeakViewerCount { get; set; }

        [Column("bitsDonated")]
        public long BitsDonated { get; set; }

        [Column("commandsSent")]
        public long CommandsSent { get; set; }

        [Column("discordRanksEarnt")]
        public long DiscordRanksEarnt { get; set; }

        [Column("startingFollowerCount")]
        public long StartingFollowerCount { get; set; }

        [Column("endingFollowerCount")]
        public long EndingFollowerCount { get; set; }

        [Column("startingSubscriberCount")]
        public long StartingSubscriberCount { get; set; }

        [Column("endingSubscriberCount")]
        public long EndingSubscriberCount { get; set; }

        [Column("messagesReceived")]
        public long MessagesReceived { get; set; }

        [Column("newFollowers")]
        public long NewFollowers { get; set; }

        [Column("newSubscribers")]
        public long NewSubscribers { get; set; }

        [Column("pointsGainedSubscribing")]
        public long PointsGainedSubscribing { get; set; }

        [Column("pointsGainedWatching")]
        public long PointsGainedWatching { get; set; }

        [Column("pointsGambled")]
        public long PointsGambled { get; set; }

        [Column("pointsLost")]
        public long PointsLost { get; set; }

        [Column("pointsWon")]
        public long PointsWon { get; set; }

        [Column("songRequestsBlacklisted")]
        public long SongRequestsBlacklisted { get; set; }

        [Column("songRequestsLiked")]
        public long SongRequestsLiked { get; set; }

        [Column("songRequestsSent")]
        public long SongRequestsSent { get; set; }

        [Column("totalBans")]
        public long TotalBans { get; set; }

        [Column("totalTimeouts")]
        public long TotalTimeouts { get; set; }

        [Column("newGiftedSubs")]
        public long NewGiftedSubs { get; set; }

        [Column("uniquePeople")]
        public long UniquePeople { get; set; }

        [Column("uptime")]
        public TimeSpan Uptime { get; set; }

        [Column("streamStarted")]
        public DateTime StreamStarted { get; set; }

        [Column("streamEnded")]
        public DateTime StreamEnded { get; set; }

        [Column("giftedPoints")]
        public long GiftedPoints { get; set; }

        [Column("totalSpins")]
        public long TotalSpins { get; set; }

        [Column("kappaWins")]
        public long KappaWins { get; set; }

        [Column("foreheadWins")]
        public long ForeheadWins { get; set; }

        [Column("lulWins")]
        public long LulWins { get; set; }

        [Column("smorcWins")]
        public long SmorcWins { get; set; }

        [Column("jackpotWins")]
        public long JackpotWins { get; set; }

        [Column("totalUsersClaimed")]
        public long TotalUsersClaimed { get; set; }

        [Column("totalPointsClaimed")]
        public long TotalPointsClaimed { get; set; }

        [Column("amountOfUsersReset")]
        public long AmountOfUsersReset { get; set; }

        [Column("amountOfRewardsRedeemed")]
        public long AmountOfRewardsRedeemed { get; set; }

        [Column("rewardRedeemCost")]
        public long RewardRedeemCost { get; set; }

        [Column("amountOfDiscordUsersJoined")]
        public long AmountOfDiscordUsersJoined { get; set; }
    }
}
