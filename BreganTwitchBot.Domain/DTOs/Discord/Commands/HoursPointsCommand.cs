namespace BreganTwitchBot.Domain.DTOs.Discord.Commands
{
    public class HoursPointsCommand
    {
        public required ulong GuildId { get; set; }
        public required ulong ChannelId { get; set; }

        /// <summary>
        /// The discord user who ran the command
        /// </summary>
        public required ulong CallerDiscordUserId { get; set; }

        /// <summary>
        /// A discord user to look up instead of the caller, if one was given
        /// </summary>
        public ulong? DiscordUserId { get; set; }

        /// <summary>
        /// A twitch username to look up instead of the caller, if one was given
        /// </summary>
        public string? TwitchUsername { get; set; }
    }
}
