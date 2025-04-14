using BreganTwitchBot.Domain.DTOs.Discord.Events;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord
{
    public class DiscordService : IDiscordService
    {
        private readonly DiscordSocketClient _client;
        private readonly IEnvironmentalSettingHelper _environmentalSettingHelper;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _services;
        private readonly IDiscordEventHelperService _discordEventHelperService;
        private readonly IDiscordHelperService _discordHelperService;

        public DiscordSocketClient Client => _client;

        public DiscordService(IEnvironmentalSettingHelper environmentalSettingHelper, IServiceProvider serviceProvider, IDiscordEventHelperService discordEventHelperService, IDiscordHelperService discordHelperService)
        {
            _environmentalSettingHelper = environmentalSettingHelper;
            _services = serviceProvider;

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                MessageCacheSize = 2000,
                AlwaysDownloadUsers = true,
                LogGatewayIntentWarnings = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

            _interactionService = new InteractionService(_client.Rest);
            _discordEventHelperService = discordEventHelperService;
            _discordHelperService = discordHelperService;
        }

        public async Task StartAsync()
        {
            var token = _environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.DiscordBotToken);

            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Discord bot token is not set.");
            }

            // Hook the handler BEFORE starting
            _client.Ready += Ready;
            _client.InteractionCreated += InteractionCreated;
            _client.Disconnected += Disconnected;
            _client.GuildMembersDownloaded += GuildMembersDownloaded;
            _client.Log += LogData;
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            _client.UserLeft += UserLeft;
            _client.UserJoined += UserJoined;
            _client.MessageUpdated += MessageUpdated;
            _client.MessageReceived += MessageReceived;
            _client.UserIsTyping += UserIsTyping;
            _client.MessageDeleted += MessageDeleted;
            _client.ButtonExecuted += ButtonExecuted;
            _client.PresenceUpdated += PresenceUpdated;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            Log.Information("Discord Setup");
        }

        private Task PresenceUpdated(SocketUser user, SocketPresence previous, SocketPresence newUserUpdate)
        {
            //Don't do anything if the update is nothing
            if (newUserUpdate.Activities.Count == 0)
            {
                return Task.CompletedTask;
            }

            var previousStatusData = previous.Activities?.Where(x => x.Type == ActivityType.CustomStatus).FirstOrDefault() as CustomStatusGame;
            var newStatusData = newUserUpdate.Activities.Where(x => x.Type == ActivityType.CustomStatus).FirstOrDefault() as CustomStatusGame;


            //If the statuses match then don't process them
            if (previousStatusData?.State == newStatusData?.State)
            {
                return Task.CompletedTask;
            }

            //We might as well log the statuses for the memes
            Log.Information($"[User Status] Discord status for user {user.Username} changed from {previousStatusData?.State ?? "null"} to {newStatusData?.State ?? "null"} - userId {user.Id}");
            return Task.CompletedTask;
        }

        private Task ButtonExecuted(SocketMessageComponent arg)
        {
            throw new NotImplementedException();
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> oldMessage, Cacheable<IMessageChannel, ulong> channel)
        {
            await oldMessage.DownloadAsync();

            if (oldMessage.Value.Content == null || oldMessage.Value.Author.Id == _client.CurrentUser.Id)
            {
                return;
            }

            if (oldMessage.Value.Content.Replace(Environment.NewLine, "").Replace(" ", "") == "****" || oldMessage.Value.Content.Replace(Environment.NewLine, "").Replace(" ", "") == "__")
            {
                return;
            }

            var messageDeleted = new MessageDeletedEvent
            {
                GuildId = channel.Id,
                UserId = oldMessage.Value.Author.Id,
                Username = oldMessage.Value.Author.Username,
                MessageId = oldMessage.Value.Id,
                ChannelId = channel.Id,
                ChannelName = channel.Value.Name,
                MessageContent = oldMessage.Value.Content
            };

            await _discordEventHelperService.HandleMessageDeletedEvent(messageDeleted);

            Log.Information($"[Discord Message Deleted] Sender: {oldMessage.Value.Author.Username} \n Message: {oldMessage.Value.Content} \n Channel: {channel.Value.Name} \n ChannelId: {channel.Id}");
        }

        private Task UserIsTyping(Cacheable<IUser, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2)
        {
            Log.Information($"[Discord Typing] {arg1.Value.Username} is typing a message in {arg2.Value.Name}");
            return Task.CompletedTask;
        }

        private Task MessageReceived(SocketMessage arg)
        {
            throw new NotImplementedException();
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage, ISocketMessageChannel channel)
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

        private async Task UserJoined(SocketGuildUser arg)
        {
            var userJoined = new EventBase
            {
                GuildId = arg.Guild.Id,
                UserId = arg.Id,
                Username = arg.Username
            };

            await _discordEventHelperService.HandleUserJoinedEvent(userJoined);
            Log.Information($"[Discord User Joined] {arg.Username} joined the server {arg.Guild.Name}");
        }

        private async Task UserLeft(SocketGuild arg1, SocketUser arg2)
        {
            var userLeft = new EventBase
            {
                GuildId = arg1.Id,
                UserId = arg2.Id,
                Username = arg2.Username
            };

            await _discordEventHelperService.HandleUserLeftEvent(userLeft);
            Log.Information($"[Discord User Left] {arg2.Username} left the server {arg1.Name}");
        }

        private Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            if (oldState.VoiceChannel == null && newState.VoiceChannel != null)
            {
                Log.Information($"[Discord Voice] {user.Username} joined {newState.VoiceChannel.Name}");
            }
            else if (oldState.VoiceChannel != null && newState.VoiceChannel == null)
            {
                Log.Information($"[Discord Voice] {user.Username} left {oldState.VoiceChannel.Name}");
            }
            else if (oldState.VoiceChannel != null && newState.VoiceChannel != null)
            {
                Log.Information($"[Discord Voice] {user.Username} moved from {oldState.VoiceChannel.Name} to {newState.VoiceChannel.Name}");
            }

            return Task.CompletedTask;
        }

        private Task LogData(LogMessage arg)
        {
            Log.Information($"[Discord] {arg.Severity} - {arg.Source} - {arg.Message} - {arg.Exception}");
            return Task.CompletedTask;
        }

        private Task GuildMembersDownloaded(SocketGuild arg)
        {
            Log.Information($"[Discord] Guild members downloaded for {arg.Name}");
            return Task.CompletedTask;
        }

        private Task Disconnected(Exception arg)
        {
            Log.Warning("[Discord] Disconnected from Discord");
            return Task.CompletedTask;
        }

        private async Task InteractionCreated(SocketInteraction interaction)
        {
            var command = interaction as SocketSlashCommand;

            if (command == null)
            {
                return;
            }

            var isMod = _discordHelperService.IsUserMod(command.User.Id, command.GuildId ?? 0);

            var ctx = new SocketInteractionContext(_client, interaction);
            await _interactionService.ExecuteCommandAsync(ctx, _services);
        }

        private async Task Ready()
        {

            await _client.SetGameAsync("many users", type: ActivityType.Watching);
            await _client.DownloadUsersAsync(_client.Guilds);

#if DEBUG
            await _interactionService.RegisterCommandsToGuildAsync(196696160486948864, true);
#else
            await _interactionService.RegisterCommandsGloballyAsync(true);
#endif
        }

        public async Task StopAsync()
        {
            await _client.LogoutAsync();
            await _client.StopAsync();
        }
    }
}
