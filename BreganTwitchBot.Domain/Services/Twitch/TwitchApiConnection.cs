using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.Concurrent;
using TwitchLib.Api;
using TwitchLib.Api.Core.Exceptions;

namespace BreganTwitchBot.Domain.Services.Twitch
{
    /* for the bot
        * https://id.twitch.tv/oauth2/authorize
                ?response_type=code
                &client_id=
                &redirect_uri=http://localhost
                &scope=clips:edit+moderator:read:suspicious_users+user:write:chat+moderator:manage:warnings+channel:moderate+moderation:read+moderator:manage:banned_users+moderator:read:blocked_terms+moderator:manage:blocked_terms+moderator:read:chat_settings+moderator:manage:chat_settings+moderator:manage:announcements+moderator:manage:chat_messages+moderator:read:chatters+user:read:chat+user:read:emotes
    */

    /* for the broadcaster
         * https://id.twitch.tv/oauth2/authorize
                ?response_type=code
                &client_id=
                &redirect_uri=http://localhost
                &scope=bits:read+channel:moderate+moderator:read:chatters+channel:read:subscriptions+moderation:read+channel:read:redemptions+channel:read:hype_train+channel:manage:broadcast+channel:manage:redemptions+channel:manage:polls+channel:manage:predictions+channel:manage:raids+channel:read:vips+moderator:manage:shoutouts+moderator:read:followers+moderator:manage:unban_requests
 */

    /// <summary>
    /// Holds the Twitch API clients.
    ///
    /// There is a SINGLE bot account shared across every channel - it reads chat, sends messages
    /// and moderates everywhere, and its credentials come from the environmental settings rather
    /// than from a channel row.
    ///
    /// Each channel still has its OWN broadcaster account. These are deliberately kept per channel
    /// because Twitch only exposes a channel's own data to that broadcaster's token - subscriptions,
    /// cheers/bits, follows, polls, predictions, channel point redemptions and bans can't be read
    /// with the bot's token no matter what the bot is granted in the channel.
    /// </summary>
    public class TwitchApiConnection(IServiceProvider serviceProvider, IEnvironmentalSettingHelper environmentalSettingHelper) : ITwitchApiConnection
    {
        private readonly ConcurrentDictionary<string, TwitchAccount> _broadcasterClients = new();
        private readonly ConcurrentDictionary<string, ChannelDetails> _channels = new();
        private TwitchAccount? _botAccount;

        public async Task InitialiseConnectionsAsync()
        {
            using var scope = serviceProvider.CreateScope();
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var channelsToConnectTo = await dbContext.Channels.ToArrayAsync();

                ConnectBot();

                foreach (var channel in channelsToConnectTo)
                {
                    ConnectBroadcaster(channel.BroadcasterTwitchChannelName, channel.Id, channel.BroadcasterTwitchChannelId,
                        channel.BroadcasterTwitchChannelOAuthToken, channel.BroadcasterTwitchChannelRefreshToken);

                    _channels[channel.BroadcasterTwitchChannelId] = new ChannelDetails(channel.Id, channel.BroadcasterTwitchChannelId, channel.BroadcasterTwitchChannelName);
                }
            }
        }

        /// <summary>
        /// Connects the single bot account that is used across every channel. Its credentials are
        /// stored in the environmental settings rather than against an individual channel.
        /// </summary>
        public void ConnectBot()
        {
            var botChannelId = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.BotTwitchChannelId);
            var botChannelName = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.BotTwitchChannelName);
            var accessToken = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.BotTwitchChannelOAuthToken);
            var refreshToken = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.BotTwitchChannelRefreshToken);

            if (string.IsNullOrWhiteSpace(botChannelId) || string.IsNullOrWhiteSpace(botChannelName) || string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                Log.Fatal("[Twitch API Connection] Bot account credentials are missing from the environmental settings. The bot will not connect");
                return;
            }

            _botAccount = CreateAccount(botChannelName, botChannelId, accessToken, refreshToken, AccountType.Bot, null);

            if (_botAccount != null)
            {
                Log.Information($"[Twitch API Connection] Connected to the Twitch API for the bot account {botChannelName}");
            }
        }

        /// <summary>
        /// Connects a broadcaster account to the Twitch API
        /// </summary>
        /// <param name="channelName">The twitch channel name</param>
        /// <param name="databaseChannelId">This is the database generated id from the row</param>
        /// <param name="broadcasterChannelId">The broadcaster's twitch channel ID</param>
        /// <param name="accessToken">The twitch channel access token</param>
        /// <param name="refreshToken">Twitch channel refresh token</param>
        public void ConnectBroadcaster(string channelName, int databaseChannelId, string broadcasterChannelId, string accessToken, string refreshToken)
        {
            var account = CreateAccount(channelName, broadcasterChannelId, accessToken, refreshToken, AccountType.Broadcaster, databaseChannelId);

            if (account == null)
            {
                return;
            }

            _broadcasterClients[channelName.ToLower()] = account;
            Log.Information($"[Twitch API Connection] Connected to the Twitch API for {channelName}");
        }

        private TwitchAccount? CreateAccount(string channelName, string twitchChannelClientId, string accessToken, string refreshToken, AccountType type, int? databaseChannelId)
        {
            try
            {
                var apiClient = new TwitchAPI();
                apiClient.Settings.ClientId = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.TwitchAPIClientID);
                apiClient.Settings.AccessToken = accessToken;

                return new TwitchAccount(apiClient, twitchChannelClientId, channelName.ToLower(), accessToken, refreshToken, type, databaseChannelId);
            }
            catch (BadGatewayException)
            {
                Log.Fatal($"[Twitch API Connection] BadGatewayException Error connecting to the Twitch API for {channelName}");
            }
            catch (InternalServerErrorException)
            {
                Log.Fatal($"[Twitch API Connection] InternalServerErrorException Error connecting to the Twitch API for {channelName}");
            }

            return null;
        }

        /// <summary>
        /// Gets the single bot account that is shared across all channels
        /// </summary>
        public TwitchAccount? GetBotApiClient()
        {
            return _botAccount;
        }

        /// <summary>
        /// Gets the broadcaster Twitch API client from the provided channel name
        /// </summary>
        public TwitchAccount? GetBroadcasterApiClientFromChannelName(string channelName)
        {
            return _broadcasterClients.TryGetValue(channelName.ToLower(), out var account) ? account : null;
        }

        /// <summary>
        /// Gets all the broadcaster Twitch API clients
        /// </summary>
        public TwitchAccount[] GetAllBroadcasterApiClients()
        {
            return _broadcasterClients.Values.ToArray();
        }

        public string[] GetAllBroadcasterChannelIds()
        {
            return _channels.Keys.ToArray();
        }

        /// <summary>
        /// Gets the details of every channel the bot is active in
        /// </summary>
        public ChannelDetails[] GetAllChannels()
        {
            return _channels.Values.ToArray();
        }

        /// <summary>
        /// Gets the details of a single channel from its broadcaster channel id
        /// </summary>
        public ChannelDetails? GetChannelDetails(string broadcasterChannelId)
        {
            return _channels.TryGetValue(broadcasterChannelId, out var channel) ? channel : null;
        }

        /// <summary>
        /// Refreshes all the access tokens - the single bot account plus every broadcaster account
        /// </summary>
        public async Task RefreshAllApiKeys()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var environmentalSettingHelper = scope.ServiceProvider.GetRequiredService<IEnvironmentalSettingHelper>();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var clientSecret = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.TwitchAPISecret);

                if (_botAccount != null)
                {
                    try
                    {
                        var newAccessToken = await _botAccount.ApiClient.Auth.RefreshAuthTokenAsync(_botAccount.RefreshToken, clientSecret);
                        _botAccount.ApiClient.Settings.AccessToken = newAccessToken.AccessToken;
                        _botAccount.AccessToken = newAccessToken.AccessToken;
                        _botAccount.RefreshToken = newAccessToken.RefreshToken;

                        var accessTokenSaved = await environmentalSettingHelper.UpdateEnviromentalSettingValue(EnvironmentalSettingEnum.BotTwitchChannelOAuthToken, newAccessToken.AccessToken);
                        var refreshTokenSaved = await environmentalSettingHelper.UpdateEnviromentalSettingValue(EnvironmentalSettingEnum.BotTwitchChannelRefreshToken, newAccessToken.RefreshToken);

                        if (!accessTokenSaved || !refreshTokenSaved)
                        {
                            // the settings rows are missing, so the new tokens only exist in memory
                            // and will be lost on restart - the old refresh token is now spent
                            Log.Fatal("[Twitch API Connection] Refreshed the bot token but could not save it. The BotTwitchChannelOAuthToken/BotTwitchChannelRefreshToken rows are missing from the environmental settings");
                        }

                        Log.Information($"[Twitch API Connection] Refreshed access token for the bot account {_botAccount.TwitchUsername}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[Twitch API Connection] Error refreshing access token for the bot account {_botAccount.TwitchUsername}: {ex.Message}");
                    }
                }

                foreach (var apiClient in _broadcasterClients.Values)
                {
                    try
                    {
                        var newAccessToken = await apiClient.ApiClient.Auth.RefreshAuthTokenAsync(apiClient.RefreshToken, clientSecret);
                        apiClient.ApiClient.Settings.AccessToken = newAccessToken.AccessToken;
                        apiClient.AccessToken = newAccessToken.AccessToken;
                        apiClient.RefreshToken = newAccessToken.RefreshToken;

                        // update the access token in the database
                        var channel = apiClient.DatabaseChannelId == null ? null : await context.Channels.FindAsync(apiClient.DatabaseChannelId);
                        if (channel != null)
                        {
                            channel.BroadcasterTwitchChannelOAuthToken = newAccessToken.AccessToken;
                            channel.BroadcasterTwitchChannelRefreshToken = newAccessToken.RefreshToken;

                            await context.SaveChangesAsync();
                        }

                        Log.Information($"[Twitch API Connection] Refreshed access token for {apiClient.TwitchUsername}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[Twitch API Connection] Error refreshing access token for {apiClient.TwitchUsername}: {ex.Message}");
                    }
                }
            }
        }

        public class TwitchAccount(TwitchAPI apiClient, string twitchChannelClientId, string twitchUsername, string acccessToken, string refreshToken, AccountType type, int? databaseChannelId = null)
        {
            public TwitchAPI ApiClient { get; } = apiClient;
            public string TwitchChannelClientId { get; } = twitchChannelClientId;
            public string TwitchUsername { get; } = twitchUsername;
            public string AccessToken { get; set; } = acccessToken;
            public string RefreshToken { get; set; } = refreshToken;
            public AccountType Type { get; } = type;

            /// <summary>
            /// The database generated channel id. Only set for broadcaster accounts - the bot
            /// account is not tied to a single channel row.
            /// </summary>
            public int? DatabaseChannelId { get; } = databaseChannelId;
        }

        /// <summary>
        /// A channel the bot is active in. The bot uses these to know which channels to join
        /// and to look up a channel's name/id without needing a per channel bot account.
        /// </summary>
        public class ChannelDetails(int databaseChannelId, string broadcasterChannelId, string broadcasterChannelName)
        {
            public int DatabaseChannelId { get; } = databaseChannelId;
            public string BroadcasterChannelId { get; } = broadcasterChannelId;
            public string BroadcasterChannelName { get; } = broadcasterChannelName;
        }
    }
}
