using BreganTwitchBot.Domain.DTOs.Discord.Events;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

namespace BreganTwitchBot.Domain.Data.Services.Discord
{
    public class DiscordService : IDiscordService, IDiscordClientProvider
    {
        public DiscordSocketClient Client { get; }

        private readonly IEnvironmentalSettingHelper _environmentalSettingHelper;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _services;
        private readonly IDiscordUserLookupService _discordUserLookupService;
        private readonly IConfigHelperService _configHelperService;

        public DiscordService(IEnvironmentalSettingHelper environmentalSettingHelper, IServiceProvider serviceProvider, IDiscordUserLookupService discordUserLookupService, IConfigHelperService configHelperService)
        {
            _environmentalSettingHelper = environmentalSettingHelper;
            _services = serviceProvider;

            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All,
                MessageCacheSize = 2000,
                AlwaysDownloadUsers = true,
                LogGatewayIntentWarnings = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

            _interactionService = new InteractionService(Client.Rest);
            _discordUserLookupService = discordUserLookupService;
            _configHelperService = configHelperService;
        }

        public async Task StartAsync()
        {
            var token = _environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.DiscordBotToken);

            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Discord bot token is not set.");
            }

            // Hook the handler BEFORE starting
            Client.Ready += Ready;
            Client.InteractionCreated += InteractionCreated;
            Client.Disconnected += Disconnected;
            Client.GuildMembersDownloaded += GuildMembersDownloaded;
            Client.Log += LogData;
            Client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            Client.UserLeft += UserLeft;
            Client.UserJoined += UserJoined;
            Client.MessageUpdated += MessageUpdated;
            Client.MessageReceived += MessageReceived;
            Client.UserIsTyping += UserIsTyping;
            Client.MessageDeleted += MessageDeleted;
            Client.ButtonExecuted += ButtonExecuted;
            Client.PresenceUpdated += PresenceUpdated;

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();

            await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

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

        private async Task ButtonExecuted(SocketMessageComponent arg)
        {
            Log.Information($"[Discord Button Pressed] Sender: {arg.User.Username} \n Button: {arg.Data.CustomId} \n Channel: {arg.Channel.Name} \n ChannelId: {arg.Channel.Id}");
            await arg.DeferAsync();

            using (var scope = _services.CreateScope())
            {
                var discordEventHelperService = scope.ServiceProvider.GetRequiredService<IDiscordEventHelperService>();

                var res = await discordEventHelperService.HandleButtonPressEvent(new ButtonPressedEvent
                {
                    GuildId = arg.GuildId ?? 0,
                    UserId = arg.User.Id,
                    Username = arg.User.Username,
                    CustomId = arg.Data.CustomId
                });

                await arg.FollowupAsync(res.MessageToSend, ephemeral: res.Ephemeral);
            }
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> oldMessage, Cacheable<IMessageChannel, ulong> channel)
        {
            await oldMessage.DownloadAsync();

            if (oldMessage.Value.Content == null || oldMessage.Value.Author.Id == Client.CurrentUser.Id)
            {
                return;
            }

            if (oldMessage.Value.Content.Replace(Environment.NewLine, "").Replace(" ", "") == "****" || oldMessage.Value.Content.Replace(Environment.NewLine, "").Replace(" ", "") == "__")
            {
                return;
            }

            using (var scope = _services.CreateScope())
            {
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

                var discordEventHelperService = scope.ServiceProvider.GetRequiredService<IDiscordEventHelperService>();
                await discordEventHelperService.HandleMessageDeletedEvent(messageDeleted);
            }

            Log.Information($"[Discord Message Deleted] Sender: {oldMessage.Value.Author.Username} \n Message: {oldMessage.Value.Content} \n Channel: {channel.Value.Name} \n ChannelId: {channel.Id}");
        }

        private Task UserIsTyping(Cacheable<IUser, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2)
        {
            Log.Information($"[Discord Typing] {arg1.Value.Username} is typing a message in {arg2.Value.Name}");
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            Log.Information($"[Discord Message] Sender: {arg.Author.Username} \n Message: {arg.Content} \n Channel: {arg.Channel.Name} \n ChannelId: {arg.Channel.Id} \n");

            using (var scope = _services.CreateScope())
            {
                var discordEventHelperService = scope.ServiceProvider.GetRequiredService<IDiscordEventHelperService>();

                // convert channel to guildCHannel
                var guildChannel = arg.Channel as SocketGuildChannel;

                if (guildChannel == null)
                {
                    Log.Warning($"[Discord Message] Channel is not a guild channel. ChannelId: {arg.Channel.Id}");
                    return;
                }

                var messageReceived = new MessageReceivedEvent
                {
                    GuildId = guildChannel.Guild.Id,
                    UserId = arg.Author.Id,
                    Username = arg.Author.Username,
                    ChannelId = arg.Channel.Id,
                    ChannelName = arg.Channel.Name,
                    MessageContent = arg.Content,
                    HasAttachments = arg.Attachments.Count > 0,
                    MessageId = arg.Id
                };

                await discordEventHelperService.HandleMessageReceivedEvent(messageReceived);
            }
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
            using (var scope = _services.CreateScope())
            {
                var discordEventHelperService = scope.ServiceProvider.GetRequiredService<IDiscordEventHelperService>();

                var userJoined = new EventBase
                {
                    GuildId = arg.Guild.Id,
                    UserId = arg.Id,
                    Username = arg.Username
                };

                await discordEventHelperService.HandleUserJoinedEvent(userJoined);
            }

            Log.Information($"[Discord User Joined] {arg.Username} joined the server {arg.Guild.Name}");
        }

        private async Task UserLeft(SocketGuild arg1, SocketUser arg2)
        {
            using (var scope = _services.CreateScope())
            {
                var discordEventHelperService = scope.ServiceProvider.GetRequiredService<IDiscordEventHelperService>();

                var userLeft = new EventBase
                {
                    GuildId = arg1.Id,
                    UserId = arg2.Id,
                    Username = arg2.Username
                };

                await discordEventHelperService.HandleUserLeftEvent(userLeft);
            }

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
            var config = _configHelperService.GetDiscordConfig(interaction.GuildId ?? 0);

            if (config == null)
            {
                await interaction.RespondAsync("This command is not available in this server.", ephemeral: true);
                return;
            }

            if (command == null)
            {
                return;
            }

            var user = Client.GetGuild(interaction.GuildId ?? 0).GetUser(command.User.Id);

            var isMod = _discordUserLookupService.IsUserMod(command.User.Id, user);

            // check if they are linked by getting twitch username from their id
            var twitchUsername = _discordUserLookupService.GetTwitchUsernameFromDiscordUser(command.User.Id);

            if (twitchUsername == null && command.CommandName != "link")
            {
                await interaction.RespondAsync($"You need to link your Twitch account to use this command. Use `/link` to link your account in the channel <#{config.DiscordUserCommandsChannelId}>", ephemeral: true);
                return;
            }

            // hardcoded socks channel for blocksssssss meh
            if (interaction.Channel.Id != 713365310408884236 && command.CommandName == "socks")
            {
                await interaction.RespondAsync("The socks command is only supported in Blocksssssss Discord in <#713365310408884236> . You can join the Discord here https://discord.gg/s2GGPpj", ephemeral: true);
                return;
            }

            // process it if it does match as we don't want to force the user to use the commands channel
            if (interaction.Channel.Id == 713365310408884236 && command.CommandName == "socks")
            {
                var contextSocks = new SocketInteractionContext(Client, interaction);
                await _interactionService.ExecuteCommandAsync(contextSocks, _services);
                return;
            }

            if (interaction.Channel.Id != config.DiscordUserCommandsChannelId && !isMod)
            {
                await interaction.RespondAsync($"Please use the bot commands channel! The command channel is <#{config.DiscordUserCommandsChannelId}>", ephemeral: true);
            }

            var ctx = new SocketInteractionContext(Client, interaction);
            await _interactionService.ExecuteCommandAsync(ctx, _services);
        }

        private async Task Ready()
        {

            await Client.SetGameAsync("todo: this message", type: ActivityType.Watching);
            await Client.DownloadUsersAsync(Client.Guilds);

#if DEBUG
            await _interactionService.RegisterCommandsToGuildAsync(196696160486948864, true);
#else
            await _interactionService.RegisterCommandsGloballyAsync(true);
#endif
        }

        public async Task StopAsync()
        {
            await Client.LogoutAsync();
            await Client.StopAsync();
        }
    }
}
