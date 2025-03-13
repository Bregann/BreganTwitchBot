using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch;
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

    public class TwitchApiConnection : ITwitchApiConnection
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
        /// <param name="broadcasterChannelId">The channel the user/bot is active in (the broadcaster)</param>
        public void Connect(string channelName, int databaseChannelId, string twitchChannelClientId, string accessToken, string refreshToken, AccountType type, string broadcasterChannelId, string broadcasterChannelName)
        {
            try
            {
                var apiClient = new TwitchAPI();
                apiClient.Settings.ClientId = "gp762nuuoqcoxypju8c569th9wz7q5"; // TODO: move to environmental settings, hardcoded from twitchtokengenerator atm lol
                apiClient.Settings.AccessToken = accessToken;

                ApiClients[channelName.ToLower()] = new TwitchAccount(apiClient, databaseChannelId, twitchChannelClientId, channelName.ToLower(), accessToken, refreshToken, type, broadcasterChannelId, broadcasterChannelName);
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


        /// <summary>
        /// Gets the Twitch API client from the provided channel name
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public TwitchAccount? GetTwitchApiClientFromChannelName(string channelName)
        {
            return ApiClients.TryGetValue(channelName.ToLower(), out var account) ? account : null;
        }

        /// <summary>
        /// Gets the BOT Twitch API client from the provided broadcaster channel name
        /// </summary>
        /// <param name="broadcasterChannelId"></param>
        /// <returns></returns>
        public TwitchAccount? GetBotTwitchApiClientFromBroadcasterChannelId(string broadcasterChannelId)
        {
            return ApiClients.Values.FirstOrDefault(x => x.BroadcasterChannelId == broadcasterChannelId && x.Type == AccountType.Bot);
        }

        /// <summary>
        /// Gets all the Twitch API clients
        /// </summary>
        /// <returns></returns>
        public TwitchAccount[] GetAllApiClients()
        {
            return ApiClients.Values.ToArray();
        }

        public TwitchAccount[] GetAllBotApiClients()
        {
            return ApiClients.Values.Where(x => x.Type == AccountType.Bot).ToArray();
        }

        /// <summary>
        /// Refreshes the access token for the provided channel name
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="newAccessToken"></param>
        public void RefreshApiKey(string channelName, string newAccessToken)
        {
            if (ApiClients.TryGetValue(channelName, out var account))
            {
                account.ApiClient.Settings.AccessToken = newAccessToken;
                ApiClients[channelName] = account;
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
            public string BroadcasterChannelId { get; }
            public string BroadcasterChannelName { get; }

            public TwitchAccount(TwitchAPI apiClient, int databaseChannelId, string twitchChannelClientId, string twitchUsername, string acccessToken, string refreshToken, AccountType type, string broadcasterChannelId, string broadcasterChannelName)
            {
                ApiClient = apiClient;
                DatabaseChannelId = databaseChannelId;
                TwitchChannelClientId = twitchChannelClientId;
                TwitchUsername = twitchUsername;
                AccessToken = acccessToken;
                RefreshToken = refreshToken;
                Type = type;
                BroadcasterChannelId = broadcasterChannelId;
                BroadcasterChannelName = broadcasterChannelName;
            }
        }
    }
}
