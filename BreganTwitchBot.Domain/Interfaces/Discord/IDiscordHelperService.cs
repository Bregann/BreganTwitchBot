using BreganTwitchBot.Domain.DTOs.Discord.Events;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Discord
{
    public interface IDiscordHelperService
    {
        Task SendMessage(ulong channelId, string message);
        Task SendEmbedMessage(ulong channelId, EmbedBuilder embed);
        bool IsUserMod(ulong serverId, ulong userId);
        string? GetTwitchUsernameFromDiscordUser(ulong userId);
        Task AddDiscordXpToUser(ulong guildId, ulong channelId, ulong userId, long xpToAdd);
        Task AddPointsToUser(ulong serverId, ulong userId, long pointsToAdd);
        Task RemovePointsFromUser(ulong serverId, ulong userId, long pointsToRemove);
    }
}
