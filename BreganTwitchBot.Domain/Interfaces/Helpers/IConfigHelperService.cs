using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Helpers
{
    public interface IConfigHelperService
    {
        Task UpdateDailyPointsStatus(string broadcasterId, bool status);
        (bool DailyPointsAllowed, DateTime LastStreamDate, DateTime LastDailyPointedAllowedDate) GetDailyPointsStatus(string broadcasterId);
        Task UpdateStreamLiveStatus(string broadcasterId, bool status);
    }
}
