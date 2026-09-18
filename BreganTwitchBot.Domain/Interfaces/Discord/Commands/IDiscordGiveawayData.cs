namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordGiveawayData
    {
        /// <summary>
        /// Starts a giveaway in the guild's configured giveaway channel
        /// </summary>
        /// <returns>The giveaway id for the entry buttons, plus the message to respond with</returns>
        Task<(string? GiveawayId, string Response)> StartGiveawayAsync(ulong guildId, ulong channelId, ulong startedByUserId, int minimumWatchtimeHours, string? requiredRankName);

        /// <summary>
        /// Enters a user into a giveaway, or explains why they can't enter
        /// </summary>
        Task<(string Response, bool Ephemeral)> EnterGiveawayAsync(ulong guildId, ulong userId, string giveawayId);

        /// <summary>
        /// Shows a user their entries, or the totals if they started the giveaway
        /// </summary>
        Task<(string Response, bool Ephemeral)> CheckEntriesAsync(ulong guildId, ulong userId, string giveawayId);

        /// <summary>
        /// Draws a winner. Only the user who started the giveaway can do this.
        /// </summary>
        Task<(string Response, bool Ephemeral)> DrawWinnerAsync(ulong guildId, ulong userId, string giveawayId);
    }
}
