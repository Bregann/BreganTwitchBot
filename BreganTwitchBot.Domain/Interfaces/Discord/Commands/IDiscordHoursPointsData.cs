using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordHoursPointsData
    {
        /// <summary>
        /// Watchtime for the caller, or for the user they named
        /// </summary>
        Task<DiscordEmbedData> HandleHoursCommandAsync(HoursPointsCommand command);

        /// <summary>
        /// Points for the caller, or for the user they named
        /// </summary>
        Task<DiscordEmbedData> HandlePointsCommandAsync(HoursPointsCommand command);

        /// <summary>
        /// Spends the channel's point cap to gain a prestige level
        /// </summary>
        Task<string> HandlePrestigeCommandAsync(HoursPointsCommand command);
    }
}
