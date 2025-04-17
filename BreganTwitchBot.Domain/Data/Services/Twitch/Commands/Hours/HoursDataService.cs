using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Hours
{
    public class HoursDataService(AppDbContext context, ITwitchApiInteractionService twitchApiInteractionService, ITwitchApiConnection twitchApiConnection, ITwitchHelperService twitchHelperService, IDiscordRoleManagerService discordRoleManagerService) : IHoursDataService
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

            var chatters = await twitchApiInteractionService.GetChattersAsync(apiClient.ApiClient, apiClient.BroadcasterChannelId, apiClient.TwitchChannelClientId);
            var channelRanks = await context.ChannelRanks.Where(x => x.ChannelId == channel.Id).ToArrayAsync();

            var rankups = 0;

            foreach (var user in chatters.Chatters)
            {
                try
                {
                    // check if they are in the database already
                    var dbUser = await context.ChannelUsers.FirstOrDefaultAsync(x => x.TwitchUserId == user.UserId);

                    if (dbUser == null)
                    {
                        await twitchHelperService.AddOrUpdateUserToDatabase(broadcasterId, user.UserId, channel.BroadcasterTwitchChannelName, user.UserName, true);
                        continue;
                    }
                    else
                    {
                        // get the watch time of the user
                        // there is a chance they have been registered in another channel so we need to be careful on the updating
                        var watchTime = context.ChannelUserWatchtime.FirstOrDefault(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId && x.ChannelUserId == dbUser.Id);

                        if (watchTime == null)
                        {
                            await context.ChannelUserWatchtime.AddAsync(new ChannelUserWatchtime
                            {
                                ChannelId = channel.Id,
                                ChannelUserId = dbUser.Id,
                                MinutesInStream = 1,
                                MinutesWatchedThisMonth = 1,
                                MinutesWatchedThisStream = 1,
                                MinutesWatchedThisWeek = 1,
                                MinutesWatchedThisYear = 1
                            });
                        }
                        else
                        {
                            watchTime.MinutesInStream += 1;
                            watchTime.MinutesWatchedThisStream += 1;
                            watchTime.MinutesWatchedThisWeek += 1;
                            watchTime.MinutesWatchedThisMonth += 1;
                            watchTime.MinutesWatchedThisYear += 1;

                            // check if the user has got any ranks
                            var rankEarned = channelRanks.FirstOrDefault(x => x.RankMinutesRequired == watchTime.MinutesInStream);

                            if (rankEarned != null && rankups != 10)
                            {
                                await context.ChannelUserRankProgress.AddAsync(new ChannelUserRankProgress
                                {
                                    ChannelUserId = dbUser.Id,
                                    ChannelId = channel.Id,
                                    ChannelRankId = rankEarned.Id,
                                    AchievedAt = DateTime.UtcNow
                                });

                                await twitchHelperService.AddPointsToUser(broadcasterId, dbUser.TwitchUserId, rankEarned.BonusRankPointsEarned, channel.BroadcasterTwitchChannelName, dbUser.TwitchUsername);
                                
                                if (!channel.DiscordEnabled)
                                {
                                    await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channel.BroadcasterTwitchChannelName, $"Congrats you earned the {rankEarned.RankName} rank by watching {rankEarned.RankMinutesRequired} minutes in the stream! Keep watching to earn a higher rank!");
                                }
                                else if (channel.DiscordEnabled && dbUser.DiscordUserId == 0)
                                {
                                    await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channel.BroadcasterTwitchChannelName, $"Congrats you earned the {rankEarned.RankName} rank by watching {rankEarned.RankMinutesRequired} minutes in the stream! Make sure to join the Discord and link your Twitch account to unlock your rank role!");
                                }
                                else
                                {
                                    await discordRoleManagerService.ApplyRoleOnDiscordWatchtimeRankup(dbUser.TwitchUserId, broadcasterId);
                                    await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, channel.BroadcasterTwitchChannelName, $"Congrats you earned {rankEarned.RankName} rank by watching {rankEarned.RankMinutesRequired} minutes in the stream! Your rank has been applied in the Discord");
                                }

                                rankups++;
                            }
                        }
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
