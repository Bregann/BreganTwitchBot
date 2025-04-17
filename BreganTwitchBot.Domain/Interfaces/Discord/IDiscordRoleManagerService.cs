using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Discord
{
    public interface IDiscordRoleManagerService
    {
        Task AddRolesToUserOnGuildJoin(ulong discordUserId, ulong guildId);
        Task AddRolesToUserOnLink(string twitchUserId);
        Task ApplyRoleOnDiscordWatchtimeRankup(string twitchUserId, string broadcasterChannelId);
    }
}
