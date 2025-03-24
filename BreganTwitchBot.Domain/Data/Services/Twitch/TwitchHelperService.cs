using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Exceptions;
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

        public async Task<string?> GetPointsName(string broadcasterChannelId, string broadcasterChannelName = "")
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

        /// <summary>
        /// Checks if the user has moderator permissions. This will check if they either have mod permissions, are the broadcaster or are a super mod
        /// </summary>
        /// <param name="isMod"></param>
        /// <param name="isBroadcaster"></param>
        /// <param name="viewerUsername"></param>
        /// <param name="viewerChannelId"></param>
        /// <param name="broadcasterChannelId"></param>
        /// <param name="broadcasterChannelName"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>

        public async Task EnsureUserHasModeratorPermissions(bool isMod, bool isBroadcaster, string viewerUsername, string viewerChannelId, string broadcasterChannelId, string broadcasterChannelName)
        {
            var isSuperMod = await IsUserSuperModInChannel(broadcasterChannelId, viewerChannelId);

            if (!isSuperMod && !isMod && !isBroadcaster)
            {
                Log.Warning($"User {viewerUsername} attempted to add a command without permission in channel {broadcasterChannelName}");
                throw new UnauthorizedAccessException("You are not authorised to use this command! Straight to jail Kappa");
            }
        }


        //TODO: test this method
        /// <summary>
        /// Adds points to a user in a channel
        /// </summary>
        /// <param name="broadcasterChannelId"></param>
        /// <param name="viewerChannelId"></param>
        /// <param name="pointsToAdd"></param>
        /// <param name="broadcasterChannelName"></param>
        /// <param name="viewerUsername"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TwitchUserNotFoundException"></exception>
        public async Task AddPointsToUser(string broadcasterChannelId, string viewerChannelId, int pointsToAdd, string broadcasterChannelName, string viewerUsername)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var channel = await context.Channels.FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelId == broadcasterChannelId);

                if (channel == null)
                {
                    Log.Error($"[Twitch Helper Service] Error adding points to {viewerUsername}, channelId is null");
                    throw new InvalidOperationException("Channel not found");
                }

                var user = await context.ChannelUsers.FirstOrDefaultAsync(x => x.TwitchUserId == viewerChannelId.ToLower().Trim());

                if (user == null)
                {
                    Log.Error($"[Twitch Helper Service] Error adding points to {viewerUsername}, user is null");
                    throw new TwitchUserNotFoundException("Error adding points to the user - user not found");
                }

                var userPoints = await context.ChannelUserData.FirstAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

                // check if the new amount of points will make the user above the points cap
                if (userPoints.Points + pointsToAdd > channel.ChannelConfig.CurrencyPointCap)
                {
                    userPoints.Points = channel.ChannelConfig.CurrencyPointCap;
                    await context.SaveChangesAsync();
                    Log.Information($"[Twitch Helper Service] Added {pointsToAdd} points to {viewerUsername} in {broadcasterChannelName}, but capped at {channel.ChannelConfig.CurrencyPointCap}");
                    return;
                }

                userPoints.Points += pointsToAdd;

                await context.SaveChangesAsync();
                Log.Information($"[Twitch Helper Service] Added {pointsToAdd} points to {viewerUsername} in {broadcasterChannelName}");
            }
        }
    }
}
