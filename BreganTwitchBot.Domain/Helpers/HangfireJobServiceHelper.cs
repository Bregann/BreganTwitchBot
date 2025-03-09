using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Services.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using Hangfire;
using Serilog;

namespace BreganTwitchBot.Domain.Helpers
{
    public class HangfireJobServiceHelper(TwitchApiConnection twitchApiConnection, ITwitchHelperService twitchHelperService, AppDbContext context)
    {
        public async Task SetupHangfireJobs()
        {
            RecurringJob.AddOrUpdate("BigBenBong", () => BigBenBong(), "0 * * * *");
        }

        public async Task BigBenBong()
        {
            var apiClients = twitchApiConnection.GetAllBotApiClients();

            if (apiClients == null)
            {
                Log.Error("[Hangfire Job Service] Error sending message to all channels, apiClients is null");
                return;
            }

            foreach (var bot in apiClients)
            {
                switch (TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "GMT Standard Time").Hour)
                {
                    case 1:
                    case 13:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕑 BONG");
                            break;
                        }
                    case 2:
                    case 14:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕑 BONG BONG");
                            break;
                        }
                    case 3:
                    case 15:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕒 BONG BONG BONG");
                            break;
                        }
                    case 4:
                    case 16:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕓 BONG BONG BONG BONG");
                            break;
                        }
                    case 5:
                    case 17:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕔 BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 6:
                    case 18:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕕 BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 7:
                    case 19:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕖 BONG BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 8:
                    case 20:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕗 BONG BONG BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 9:
                    case 21:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕘 BONG BONG BONG BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 10:
                    case 22:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕙 BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 11:
                    case 23:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕚 BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                    case 12:
                    case 24:
                        {
                            await twitchHelperService.SendTwitchMessageToChannel(bot.BroadcasterChannelId, bot.BroadcasterChannelName, "🕛 BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG BONG");
                            break;
                        }
                }

                Log.Information($"[Hangfire Job Service] Big Ben Bonged in channel name {bot.BroadcasterChannelName}");
            }
        }
    }
}