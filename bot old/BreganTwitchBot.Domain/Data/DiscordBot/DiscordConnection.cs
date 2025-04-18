using BreganTwitchBot.Domain.Data.DiscordBot.Events;
using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

namespace BreganTwitchBot.Domain.Data.DiscordBot
{
    public class DiscordConnection
    {
        private static async Task ButtonPressed(SocketMessageComponent arg)
        {
            await arg.DeferAsync();

            try
            {
                await ButtonPressedEvent.HandleButtonRoles(arg);
                await ButtonPressedEvent.HandleGiveawayButtons(arg);
            }
            catch (Exception e)
            {
                Log.Fatal($"[Discord Button Pressed] Error with button: {e}- {e.InnerException}");
                return;
            }
        }

        private static async Task MessageReceived(SocketMessage arg)
        {
            try
            {
                Log.Information($"[Discord Message Received] Username: {arg.Author.Username} Message: {arg.Content} Channel: {arg.Channel.Name}");
                await MessageReceivedEvent.CheckBlacklistedWords(arg);
                await MessageReceivedEvent.HandleCustomCommand(arg);
            }
            catch (Exception e)
            {
                Log.Fatal($"[Discord Message Received] Error with received message: {e}- {e.InnerException}");
                return;
            }
        }
    }
}