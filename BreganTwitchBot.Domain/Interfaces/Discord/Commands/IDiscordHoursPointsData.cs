using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordHoursPointsData
    {
        /// <summary>
        /// Watchtime for the caller, or for the user they named
        /// </summary>
        Task<DiscordEmbedData> HandleHoursCommand(HoursPointsCommand command);

        /// <summary>
        /// Points for the caller, or for the user they named
        /// </summary>
        Task<DiscordEmbedData> HandlePointsCommand(HoursPointsCommand command);

        /// <summary>
        /// Spends the channel's point cap to gain a prestige level
        /// </summary>
        Task<string> HandlePrestigeCommand(HoursPointsCommand command);
    }
}
