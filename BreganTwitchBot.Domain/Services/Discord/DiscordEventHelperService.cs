using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Discord.Events;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Discord
{
    public class DiscordEventHelperService(
        AppDbContext context, 
        IConfigHelperService configHelper, 
        IDiscordHelperService discordHelper, 
        IDiscordRoleManagerService discordRoleManagerService, 
        IDiscordUserLookupService discordUserLookupService
        ) : IDiscordEventHelperService
    {
        public async Task HandleUserJoinedEvent(EventBase userJoined)
        {
            var discordConfig = configHelper.GetDiscordConfig(userJoined.GuildId);

            if (discordConfig == null)
            {
                Log.Warning($"[Discord Event] Discord config not found for guild {userJoined.GuildId} in the HandleUserJoinedEvent");
                return;
            }

            var twitchUsername = discordUserLookupService.GetTwitchUsernameFromDiscordUser(userJoined.UserId);

            await discordHelper.AddDiscordUserToDatabaseOnGuildJoin(userJoined.GuildId, userJoined.UserId);
            await discordRoleManagerService.AddRolesToUserOnGuildJoin(userJoined.UserId, userJoined.GuildId);

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
                    message = $"Welcome <@{userJoined.UserId}> to the server! You are already linked to Twitch as {twitchUsername}! {(discordConfig.DiscordUserCommandsChannelId != null ? $"You can use commands in <#{discordConfig.DiscordUserCommandsChannelId.Value}> ! :D" : "")}";
                }
                else
                {
                    message = $"Welcome <@{userJoined.UserId}> to the server! {(discordConfig.DiscordUserCommandsChannelId != null ? $"To access awesome features head over to <#{discordConfig.DiscordUserCommandsChannelId.Value}> and use the command /link to link your Twitch account to the bot!! :D" : "")}";
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
                var broadcaster = await context.Channels.FirstAsync(x => x.ChannelConfig.DiscordGuildId == userLeft.GuildId);

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
                messageEmbed.AddField("Channel Name", messageDeletedEvent.ChannelName);
                messageEmbed.AddField("Message Content", string.IsNullOrWhiteSpace(messageDeletedEvent.MessageContent) ? "*No content (probably an embed or attachment)*" : messageDeletedEvent.MessageContent);
                await discordHelper.SendEmbedMessage(discordConfig.DiscordEventChannelId.Value, messageEmbed);
            }
        }

        public async Task HandleMessageReceivedEvent(MessageReceivedEvent messageReceivedEvent)
        {
            // george food hardcoded memes, only for blocksssssss
            if (messageReceivedEvent.UserId == 153974235809710081 && messageReceivedEvent.ChannelId == 1153032190234464347 && messageReceivedEvent.MessageContent.ToLower().Contains("#ping"))
            {
                await discordHelper.SendMessage(1153032190234464347, "<@&1164590388296810648> some banging new food just dropped by the legend himself");
            }

            if (messageReceivedEvent.HasAttachments)
            {
                await discordHelper.AddDiscordXpToUser(messageReceivedEvent.GuildId, messageReceivedEvent.ChannelId, messageReceivedEvent.UserId, 10);
            }
            else
            {
                await discordHelper.AddDiscordXpToUser(messageReceivedEvent.GuildId, messageReceivedEvent.ChannelId, messageReceivedEvent.UserId, 5);
            }
        }

        public async Task<(string MessageToSend, bool Ephemeral)> HandleButtonPressEvent(ButtonPressedEvent buttonPressedEvent, DiscordSocketClient client)
        {
            var emojiToAdd = "";

            switch (buttonPressedEvent.CustomId)
            {
                case "christmas-snowman":
                    emojiToAdd = "⛄";
                    break;
                case "christmas-gift":
                    emojiToAdd = "🎁";
                    break;
                case "christmas-tree":
                    emojiToAdd = "🎄";
                    break;
                case "christmas-santa":
                    emojiToAdd = "🎅";
                    break;
                case "christmas-mrsanta":
                    emojiToAdd = "🤶";
                    break;
                case "christmas-star":
                    emojiToAdd = "🌟";
                    break;
                case "christmas-socks":
                    emojiToAdd = "🧦";
                    break;
                case "christmas-bell":
                    emojiToAdd = "🔔";
                    break;
                case "christmas-deer":
                    emojiToAdd = "🦌";
                    break;
                case "christmas-resetusername":
                    emojiToAdd = "";
                    break;
                default:
                    return ("invalid button", true);
            }

            var guild = client.GetGuild(buttonPressedEvent.GuildId);
            var user = guild.GetUser(buttonPressedEvent.UserId);

            if (emojiToAdd == "")
            {
                var nickNameToSet = user.Nickname.Replace("⛄", "").Replace("🎁", "").Replace("🎄", "").Replace("🎅", "").Replace("🤶", "").Replace("🌟", "").Replace("🧦", "").Replace("🔔", "").Replace("🦌", "");
                await user.ModifyAsync(user => user.Nickname = nickNameToSet);
                return ("Your nickname has been cleared!", true);
            }
            else
            {
                var nickNameToSet = "";

                if (user.DisplayName != null)
                {
                    nickNameToSet = emojiToAdd + user.DisplayName + emojiToAdd;
                }
                else
                {
                    nickNameToSet = emojiToAdd + user.Username + emojiToAdd;
                }

                await user.ModifyAsync(user => user.Nickname = nickNameToSet);
                return ("Your nickname has been set! Woooo", true);
            }
        }
    }
}
