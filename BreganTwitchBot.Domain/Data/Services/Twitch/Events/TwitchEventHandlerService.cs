using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Events;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Events
{
    public class TwitchEventHandlerService(AppDbContext context, ITwitchHelperService twitchHelperService) : ITwitchEventHandlerService
    {
        public async Task HandleChannelCheerEvent(BitsCheeredParams cheerParams)
        {
            await twitchHelperService.SendTwitchMessageToChannel(cheerParams.BroadcasterChannelId, cheerParams.BroadcasterChannelName, $"Thank you for the {cheerParams.Amount} bits, {cheerParams.ChatterChannelName}! PogChamp");
        }

        public async Task HandleChannelResubscribeEvent(ChannelResubscribeParams resubscribeParams)
        {
            var pointsToAdd = resubscribeParams.SubTier switch
            {
                SubTierEnum.Tier1 => 20000 + (resubscribeParams.CumulativeMonths * 1500) + (resubscribeParams.StreakMonths ?? 0 * 3000),
                SubTierEnum.Tier2 => 40000 + (resubscribeParams.CumulativeMonths * 3000) + (resubscribeParams.StreakMonths ?? 0 * 3000),
                SubTierEnum.Tier3 => 60000 + (resubscribeParams.CumulativeMonths * 6000) + (resubscribeParams.StreakMonths ?? 0 * 3000),
                _ => 0
            };

            await twitchHelperService.AddPointsToUser(resubscribeParams.BroadcasterChannelId, resubscribeParams.ChatterChannelId, pointsToAdd, resubscribeParams.BroadcasterChannelName, resubscribeParams.ChatterChannelName);

            var streakMessage = resubscribeParams.StreakMonths > 0 ? $"They are on a {resubscribeParams.StreakMonths} month sub streak! <3" : "they did not share their sub streak :(";
            await twitchHelperService.SendTwitchMessageToChannel(resubscribeParams.BroadcasterChannelId, resubscribeParams.BroadcasterChannelName, $"Thank you for the {resubscribeParams.CumulativeMonths} month resubscription, {resubscribeParams.ChatterChannelName}! {streakMessage} <3");
        }
    }
}
