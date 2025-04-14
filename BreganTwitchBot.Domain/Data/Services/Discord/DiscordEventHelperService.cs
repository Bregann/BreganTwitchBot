using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.DTOs.Discord.Events;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Discord;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;

namespace BreganTwitchBot.Domain.Data.Services.Discord
{
    public class DiscordEventHelperService (IDiscordService discordService, IConfigHelperService configHelper, IDiscordHelperService discordHelper, AppDbContext context) : IDiscordEventHelperService
    {
        public async Task HandleUserJoinedEvent(EventBase userJoined)
        {
            var discordConfig = configHelper.GetDiscordConfig(userJoined.GuildId);

            if (discordConfig == null)
            {
                Log.Warning($"[Discord Event] Discord config not found for guild {userJoined.GuildId} in the HandleUserJoinedEvent");
                return;
            }

            var twitchUsername = discordHelper.GetTwitchUsernameFromDiscordUser(userJoined.UserId);

            // Check if the event channel ID is set, if so then send the event message
            if (discordConfig.DiscordEventChannelId != null)
            {
                var messageEmbed = new EmbedBuilder()
                {
                    Title = "User Joined",
                    Timestamp = DateTime.Now,
                    Color = new Color(3, 207, 252)
                };

                messageEmbed.AddField("Discord Username", userJoined.Username);
                messageEmbed.AddField("User ID", userJoined.UserId.ToString());

                if (twitchUsername != null)
                {
                    messageEmbed.AddField("Twitch Username", twitchUsername);
                }

                await discordHelper.SendEmbedMessage(discordConfig.DiscordEventChannelId.Value, messageEmbed);
            }

            // check if the are linked, change the welcome message based on it
            if (discordConfig.DiscordWelcomeMessageChannelId != null)
            {
                var message = string.Empty;

                if (twitchUsername != null)
                {
                    message = $"Welcome <@{userJoined.UserId}> to the server! You already linked to Twitch as {twitchUsername}! {(discordConfig.DiscordUserCommandsChannelId != null ? $"You can use commands in <#{discordConfig.DiscordUserCommandsChannelId.Value}> ! :D" : "")}";
                }
                else
                {
                    message = $"Welcome <@{userJoined.UserId}> to the server! ${(discordConfig.DiscordUserCommandsChannelId != null ? $"To access awesome features head over to <#{discordConfig.DiscordUserCommandsChannelId.Value}> and use the command /link to link your Twitch account to the bot!! :D" : "")}";
                }

                await discordHelper.SendMessage(discordConfig.DiscordWelcomeMessageChannelId.Value, message);
            }
        }

        public async Task HandleUserLeftEvent(EventBase userLeft)
        {
            var discordConfig = configHelper.GetDiscordConfig(userLeft.GuildId);

            if (discordConfig == null)
            {
                Log.Warning($"[Discord Event] Discord config not found for guild {userLeft.GuildId} in the HandleUserLeftEvent");
                return;
            }

            // Check if the event channel ID is set, if so then send the event message
            if (discordConfig.DiscordEventChannelId != null)
            {
                var messageEmbed = new EmbedBuilder()
                {
                    Title = "User Left",
                    Timestamp = DateTime.Now,
                    Color = new Color(255, 0, 0)
                };
                messageEmbed.AddField("Discord Username", userLeft.Username);
                messageEmbed.AddField("User ID", userLeft.UserId.ToString());

                // check if they are linked, if so then add the twitch username and watch time stats for the channel
                var twitchUser = await context.ChannelUsers.FirstOrDefaultAsync(x => x.DiscordUserId == userLeft.UserId);
                var broadcaster = await context.Channels.FirstAsync(x => x.DiscordGuildId == userLeft.GuildId);

                if (twitchUser != null)
                {
                    messageEmbed.AddField("Twitch Username", twitchUser.TwitchUsername);

                    var userTime = await context.ChannelUserWatchtime
                        .Where(x => x.ChannelUserId == twitchUser.Id && x.ChannelId == broadcaster.Id)
                        .Select(x => x.MinutesInStream)
                        .FirstOrDefaultAsync();

                    var watchtimeTs = TimeSpan.FromMinutes(userTime);
                    messageEmbed.AddField("Watch time", $"{watchtimeTs.TotalMinutes} minutes (about {Math.Round(watchtimeTs.TotalMinutes / 60, 2)} hours)");
                    messageEmbed.AddField("Last seen by bot", twitchUser.LastSeen.ToString("dd/MM/yyyy HH:mm:ss"));
                }

                await discordHelper.SendEmbedMessage(discordConfig.DiscordEventChannelId.Value, messageEmbed);
            }
        }

        public async Task HandleMessageDeletedEvent(MessageDeletedEvent messageDeletedEvent)
        {
            var discordConfig = configHelper.GetDiscordConfig(messageDeletedEvent.GuildId);

            if (discordConfig == null)
            {
                Log.Warning($"[Discord Event] Discord config not found for guild {messageDeletedEvent.GuildId} in the HandleMessageDeletedEvent");
                return;
            }

            // Check if the event channel ID is set, if so then send the event message
            if (discordConfig.DiscordEventChannelId != null)
            {
                var messageEmbed = new EmbedBuilder()
                {
                    Title = "Message Deleted",
                    Timestamp = DateTime.Now,
                    Color = new Color(255, 0, 0)
                };

                messageEmbed.AddField("Discord Username", messageDeletedEvent.Username);
                messageEmbed.AddField("User ID", messageDeletedEvent.UserId.ToString());
                messageEmbed.AddField("Message ID", messageDeletedEvent.MessageId.ToString());
                messageEmbed.AddField("Channel ID", messageDeletedEvent.ChannelId.ToString());
                await discordHelper.SendEmbedMessage(discordConfig.DiscordEventChannelId.Value, messageEmbed);
            }
        }
    }
}
