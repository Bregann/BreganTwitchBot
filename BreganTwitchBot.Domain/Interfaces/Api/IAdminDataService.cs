using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Api
{
    /// <summary>
    /// The write side of the admin area. Every method is addressed by channel name
    /// and the caller's permission is checked before these are reached.
    /// </summary>
    public interface IAdminDataService
    {
        Task<GetChannelConfigResponse?> GetChannelConfigAsync(string broadcasterChannelName);
        Task UpdateChannelConfigAsync(string broadcasterChannelName, UpdateChannelConfigRequest request);

        Task UpsertCustomCommandAsync(string broadcasterChannelName, UpsertCustomCommandRequest request);
        Task DeleteCustomCommandAsync(string broadcasterChannelName, string commandName);

        Task<List<GetChannelRankResponse>?> GetRanksAsync(string broadcasterChannelName);
        Task UpsertRankAsync(string broadcasterChannelName, UpsertChannelRankRequest request);
        Task DeleteRankAsync(string broadcasterChannelName, int rankId);

        Task<List<GetBlacklistWordResponse>?> GetBlacklistAsync(string broadcasterChannelName);
        Task AddBlacklistWordAsync(string broadcasterChannelName, AddBlacklistWordRequest request);
        Task DeleteBlacklistWordAsync(string broadcasterChannelName, int wordId);

        Task<GetDiscordConfigResponse?> GetDiscordConfigAsync(string broadcasterChannelName);
        Task UpdateDiscordConfigAsync(string broadcasterChannelName, UpdateDiscordConfigRequest request);

        Task<List<GetChannelPointRewardResponse>?> GetRewardsAsync(string broadcasterChannelName);
        Task UpsertRewardAsync(string broadcasterChannelName, UpsertChannelPointRewardRequest request);
        Task DeleteRewardAsync(string broadcasterChannelName, int rewardId);

        Task<List<GetSubathonRateResponse>?> GetSubathonRatesAsync(string broadcasterChannelName);
        Task UpsertSubathonRateAsync(string broadcasterChannelName, UpsertSubathonRateRequest request);
        Task DeleteSubathonRateAsync(string broadcasterChannelName, int rateId);

        Task<GetGiveawayConfigResponse?> GetGiveawayConfigAsync(string broadcasterChannelName);
        Task UpdateGiveawayConfigAsync(string broadcasterChannelName, UpdateGiveawayConfigRequest request);
    }
}
