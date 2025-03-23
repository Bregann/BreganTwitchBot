using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IDailyPointsDataService
    {
        void ScheduleDailyPointsCollection(string broadcasterId);
        void CancelDailyPointsCollection(string broadcasterId);
        Task AllowDailyPointsCollecting(string broadcasterId);
    }
}
