using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Marbles
{
    public class MarblesDataService(AppDbContext dbContext, ITwitchHelperService twitchHelperService) : IMarblesDataService
    {
        public async Task<string> AddMarblesWinAsync(ChannelChatMessageReceivedParams msgParams)
        {
            await twitchHelperService.EnsureUserHasModeratorPermissions(msgParams.IsMod, msgParams.IsBroadcaster, msgParams.ChatterChannelName, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            if (msgParams.MessageParts.Length < 2)
            {
                throw new InvalidCommandException("The format is !addmarbleswin <username>");
            }

            var username = msgParams.MessageParts[1].TrimStart('@').ToLower();
            var userId = await twitchHelperService.GetTwitchUserIdFromUsername(username);

            if (userId == null)
            {
                throw new TwitchUserNotFoundException("That username does not exist :(");
            }

            var userStats = await dbContext.ChannelUserStats.FirstOrDefaultAsync(x => x.User.TwitchUserId == userId && x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId);

            if (userStats == null)
            {
                throw new TwitchUserNotFoundException($"{username} has not been seen in this channel yet");
            }

            userStats.MarblesWins++;
            await dbContext.SaveChangesAsync();

            return $"@{msgParams.ChatterChannelName} => The marbol win has been added! {username} now has {userStats.MarblesWins:N0} wins";
        }

        public async Task<string> GetMarblesWinsAsync(ChannelChatMessageReceivedParams msgParams)
        {
            var twitchIdToCheck = msgParams.ChatterChannelId;
            var twitchUsernameToCheck = msgParams.ChatterChannelName;

            // if a username was supplied then look that user up instead of the caller
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

            var userStats = await dbContext.ChannelUserStats.FirstOrDefaultAsync(x => x.User.TwitchUserId == twitchIdToCheck && x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId);

            return $"{twitchUsernameToCheck} has won {(userStats?.MarblesWins ?? 0):N0} games of marbles";
        }
    }
}
