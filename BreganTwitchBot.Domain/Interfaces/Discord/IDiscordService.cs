using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Interfaces.Discord
{
    public interface IDiscordService
    {
        Task StartAsync();
        Task StopAsync();

        DiscordSocketClient Client { get; }
    }
}
