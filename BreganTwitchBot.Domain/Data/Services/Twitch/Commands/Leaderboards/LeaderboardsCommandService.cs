using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Leaderboards
{
    public class LeaderboardsCommandService (IServiceProvider serviceProvider)
    {
        [TwitchCommand("pointslb", ["pointsleaderboard", "lbpoints"])]
        public async Task HandlePointsLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.Points);

        [TwitchCommand("alltimehourslb", ["alltimehoursleaderboard", "lballtimehours", "hrslb", "hourslb", "hoursleaderboard"])]
        public async Task HandleAllTimeHoursLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.AllTimeHours);

        [TwitchCommand("streamhourslb", ["streamhoursleaderboard", "lbstreamhours", "streamhrslb", "streamhourslb"])]
        public async Task HandleStreamHoursLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.StreamHours);

        [TwitchCommand("weeklyhourslb", ["weeklyhoursleaderboard", "lbweeklyhours", "weekhrslb", "weeklyhourslb"])]
        public async Task HandleWeeklyHoursLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.WeeklyHours);

        [TwitchCommand("monthlyhourslb", ["monthlyhoursleaderboard", "lbmonthlyhours", "monthhrslb", "monthlyhourslb"])]
        public async Task HandleMonthlyHoursLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.MonthlyHours);

        [TwitchCommand("pointswonlb", ["pointswonleaderboard", "lbpointswon"])]
        public async Task HandlePointsWonLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.PointsWon);

        [TwitchCommand("pointslostlb", ["pointslostleaderboard", "lbpointslost"])]
        public async Task HandlePointsLostLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.PointsLost);

        [TwitchCommand("pointsgambledlb", ["pointsgambledleaderboard", "lbpointsgambled"])]
        public async Task HandlePointsGambledLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.PointsGambled);

        [TwitchCommand("totalspinslb", ["totalspinsleaderboard", "lbtotalspins"])]
        public async Task HandleTotalSpinsLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.TotalSpins);

        [TwitchCommand("currentdailystreaklb", ["currentdailystreakleaderboard", "lbcurrentdailystreak", "dailystreaklb"])]
        public async Task HandleCurrentDailyStreakLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.CurrentDailyStreak);

        [TwitchCommand("highestdailystreaklb", ["highestdailystreakleaderboard", "lbhighestdailystreak"])]
        public async Task HandleHighestDailyStreakLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.HighestDailyStreak);

        [TwitchCommand("currentweeklystreaklb", ["currentweeklystreakleaderboard", "lbcurrentweeklystreak", "weeklystreaklb"])]
        public async Task HandleCurrentWeeklyStreakLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.CurrentWeeklyStreak);

        [TwitchCommand("highestweeklystreaklb", ["highestweeklystreakleaderboard", "lbhighestweeklystreak"])]
        public async Task HandleHighestWeeklyStreakLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.HighestWeeklyStreak);

        [TwitchCommand("currentmonthlystreaklb", ["currentmonthlystreakleaderboard", "lbcurrentmonthlystreak", "monthlystreaklb"])]
        public async Task HandleCurrentMonthlyStreakLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.CurrentMonthlyStreak);

        [TwitchCommand("highestmonthlystreaklb", ["highestmonthlystreakleaderboard", "lbhighestmonthlystreak"])]
        public async Task HandleHighestMonthlyStreakLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.HighestMonthlyStreak);

        [TwitchCommand("currentyearlystreaklb", ["currentyearlystreakleaderboard", "lbcurrentyearlystreak", "yearlystreaklb"])]
        public async Task HandleCurrentYearlyStreakLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.CurrentYearlyStreak);

        [TwitchCommand("highestyearlystreaklb", ["highestyearlystreakleaderboard", "lbhighestyearlystreak"])]
        public async Task HandleHighestYearlyStreakLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.HighestYearlyStreak);

        [TwitchCommand("bossesdonelb", ["bossesdoneleaderboard", "lbbossesdone", "bosslb"])]
        public async Task HandleBossesDoneLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.BossesDone);

        [TwitchCommand("bossespointswonlb", ["bossespointswonleaderboard", "lbbossespointswon"])]
        public async Task HandleBossesPointsWonLbCommand(ChannelChatMessageReceivedParams msgParams)
            => await HandleLeaderboardCommand(msgParams, LeaderboardType.BossesPointsWon);

        private async Task HandleLeaderboardCommand(ChannelChatMessageReceivedParams msgParams, LeaderboardType type)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var leaderboardsDataService = scope.ServiceProvider.GetRequiredService<ILeaderboardsDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();
                try
                {
                    var response = await leaderboardsDataService.HandleLeaderboardCommand(msgParams, type);
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, $"Error while handling leaderboard command for type {type} in channel {msgParams.BroadcasterChannelId}");
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, "uh oh there has been an errory werrory trying to get the leaderboard data :( try again pwease :3", msgParams.MessageId);
                }
            }
        }
    }
}
