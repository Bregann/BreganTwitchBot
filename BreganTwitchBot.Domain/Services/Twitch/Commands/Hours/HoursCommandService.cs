using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Hours
{
    public class HoursCommandService(IServiceProvider serviceProvider)
    {
        [TwitchCommand("hours", ["hrs", "watchtime"])]
        public async Task HandleHoursCommand(ChannelChatMessageReceivedParams msgParams) => await HandleHoursCommand(msgParams, HoursWatchTypes.AllTime);

        [TwitchCommand("streamhours", ["streamhrs", "streamwatchtime"])]
        public async Task HandleStreamHoursCommand(ChannelChatMessageReceivedParams msgParams) => await HandleHoursCommand(msgParams, HoursWatchTypes.Stream);

        [TwitchCommand("weeklyhours", ["weekhrs", "weekwatchtime", "weekhours"])]
        public async Task HandleWeekHoursCommand(ChannelChatMessageReceivedParams msgParams) => await HandleHoursCommand(msgParams, HoursWatchTypes.Week);

        [TwitchCommand("monthlyhours", ["monthhrs", "monthwatchtime", "monthhours"])]
        public async Task HandleMonthHoursCommand(ChannelChatMessageReceivedParams msgParams) => await HandleHoursCommand(msgParams, HoursWatchTypes.Month);

        private async Task HandleHoursCommand(ChannelChatMessageReceivedParams msgParams, HoursWatchTypes watchTypes)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var hoursCommandService = scope.ServiceProvider.GetRequiredService<IHoursDataService>();
                var twitchHelperService = scope.ServiceProvider.GetRequiredService<ITwitchHelperService>();

                try
                {
                    var response = await hoursCommandService.GetHoursCommand(msgParams, watchTypes);

                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, response, msgParams.MessageId);
                }
                catch (TwitchUserNotFoundException ex)
                {
                    await twitchHelperService.SendTwitchMessageToChannel(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName, ex.Message, msgParams.MessageId);
                }
            }
        }
    }
}
