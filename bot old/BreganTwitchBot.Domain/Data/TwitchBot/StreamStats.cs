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
        private static StreamStatsData _streamData = new()
        {
            AmountOfDiscordUsersJoined = 0,
            AmountOfRewardsRedeemed = 0,
            AmountOfUsersReset = 0,
            BitsDonated = 0,
            CommandsSent = 0,
            DiscordRanksEarnt = 0,
            ForeheadWins = 0,
            JackpotWins = 0,
            KappaWins = 0,
            LulWins = 0,
            MessagesReceived = 0,
            NewFollowers = 0,
            NewGiftedSubs = 0,
            NewSubscribers = 0,
            PointsGainedSubscribing = 0,
            PointsGainedWatching = 0,
            PointsGambled = 0,
            PointsLost = 0,
            PointsWon = 0,
            RewardRedeemCost = 0,
            SmorcWins = 0,
            StreamId = 0,
            TotalBans = 0,
            TotalPointsClaimed = 0,
            TotalSpins = 0,
            TotalTimeouts = 0,
            TotalUsersClaimed = 0,
            UniquePeople = 0
        };

        private static List<UniqueViewers> _usersToAdd = new();

        public static void UpdateStreamStat(long amount, StatTypes statType)
        {
            if (!AppConfig.StreamerLive)
            {
                return;
            }

            switch (statType)
            {
                case StatTypes.BitsDonated:
                    _streamData.BitsDonated += amount;
                    break;

                case StatTypes.CommandsSent:
                    _streamData.CommandsSent += amount;
                    break;

                case StatTypes.DiscordRanksEarnt:
                    _streamData.DiscordRanksEarnt += amount;
                    break;

                case StatTypes.MessagesReceived:
                    _streamData.MessagesReceived += amount;
                    break;

                case StatTypes.PointsGainedSubscribing:
                    _streamData.PointsGainedSubscribing += amount;
                    break;

                case StatTypes.PointsGainedWatching:
                    _streamData.PointsGainedWatching += amount;
                    break;

                case StatTypes.PointsGambled:
                    _streamData.PointsGambled += amount;
                    break;

                case StatTypes.PointsLost:
                    _streamData.PointsLost += amount;
                    break;

                case StatTypes.PointsWon:
                    _streamData.PointsWon += amount;
                    break;

                case StatTypes.TotalBans:
                    _streamData.TotalBans += amount;
                    break;

                case StatTypes.TotalTimeouts:
                    _streamData.TotalTimeouts += amount;
                    break;

                case StatTypes.NewGiftedSubs:
                    _streamData.NewGiftedSubs += amount;
                    break;

                case StatTypes.TotalSpins:
                    _streamData.TotalSpins += amount;
                    break;

                case StatTypes.KappaWins:
                    _streamData.KappaWins += amount;
                    break;

                case StatTypes.ForeheadWins:
                    _streamData.ForeheadWins += amount;
                    break;

                case StatTypes.LULWins:
                    _streamData.LulWins += amount;
                    break;

                case StatTypes.SMOrcWins:
                    _streamData.SmorcWins += amount;
                    break;

                case StatTypes.jackpotWins:
                    _streamData.JackpotWins += amount;
                    break;

                case StatTypes.TotalUsersClaimed:
                    _streamData.TotalUsersClaimed += amount;
                    break;

                case StatTypes.TotalPointsClaimed:
                    _streamData.TotalPointsClaimed += amount;
                    break;

                case StatTypes.AmountOfUsersReset:
                    _streamData.AmountOfUsersReset += amount;
                    break;

                case StatTypes.AmountOfRewardsRedeemd:
                    _streamData.AmountOfRewardsRedeemed += amount;
                    break;

                case StatTypes.RewardRedeemCost:
                    _streamData.RewardRedeemCost += amount;
                    break;

                case StatTypes.AmountOfDiscordUsersJoined:
                    _streamData.AmountOfDiscordUsersJoined += amount;
                    break;

                case StatTypes.NewSubscriber:
                    _streamData.NewSubscribers += amount;
                    break;
            }
        }

        public static async Task UpdateStatsInDatabase()
        {
            if (!AppConfig.StreamerLive)
            {
                return;
            }

            using (var context = new DatabaseContext())
            {
                var lastStream = context.StreamStats.OrderBy(x => x.StreamId).Last();

                lastStream.AmountOfDiscordUsersJoined += _streamData.AmountOfDiscordUsersJoined;
                lastStream.AmountOfRewardsRedeemed += _streamData.AmountOfRewardsRedeemed;
                lastStream.AmountOfUsersReset += _streamData.AmountOfUsersReset;
                lastStream.BitsDonated += _streamData.BitsDonated;
                lastStream.CommandsSent += _streamData.CommandsSent;
                lastStream.DiscordRanksEarnt += _streamData.DiscordRanksEarnt;
                lastStream.ForeheadWins += _streamData.ForeheadWins;
                lastStream.JackpotWins += _streamData.JackpotWins;
                lastStream.KappaWins += _streamData.KappaWins;
                lastStream.LulWins += _streamData.LulWins;
                lastStream.MessagesReceived += _streamData.MessagesReceived;
                lastStream.NewFollowers += _streamData.NewFollowers;
                lastStream.NewGiftedSubs += _streamData.NewGiftedSubs;
                lastStream.NewSubscribers += _streamData.NewSubscribers;
                lastStream.PointsGainedSubscribing += _streamData.PointsGainedSubscribing;
                lastStream.PointsGainedWatching += _streamData.PointsGainedWatching;
                lastStream.PointsGambled += _streamData.PointsGambled;
                lastStream.PointsLost += _streamData.PointsLost;
                lastStream.PointsWon += _streamData.PointsWon;
                lastStream.RewardRedeemCost += _streamData.RewardRedeemCost;
                lastStream.SmorcWins += _streamData.SmorcWins;
                lastStream.StreamId += _streamData.StreamId;
                lastStream.TotalBans += _streamData.TotalBans;
                lastStream.TotalPointsClaimed += _streamData.TotalPointsClaimed;
                lastStream.TotalSpins += _streamData.TotalSpins;
                lastStream.TotalTimeouts += _streamData.TotalTimeouts;
                lastStream.TotalUsersClaimed += _streamData.TotalUsersClaimed;
                lastStream.UniquePeople += _streamData.UniquePeople;

                await context.SaveChangesAsync();
                Log.Information("[Stream Stats] Stats updated");
            }

            _streamData = new()
            {
                AmountOfDiscordUsersJoined = 0,
                AmountOfRewardsRedeemed = 0,
                AmountOfUsersReset = 0,
                BitsDonated = 0,
                CommandsSent = 0,
                DiscordRanksEarnt = 0,
                ForeheadWins = 0,
                JackpotWins = 0,
                KappaWins = 0,
                LulWins = 0,
                MessagesReceived = 0,
                NewFollowers = 0,
                NewGiftedSubs = 0,
                NewSubscribers = 0,
                PointsGainedSubscribing = 0,
                PointsGainedWatching = 0,
                PointsGambled = 0,
                PointsLost = 0,
                PointsWon = 0,
                RewardRedeemCost = 0,
                SmorcWins = 0,
                StreamId = 0,
                TotalBans = 0,
                TotalPointsClaimed = 0,
                TotalSpins = 0,
                TotalTimeouts = 0,
                TotalUsersClaimed = 0,
                UniquePeople = 0
            };
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
                    _usersToAdd.Clear();

                    context.StreamViewCount.Add(new StreamViewCount
                    {
                        Time = DateTime.UtcNow,
                        ViewCount = viewCount
                    });

                    _usersToAdd = context.Users.Where(x => x.InStream == true).Select(chatter => new UniqueViewers
                    {
                        Username = chatter.Username
                    }).ToList();

                    context.UniqueViewers.UpdateRange(_usersToAdd);
                    await context.SaveChangesAsync();

                    _usersToAdd.Clear();
                    Log.Information($"[Stream Stats] {viewCount} view count added and {_usersToAdd.Count} unique viewers added");
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
                streamStats.NewFollowers = followerCount - streamStats.StartingFollowerCount;
                context.SaveChanges();
            }

            await DiscordHelper.SendStreamStats();
        }
    }

    public class StreamStatsData
    {
        public required long StreamId { get; set; }
        public required long BitsDonated { get; set; }
        public required long CommandsSent { get; set; }
        public required long DiscordRanksEarnt { get; set; }
        public required long MessagesReceived { get; set; }
        public required long NewFollowers { get; set; }
        public required long NewSubscribers { get; set; }
        public required long PointsGainedSubscribing { get; set; }
        public required long PointsGainedWatching { get; set; }
        public required long PointsGambled { get; set; }
        public required long PointsLost { get; set; }
        public required long PointsWon { get; set; }
        public required long TotalBans { get; set; }
        public required long TotalTimeouts { get; set; }
        public required long NewGiftedSubs { get; set; }
        public required long UniquePeople { get; set; }
        public required long TotalSpins { get; set; }
        public required long KappaWins { get; set; }
        public required long ForeheadWins { get; set; }
        public required long LulWins { get; set; }
        public required long SmorcWins { get; set; }
        public required long JackpotWins { get; set; }
        public required long TotalUsersClaimed { get; set; }
        public required long TotalPointsClaimed { get; set; }
        public required long AmountOfUsersReset { get; set; }
        public required long AmountOfRewardsRedeemed { get; set; }
        public required long RewardRedeemCost { get; set; }
        public required long AmountOfDiscordUsersJoined { get; set; }
    }
}
