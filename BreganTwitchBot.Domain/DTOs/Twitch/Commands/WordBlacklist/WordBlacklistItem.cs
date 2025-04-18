using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.DTOs.Twitch.Commands.WordBlacklist
{
    public class WordBlacklistItem
    {
        public required string BroadcasterId { get; set; }
        public required string Word { get; set; }
        public required WordType WordType { get; set; }
    }
}
