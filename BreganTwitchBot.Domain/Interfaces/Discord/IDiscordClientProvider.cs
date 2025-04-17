using Discord.WebSocket;

namespace BreganTwitchBot.Domain.Interfaces.Discord
{
    public interface IDiscordClientProvider
    {
        DiscordSocketClient Client { get; }
    }
}
