using BreganTwitchBot.Domain.DTOs.Discord.Commands;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordLevellingData
    {
        Task<string> HandleToggleLevelUpCommand(DiscordCommand command);
    }
}
