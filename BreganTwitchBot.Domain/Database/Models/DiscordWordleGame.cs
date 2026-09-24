using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    /// <summary>
    /// One user's attempt at one day's Wordle. Everyone in a channel guesses the same word that day,
    /// so the word itself isn't stored - it comes from the date.
    /// </summary>
    [Index(nameof(ChannelId), nameof(DiscordUserId), nameof(Date), IsUnique = true)]
    public class DiscordWordleGame
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }
        public virtual Channel Channel { get; set; } = null!;

        /// <summary>
        /// Keyed on the discord user rather than a ChannelUser so people who haven't linked
        /// their Twitch account can still play, they just don't earn points
        /// </summary>
        [Required]
        public required ulong DiscordUserId { get; set; }

        [Required]
        public required DateOnly Date { get; set; }

        [Required]
        public required List<string> Guesses { get; set; }

        [Required]
        public required bool Solved { get; set; }

        [Required]
        public required long PointsAwarded { get; set; }
    }
}
