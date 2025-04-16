using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Discord.Commands
{
    public interface IDiscordLinkingData
    {
        Task<string> NewLinkRequest(DiscordCommand command);
        Task AddRolesToUserOnLink(string twitchUserId);
        Task AddRolesToUserOnGuildJoin(ulong discordUserId, ulong guildId);
    }
}
