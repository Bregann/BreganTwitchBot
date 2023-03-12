using BreganTwitchBot.Domain;
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

            DiscordClient.Ready += Ready;
            DiscordClient.Disconnected += Disconnected;
            DiscordClient.GuildMembersDownloaded += GuildMembersDownloaded;
            DiscordClient.InteractionCreated += InteractionCreated;
            DiscordClient.Log += LogData;
            DiscordClient.UserVoiceStateUpdated += UserVoiceStateUpdated;
            DiscordClient.UserLeft += UserLeft;
            DiscordClient.UserJoined += UserJoined;
            DiscordClient.MessageUpdated += MessageUpdated;
            DiscordClient.MessageReceived += MessageReceived;
            DiscordClient.UserIsTyping += UserIsTyping;
            DiscordClient.MessageDeleted += MessageDeleted;
            DiscordClient.ButtonExecuted += ButtonPressed;
            DiscordClient.PresenceUpdated += PresenceUpdated;
            DiscordClient.Log += LogError;

            await DiscordClient.LoginAsync(TokenType.Bot, AppConfig.DiscordAPIKey);
            await DiscordClient.StartAsync();
            _services = new ServiceCollection().AddSingleton(DiscordClient).AddSingleton<InteractionService>().BuildServiceProvider();
        }

        private static Task LogError(LogMessage arg)
        {
            Log.Information(arg.Message);
            return Task.CompletedTask;
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

        private static Task UserIsTyping(Cacheable<IUser, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2)
        {
            Log.Information($"[Discord Typing] {arg1.Value.Username} is typing a message in {arg2.Value.Name}");
            return Task.CompletedTask;
        }

        private static async Task MessageReceived(SocketMessage arg)
        {
            try
            {
                Log.Information($"[Discord Message Received] Username: {arg.Author.Username} Message: {arg.Content} Channel: {arg.Channel.Name}");
                await MessageReceivedEvent.CheckBlacklistedWords(arg);
                await MessageReceivedEvent.CheckStreamLiveMessages(arg);
                await MessageReceivedEvent.HandleCustomCommand(arg);
                MessageReceivedEvent.AddDiscordXp(arg);
            }
            catch (Exception e)
            {
                Log.Fatal($"[Discord Message Received] Error with received message: {e}- {e.InnerException}");
                return;
            }
        }

        private static async Task MessageUpdated(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage, ISocketMessageChannel channel)
        {
            await oldMessage.GetOrDownloadAsync();

            if (oldMessage.Value == null)
            {
                return;
            }

            if (oldMessage.Value.Content.ToLower() == newMessage.Content.ToLower())
            {
                return;
            }

            Log.Information($"[Discord Message Updated] Sender: {newMessage.Author.Username} \n Old Message: {oldMessage.Value.Content} \n New Message: {newMessage.Content}");
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

        private static Task Disconnected(Exception arg)
        {
            ConnectionStatus = false;
            Log.Warning("[Discord] Disconnected from Discord");
            BackgroundJob.Schedule(() => HangfireJobs.SendDiscordConnectionStatusToProjectMonitor(), TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private static Task LogData(LogMessage arg)
        {
            Log.Information($"{arg}");
            return Task.CompletedTask;
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

            _interactionService = new InteractionService(DiscordClient.Rest);
            await _interactionService.AddModulesAsync(assembly: Assembly.GetExecutingAssembly(), null);
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