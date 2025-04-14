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

        public static async Task MainAsync()
        {
            DiscordClient.InteractionCreated += InteractionCreated;
            DiscordClient.MessageReceived += MessageReceived;

            DiscordClient.ButtonExecuted += ButtonPressed;

        }

        private static async Task ButtonPressed(SocketMessageComponent arg)
        {
            await arg.DeferAsync();

            try
            {
                await ButtonPressedEvent.HandleButtonRoles(arg);
                await ButtonPressedEvent.HandleNameEmojiButtons(arg);
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
                await MessageReceivedEvent.CheckStreamLiveMessages(arg);
                await MessageReceivedEvent.HandleCustomCommand(arg);
                await MessageReceivedEvent.PingFoodEnjoyerOnImage(arg);
                MessageReceivedEvent.AddDiscordXp(arg);
            }
            catch (Exception e)
            {
                Log.Fatal($"[Discord Message Received] Error with received message: {e}- {e.InnerException}");
                return;
            }
        }

        private static async Task InteractionCreated(SocketInteraction arg)
        {
            var isMod = DiscordHelper.IsUserMod(arg.User.Id);

            var command = arg as SocketSlashCommand;

            if (command == null)
            {
                return;
            }

            if (arg.Channel.Id != AppConfig.DiscordSocksChannelID && command.CommandName == "socks") //socks channel   713365310408884236
            {
                await arg.RespondAsync("Please use the socks maker channel!", ephemeral: true);
                return;
            }

            if (command.CommandName == "socks" && arg.Channel.Id == AppConfig.DiscordSocksChannelID)
            {
                var contextSocks = new SocketInteractionContext(DiscordClient, arg);
                await _interactionService.ExecuteCommandAsync(contextSocks, _services);
                return;
            }

            if (command.CommandName == "link" && arg.Channel.Id == AppConfig.DiscordLinkingChannelID)
            {
                var contextLinking = new SocketInteractionContext(DiscordClient, arg);
                await _interactionService.ExecuteCommandAsync(contextLinking, _services);
                return;
            }

            if (arg.Channel.Id != AppConfig.DiscordCommandsChannelID && !isMod)
            {
                await arg.RespondAsync("Please use the bot commands channel!", ephemeral: true);
            }

            var context = new SocketInteractionContext(DiscordClient, arg);
            await _interactionService.ExecuteCommandAsync(context, _services);
        }
    }
}