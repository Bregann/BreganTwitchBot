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
        public static DiscordSocketClient DiscordClient;
        private static InteractionService _interactionService;
        private static IServiceProvider _services;
        private static bool botLoaded = false;
        public static bool ConnectionStatus;

        public static async Task StartDiscordBot()
        {
            await Task.Delay(2000);

            //Start Discord
            var discordThread = new Thread(MainAsync().GetAwaiter().GetResult);
            discordThread.Start();
        }

        public static async Task MainAsync()
        {
            DiscordClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                MessageCacheSize = 2000,
                AlwaysDownloadUsers = true,
                LogGatewayIntentWarnings = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });
            
            // _services = new ServiceCollection().AddSingleton(DiscordClient).AddSingleton<InteractionService>().BuildServiceProvider();
            _services = new ServiceCollection()
                .AddSingleton(DiscordClient)
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>())) // Register InteractionService
                .AddSingleton(x => new InteractionService(DiscordClient.Rest))
                .BuildServiceProvider();

            DiscordClient.Ready += Ready;
            DiscordClient.GuildMembersDownloaded += GuildMembersDownloaded;
            DiscordClient.InteractionCreated += InteractionCreated;
            DiscordClient.UserVoiceStateUpdated += UserVoiceStateUpdated;
            DiscordClient.UserLeft += UserLeft;
            DiscordClient.UserJoined += UserJoined;
            DiscordClient.MessageReceived += MessageReceived;
            DiscordClient.MessageDeleted += MessageDeleted;

            DiscordClient.ButtonExecuted += ButtonPressed;
            DiscordClient.PresenceUpdated += PresenceUpdated;

            await DiscordClient.LoginAsync(TokenType.Bot, AppConfig.DiscordAPIKey);
            await DiscordClient.StartAsync();
        }

        private static async Task PresenceUpdated(SocketUser user, SocketPresence previous, SocketPresence newUserUpdate)
        {
            //Don't do anything if the update is nothing
            if (newUserUpdate.Activities.Count == 0)
            {
                return;
            }

            try
            {
                PresenceEvent.TrackUserStatus(user, previous, newUserUpdate);
                await PresenceEvent.UpdateStreamerStatusMessage(user, previous, newUserUpdate);
            }
            catch (Exception e)
            {
                Log.Fatal($"[Discord Presence Updated] Error with presence: {e}- {e.InnerException}");
                return;
            }
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

        private static async Task MessageDeleted(Cacheable<IMessage, ulong> oldMessage, Cacheable<IMessageChannel, ulong> channel)
        {
            try
            {
                await MessageDeletedEvent.SendDeletedMessageToEvents(oldMessage, channel);
            }
            catch (Exception e)
            {
                Log.Fatal($"[Discord Message Deleted] Error with deleted message: {e}- {e.InnerException}");
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

        private static async Task UserJoined(SocketGuildUser userJoined)
        {
            await UserJoinedEvent.SendNewUserInfo(userJoined);
            Log.Information($"[Discord] User joined: {userJoined.Username}");
        }

        private static async Task UserLeft(SocketGuild guild, SocketUser user)
        {
            if (guild.Id != AppConfig.DiscordGuildID)
            {
                return;
            }

            await UserLeftEvent.UnlinkUserFromDiscordAndAlert(user);
            Log.Information($"[Discord] User Left: {user.Username}");
        }

        private static async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState prevState, SocketVoiceState newState)
        {
            await VoiceStateUpdatedEvent.AddOrRemoveVCRole(user, newState);
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

        private static Task GuildMembersDownloaded(SocketGuild arg)
        {
            Log.Information("[Discord] Members downloaded");
            return Task.CompletedTask;
        }

        private static async Task Ready()
        {
            ConnectionStatus = true;

            if (botLoaded)
            {
                Log.Information("[Discord Connection] Discord client already setup");
                return;
            }

            await DiscordClient.SetGameAsync("many users", type: ActivityType.Watching);
            await DiscordClient.DownloadUsersAsync(DiscordClient.Guilds);

            // Resolve InteractionService from DI

            try
            {
                _interactionService = _services.GetRequiredService<InteractionService>();

                // Add modules using DI
                await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding modules to InteractionService");
                throw;
            }

#if DEBUG
            await _interactionService.RegisterCommandsToGuildAsync(AppConfig.DiscordGuildID, true);
#else
    await _interactionService.RegisterCommandsGloballyAsync();
#endif
            botLoaded = true;
            Log.Information("[Discord Connection] Discord Client Ready");
        }
    }
}