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

        public async Task<string?> GetPointsName(string broadcasterChannelId, string broadcasterChannelName)
        {
            var sanitisedBroadcasterChannelId = broadcasterChannelId.ToLower().Trim();

            if (_pointsNames.TryGetValue(sanitisedBroadcasterChannelId, out var pointsName))
            {
                return pointsName;
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var channel = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelId == sanitisedBroadcasterChannelId);

                if (channel == null)
                {
                    Log.Error($"[Twitch Helper Service] Error getting points name for {broadcasterChannelName}, channelId is null");
                    return null;
                }

                _pointsNames[sanitisedBroadcasterChannelId] = channel.ChannelConfig.ChannelCurrencyName;
                return channel.ChannelConfig.ChannelCurrencyName;
            }
        }

        public async Task<bool> IsUserSuperModInChannel(string broadcasterChannelId, string viewerChannelId)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userData = await context.ChannelUserData.Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterChannelId && x.ChannelUser.TwitchUserId == viewerChannelId.ToLower().Trim()).FirstOrDefaultAsync();

                if (userData == null)
                {
                    return false;
                }

                return userData.IsSuperMod;
            }
        }
    }
}
