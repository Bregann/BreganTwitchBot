using BreganTwitchBot.Domain;
using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Data.TwitchBot;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganUtils.ProjectMonitor.Projects;
using Serilog;

namespace BreganTwitchBot
{
    public class AppConfig
    {
        //Hardcoded config
        //Fields are loaded from DB
        public static string BroadcasterName { get; private set; }

        public static string BroadcasterOAuth { get; private set; }
        public static string BroadcasterRefresh { get; private set; }
        public static bool DailyPointsCollectingAllowed { get; private set; } = false;
        public static bool StreamAnnounced { get; private set; } = false;
        public static bool StreamerLive { get; private set; } = false;
        public static TimeSpan SubathonTime { get; private set; }
        public static string PinnedStreamMessage { get; private set; }
        public static ulong PinnedStreamMessageId { get; private set; }
        public static DateTime PinnedMessageDate { get; private set; }
        public static string BotName { get; private set; }
        public static string PointsName { get; private set; }
        public static string TwitchChannelID { get; private set; }
        public static string BotOAuth { get; private set; }
        public static string TwitchAPIClientID { get; private set; }
        public static string TwitchAPISecret { get; private set; }
        public static string DiscordAPIKey { get; private set; }
        public static ulong DiscordGuildOwnerID { get; private set; }
        public static ulong DiscordEventChannelID { get; private set; }
        public static ulong DiscordStreamAnnouncementChannelID { get; private set; }
        public static ulong DiscordLinkingChannelID { get; private set; }
        public static ulong DiscordCommandsChannelID { get; private set; }
        public static ulong DiscordRankUpAnnouncementChannelID { get; private set; }
        public static ulong DiscordGiveawayChannelID { get; private set; }
        public static ulong DiscordSocksChannelID { get; private set; }
        public static ulong DiscordReactionRoleChannelID { get; private set; }
        public static ulong DiscordGeneralChannel { get; private set; }
        public static ulong DiscordGuildID { get; private set; }
        public static ulong DiscordBanRole { get; private set; }
        public static long PrestigeCap { get; private set; }
        public static string HFConnectionString { get; private set; }
        public static string ProjectMonitorApiKey { get; private set; }
        public static string TwitchBotApiKey { get; private set; }
        public static string TwitchBotApiRefresh { get; private set; }
        public static string BotChannelId { get; private set; }
        public static bool AiEnabled { get; private set; }
        public static bool SubathonActive { get; private set; }

        private static bool _doublePingJobStarted = false;

        public static void LoadConfig()
        {
            using (var context = new DatabaseContext())
            {
                //Load the config to the variables
                var configVariables = context.Config.First();
                BroadcasterName = configVariables.BroadcasterName;
                BroadcasterOAuth = configVariables.BroadcasterOAuth;
                BroadcasterRefresh = configVariables.BroadcasterRefresh;
                DailyPointsCollectingAllowed = configVariables.DailyPointsCollectingAllowed;
                StreamAnnounced = configVariables.StreamAnnounced;
                SubathonTime = configVariables.SubathonTime;
                PinnedStreamMessage = configVariables.PinnedStreamMessage;
                PinnedStreamMessageId = configVariables.PinnedStreamMessageId;
                PinnedMessageDate = configVariables.PinnedStreamDate;
                BotName = configVariables.BotName;
                PointsName = configVariables.PointsName;
                TwitchChannelID = configVariables.TwitchChannelID;
                BotOAuth = configVariables.BotOAuth;
                TwitchAPIClientID = configVariables.TwitchAPIClientID;
                TwitchAPISecret = configVariables.TwitchAPISecret;
                DiscordAPIKey = configVariables.DiscordAPIKey;
                DiscordGuildOwnerID = configVariables.DiscordGuildOwner;
                DiscordEventChannelID = configVariables.DiscordEventChannelID;
                DiscordStreamAnnouncementChannelID = configVariables.DiscordStreamAnnouncementChannelID;
                DiscordLinkingChannelID = configVariables.DiscordLinkingChannelID;
                DiscordCommandsChannelID = configVariables.DiscordCommandsChannelID;
                DiscordRankUpAnnouncementChannelID = configVariables.DiscordRankUpAnnouncementChannelID;
                DiscordGiveawayChannelID = configVariables.DiscordGiveawayChannelID;
                DiscordSocksChannelID = configVariables.DiscordSocksChannelID;
                DiscordReactionRoleChannelID = configVariables.DiscordReactionRoleChannelID;
                DiscordGeneralChannel = configVariables.DiscordGeneralChannel;
                DiscordGuildID = configVariables.DiscordGuildID;
                DiscordBanRole = configVariables.DiscordBanRole;
                PrestigeCap = configVariables.PrestigeCap;
                HFConnectionString = configVariables.HFConnectionString;
                ProjectMonitorApiKey = configVariables.ProjectMonitorApiKey;
                TwitchBotApiKey = configVariables.TwitchBotApiKey;
                TwitchBotApiRefresh = configVariables.TwitchBotApiRefresh;
                BotChannelId = configVariables.BotChannelId;
                AiEnabled = configVariables.AiEnabled;
                SubathonActive = configVariables.SubathonActive;
            }
        }

        public static void UpdateDailyPointsCollecting(bool status)
        {
            DailyPointsCollectingAllowed = status;
            ProjectMonitorBreganTwitchBot.SendDiscordDailyPointsCollectingUpdate(status);

            using (var context = new DatabaseContext())
            {
                context.Config.First().DailyPointsCollectingAllowed = status;
                context.Config.First().LastDailyPointsAllowed = DateTime.UtcNow;
                context.SaveChanges();
            }
        }

        /*
         * https://id.twitch.tv/oauth2/authorize?response_type=code
                &client_id=
                &redirect_uri=http://localhost
                &scope=bits%3Aread%20channel%3Amanage%3Abroadcast%20channel%3Amanage%3Apolls%20channel%3Amanage%3Apredictions%20channel%3Amanage%3Aredemptions%20channel%3Aread%3Ahype_train%20channel%3Aread%3Apolls%20channel%3Aread%3Apredictions%20channel%3Aread%3Aredemptions%20channel%3Aread%3Asubscriptions%20channel%3Aread%3Avips%20channel%3Amanage%3Avips%20moderation%3Aread%20user%3Amanage%3Ablocked_users
         */

        public static void UpdateStreamerApiCredentials(string refreshToken, string accessToken)
        {
            using (var context = new DatabaseContext())
            {
                var config = context.Config.First();
                config.BroadcasterOAuth = accessToken;
                config.BroadcasterRefresh = refreshToken;
                context.SaveChanges();
            }

            BroadcasterOAuth = accessToken;
            BroadcasterRefresh = refreshToken;
        }


        /* for the bot
         * https://id.twitch.tv/oauth2/authorize
                ?response_type=code
                &client_id=
                &redirect_uri=http://localhost
                &scope=moderation%3Aread+moderator%3Amanage%3Aannouncements+moderator%3Amanage%3Abanned_users+moderator%3Amanage%3Achat_messages+moderator%3Amanage%3Achat_settings+moderation%3Aread+channel%3Aread%3Avips
         */
        public static void UpdateBotApiCredentials(string refreshToken, string accessToken)
        {
            using (var context = new DatabaseContext())
            {
                var config = context.Config.First();
                config.TwitchBotApiKey = accessToken;
                config.TwitchBotApiRefresh = refreshToken;
                context.SaveChanges();
            }

            TwitchBotApiKey = accessToken;
            TwitchBotApiRefresh = refreshToken;
        }

        public static void UpdatePinnedMessageAndMessageId(string newMessage, ulong id)
        {
            using (var context = new DatabaseContext())
            {
                context.Config.First().PinnedStreamMessage = newMessage;
                context.Config.First().PinnedStreamMessageId = id;
                context.Config.First().PinnedStreamDate = DateTime.UtcNow;
                context.SaveChanges();
            }

            PinnedStreamMessage = newMessage;
            PinnedStreamMessageId = id;
            PinnedMessageDate = DateTime.UtcNow;
        }

        public static async Task CheckAndUpdateIfStreamIsLive()
        {
            try
            {
                var getStreams = await TwitchApiConnection.ApiClient.Helix.Streams.GetStreamsAsync(userIds: new List<string> { TwitchChannelID });

                if (getStreams.Streams.Length == 0)
                {
                    ProjectMonitorBreganTwitchBot.SendStreamStatusUpdate(false);
                    StreamerLive = false;
                }
                else
                {
                    ProjectMonitorBreganTwitchBot.SendStreamStatusUpdate(true);
                    StreamerLive = true;
                }

                //If live and not announced then announce the stream
                if (StreamerLive && !StreamAnnounced)
                {
                    using (var context = new DatabaseContext())
                    {
                        context.Config.First().StreamAnnounced = true;

                        var usersInStream = context.Users.Where(x => x.InStream).ToList();

                        foreach (var user in usersInStream)
                        {
                            user.InStream = false;
                        }

                        context.SaveChanges();
                    }

                    StreamAnnounced = true;
                    ProjectMonitorBreganTwitchBot.SendStreamAnnouncedUpdate(true);
                    await StreamStatsService.AddNewStream();
                    await DiscordHelper.AnnounceStream();
                    HangfireJobs.StartTwitchBossStreamAnnounceJob();
                    return;
                }

                //if the stream isnt show as live and the stream has announced then start the double ping prevention job
                if (!StreamerLive && StreamAnnounced)
                {
                    if (!_doublePingJobStarted)
                    {
                        await StreamStatsService.CalculateEndStreamData();
                        HangfireJobs.StartDoublePingPreventionJob();
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
            _doublePingJobStarted = false;
            ProjectMonitorBreganTwitchBot.SendStreamAnnouncedUpdate(false);

            using (var context = new DatabaseContext())
            {
                context.Config.First().StreamAnnounced = false;
                context.SaveChanges();
            }
        }

        public static async Task AddSubathonTime(TimeSpan tsToAdd)
        {
            SubathonTime += tsToAdd;
            Log.Information($"[Subathon Time] {SubathonTime}");

            using (var context = new DatabaseContext())
            {
                context.Config.First().SubathonTime = SubathonTime;
                await context.SaveChangesAsync();
            }

            Log.Information($"[Subathon Time] {tsToAdd} added");
        }

        public static void ToggleAiEnabled()
        {
            AiEnabled = AiEnabled != true;
        }
    }
}