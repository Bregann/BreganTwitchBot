using BreganTwitchBot.Core.Twitch.Services;
using BreganTwitchBot.Data;
using BreganTwitchBot.DiscordBot.Helpers;
using BreganTwitchBot.Services;
using Serilog;

namespace BreganTwitchBot
{

    public class Config
    {
        //Hardcoded config
#if DEBUG
        //Twitch
        public static readonly string BroadcasterName = "";
        public static readonly string BotName = "";
        public static readonly string PointsName = "";
        public static readonly string TwitchChannelID = "";
        public static readonly string BotOAuth = "";
        public static readonly string TwitchAPIClientID = "";
        public static readonly string TwitchAPISecret = "";

        //Fields are loaded from DB/Set via methods
        public static string BroadcasterOAuth { get; private set; } = string.Empty;
        public static string BroadcasterRefresh { get; private set; } = string.Empty;
        public static bool DailyPointsCollectingAllowed { get; private set; } = false;
        public static bool StreamAnnounced { get; private set; } = false;
        public static bool StreamerLive { get; private set; } = true;
        public static TimeSpan SubathonTime { get; private set; }


        //Discord - private Discord
        public static readonly string DiscordAPIKey = "";
        public static readonly ulong DiscordGuildOwner = 0;
        public static readonly ulong DiscordEventChannelID = 0;
        public static readonly ulong DiscordStreamAnnouncementChannelID = 0;
        public static readonly ulong DiscordLinkingChannelID = 0;
        public static readonly ulong DiscordCommandsChannelID = 0;
        public static readonly ulong DiscordRankUpAnnouncementChannelID = 0;
        public static readonly ulong DiscordGiveawayChannelID = 0;
        public static readonly ulong DiscordSocksChannelID = 0;
        public static readonly ulong DiscordReactionRoleChannelID = 0;
        public static readonly ulong DiscordGuildID = 0;
        public static readonly long PrestigeCap = 0;

#else
        //Twitch
        public static readonly string BroadcasterName = "";
        public static readonly string BotName = "";
        public static readonly string PointsName = "";
        public static readonly string TwitchChannelID = "";
        public static readonly string BotOAuth = "";
        public static readonly string TwitchAPIClientID = "";
        public static readonly string TwitchAPISecret = "";

        //Fields are loaded from DB
        public static string BroadcasterOAuth { get; private set; } = string.Empty;
        public static string BroadcasterRefresh { get; private set; } = string.Empty;
        public static bool DailyPointsCollectingAllowed { get; private set; } = false;
        public static bool StreamAnnounced { get; private set; } = false;
        public static bool StreamerLive { get; private set; } = false;
        public static TimeSpan SubathonTime { get; private set; }

        //Discord
        public static readonly string DiscordAPIKey = "";
        public static readonly ulong DiscordGuildOwner = 0;
        public static readonly ulong DiscordEventChannelID = 0;
        public static readonly ulong DiscordStreamAnnouncementChannelID = 0;
        public static readonly ulong DiscordLinkingChannelID = 0;
        public static readonly ulong DiscordCommandsChannelID = 0;
        public static readonly ulong DiscordRankUpAnnouncementChannelID = 0;
        public static readonly ulong DiscordGiveawayChannelID = 0;
        public static readonly ulong DiscordSocksChannelID = 0;
        public static readonly ulong DiscordReactionRoleChannelID = 0;
        public static readonly ulong DiscordGuildID = 0;
        public static readonly long PrestigeCap = 0;
#endif

        private static bool _doublePingJobStarted = false;
        public static readonly bool SubathonActive = true;

        public static async Task LoadConfigAndInsertDatabaseFieldsIfNeeded()
        {
            using (var context = new DatabaseContext())
            {
                //Check if the fields already exist for config and the spin stats - if not then create them

                if (!context.Config.Any())
                {
                    context.Config.Add(new BreganTwitchBot.Data.Models.Config
                    {
                        BroadcasterName = BroadcasterName,
                        BroadcasterOAuth = "", //these need to be filled out
                        DailyPointsCollectingAllowed = false,
                        BroadcasterRefresh = "",
                        StreamAnnounced = false,
                        LastDailyPointsAllowed = DateTime.Now.AddDays(-1)
                    });

                    await context.SaveChangesAsync();
                }

                if (!context.SlotMachine.Any())
                {
                    context.SlotMachine.Add(new BreganTwitchBot.Data.Models.SlotMachine
                    {
                        StreamName = BroadcasterName,
                        DiscordTotalSpins = 0,
                        CheeseWins = 0,
                        CherriesWins = 0,
                        CucumberWins = 0,
                        EggplantWins = 0,
                        GrapesWins = 0,
                        JackpotAmount = 0,
                        JackpotWins = 0,
                        PineappleWins = 0,
                        SmorcWins = 0,
                        Tier1Wins = 0,
                        Tier2Wins = 0,
                        Tier3Wins = 0,
                        TotalSpins = 0
                    });

                    await context.SaveChangesAsync();
                }

                //Load the config to the variables
                var configVariables = context.Config.First();
                BroadcasterOAuth = configVariables.BroadcasterOAuth;
                BroadcasterRefresh = configVariables.BroadcasterRefresh;
                DailyPointsCollectingAllowed = configVariables.DailyPointsCollectingAllowed;
                StreamAnnounced = configVariables.StreamAnnounced;
            }
        }

        public static void UpdateDailyPointsCollecting(bool status)
        {
            DailyPointsCollectingAllowed = status;

            using (var context = new DatabaseContext())
            {
                context.Config.First().DailyPointsCollectingAllowed = status;
                context.SaveChanges();
            }
        }

        public static void UpdateStreamConfig(string refreshToken, string accessToken)
        {
            using (var context = new DatabaseContext())
            {
                var config = context.Config.First();
                config.BroadcasterOAuth = accessToken;
                config.BroadcasterRefresh = refreshToken;
                context.Config.Update(config);
                context.SaveChanges();
            }

            BroadcasterOAuth = accessToken;
            BroadcasterRefresh = refreshToken;
        }

        public static async Task CheckAndUpdateIfStreamIsLive()
        {
            try
            {
                var getStreams = await TwitchApiConnection.ApiClient.Helix.Streams.GetStreamsAsync(userIds: new List<string> { TwitchChannelID });

                if (getStreams.Streams.Length == 0)
                {
                    StreamerLive = false;
                }
                else
                {
                    StreamerLive = true;
                }

                //If live and not announced then announce the stream
                if (StreamerLive && !StreamAnnounced)
                {
                    using (var context = new DatabaseContext())
                    {
                        context.Config.First().StreamAnnounced = true;
                        context.SaveChanges();
                    }

                    StreamAnnounced = true;
                    await StreamStatsService.AddNewStream();
                    await DiscordHelper.AnnounceStream();
                    await JobScheduler.StartTwitchBossStreamAnnounceJob();
                    return;
                }

                //if the stream isnt show as live and the stream has announced then start the double ping prevention job
                if (!StreamerLive && StreamAnnounced)
                {
                    if (!_doublePingJobStarted)
                    {
                        await StreamStatsService.CalculateEndStreamData();
                        await JobScheduler.CreateDoublePingPreventionJob();
                        _doublePingJobStarted = true;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Fatal($"[Stream Live Check] {e}");
            }
            return;
        }

        public static void SetStreamAnnouncedToFalse() //this is only needed by the job scheduler for double pings
        {
            StreamAnnounced = false;

            using (var context = new DatabaseContext())
            {
                context.Config.First().StreamAnnounced = false;
                context.SaveChanges();
            }
        }

        public static void AddSubathonTime(TimeSpan tsToAdd)
        {
            SubathonTime += tsToAdd;
            Log.Information($"[Subathon Time] {SubathonTime}");

            using (var context = new DatabaseContext())
            {
                context.Config.First().SubathonTime = SubathonTime;
                context.SaveChanges();
            }

            Log.Information($"[Subathon Time] {tsToAdd} added");
        }
    }
}
