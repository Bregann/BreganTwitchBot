using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.DTOs.Helpers
{
    public class DiscordConfig
    {
        public ulong? DiscordGuildOwnerId { get; set; } = null;
        public ulong? DiscordEventChannelId { get; set; } = null;
        public ulong? DiscordStreamAnnouncementChannelId { get; set; } = null;
        public ulong? DiscordUserLinkingChannelId { get; set; } = null;
        public ulong? DiscordUserCommandsChannelId { get; set; } = null;
        public ulong? DiscordUserRankUpAnnouncementChannelId { get; set; } = null;
        public ulong? DiscordGiveawayChannelId { get; set; } = null;
        public ulong? DiscordReactionRoleChannelId { get; set; } = null;
        public ulong? DiscordGeneralChannelId { get; set; } = null;
        public ulong? DiscordBanRoleChannelId { get; set; } = null;
        public ulong? DiscordModeratorRoleId { get; set; } = null;
    }
}
