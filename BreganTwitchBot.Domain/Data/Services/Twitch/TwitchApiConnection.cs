using BreganTwitchBot.Domain.Enums;
using Serilog;
using System.Collections.Concurrent;
using TwitchLib.Api;
using TwitchLib.Api.Core.Exceptions;

namespace BreganTwitchBot.Domain.Data.Services.Twitch
{
    public class TwitchApiConnection
    {
        private readonly ConcurrentDictionary<string, TwitchAccount> ApiClients = new();

        /// <summary>
        /// Connects to the Twitch API using the provided credentials
        /// </summary>
        /// <param name="channelName">The twitch channel name</param>
        /// <param name="channelId">This is the database generated id from the row</param>
        /// <param name="clientId">The twitch channel ID</param>
        /// <param name="accessToken">The twitch channel access token</param>
        /// <param name="refreshToken">Twitch channel refresh token</param>
        /// <param name="type">if it is a bot account or a broadcaster account</param>
        public void Connect(string channelName, int channelId, string clientId, string accessToken, string refreshToken, AccountType type)
        {
            try
            {
                var apiClient = new TwitchAPI();
                apiClient.Settings.ClientId = clientId;
                apiClient.Settings.AccessToken = accessToken;

                ApiClients[channelName] = new TwitchAccount(apiClient, channelId, refreshToken, type);
                Log.Information($"[Twitch API Connection] Connected to the Twitch API for {channelName}");
            }
            catch (BadGatewayException)
            {
                Log.Fatal($"[Twitch API Connection] BadGatewayException Error connecting to the Twitch API for {channelName}");
            }
            catch (InternalServerErrorException)
            {
                Log.Fatal($"[Twitch API Connection] InternalServerErrorException Error connecting to the Twitch API for {channelName}");
            }
        }

        public TwitchAccount? GetApiClient(string key)
        {
            return ApiClients.TryGetValue(key, out var account) ? account : null;
        }

        public void RefreshApiKey(string key, string newAccessToken)
        {
            if (ApiClients.TryGetValue(key, out var account))
            {
                account.ApiClient.Settings.AccessToken = newAccessToken;
                ApiClients[key] = account;
            }
        }

        public class TwitchAccount
        {
            public TwitchAPI ApiClient { get; }
            public int ChannelId { get; }
            public string RefreshToken { get; }
            public AccountType Type { get; }

            public TwitchAccount(TwitchAPI apiClient, int channelId, string refreshToken, AccountType type)
            {
                ApiClient = apiClient;
                ChannelId = channelId;
                RefreshToken = refreshToken;
                Type = type;
            }
        }
    }
}
