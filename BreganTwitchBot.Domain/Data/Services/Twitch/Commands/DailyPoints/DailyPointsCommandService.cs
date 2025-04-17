using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.DailyPoints
{
    public class DailyPointsCommandService(IServiceProvider serviceProvider)
    {
        [TwitchCommand("daily", ["dailypoints", "collect"])]
        public async Task HandleDailyPointsCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleClaimCommand(msgParams, PointsClaimType.Daily);
        }

        [TwitchCommand("weekly", ["weeklypoints"])]
        public async Task HandleWeeklyPointsCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleClaimCommand(msgParams, PointsClaimType.Weekly);
        }

        [TwitchCommand("monthly", ["monthlypoints"])]
        public async Task HandleMonthlyPointsCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleClaimCommand(msgParams, PointsClaimType.Monthly);
        }

        [TwitchCommand("yearly", ["yearlypoints"])]
        public async Task HandleYearlyPointsCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleClaimCommand(msgParams, PointsClaimType.Yearly);
        }

        [TwitchCommand("streak", ["dailystreak", "dailypointstreak"])]
        public async Task HandleDailyStreakCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleStreakCommand(msgParams, PointsClaimType.Daily);
        }

        [TwitchCommand("weeklystreak", ["weeklypointstreak"])]
        public async Task HandleWeeklyStreakCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleStreakCommand(msgParams, PointsClaimType.Weekly);
        }

        [TwitchCommand("monthlystreak", ["monthlypointstreak"])]
        public async Task HandleMonthlyStreakCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleStreakCommand(msgParams, PointsClaimType.Monthly);
        }

        [TwitchCommand("yearlystreak", ["yearlypointstreak"])]
        public async Task HandleYearlyStreakCommand(ChannelChatMessageReceivedParams msgParams)
        {
            await HandleStreakCommand(msgParams, PointsClaimType.Yearly);
        }

        private async Task HandleClaimCommand(ChannelChatMessageReceivedParams msgParams, PointsClaimType pointsClaimType)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dailyPointsDataService = scope.ServiceProvider.GetRequiredService<IDailyPointsDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                try
                {
                    var dailyPointsResponse = await dailyPointsDataService.HandlePointsClaimed(msgParams, pointsClaimType);
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, dailyPointsResponse, msgParams.MessageId);
                }
                catch (TwitchUserNotFoundException ex)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
                }
                catch (Exception ex)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, "uh oh error", msgParams.MessageId);
                    Log.Fatal(ex, "Error handling daily points command");
                }
            }
        }

        private async Task HandleStreakCommand(ChannelChatMessageReceivedParams msgParams, PointsClaimType pointsClaimType)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dailyPointsDataService = scope.ServiceProvider.GetRequiredService<IDailyPointsDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                try
                {
                    var dailyPointsResponse = await dailyPointsDataService.HandleStreakCheckCommand(msgParams, pointsClaimType);
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, dailyPointsResponse, msgParams.MessageId);
                }
                catch (TwitchUserNotFoundException ex)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
                }
                catch (Exception ex)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, "uh oh error", msgParams.MessageId);
                    Log.Fatal(ex, "Error handling daily streak command");
                }
            }
        }
    }
}
