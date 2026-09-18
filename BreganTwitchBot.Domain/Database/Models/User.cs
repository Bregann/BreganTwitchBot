using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    /// <summary>
    /// Someone who has logged into the website.
    ///
    /// Sign in is Twitch only, so a user is a Twitch identity - there are no
    /// passwords to store. Their stats are found by matching TwitchUserId against
    /// ChannelUser, so a viewer the bot has never seen still gets a session, just
    /// with nothing to show yet.
    /// </summary>
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;

        [Required]
        public required string TwitchUserId { get; set; }

        [Required]
        public required string TwitchUsername { get; set; }

        public string? TwitchDisplayName { get; set; }

        public string? ProfileImageUrl { get; set; }

        [Required]
        public required DateTime FirstLoggedInAt { get; set; }

        [Required]
        public required DateTime LastLoggedInAt { get; set; }
    }
}
