using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Discord
{
    public class DiscordEmbedData
    {
        public required Color Colour { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required Dictionary<string, string> Fields { get; set; }
    }
}
