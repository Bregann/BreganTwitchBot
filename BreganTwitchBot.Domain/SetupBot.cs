using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.WordBlacklist;
using BreganTwitchBot.Domain.Bot.Twitch.Events;
using BreganTwitchBot.Domain.Bot.Twitch.Services;
using BreganTwitchBot.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain
{
    public class SetupBot
    {
        public static async Task Setup()
        {
            #region Twitch

            var bot = new TwitchBotConnection();
            var twitchThread = new Thread(bot.Connect().GetAwaiter().GetResult);
            twitchThread.Start();
            Log.Information("[Twitch Client] Started Twitch Thread");

            await Task.Delay(2000);

            var twitchApi = new TwitchApiConnection();
            twitchApi.Connect();
            Log.Information("[Twitch API] Connected To Twitch API");

            await Task.Delay(2000);

            var pubSub = new TwitchPubSubConnection();
            pubSub.Connect();
            Log.Information("[Twitch PubSub] Connected To Twitch PubSub");

            await Task.Delay(2000);

            TwitchEvents.SetupTwitchEvents();
            WordBlacklist.LoadBlacklistedWords();

            #endregion Twitch

            #region Discord

            await Task.Delay(2000);

            //Start Discord
            var discordThread = new Thread(DiscordConnection.MainAsync().GetAwaiter().GetResult);
            discordThread.Start();

            #endregion Discord

            HangfireJobs.SetupHangfireJobs();
        }
    }
}
