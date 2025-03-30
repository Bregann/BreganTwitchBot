using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Hours
{
    public class HoursDataService(AppDbContext context, ITwitchApiInteractionService twitchApiInteractionService, ITwitchApiConnection twitchApiConnection, ITwitchHelperService twitchHelperService) : IHoursDataService
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
                        var watchTime = dbUser.ChannelUserWatchtimes.FirstOrDefault(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId);

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
    }
}
