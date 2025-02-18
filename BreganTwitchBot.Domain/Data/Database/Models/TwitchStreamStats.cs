using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class TwitchStreamStats
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public required int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required long StreamId { get; set; }

        [Required]
        public required double AvgViewCount { get; set; } = 0;

        [Required]
        public required long PeakViewerCount { get; set; } = 0;

        [Required]
        public required long BitsDonated { get; set; }

        [Required]
        public required long CommandsSent { get; set; }

        [Required]
        public required long DiscordRanksEarnt { get; set; }

        [Required]
        public required long StartingFollowerCount { get; set; }

        [Required]
        public required long EndingFollowerCount { get; set; }

        [Required]
        public required long StartingSubscriberCount { get; set; }

        [Required]
        public required long EndingSubscriberCount { get; set; }

        [Required]
        public required long MessagesReceived { get; set; }

        [Required]
        public required long NewFollowers { get; set; }

        [Required]
        public required long NewSubscribers { get; set; }

        [Required]
        public required long PointsGainedSubscribing { get; set; }

        [Required]
        public required long PointsGainedWatching { get; set; }

        [Required]
        public required long PointsGambled { get; set; }

        [Required]
        public required long PointsLost { get; set; }

        [Required]
        public required long PointsWon { get; set; }

        [Required]
        public required long SongRequestsBlacklisted { get; set; }

        [Required]
        public required long SongRequestsLiked { get; set; }

        [Required]
        public required long SongRequestsSent { get; set; }

        [Required]
        public required long TotalBans { get; set; }

        [Required]
        public required long TotalTimeouts { get; set; }

        [Required]
        public required long NewGiftedSubs { get; set; }

        [Required]
        public required long UniquePeople { get; set; }

        [Required]
        public TimeSpan Uptime { get; set; }

        [Required]
        public required DateTime StreamStarted { get; set; }

        [Required]
        public DateTime StreamEnded { get; set; }

        [Required]
        public required long GiftedPoints { get; set; }

        [Required]
        public required long TotalSpins { get; set; }

        [Required]
        public required long KappaWins { get; set; }

        [Required]
        public required long ForeheadWins { get; set; }

        [Required]
        public required long LulWins { get; set; }

        [Required]
        public required long SmorcWins { get; set; }

        [Required]
        public required long JackpotWins { get; set; }

        [Required]
        public required long TotalUsersClaimed { get; set; }

        [Required]
        public required long TotalPointsClaimed { get; set; }

        [Required]
        public required long AmountOfUsersReset { get; set; }

        [Required]
        public required long AmountOfRewardsRedeemed { get; set; }

        [Required]
        public required long RewardRedeemCost { get; set; }

        [Required]
        public required long AmountOfDiscordUsersJoined { get; set; }
    }
}
