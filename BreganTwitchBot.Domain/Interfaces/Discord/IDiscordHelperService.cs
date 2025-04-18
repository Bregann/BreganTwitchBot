using Discord;

namespace BreganTwitchBot.Domain.Interfaces.Discord
{
    public interface IDiscordHelperService
    {
        Task AddDiscordUserToDatabaseFromTwitch(string broadcasterId, string twitchUserId);
        Task AddDiscordXpToUser(ulong guildId, ulong channelId, ulong userId, long baseXpToAdd);
        Task AddPointsToUser(ulong serverId, ulong userId, long pointsToAdd);
        Task RemovePointsFromUser(ulong serverId, ulong userId, long pointsToRemove);
        Task SendEmbedMessage(ulong channelId, EmbedBuilder embed);
        Task SendMessage(ulong channelId, string message);
        Task AddDiscordUserToDatabaseOnGuildJoin(ulong guildId, ulong userId);
    }
}
