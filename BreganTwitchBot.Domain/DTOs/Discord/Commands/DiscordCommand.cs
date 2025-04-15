using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Discord.Commands
{
    public class DiscordCommand
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
    }
}
