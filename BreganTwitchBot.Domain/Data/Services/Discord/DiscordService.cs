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

        public DiscordSocketClient Client => _client;

        public DiscordService(IEnvironmentalSettingHelper environmentalSettingHelper, IServiceProvider serviceProvider)
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



            Serilog.Log.Information("Discord Setup");
        }

        private Task PresenceUpdated(SocketUser arg1, SocketPresence arg2, SocketPresence arg3)
        {
            throw new NotImplementedException();
        }

        private Task ButtonExecuted(SocketMessageComponent arg)
        {
            throw new NotImplementedException();
        }

        private Task MessageDeleted(Cacheable<IMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2)
        {
            throw new NotImplementedException();
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

        private Task UserJoined(SocketGuildUser arg)
        {
            throw new NotImplementedException();
        }

        private Task UserLeft(SocketGuild arg1, SocketUser arg2)
        {
            throw new NotImplementedException();
        }

        private Task UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            throw new NotImplementedException();
        }

        private Task LogData(LogMessage arg)
        {
            Log.Information($"[Discord] {arg.Severity} - {arg.Source} - {arg.Message} - {arg.Exception}");
            return Task.CompletedTask;
        }

        private Task GuildMembersDownloaded(SocketGuild arg)
        {
            throw new NotImplementedException();
        }

        private Task Disconnected(Exception arg)
        {
            Log.Warning("[Discord] Disconnected from Discord");
            return Task.CompletedTask;
        }

        private async Task InteractionCreated(SocketInteraction interaction)
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await _interactionService.ExecuteCommandAsync(ctx, _services);
        }

        private async Task Ready()
        {

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
