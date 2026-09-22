using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    /// <summary>
    /// A channel point reward the bot responds to in chat. The old bot hardcoded its one reward
    /// in a switch, which does not work now a single bot serves many channels, so each channel
    /// configures its own rewards here.
    /// </summary>
    public class ChannelPointReward
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }

        public virtual Channel Channel { get; set; } = null!;

        /// <summary>
        /// The reward title as it appears on Twitch. Matched case insensitively.
        /// </summary>
        [Required]
        public required string RewardTitle { get; set; }

        /// <summary>
        /// The message to send when the reward is redeemed. {user} is replaced with the name of
        /// whoever redeemed it.
        /// </summary>
        [Required]
        public required string ResponseMessage { get; set; }

        [Required]
        public required bool Enabled { get; set; } = true;

        [Required]
        public required int TimesRedeemed { get; set; }
    }
}
