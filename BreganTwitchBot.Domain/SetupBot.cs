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
            #region Discord

            await Task.Delay(2000);

            //Start Discord
            var discordThread = new Thread(DiscordConnection.MainAsync().GetAwaiter().GetResult);
            discordThread.Start();

            #endregion Discord

            
        }
    }
}
