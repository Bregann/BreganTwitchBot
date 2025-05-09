﻿namespace BreganTwitchBot.Domain.DTOs.Helpers
{
    public class DiscordConfig
    {
        public ulong? DiscordGuildOwnerId { get; set; } = null;
        public ulong? DiscordEventChannelId { get; set; } = null;
        public ulong? DiscordStreamAnnouncementChannelId { get; set; } = null;
        public ulong? DiscordUserCommandsChannelId { get; set; } = null;
        public ulong? DiscordUserRankUpAnnouncementChannelId { get; set; } = null;
        public ulong? DiscordGiveawayChannelId { get; set; } = null;
        public ulong? DiscordModeratorRoleId { get; set; } = null;
        public ulong? DiscordWelcomeMessageChannelId { get; set; } = null;
        public ulong? DiscordGeneralChannelId { get; set; } = null;
        public ulong? DiscordGuildId { get; set; } = null;
    }
}
