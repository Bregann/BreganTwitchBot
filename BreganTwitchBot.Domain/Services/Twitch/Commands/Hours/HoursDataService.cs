using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Hours
{
    public class HoursDataService(AppDbContext context, ITwitchApiInteractionService twitchApiInteractionService, ITwitchApiConnection twitchApiConnection, ITwitchHelperService twitchHelperService, IDiscordRoleManagerService discordRoleManagerService, IConfigHelperService configHelperService) : IHoursDataService
    {
        public async Task UpdateWatchtimeForChannel(string broadcasterId)
        {
            var channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == broadcasterId);
            var apiClient = twitchApiConnection.GetBotApiClient();

            if (!channel.ChannelConfig.BroadcasterLive)
            {
                return;
            }

            if (apiClient == null)
            {
                Log.Error($"Unable to get api client for {channel.BroadcasterTwitchChannelName}");
                return;
            }

            Log.Information($"Updating watchtime for channel {broadcasterId}");

            var chatters = await twitchApiInteractionService.GetChatters(apiClient.ApiClient, broadcasterId, apiClient.TwitchChannelClientId);
            var channelRanks = await context.ChannelRanks.Where(x => x.ChannelId == channel.Id).ToArrayAsync();

            var discordEnabled = configHelperService.IsDiscordEnabled(broadcasterId);
            var rankUps = new List<RankUp>();

            foreach (var user in chatters.Chatters)
            {
                try
                {
                    // check if they are in the database already
                    await twitchHelperService.AddOrUpdateUserToDatabase(broadcasterId, user.UserId, channel.BroadcasterTwitchChannelName, user.UserName);

                    // grab the user
                    var dbUser = await context.ChannelUsers.FirstOrDefaultAsync(x => x.TwitchUserId == user.UserId);

                    if (dbUser == null)
                    {
                        Log.Fatal($"Error finding the db user after adding to the database - {user.UserName} {user.UserId}");
                        continue;
                    }

                    // get the watch time of the user
                    // there is a chance they have been registered in another channel so we need to be careful on the updating
                    var watchTime = await context.ChannelUserWatchtime.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId && x.ChannelUserId == dbUser.Id);

                    watchTime.MinutesInStream += 1;
                    watchTime.MinutesWatchedThisStream += 1;
                    watchTime.MinutesWatchedThisWeek += 1;
                    watchTime.MinutesWatchedThisMonth += 1;
                    watchTime.MinutesWatchedThisYear += 1;

                    // add points to the user
                    var dbUserChannelData = await context.ChannelUserData.FirstAsync(x => x.ChannelUserId == dbUser.Id && x.ChannelId == channel.Id);
                    dbUserChannelData.Points += (dbUserChannelData.IsSub | dbUserChannelData.IsVip) ? 200 : 100;

                    // check if the user has got any ranks
                    var rankEarned = channelRanks.FirstOrDefault(x => x.RankMinutesRequired == watchTime.MinutesInStream);

                    if (rankEarned != null)
                    {
                        await context.ChannelUserRankProgress.AddAsync(new ChannelUserRankProgress
                        {
                            ChannelUserId = dbUser.Id,
                            ChannelId = channel.Id,
                            ChannelRankId = rankEarned.Id,
                            AchievedAt = DateTime.UtcNow
                        });

                        await twitchHelperService.AddPointsToUser(broadcasterId, dbUser.TwitchUserId, rankEarned.BonusRankPointsEarned, channel.BroadcasterTwitchChannelName, dbUser.TwitchUsername);

                        rankUps.Add(new RankUp(dbUser.TwitchUsername, rankEarned, twitchHelperService.HasUserChattedInCurrentStream(broadcasterId, dbUser.TwitchUserId), discordEnabled && dbUser.DiscordUserId != 0));
                    }

                    await context.SaveChangesAsync();

                    // everyone linked gets their role, whether or not they end up named in chat. It used to
                    // only be applied along with a chat message, so anyone skipped missed out, and before the
                    // new rank was saved, so the role it looks up from the saved ranks was never the new one
                    if (rankEarned != null && discordEnabled && dbUser.DiscordUserId != 0)
                    {
                        await ApplyDiscordRole(dbUser.TwitchUserId, dbUser.TwitchUsername, broadcasterId);
                    }
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, $"Error updating user {user.UserName}");
                    continue;
                }
            }

            await AnnounceRankUps(broadcasterId, channel.BroadcasterTwitchChannelName, rankUps, discordEnabled);

            Log.Information($"Watchtime update completed. {chatters.Chatters.Count} users updated");
        }

        /// <summary>
        /// The most rank up messages sent in one minute, so a lot of different ranks at once can't flood chat
        /// </summary>
        public const int MaxRankUpMessagesPerMinute = 2;

        /// <summary>
        /// Chat messages there have to have been since the last rank up message, so they don't take over a quiet chat
        /// </summary>
        public const int ChatMessagesBetweenRankUpMessages = 5;

        private async Task ApplyDiscordRole(string twitchUserId, string twitchUsername, string broadcasterId)
        {
            try
            {
                await discordRoleManagerService.ApplyRoleOnDiscordWatchtimeRankup(twitchUserId, broadcasterId);
            }
            catch (Exception ex)
            {
                // the rank and watchtime are already saved, so a discord problem shouldn't lose them
                Log.Error(ex, $"Error applying the Discord rank role for {twitchUsername} in {broadcasterId}");
            }
        }

        private record RankUp(string TwitchUsername, ChannelRank Rank, bool ChattedThisStream, bool DiscordLinked);

        /// <summary>
        /// One message per rank rather than per person. It names up to three people who've chatted
        /// this stream and counts everyone else, so lurkers aren't pinged
        /// </summary>
        private async Task AnnounceRankUps(string broadcasterId, string channelName, List<RankUp> rankUps, bool discordEnabled)
        {
            if (rankUps.Count == 0)
            {
                return;
            }

            if (twitchHelperService.GetChatMessageCount(broadcasterId) < ChatMessagesBetweenRankUpMessages)
            {
                Log.Information($"Skipping {rankUps.Count} rank up messages for {broadcasterId} (fewer than {ChatMessagesBetweenRankUpMessages} chat messages since the last rank-up message)");
                return;
            }

            var messagesSent = 0;

            foreach (var rankGroup in rankUps.GroupBy(x => x.Rank.Id).OrderBy(x => x.First().Rank.RankMinutesRequired))
            {
                var rank = rankGroup.First().Rank;
                var named = rankGroup.Where(x => x.ChattedThisStream).Take(RankUpMessageHelper.MaxNamedUsers).Select(x => x.TwitchUsername).ToList();

                if (named.Count == 0)
                {
                    Log.Information($"Skipping the {rank.RankName} rank up message for {broadcasterId} (nobody who earned it has chatted in the current stream)");
                    continue;
                }

                if (messagesSent >= MaxRankUpMessagesPerMinute)
                {
                    Log.Information($"Rank up message limit reached for {broadcasterId}, skipping the {rank.RankName} rank");
                    continue;
                }

                var message = RankUpMessageHelper.BuildMessage(rank.RankName, rank.RankMinutesRequired, named, rankGroup.Count() - named.Count, discordEnabled, rankGroup.Count(x => x.DiscordLinked));
                await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channelName, message);
                messagesSent++;
            }

            if (messagesSent > 0)
            {
                twitchHelperService.ResetChatMessageCount(broadcasterId);
                Log.Information($"Reset chat message count for {broadcasterId} after sending {messagesSent} rank up messages for {rankUps.Count} rank ups");
            }
        }

        public async Task ResetMinutes()
        {
            if (DateTime.UtcNow.Day == 1 && DateTime.UtcNow.Month == 1)
            {
                await context.ChannelUserWatchtime.Where(x => x.MinutesWatchedThisYear != 0).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.MinutesWatchedThisYear, 0));
            }

            if (DateTime.UtcNow.Day == 1)
            {
                await context.ChannelUserWatchtime.Where(x => x.MinutesWatchedThisMonth != 0).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.MinutesWatchedThisMonth, 0));
            }

            if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday)
            {
                await context.ChannelUserWatchtime.Where(x => x.MinutesWatchedThisWeek != 0).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.MinutesWatchedThisWeek, 0));
            }
        }

        public async Task ResetStreamMinutesForBroadcaster(string broadcasterId)
        {
            var rowsChanged = await context.ChannelUserWatchtime.Where(x => x.MinutesWatchedThisStream != 0 && x.Channel.BroadcasterTwitchChannelId == broadcasterId).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.MinutesWatchedThisStream, 0));
            Log.Information($"Reset {rowsChanged} rows of stream minutes for broadcaster {broadcasterId}");
        }

        public async Task<string> GetHoursCommand(ChannelChatMessageReceivedParams msgParams, HoursWatchTypes hoursType)
        {
            var twitchIdToCheck = msgParams.ChatterChannelId;
            var twitchUsernameToCheck = msgParams.ChatterChannelName;

            // If theres more than one part to the message, we need to check if the second part is a user
            if (msgParams.MessageParts.Length > 1)
            {
                var userToCheck = await twitchHelperService.GetTwitchUserIdFromUsername(msgParams.MessageParts[1].TrimStart('@').ToLower());

                if (userToCheck == null)
                {
                    throw new TwitchUserNotFoundException($"User not found!");
                }

                twitchIdToCheck = userToCheck;
                twitchUsernameToCheck = msgParams.MessageParts[1].TrimStart('@');
            }

            var userWatchtime = await context.ChannelUserWatchtime.FirstOrDefaultAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId && x.ChannelUser.TwitchUserId == twitchIdToCheck);

            if (userWatchtime == null)
            {
                throw new TwitchUserNotFoundException($"Oh dear this user doesn't have any watchtime in the channel!");
            }

            var timeSpan = hoursType switch
            {
                HoursWatchTypes.Stream => TimeSpan.FromMinutes(userWatchtime.MinutesWatchedThisStream),
                HoursWatchTypes.Week => TimeSpan.FromMinutes(userWatchtime.MinutesWatchedThisWeek),
                HoursWatchTypes.Month => TimeSpan.FromMinutes(userWatchtime.MinutesWatchedThisMonth),
                HoursWatchTypes.AllTime => TimeSpan.FromMinutes(userWatchtime.MinutesInStream),
                _ => throw new ArgumentOutOfRangeException(nameof(hoursType), hoursType, null)
            };

            var msgType = hoursType switch
            {
                HoursWatchTypes.Stream => "this stream!",
                HoursWatchTypes.Week => "this week!",
                HoursWatchTypes.Month => "this month!",
                HoursWatchTypes.AllTime => "in the stream!",
                _ => throw new ArgumentOutOfRangeException(nameof(hoursType), hoursType, null)
            };

            return msgParams.MessageParts.Length > 1 ? $"{twitchUsernameToCheck} has {timeSpan.TotalMinutes} minutes (about {Math.Round(timeSpan.TotalMinutes / 60, 2)} hours) {msgType}" : $"You have {timeSpan.TotalMinutes} minutes (about {Math.Round(timeSpan.TotalMinutes / 60, 2)} hours) {msgType}";
        }
    }
}
