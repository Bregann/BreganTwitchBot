using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot
{
    public class StreamStatsService
    {
        public static async Task UpdateStreamStat(long amount, StatTypes statType)
        {
            if (!AppConfig.StreamerLive)
            {
                return;
            }

            using (var context = new DatabaseContext())
            {
                switch (statType)
                {
                    case StatTypes.BitsDonated:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().BitsDonated += amount;
                        break;

                    case StatTypes.CommandsSent:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().CommandsSent += amount;
                        break;

                    case StatTypes.DiscordRanksEarnt:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().DiscordRanksEarnt += amount;
                        break;

                    case StatTypes.MessagesReceived:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().MessagesReceived += amount;
                        break;

                    case StatTypes.PointsGainedSubscribing:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().PointsGainedSubscribing += amount;
                        break;

                    case StatTypes.PointsGainedWatching:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().PointsGainedWatching += amount;
                        break;

                    case StatTypes.PointsGambled:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().PointsGambled += amount;
                        break;

                    case StatTypes.PointsLost:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().PointsLost += amount;
                        break;

                    case StatTypes.PointsWon:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().PointsWon += amount;
                        break;

                    case StatTypes.TotalBans:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().TotalBans += amount;
                        break;

                    case StatTypes.TotalTimeouts:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().TotalTimeouts += amount;
                        break;

                    case StatTypes.NewGiftedSubs:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().NewGiftedSubs += amount;
                        break;

                    case StatTypes.TotalSpins:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().TotalSpins += amount;
                        break;

                    case StatTypes.KappaWins:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().KappaWins += amount;
                        break;

                    case StatTypes.ForeheadWins:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().ForeheadWins += amount;
                        break;

                    case StatTypes.LULWins:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().LulWins += amount;
                        break;

                    case StatTypes.SMOrcWins:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().SmorcWins += amount;
                        break;

                    case StatTypes.jackpotWins:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().JackpotWins += amount;
                        break;

                    case StatTypes.TotalUsersClaimed:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().TotalUsersClaimed += amount;
                        break;

                    case StatTypes.TotalPointsClaimed:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().TotalPointsClaimed += amount;
                        break;

                    case StatTypes.AmountOfUsersReset:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().AmountOfUsersReset += amount;
                        break;

                    case StatTypes.AmountOfRewardsRedeemd:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().AmountOfRewardsRedeemed += amount;
                        break;

                    case StatTypes.RewardRedeemCost:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().RewardRedeemCost += amount;
                        break;

                    case StatTypes.AmountOfDiscordUsersJoined:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().AmountOfDiscordUsersJoined += amount;
                        break;

                    case StatTypes.NewSubscriber:
                        context.StreamStats.OrderBy(x => x.StreamId).Last().NewSubscribers += amount;
                        break;
                }

                await context.SaveChangesAsync();
            }
        }

        public static async Task AddNewStream()
        {
            long followerCount = 0;
            int subCount = 0;

            try
            {
                var followerCountApiRequest = await TwitchApiConnection.ApiClient.Helix.Users.GetUsersFollowsAsync(toId: AppConfig.TwitchChannelID);
                followerCount = followerCountApiRequest.TotalFollows;

                var subCountApiRequest = await TwitchApiConnection.ApiClient.Helix.Subscriptions.GetBroadcasterSubscriptionsAsync(AppConfig.TwitchChannelID, accessToken: AppConfig.BroadcasterOAuth);
                subCount = subCountApiRequest.Total;
            }
            catch (Exception exception)
            {
                Log.Fatal($"[Stream Stats] Error getting API data for AddNewStream: {exception}");
            }

            using (var context = new DatabaseContext())
            {
                long streamId = 0;

                if (context.StreamStats.Any())
                {
                    streamId = context.StreamStats.OrderBy(x => x.StreamId).Last().StreamId;
                }

                var newStreamId = streamId + 1;

                //Clear out the tables
                context.UniqueViewers.FromSqlRaw("DELETE from UniqueViewers");
                context.StreamViewCount.FromSqlRaw("DELETE from StreamViewCount");

                //Add in a new stream stats
                context.StreamStats.Add(new StreamStats
                {
                    StreamId = newStreamId,
                    AmountOfDiscordUsersJoined = 0,
                    AmountOfRewardsRedeemed = 0,
                    AmountOfUsersReset = 0,
                    AvgViewCount = 0,
                    BitsDonated = 0,
                    CommandsSent = 0,
                    DiscordRanksEarnt = 0,
                    EndingFollowerCount = 0,
                    EndingSubscriberCount = 0,
                    ForeheadWins = 0,
                    GiftedPoints = 0,
                    JackpotWins = 0,
                    KappaWins = 0,
                    LulWins = 0,
                    MessagesReceived = 0,
                    NewFollowers = 0,
                    NewGiftedSubs = 0,
                    NewSubscribers = 0,
                    PeakViewerCount = 0,
                    PointsGainedSubscribing = 0,
                    PointsGainedWatching = 0,
                    PointsGambled = 0,
                    PointsLost = 0,
                    PointsWon = 0,
                    RewardRedeemCost = 0,
                    SmorcWins = 0,
                    SongRequestsBlacklisted = 0,
                    SongRequestsLiked = 0,
                    SongRequestsSent = 0,
                    StartingFollowerCount = followerCount,
                    StartingSubscriberCount = subCount,
                    StreamEnded = new DateTime(0),
                    StreamStarted = DateTime.UtcNow,
                    TotalBans = 0,
                    TotalPointsClaimed = 0,
                    TotalSpins = 0,
                    TotalTimeouts = 0,
                    TotalUsersClaimed = 0,
                    UniquePeople = 0,
                    Uptime = TimeSpan.FromSeconds(1)
                });

                await context.SaveChangesAsync();

                Log.Information($"[Stream Stats] New stream {newStreamId} added");
            }
        }

        public static async Task GetUserListAndViewCountAndAddToTables()
        {
            if (AppConfig.StreamAnnounced && AppConfig.StreamerLive)
            {
                int viewCount = 0;

                try
                {
                    var streamInfo = await TwitchApiConnection.ApiClient.Helix.Streams.GetStreamsAsync(userIds: new List<string> { AppConfig.TwitchChannelID });

                    if (streamInfo.Streams.Length != 0)
                    {
                        viewCount = streamInfo.Streams[0].ViewerCount;
                    }
                    else
                    {
                        viewCount = 0;
                    }
                }
                catch (Exception exception)
                {
                    Log.Fatal($"[Stream Stats] Error getting API data: {exception}");
                    viewCount = 0;
                }

                using (var context = new DatabaseContext())
                {
                    context.StreamViewCount.Add(new StreamViewCount
                    {
                        Time = DateTime.UtcNow,
                        ViewCount = viewCount
                    });

                    var usersToAdd = context.Users.Where(x => x.InStream == true).Select(chatter => new UniqueViewers
                    {
                        Username = chatter.Username
                    }).ToList();

                    context.UniqueViewers.UpdateRange(usersToAdd);
                    await context.SaveChangesAsync();

                    Log.Information($"[Stream Stats] {viewCount} view count added and {usersToAdd.Count} unique viewers added");
                }
            }
        }

        public static async Task CalculateEndStreamData()
        {
            long followerCount = 0;
            int subCount = 0;

            try
            {
                var followerCountApiRequest = await TwitchApiConnection.ApiClient.Helix.Users.GetUsersFollowsAsync(toId: AppConfig.TwitchChannelID);
                followerCount = followerCountApiRequest.TotalFollows;

                var subCountApiRequest = await TwitchApiConnection.ApiClient.Helix.Subscriptions.GetBroadcasterSubscriptionsAsync(AppConfig.TwitchChannelID, accessToken: AppConfig.BroadcasterOAuth);
                subCount = subCountApiRequest.Total;
            }
            catch (Exception exception)
            {
                Log.Fatal($"[Stream Stats] Error getting API data for CalculateEndStreamData: {exception}");
                followerCount = 0;
                subCount = 0;
            }

            using (var context = new DatabaseContext())
            {
                var averageViewers = context.StreamViewCount.ToList().Average(x => x.ViewCount);
                var peakViewCount = context.StreamViewCount.OrderByDescending(x => x.ViewCount).First();

                //Set the end stream stuff
                var streamStats = context.StreamStats.OrderBy(x => x.StreamId).Last();
                streamStats.StreamEnded = DateTime.UtcNow;
                streamStats.Uptime = streamStats.StreamEnded - streamStats.StreamStarted;
                streamStats.AvgViewCount = averageViewers;
                streamStats.PeakViewerCount = peakViewCount.ViewCount;
                streamStats.EndingFollowerCount = followerCount;
                streamStats.EndingSubscriberCount = subCount;
                streamStats.NewFollowers = streamStats.StartingFollowerCount - followerCount;
                context.SaveChanges();
            }

            await DiscordHelper.SendStreamStats();
        }
    }
}