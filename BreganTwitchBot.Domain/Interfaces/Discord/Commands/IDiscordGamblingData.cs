using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordGamblingData
    {
        Task<DiscordEmbedData> HandleSpinCommand(DiscordCommand command);
    }
}
