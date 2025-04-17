using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
