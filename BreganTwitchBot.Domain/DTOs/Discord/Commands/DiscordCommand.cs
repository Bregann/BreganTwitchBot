using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Discord.Commands
{
    public class DiscordCommand
    {
        public required ulong GuildId { get; set; }
        public required ulong UserId { get; set; }
        public required ulong ChannelId { get; set; }
        public string? CommandText { get; set; }
    }
}
