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

        public async Task<string> GetPointsName(string broadcasterChannelId, string broadcasterChannelName = "")
        {
            var sanitisedBroadcasterChannelId = broadcasterChannelId.ToLower().Trim();

            if (_pointsNames.TryGetValue(sanitisedBroadcasterChannelId, out var pointsName))
            {
                return pointsName;
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == sanitisedBroadcasterChannelId);

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
        public async Task AddPointsToUser(string broadcasterChannelId, string viewerChannelId, long pointsToAdd, string broadcasterChannelName, string viewerUsername)
        {
            if (pointsToAdd <= 0)
            {
                Log.Warning($"[Twitch Helper Service] Error adding points to {viewerUsername}, pointsToAdd is less than or equal to 0");
                throw new InvalidOperationException("Points to add must be greater than 0");
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == broadcasterChannelId);

                var userPoints = await context.ChannelUserData.FirstOrDefaultAsync(x => x.Channel.BroadcasterTwitchChannelId == broadcasterChannelId && x.ChannelUser.TwitchUserId == viewerChannelId);

                if (userPoints == null)
                {
                    Log.Warning($"[Twitch Helper Service] Error removing points from {viewerUsername}, userPoints is null");
                    throw new TwitchUserNotFoundException($"User {viewerUsername} not found in channel {broadcasterChannelName} when attempting to add points");
                }

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

        /// <summary>
        /// Removes points from a user in a channel
        /// </summary>
        /// <param name="broadcasterChannelId"></param>
        /// <param name="viewerChannelId"></param>
        /// <param name="pointsToRemove"></param>
        /// <param name="broadcasterChannelName"></param>
        /// <param name="viewerUsername"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TwitchUserNotFoundException"></exception>
        public async Task RemovePointsFromUser(string broadcasterChannelId, string viewerChannelId, long pointsToRemove, string broadcasterChannelName, string viewerUsername)
        {
            if (pointsToRemove <= 0)
            {
                Log.Warning($"[Twitch Helper Service] Error removing points from {viewerUsername}, pointsToRemove is less than or equal to 0");
                throw new InvalidOperationException("Points to remove must be greater than 0");
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var userPoints = await context.ChannelUserData.FirstOrDefaultAsync(x => x.Channel.BroadcasterTwitchChannelId == broadcasterChannelId && x.ChannelUser.TwitchUserId == viewerChannelId);
                
                if(userPoints == null)
                {
                    Log.Warning($"[Twitch Helper Service] Error removing points from {viewerUsername}, userPoints is null");
                    throw new TwitchUserNotFoundException($"User {viewerUsername} not found in channel {broadcasterChannelName} when attempting to remove points");
                }

                // check if the new amount of points will make the user below 0
                if (userPoints.Points - pointsToRemove < 0)
                {
                    userPoints.Points = 0;
                    await context.SaveChangesAsync();
                    Log.Information($"[Twitch Helper Service] Removed {pointsToRemove} points from {viewerUsername} in {broadcasterChannelName}, but capped at 0");
                    return;
                }

                userPoints.Points -= pointsToRemove;
                await context.SaveChangesAsync();

                Log.Information($"[Twitch Helper Service] Removed {pointsToRemove} points from {viewerUsername} in {broadcasterChannelName}");
            }
        }

        public async Task<bool> IsBroadcasterLive(string broadcasterChannelId)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var config = await context.ChannelConfig.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == broadcasterChannelId);

                return config.BroadcasterLive;
            }
        }

        public async Task<long> GetPointsForUser(string broadcasterChannelId, string userChannelId, string broadcasterUsername, string userChannelName)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var userPoints = await context.ChannelUserData.FirstOrDefaultAsync(x => x.Channel.BroadcasterTwitchChannelId == broadcasterChannelId && x.ChannelUser.TwitchUserId == userChannelId);

                if (userPoints == null)
                {
                    Log.Warning($"[Twitch Helper Service] Error getting points for {userChannelName}, userPoints is null");
                    throw new TwitchUserNotFoundException($"User {userChannelName} not found in channel {broadcasterUsername}");
                }

                return userPoints.Points;
            }
        }
    }
}
