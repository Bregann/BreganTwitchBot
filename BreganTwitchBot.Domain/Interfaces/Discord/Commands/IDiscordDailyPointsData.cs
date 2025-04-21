using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordDailyPointsData
    {
        Task<DiscordEmbedData> HandleDiscordDailyPointsCommand(DiscordCommand command);
        Task ResetDiscordDailyStreaks();
    }
}
