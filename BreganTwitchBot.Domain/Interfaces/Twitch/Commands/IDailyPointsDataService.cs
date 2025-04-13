using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IDailyPointsDataService
    {
        Task ScheduleDailyPointsCollection(string broadcasterId);
        Task CancelDailyPointsCollection(string broadcasterId);
        Task AllowDailyPointsCollecting(string broadcasterId);
        Task<string> HandlePointsClaimed(ChannelChatMessageReceivedParams msgParams, PointsClaimType pointsClaimType);
        Task<string> HandleStreakCheckCommand(ChannelChatMessageReceivedParams msgParams, PointsClaimType pointsClaimType);
        Task AnnouncePointsReminder(string broadcasterId);
        Task ResetStreaks();
    }
}
