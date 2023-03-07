using BreganTwitchBot.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Bot.DiscordBot.SlashCommands.Data.GeneralCommands.Giveaway;
using BreganTwitchBot.Domain.Bot.DiscordBot.SlashCommands.Data.Levelling;
using BreganTwitchBot.Domain.Data.DiscordBot.Events;
using BreganTwitchBot.Domain.Data.TwitchBot;
using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Infrastructure.Database.Models;
using BreganTwitchBot.Services;
using Discord;
using Discord.WebSocket;
using Hangfire;
using Humanizer;
using Humanizer.Localisation;
using Serilog;
using System.Text.RegularExpressions;

namespace BreganTwitchBot.DiscordBot.Events
{
    public class DiscordEvents
    {
        private static List<string> _urlDomains;
        private static List<string> _permBanWords;
        private static long _lastAskedMessageCount = 6;

        public static async Task SetupDiscordEvents()
        {
            DiscordConnection.DiscordClient.UserVoiceStateUpdated += UserVoiceStateUpdated;
            DiscordConnection.DiscordClient.UserLeft += UserLeft;
            DiscordConnection.DiscordClient.UserJoined += UserJoined;
            DiscordConnection.DiscordClient.MessageUpdated += MessageUpdated;
            DiscordConnection.DiscordClient.MessageReceived += MessageReceived;
            DiscordConnection.DiscordClient.UserIsTyping += UserIsTyping;
            DiscordConnection.DiscordClient.MessageDeleted += MessageDeleted;
            DiscordConnection.DiscordClient.ButtonExecuted += ButtonPressed;
            DiscordConnection.DiscordClient.PresenceUpdated += PresenceUpdated;
            DiscordConnection.DiscordClient.Log += LogError;
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
                await ButtonPressedEvent.HandleGiveawayButton(arg);
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
            Log.Information($"[Discord Message Received] Username: {arg.Author.Username} Message: {arg.Content} Channel: {arg.Channel.Name}");
            await MessageReceivedEvent.CheckBlacklistedWords(arg);
            await MessageReceivedEvent.CheckStreamLiveMessages(arg);
            await MessageReceivedEvent.SendGiveawayMessage(arg);
            MessageReceivedEvent.AddDiscordXp(arg);
        }

        private static async Task MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            await arg1.GetOrDownloadAsync();

            if (arg1.Value == null)
            {
                return;
            }

            if (arg1.Value.Content.ToLower() == arg2.Content.ToLower())
            {
                return;
            }

            var message = arg2.Content.Replace(Environment.NewLine, "").Replace(" ", "");
            if (message == "****" || message == "__")
            {
                await arg2.DeleteAsync();
                return;
            }

            Log.Information($"[Discord Message Updated] Sender: {arg2.Author.Username} \n Old Message: {arg1.Value.Content} \n New Message: {arg2.Content}");
        }

        private static async Task UserJoined(SocketGuildUser arg)
        {
            if (arg.Guild.Id != AppConfig.DiscordGuildID)
            {
                return;
            }

            StreamStatsService.UpdateStreamStat(1, StatTypes.AmountOfDiscordUsersJoined);

            var messageEmbed = new EmbedBuilder()
            {
                Title = "User Joined",
                Timestamp = DateTime.Now,
                Color = new Color(3, 207, 252)
            };

            messageEmbed.AddField("Discord Username", arg.Username);

            var channel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordEventChannelID) as IMessageChannel;
            await channel.SendMessageAsync(embed: messageEmbed.Build());

            using (var how2LinkGif = new FileStream($"Skins/how2link.gif", FileMode.Open))
            {
                var channel2 = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordLinkingChannelID) as IMessageChannel;
                await channel2.SendFileAsync(how2LinkGif, "how2link.gif", $"Welcome {arg.Mention}! \n\n To access the Discord Server, you must link your Twitch account with Discord by using the ``/link`` command!");
            };

            Log.Information($"[Discord] User joined: {arg.Username}");
        }

        private static async Task UserLeft(SocketGuild arg1, SocketUser arg2)
        {
            if (arg1.Id != AppConfig.DiscordGuildID)
            {
                return;
            }

            var messageEmbed = new EmbedBuilder()
            {
                Title = "User Left",
                Timestamp = DateTime.Now,
                Color = new Color(255, 112, 51)
            };

            Users? user = null;

            using (var context = new DatabaseContext())
            {
                user = context.Users.Where(x => x.DiscordUserId == arg2.Id).FirstOrDefault();
            }

            messageEmbed.AddField("Userleft", arg2.Username, true);

            if (user == null)
            {
                messageEmbed.AddField("Was Linked?", "false", true);
                messageEmbed.AddField("Twitch username", "n/a", true);
            }
            else //
            {
                var userTime = TimeSpan.FromMinutes(user.MinutesInStream);

                messageEmbed.AddField("Twitch username", user.Username);
                messageEmbed.AddField("Minutes watched", userTime.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7), true);
                messageEmbed.AddField("Last in stream", user.LastSeenDate, true);

                using (var context = new DatabaseContext())
                {
                    context.Users.Where(x => x.DiscordUserId == arg2.Id).FirstOrDefault().DiscordUserId = 0;
                    context.SaveChanges();
                }
            }

            var channel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordEventChannelID) as IMessageChannel;
            await channel.SendMessageAsync(embed: messageEmbed.Build());

            Log.Information($"[Discord] User Left: {arg2.Username}");
        }

        private static async Task UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
            var user = guild.GetUser(arg1.Id);
            var vcRole = guild.Roles.First(x => x.Name == "VC");

            if (arg3.VoiceChannel == null)
            {
                await user.RemoveRoleAsync(vcRole);
            }
            else
            {
                await user.AddRoleAsync(vcRole);
            }
        }
    }
}