using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Levelling;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Infrastructure.Database.Enums;
using Discord;
using Discord.WebSocket;
using Hangfire;
using Hangfire.Annotations;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.DiscordBot.Events
{
    public class MessageReceivedEvent
    {
        private static List<string> _permBanWords = new();
        private static readonly List<string> _urlDomains = new() { ".xxx", ".tv", ".travel", ".tel", ".ru", ".org", ".net", ".name", ".museum", ".mobi", ".me", ".ly", ".jobs", ".int", ".info", ".gov", ".gg", ".coop", ".xyz", ".com", ".co.uk", ".cat", ".biz", "asia", ".aero", ".gift" };
        private static long _lastAskedMessageCount = 6;
        public static Dictionary<ulong, DateTime> XpCooldownDict = new Dictionary<ulong, DateTime>();

        public static void LoadBlacklistedWords()
        {
            using (var context = new DatabaseContext())
            {
                _permBanWords = context.Blacklist.Where(x => x.WordType == WordTypes.PermBanWord).Select(x => x.Word).ToList();
            }
        }

        public static async Task CheckBlacklistedWords(SocketMessage message)
        {
            _lastAskedMessageCount++;

            var removedMessage = message.Content.Replace(Environment.NewLine, "").Replace(" ", "");
            if (removedMessage == "****" || removedMessage == "__")
            {
                await message.DeleteAsync();
                return;
            }

            if (message.Author.IsBot)
            {
                return;
            }

            if (message.Channel.Id == AppConfig.DiscordSocksChannelID)
            {
                await message.DeleteAsync();
            }

            if (message.Content.ToLower().Contains("nitro") || message.Content.ToLower().Contains(".gift"))
            {
                Log.Information("[Discord Scam Check] Potential scam");

                if (message.Content.ToLower().Contains("free") || message.Content.ToLower().Contains("gift") || message.Content.ToLower().Contains("nitro") || message.Content.ToLower().Contains(".gift"))
                {
                    if (message.Content.ToLower().Contains("discord.com") || message.Content.ToLower().Contains("discord.gg"))
                    {
                        Log.Information("[Discord Scam Check] Not a scam");
                    }
                    else
                    {
                        if (_urlDomains.Any(message.Content.ToLower().Contains))
                        {
                            Log.Information("[Discord Scam Check] probably a scam");
                            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
                            var user = guild.GetUser(message.Author.Id);
                            var muteRole = guild.Roles.First(x => x.Name == "mute");

                            await user.AddRoleAsync(muteRole);
                            await DiscordHelper.SendMessage(message.Channel.Id, $"<@&{AppConfig.DiscordBanRole}> potential scam link");
                            return;
                        }
                    }
                }
            }

            var messageRegex = Regex.Replace(message.Content, @"[^0-9a-zA-Z⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊⠋⠌⠍⠎⠏⠐⠑⠒⠓⠔⠕⠖⠗⠘⠙⠚⠛⠜⠝⠞⠟⠠⠡⠢⠣⠤⠥⠦⠧⠨⠩⠪⠫⠬⠭⠮⠯⠰⠱⠲⠳⠴⠵⠶⠷⠸⠹⠺⠻⠼⠽⠾⠿⡀⡁⡂⡃⡄⡅⡆⡇⡈⡉⡊⡋⡌⡍⡎⡏⡐⡑⡒⡓⡔⡕⡖⡗⡘⡙⡚⡛⡜⡝⡞⡟⡠⡡⡢⡣⡤⡥⡦⡧⡨⡩⡪⡫⡬⡭⡮⡯⡰⡱⡲⡳⡴⡵⡶⡷⡸⡹⡺⡻⡼⡽⡾⡿⢀⢁⢂⢃⢄⢅⢆⢇⢈⢉⢊⢋⢌⢍⢎⢏⢐⢑⢒⢓⢔⢕⢖⢗⢘⢙⢚⢛⢜⢝⢞⢟⢠⢡⢢⢣⢤⢥⢦⢧⢨⢩⢪⢫⢬⢭⢮⢯⢰⢱⢲⢳⢴⢵⢶⢷⢸⢹⢺⢻⢼⢽⢾⢿⣀⣁⣂⣃⣄⣅⣆⣇⣈⣉⣊⣋⣌⣍⣎⣏⣐⣑⣒⣓⣔⣕⣖⣗⣘⣙⣚⣛⣜⣝⣞⣟⣠⣡⣢⣣⣤⣥⣦⣧⣨⣩⣪⣫⣬⣭⣮⣯⣰⣱⣲⣳⣴⣵⣶⣷⣸⣹⣺⣻⣼⣽⣾⣿░█▄▌▀─\p{L}]+", "").ToLower();

            //Racist children
            if (_permBanWords.Any(messageRegex.Contains))
            {
                var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
                var user = guild.GetUser(message.Author.Id);
                var muteRole = guild.Roles.First(x => x.Name == "mute");

                await user.AddRoleAsync(muteRole);
                await DiscordHelper.SendMessage(message.Channel.Id, $"<@&{AppConfig.DiscordBanRole}> potential racist edgelord child");
                return;
            }
        }

        public static void AddDiscordXp(SocketMessage message)
        {
            if (message.Attachments.Count > 0)
            {
                var attachments = message.Attachments.ToList();
                Log.Information($"[Discord Message Received] Username: {message.Author} Image URL: {attachments[0].Url} Channel: {message.Channel.Name}");
                BackgroundJob.Enqueue(() => DiscordLevelling.AddDiscordXp(message.Author.Id, 10, message.Channel.Id));
                return;
            }

            //limit it so it needs to be a proper sentence
            if (message.Content.Length >= 3)
            {
                //Check if they are not on cooldown for the xp gains - 5s to prevent spam
                if (!XpCooldownDict.ContainsKey(message.Author.Id))
                {
                    XpCooldownDict.Add(message.Author.Id, DateTime.UtcNow);
                    BackgroundJob.Enqueue(() => DiscordLevelling.AddDiscordXp(message.Author.Id, 5, message.Channel.Id));
                    return;
                }

                if (DateTime.UtcNow - TimeSpan.FromSeconds(2) <= XpCooldownDict[message.Author.Id])
                {
                    return;
                }

                BackgroundJob.Enqueue(() => DiscordLevelling.AddDiscordXp(message.Author.Id, 5, message.Channel.Id));
                XpCooldownDict[message.Author.Id] = DateTime.UtcNow;
                return;
            }
        }

        public static async Task CheckStreamLiveMessages(SocketMessage message)
        {
            if (message.Content.ToLower().StartsWith("stream starts") && message.Author.Id == AppConfig.DiscordGuildOwner)
            {
                var msg = message as IUserMessage;

                if (msg != null)
                {
                    AppConfig.UpdatePinnedMessageAndMessageId(msg.Content, msg.Id);
                }
            }

            if (message.Content.ToLower().Contains("streaming today") || message.Content.ToLower().Contains("stream today") || message.Content.ToLower().Contains("when stream"))
            {
                if (_lastAskedMessageCount <= 5)
                {
                    return;
                }

                if (message.Author.Id == AppConfig.DiscordGuildOwner)
                {
                    await DiscordHelper.SendMessage(message.Channel.Id, "https://i.imgur.com/pEjmRjb.png");
                    return;
                }

                var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var diff = AppConfig.PinnedMessageDate - origin;
                var epochSeconds = Math.Floor(diff.TotalSeconds);

                if (DateTime.UtcNow.DayOfWeek != DayOfWeek.Sunday)
                {
                    if (AppConfig.PinnedMessageDate.Date != DateTime.UtcNow.Date)
                    {
                        await DiscordHelper.SendMessage(message.Channel.Id, "Looking for when the stream starts? Probably not today (ignore this if it's castles week then check <#354732299180441610>). Check <#754372306222186626> for his days off");
                        _lastAskedMessageCount = 0;
                        return;
                    }

                    await DiscordHelper.SendMessage(message.Channel.Id, $"Looking for when the stream starts? The last update I know is: **{AppConfig.PinnedStreamMessage}** which was set <t:{epochSeconds}:R>");
                    _lastAskedMessageCount = 0;
                }
                else
                {
                    if (AppConfig.PinnedMessageDate.Date != DateTime.UtcNow.Date)
                    {
                        await DiscordHelper.SendMessage(message.Channel.Id, "Looking for when the stream starts? Check the pins in the top right or go to <#754372306222186626> or <#354732299180441610>");
                        _lastAskedMessageCount = 0;
                        return;
                    }

                    await DiscordHelper.SendMessage(message.Channel.Id, $"Looking for when the stream starts? The last update I know is: **{AppConfig.PinnedStreamMessage}** which was set <t:{epochSeconds}:R>");
                    _lastAskedMessageCount = 0;
                }
            }

            if (message.Content.ToLower().Contains("stream late"))
            {
                await DiscordHelper.SendMessage(message.Channel.Id, "you donut, go complain in the twitch chat");
            }
        }

        public static async Task HandleCustomCommand(SocketMessage message)
        {
            var isMod = DiscordHelper.IsUserMod(message.Author.Id);
            var commandName = message.Content.Split(" ")[0].ToLower();

            //Check if its actually a command
            if (!commandName.StartsWith('!') || message.Author.IsBot)
            {
                return;
            }

            if (message.Channel.Id != AppConfig.DiscordCommandsChannelID && !isMod)
            {
                return;
            }

            if (!CommandHandler.Commands.Contains(commandName))
            {
                return;
            }

            using(var context = new DatabaseContext())
            {
                var command = context.Commands.First(x => x.CommandName == commandName);


                if (command.CommandText.Contains("[user]") || command.CommandText.Contains("[count]"))
                {
                    return;
                }

                await DiscordHelper.SendMessage(message.Channel.Id, command.CommandText);
            }
        }

        //todo: remove this and replace it with a slash command
        public static async Task SendGiveawayMessage(SocketMessage message)
        {
            if (message.Channel.Id == AppConfig.DiscordGiveawayChannelID)
            {
                var builder = new ComponentBuilder()
                    .WithButton(new ButtonBuilder()
                    {
                        Style = ButtonStyle.Primary,
                        CustomId = message.Id.ToString(),
                        Emote = Emoji.Parse("👶"),
                        Label = "omg hypixel rank"
                    });

                var channel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordGiveawayChannelID) as IMessageChannel;
                await channel.SendMessageAsync("click the button to role the giveaway", components: builder.Build());
            }
        }
    }
}
