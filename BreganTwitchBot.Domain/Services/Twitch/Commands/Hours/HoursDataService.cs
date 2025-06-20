﻿using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Hours
{
    public class HoursDataService(AppDbContext context, ITwitchApiInteractionService twitchApiInteractionService, ITwitchApiConnection twitchApiConnection, ITwitchHelperService twitchHelperService, IDiscordRoleManagerService discordRoleManagerService, IConfigHelperService configHelperService) : IHoursDataService
    {
        public async Task UpdateWatchtimeForChannel(string broadcasterId)
        {
            var channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == broadcasterId);
            var apiClient = twitchApiConnection.GetBotTwitchApiClientFromBroadcasterChannelId(broadcasterId);

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

            var chatters = await twitchApiInteractionService.GetChattersAsync(apiClient.ApiClient, apiClient.BroadcasterChannelId, apiClient.TwitchChannelClientId);
            var channelRanks = await context.ChannelRanks.Where(x => x.ChannelId == channel.Id).ToArrayAsync();

            var rankups = 0;

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

                        var discordEnabled = configHelperService.IsDiscordEnabled(broadcasterId);

                        await twitchHelperService.AddPointsToUser(broadcasterId, dbUser.TwitchUserId, rankEarned.BonusRankPointsEarned, channel.BroadcasterTwitchChannelName, dbUser.TwitchUsername);

                        // check if there has been more than 5 rank ups, if its under then carry on as normal
                        if (rankups >= 5)
                        {
                            Log.Information($"Rank up message limit reached for {broadcasterId} - {user.UserName}");
                            continue;
                        }

                        if (!discordEnabled)
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channel.BroadcasterTwitchChannelName, $"Congrats @{dbUser.TwitchUsername}, you earned the {rankEarned.RankName} rank by watching {rankEarned.RankMinutesRequired} minutes in the stream! Keep watching to earn a higher rank!");
                        }
                        else if (discordEnabled && dbUser.DiscordUserId == 0)
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channel.BroadcasterTwitchChannelName, $"Congrats @{dbUser.TwitchUsername}, you earned the {rankEarned.RankName} rank by watching {rankEarned.RankMinutesRequired} minutes in the stream! Make sure to join the Discord and link your Twitch account to unlock your rank role!");
                        }
                        else
                        {
                            await discordRoleManagerService.ApplyRoleOnDiscordWatchtimeRankup(dbUser.TwitchUserId, broadcasterId);
                            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channel.BroadcasterTwitchChannelName, $"Congrats @{dbUser.TwitchUsername}, you earned {rankEarned.RankName} rank by watching {rankEarned.RankMinutesRequired} minutes in the stream! Your rank has been applied in the Discord");
                        }

                        rankups++;
                    }

                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, $"Error updating user {user.UserName}");
                    continue;
                }
            }

            Log.Information($"Watchtime update completed. {chatters.Chatters.Count} users updated");
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
