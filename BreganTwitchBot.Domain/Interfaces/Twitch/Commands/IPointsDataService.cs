﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Twitch.Commands
{
    public interface IPointsDataService
    {
        Task<int> GetPointsAsync(string twitchUserId);
    }
}
