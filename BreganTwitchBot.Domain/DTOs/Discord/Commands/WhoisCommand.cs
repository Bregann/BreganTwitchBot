namespace BreganTwitchBot.Domain.DTOs.Discord.Commands
{
    public class WhoisCommand
    {
        public required ulong GuildId { get; set; }

        /// <summary>
        /// The discord user being looked up, if they were given
        /// </summary>
        public ulong? DiscordUserId { get; set; }

        /// <summary>
        /// The twitch username being looked up, if it was given
        /// </summary>
        public string? TwitchUsername { get; set; }
    }
}
