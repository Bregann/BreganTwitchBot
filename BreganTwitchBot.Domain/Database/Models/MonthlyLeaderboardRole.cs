using BreganTwitchBot.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    /// <summary>
    /// A Discord role handed to whoever is at a given position on a monthly leaderboard.
    ///
    /// The old bot looked these up by hardcoded role name ("#1 Bits Leaderboard" and so on) with
    /// guild.Roles.First(...), which threw if a role had been renamed or was missing. They are
    /// per channel rows holding the role id here instead.
    /// </summary>
    public class MonthlyLeaderboardRole
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        [Required]
        public required MonthlyLeaderboardType LeaderboardType { get; set; }

        /// <summary>
        /// Which place on the leaderboard this role is for, starting at 1
        /// </summary>
        [Required]
        public required int Position { get; set; }

        [Required]
        public required ulong DiscordRoleId { get; set; }
    }
}
