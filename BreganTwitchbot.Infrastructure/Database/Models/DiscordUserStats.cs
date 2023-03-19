using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Infrastructure.Database.Models
{
    public class DiscordUserStats
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Users User { get; set; }

        public string TwitchUserId { get; set; }

        public required int DiscordLevel { get; set; }

        public required int DiscordXp { get; set; }

        public required bool DiscordLevelUpNotifsEnabled { get; set; }

        public required int PrestigeLevel { get; set; }
    }
}