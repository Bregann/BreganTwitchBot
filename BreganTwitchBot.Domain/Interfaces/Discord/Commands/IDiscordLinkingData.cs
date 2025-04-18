using BreganTwitchBot.Domain.DTOs.Discord.Commands;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordLinkingData
    {
        Task<string> NewLinkRequest(DiscordCommand command);
    }
}
