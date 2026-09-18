using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.DTOs.Api
{
    public class GetChannelConfigResponse
    {
        public required string ChannelCurrencyName { get; set; }
        public required long CurrencyPointCap { get; set; }
        public required bool DiscordEnabled { get; set; }
    }

    public class UpdateChannelConfigRequest
    {
        public required string ChannelCurrencyName { get; set; }
        public required long CurrencyPointCap { get; set; }
    }

    public class UpsertCustomCommandRequest
    {
        public required string CommandName { get; set; }
        public required string CommandText { get; set; }
    }

    public class GetChannelRankResponse
    {
        public required int Id { get; set; }
        public required string RankName { get; set; }
        public required int RankMinutesRequired { get; set; }
        public required int BonusRankPointsEarned { get; set; }
        public ulong? DiscordRoleId { get; set; }
    }

    public class UpsertChannelRankRequest
    {
        public int? Id { get; set; }
        public required string RankName { get; set; }
        public required int RankMinutesRequired { get; set; }
        public required int BonusRankPointsEarned { get; set; }
        public ulong? DiscordRoleId { get; set; }
    }

    public class GetBlacklistWordResponse
    {
        public required int Id { get; set; }
        public required string Word { get; set; }
        public required WordType WordType { get; set; }
    }

    public class AddBlacklistWordRequest
    {
        public required string Word { get; set; }
        public required WordType WordType { get; set; }
    }

    public class GetDiscordConfigResponse
    {
        public required bool DiscordEnabled { get; set; }
        public ulong? DiscordGuildId { get; set; }
        public ulong? DiscordEventChannelId { get; set; }
        public ulong? DiscordStreamAnnouncementChannelId { get; set; }
        public ulong? DiscordUserCommandsChannelId { get; set; }
        public ulong? DiscordUserRankUpAnnouncementChannelId { get; set; }
        public ulong? DiscordGiveawayChannelId { get; set; }
        public ulong? DiscordGeneralChannelId { get; set; }
        public ulong? DiscordModeratorRoleId { get; set; }
        public ulong? DiscordWelcomeMessageChannelId { get; set; }
    }

    public class UpdateDiscordConfigRequest : GetDiscordConfigResponse;
}
