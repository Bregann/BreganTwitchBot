﻿using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Services.Twitch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Helpers
{
    public class HangfireJobServiceHelper(TwitchApiConnection twitchApiConnection, AppDbContext context)
    {
        private readonly TwitchApiConnection _twitchApiConnection = twitchApiConnection;
        private readonly AppDbContext _context = context;

        public async Task SetupHangfireJobs()
        {
            var test = _twitchApiConnection.GetApiClient("");
            test.ApiClient.Helix.EventSub.CreateEventSubSubscriptionAsync("test", "1",);

        }
    }
}
