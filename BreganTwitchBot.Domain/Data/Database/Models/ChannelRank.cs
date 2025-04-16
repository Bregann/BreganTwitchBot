using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Data.Database.Models
{
    public class ChannelRank
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public required int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required string RankName { get; set; }

        [Required]
        public required int RankMinutesRequired { get; set; }

        [Required]
        public required int BonusRankPointsEarned { get; set; }

        public ulong? DiscordRoleId { get; set; }
    }
}
