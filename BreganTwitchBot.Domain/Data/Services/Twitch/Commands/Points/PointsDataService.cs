using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Points
{
    public class PointsDataService(AppDbContext dbContext, ITwitchHelperService twitchHelperService) : IPointsDataService
    {
        public async Task<string> GetPointsAsync(ChannelChatMessageReceivedParams msgParams)
        {
            var twitchIdToCheck = msgParams.ChatterChannelId;
            var twitchUsernameToCheck = msgParams.ChatterChannelName;

            // If theres more than one part to the message, we need to check if the second part is a user
            if (msgParams.MessageParts.Length > 1)
            {
                var userToCheck = await twitchHelperService.GetTwitchUserIdFromUsername(msgParams.MessageParts[1].TrimStart('@').ToLower());

                if (userToCheck == null)
                {
                    throw new TwitchUserNotFoundException($"User {msgParams.MessageParts[1]} not found");
                }

                twitchIdToCheck = userToCheck;
                twitchUsernameToCheck = msgParams.MessageParts[1].TrimStart('@');
            }

            var userPoints = await dbContext.ChannelUserData.FirstOrDefaultAsync(x => x.ChannelUser.TwitchUserId == twitchIdToCheck && x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId);
            var pointsName = await twitchHelperService.GetPointsName(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            return $"{twitchUsernameToCheck} has {(userPoints == null ? 0 : userPoints.Points.ToString("N0"))} {pointsName}. Rank: {(userPoints != null ? await GetPointsRank(msgParams.BroadcasterChannelId, twitchIdToCheck) : "N / A")}";
        }

        public async Task AddPointsAsync(ChannelChatMessageReceivedParams msgParams)
        {
            var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

            if (!isSuperMod)
            {
                throw new InvalidCommandException("You don't have permission to do that");
            }

            if (msgParams.MessageParts.Length < 3)
            {
                throw new InvalidCommandException("The format is !addpoints <username> <points>");
            }

            int.TryParse(msgParams.MessageParts[2], out var pointsToAdd);

            if (pointsToAdd <= 0)
            {
                throw new InvalidCommandException("The points to add must be a number greater than 0");
            }

            var userId = await twitchHelperService.GetTwitchUserIdFromUsername(msgParams.MessageParts[1].TrimStart('@').ToLower());

            if (userId == null)
            {
                throw new TwitchUserNotFoundException($"User {msgParams.MessageParts[1]} not found");
            }

            var rowsUpdated = await dbContext.ChannelUserData
                .Where(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.ChannelUser.TwitchUserId == userId)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(x => x.Points, x => x.Points + pointsToAdd)
                );

            if (rowsUpdated == 0)
            {
                throw new TwitchUserNotFoundException($"User {msgParams.MessageParts[1]} not found");
            }
        }

        private async Task<string> GetPointsRank(string broadcasterChannelId, string twitchUserId)
        {
            var userPoints = await dbContext.ChannelUserData
                .Where(x => x.Channel.BroadcasterTwitchChannelId == broadcasterChannelId)
                .OrderByDescending(x => x.Points)
                .Select(x => x.ChannelUser.TwitchUserId)
                .ToListAsync();

            var userRank = userPoints.FindIndex(x => x == twitchUserId) + 1;
            return $"{userRank} / {userPoints.Count}";
        }
    }
}
