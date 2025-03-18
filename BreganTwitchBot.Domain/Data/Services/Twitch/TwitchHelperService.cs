using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BreganTwitchBot.Domain.Data.Services.Twitch
{
    public class TwitchHelperService(ITwitchApiConnection connection, IServiceProvider serviceProvider, ITwitchApiInteractionService twitchApiInteractionService) : ITwitchHelperService
    {
        private readonly Dictionary<string, string> _pointsNames = [];

        /// <summary>
        /// Sends a message to a Twitch channel
        /// </summary>
        /// <param name="broadcasterChannelId"></param>
        /// <param name="broadcasterChannelName"></param>
        /// <param name="message"></param>
        /// <param name="originalMessageId"></param>
        /// <returns></returns>
        public async Task SendTwitchMessageToChannel(string broadcasterChannelId, string broadcasterChannelName, string message, string? originalMessageId = null)
        {
            var apiClient = connection.GetBotTwitchApiClientFromBroadcasterChannelId(broadcasterChannelId);

            if (apiClient == null)
            {
                Log.Error($"[Twitch Helper Service] Error sending message to {broadcasterChannelName}, apiClient is null");
                return;
            }

            try
            {
                await twitchApiInteractionService.SendChatMessage(apiClient.ApiClient, apiClient.BroadcasterChannelId, apiClient.TwitchChannelClientId, message, originalMessageId);
                Log.Information($"[Twitch Helper Service] Sent message to {broadcasterChannelName}. Message contents: {message}");
            }
            catch (Exception ex)
            {
                Log.Error($"[Twitch Helper Service] Error sending message to {broadcasterChannelName}, {ex.Message}");
            }
        }

        public async Task<string?> GetTwitchUserIdFromUsername(string username)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var user = await context.ChannelUsers.FirstOrDefaultAsync(x => x.TwitchUsername == username.ToLower().Trim());
                return user?.TwitchUserId;
            }
        }

        public async Task<string?> GetPointsName(string broadcasterChannelName)
        {
            if (_pointsNames.TryGetValue(broadcasterChannelName, out var pointsName))
            {
                return pointsName;
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var channelId = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelName == broadcasterChannelName);

                if (channelId == null)
                {
                    Log.Error($"[Twitch Helper Service] Error getting points name for {broadcasterChannelName}, channelId is null");
                    return null;
                }

                var channel = await context.ChannelConfig.FirstOrDefaultAsync(x => x.ChannelId == channelId.Id);

                if (channel == null)
                {
                    Log.Error($"[Twitch Helper Service] Error getting points name for {channelId.BroadcasterTwitchChannelName}, channel is null");
                    return string.Empty;
                }

                _pointsNames[broadcasterChannelName] = channel.ChannelCurrencyName;
                return channel.ChannelCurrencyName;
            }
        }

        public async Task<bool> IsUserSuperModInChannel(string broadcasterChannelName, string viewerName)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userData = await context.ChannelUserData.Where(x => x.Channel.BroadcasterTwitchChannelName == broadcasterChannelName && x.ChannelUser.TwitchUsername == viewerName.ToLower().Trim()).FirstOrDefaultAsync();

                if (userData == null)
                {
                    return false;
                }

                return userData.IsSuperMod;
            }
        }
    }
}
