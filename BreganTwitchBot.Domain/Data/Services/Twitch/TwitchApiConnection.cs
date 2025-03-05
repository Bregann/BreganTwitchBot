using BreganTwitchBot.Domain.Enums;
using Serilog;
using System.Collections.Concurrent;
using TwitchLib.Api;
using TwitchLib.Api.Core.Exceptions;

namespace BreganTwitchBot.Domain.Data.Services.Twitch
{
    /* for the bot
        * https://id.twitch.tv/oauth2/authorize
                ?response_type=code
                &client_id=
                &redirect_uri=http://localhost
                scope=clips:edit+user:write:chat+channel:moderate+moderation:read+moderator:manage:banned_users+moderator:read:blocked_terms+moderator:manage:blocked_terms+moderator:read:chat_settings+moderator:manage:chat_settings+moderator:manage:announcements+moderator:manage:chat_messages+moderator:read:chatters+user:read:chat+user:read:emotes
    */

    /* for the broadcaster
         * https://id.twitch.tv/oauth2/authorize
                ?response_type=code
                &client_id=
                &redirect_uri=http://localhost
                &scope=bits:read+channel:moderate+channel:read:subscriptions+moderation:read+channel:read:redemptions+channel:read:hype_train+channel:manage:broadcast+channel:manage:redemptions+channel:manage:polls+channel:manage:predictions+channel:manage:raids+channel:read:vips+moderator:manage:shoutouts+moderator:read:followers+moderator:manage:unban_requests
 */

    public class TwitchApiConnection
    {
        private readonly ConcurrentDictionary<string, TwitchAccount> ApiClients = new();

        /// <summary>
        /// Connects to the Twitch API using the provided credentials
        /// </summary>
        /// <param name="channelName">The twitch channel name</param>
        /// <param name="databaseChannelId">This is the database generated id from the row</param>
        /// <param name="twitchChannelClientId">The twitch channel ID</param>
        /// <param name="accessToken">The twitch channel access token</param>
        /// <param name="refreshToken">Twitch channel refresh token</param>
        /// <param name="type">if it is a bot account or a broadcaster account</param>
        /// <param name="activeChannelId">The channel the user/bot is active in (the broadcaster)</param>
        public void Connect(string channelName, int databaseChannelId, string twitchChannelClientId, string accessToken, string refreshToken, AccountType type, string activeChannelId)
        {
            try
            {
                var apiClient = new TwitchAPI();
                apiClient.Settings.ClientId = "gp762nuuoqcoxypju8c569th9wz7q5"; // TODO: move to environmental settings, hardcoded from twitchtokengenerator atm lol
                apiClient.Settings.AccessToken = accessToken;

                ApiClients[channelName] = new TwitchAccount(apiClient, databaseChannelId, twitchChannelClientId, channelName, accessToken, refreshToken, type, activeChannelId);
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

        public TwitchAccount[] GetAllApiClients()
        {
            return ApiClients.Values.ToArray();
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
            public int DatabaseChannelId { get; }
            public string TwitchChannelClientId { get; }
            public string TwitchUsername { get; }
            public string AccessToken { get; }
            public string RefreshToken { get; }
            public AccountType Type { get; }
            public string ActiveChannelId { get; }

            public TwitchAccount(TwitchAPI apiClient, int databaseChannelId, string twitchChannelClientId, string twitchUsername, string acccessToken, string refreshToken, AccountType type, string activeChannel)
            {
                ApiClient = apiClient;
                DatabaseChannelId = databaseChannelId;
                TwitchChannelClientId = twitchChannelClientId;
                TwitchUsername = twitchUsername;
                AccessToken = acccessToken;
                RefreshToken = refreshToken;
                Type = type;
                ActiveChannelId = activeChannel;
            }
        }
    }
}
