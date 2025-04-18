using Discord.WebSocket;

namespace BreganTwitchBot.Domain.Interfaces.Discord
{
    public interface IDiscordUserLookupService
    {
        string? GetTwitchUsernameFromDiscordUser(ulong userId);
        bool IsUserMod(ulong serverId, SocketGuildUser? user);
    }
}
