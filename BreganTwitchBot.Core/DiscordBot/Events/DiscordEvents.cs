using BreganTwitchBot.Core.DiscordBot.Commands.Modules.GeneralCommands.Giveaway;
using BreganTwitchBot.Core.DiscordBot.Commands.Modules.Levelling;
using BreganTwitchBot.Core.Twitch.Services;
using BreganTwitchBot.Core.Twitch.Services.Stats.Enums;
using BreganTwitchBot.Data;
using BreganTwitchBot.Data.Models;
using BreganTwitchBot.DiscordBot.Helpers;
using BreganTwitchBot.Services;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using Serilog;
using System.Text.RegularExpressions;

namespace BreganTwitchBot.DiscordBot.Events
{
    public class DiscordEvents
    {
        public static Dictionary<ulong, DateTime> XpCooldownDict = new Dictionary<ulong, DateTime>();
        private static List<string> _urlDomains;
        private static List<string> _permBanWords;

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
            using (var context = new DatabaseContext())
            {
                _permBanWords = context.Blacklist.Where(x => x.WordType == "word").Select(x => x.Word).ToList();
            }

            _urlDomains = new List<string> { ".xxx", ".tv", ".travel", ".tel", ".ru", ".org", ".net", ".name", ".museum", ".mobi", ".me", ".ly", ".jobs", ".int", ".info", ".gov", ".gg", ".coop", ".xyz", ".com", ".co.uk", ".cat", ".biz", "asia", ".aero", ".gift" };
        }

        private static async Task ButtonPressed(SocketMessageComponent arg)
        {
            await arg.DeferAsync();
            bool addedOrRemoved = true;
            string roleName = "";

            if (arg.Channel.Id == Config.DiscordReactionRoleChannelID)
            {
                switch (arg.Data.CustomId)
                {
                    case "marbolsRole":
                        addedOrRemoved = await AddOrRemoveRole("Marbles On Stream", arg.User.Id);
                        roleName = "Marbles On Stream";
                        break;
                    case "weebRole":
                        addedOrRemoved = await AddOrRemoveRole("Weeb", arg.User.Id);
                        roleName = "Weeb";
                        break;
                    case "petsRole":
                        addedOrRemoved = await AddOrRemoveRole("Pets", arg.User.Id);
                        roleName = "Pets";
                        break;
                    case "programmerRole":
                        addedOrRemoved = await AddOrRemoveRole("Programmer", arg.User.Id);
                        roleName = "Programmer";
                        break;
                    case "susRole":
                        addedOrRemoved = await AddOrRemoveRole("Among Us", arg.User.Id);
                        roleName = "Among Us (sus)";
                        break;
                    case "freeGamesRole":
                        addedOrRemoved = await AddOrRemoveRole("Free Games", arg.User.Id);
                        roleName = "Free Games";
                        break;
                    case "politcsRole":
                        addedOrRemoved = await AddOrRemoveRole("Politics", arg.User.Id);
                        roleName = "Politics";
                        break;
                    case "photographyRole":
                        addedOrRemoved = await AddOrRemoveRole("Photography", arg.User.Id);
                        roleName = "Photography";
                        break;
                    case "botUpdatesRole":
                        addedOrRemoved = await AddOrRemoveRole("Bot Updates", arg.User.Id);
                        roleName = "Bot Updates";
                        break;
                    case "horrorGameRole":
                        addedOrRemoved = await AddOrRemoveRole("Horror Game Pings", arg.User.Id);
                        roleName = "Horror Game Pings";
                        break;
                    case "otherGamesRole":
                        addedOrRemoved = await AddOrRemoveRole("Other Games Pings", arg.User.Id);
                        roleName = "Other Games Pings";
                        break;
                    default:
                        break;
                }

                if (addedOrRemoved == true)
                {
                    await arg.FollowupAsync($"Your role **{roleName}** has been added! pooooooooo", ephemeral: true);
                }
                else
                {
                    await arg.FollowupAsync($"Your role **{roleName}** has been remove!", ephemeral: true);
                }
            }

            if (arg.Channel.Id == Config.DiscordGiveawayChannelID)
            {
                var winner = await Giveaway.HandleGiveawayButton(arg.User.Id, ulong.Parse(arg.Data.CustomId));

                if (winner == "lol")
                {
                    await arg.FollowupAsync($"You silly goose obviously you can't roll the giveaway winner! what were you expecting? A message saying you won the rank? I would never say such a thing", ephemeral: true);
                    Log.Information($"[Discord Giveaway] Jebaited user - {arg.User.Username}");
                }
                else
                {
                    await arg.FollowupAsync(winner);
                    Log.Information($"[Discord Giveaway] Real user rolled");
                }
            }
        }

        private static async Task<bool> AddOrRemoveRole(string roleName, ulong id)
        {
            var guild = DiscordConnection.DiscordClient.GetGuild(Config.DiscordGuildID);
            var roleToCheck = guild.Roles.First(x => x.Name == roleName);

            var user = guild.GetUser(id);

            if (user.Roles.Contains(roleToCheck))
            {
                await user.RemoveRoleAsync(roleToCheck);
                Log.Information($"[Discord Roles] {roleName} has been removed from {id} ({user.Nickname} - {user.Username})");
                return false;
            }
            else
            {
                await user.AddRoleAsync(roleToCheck);
                Log.Information($"[Discord Roles] {roleName} has been added to {id} ({user.Nickname} - {user.Username})");
                return true;
            }
        }

        private static async Task MessageDeleted(Cacheable<IMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2)
        {
            await arg1.GetOrDownloadAsync();
            if (arg1.Value.Content == null || arg1.Value.Author.Id == DiscordConnection.DiscordClient.CurrentUser.Id)
            {
                return;
            }

            if (arg1.Value.Content.Replace(Environment.NewLine, "").Replace(" ", "") == "****" || arg1.Value.Content.Replace(Environment.NewLine, "").Replace(" ", "") == "__")
            {
                return;
            }

            if (arg1.Value.Attachments.Count > 0)
            {
                foreach (var attachment in arg1.Value.Attachments)
                {
                    await DiscordHelper.SendMessage(Config.DiscordEventChannelID, $"Message Deleted: {arg1.Value.Content.Replace("@everyone", "").Replace("@here", "")} \nImage Name: {attachment.Filename} \nSent By: {arg1.Value.Author} \nIn Channel: {arg2.Value.Name} \nDeleted at: {DateTime.Now} \n Link: {attachment.Url}");
                    Log.Information($"[Discord Message Deleted] Message Deleted: {arg1.Value.Content.Replace("@everyone", "").Replace("@here", "")} \nImage Name: {attachment.Filename} \nSent By: {arg1.Value.Author} \nIn Channel: {arg2.Value.Name} \nDeleted at: {DateTime.Now} \n Link: {attachment.Url}");
                    return;
                }
            }

            var messageEmbed = new EmbedBuilder()
            {
                Title = "Deleted message",
                Timestamp = DateTime.Now,
                Color = new Color(250, 53, 27)
            };

            messageEmbed.AddField("Message Deleted", arg1.Value.Content.Replace("@everyone", "").Replace("@here", ""));
            messageEmbed.AddField("Sent By", arg1.Value.Author.Username);
            messageEmbed.AddField("In Channel", arg1.Value.Channel.Name);
            messageEmbed.AddField("Deleted At", DateTime.Now.ToString());

            var channel = await DiscordConnection.DiscordClient.GetChannelAsync(Config.DiscordEventChannelID) as IMessageChannel;
            await channel.TriggerTypingAsync();
            await channel.SendMessageAsync(embed: messageEmbed.Build());
        }

        private static Task UserIsTyping(Cacheable<IUser, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2)
        {
            Log.Information($"[Discord Typing] {arg1.Value.Username} is typing a message in {arg2.Value.Name}");
            return Task.CompletedTask;
        }

        private static async Task MessageReceived(global::Discord.WebSocket.SocketMessage arg)
        {
            Log.Information($"[Discord Message Received] Username: {arg.Author.Username} Message: {arg.Content} Channel: {arg.Channel.Name}");

            var removedMessage = arg.Content.Replace(Environment.NewLine, "").Replace(" ", "");
            if (removedMessage == "****" || removedMessage == "__")
            {
                await arg.DeleteAsync();
                return;
            }

            if (arg.Author.IsBot)
            {
                return;
            }

            if (arg.Channel.Id == 713365310408884236)
            {
                await arg.DeleteAsync();
            }

            if (arg.Content.ToLower().Contains("nitro") || arg.Content.ToLower().Contains(".gift"))
            {
                Log.Information("[Discord Scam Check] Potential scam");

                if (arg.Content.ToLower().Contains("free") || arg.Content.ToLower().Contains("gift") || arg.Content.ToLower().Contains("nitro") || arg.Content.ToLower().Contains(".gift"))
                {
                    if (arg.Content.ToLower().Contains("discord.com") || arg.Content.ToLower().Contains("discord.gg"))
                    {
                        Log.Information("[Discord Scam Check] Not a scam");
                    }
                    else
                    {
                        if (_urlDomains.Any(arg.Content.ToLower().Contains))
                        {
                            Log.Information("[Discord Scam Check] probably a scam");
                            var guild = DiscordConnection.DiscordClient.GetGuild(Config.DiscordGuildID);
                            var user = guild.GetUser(arg.Author.Id);
                            var muteRole = guild.Roles.First(x => x.Name == "mute");

                            await user.AddRoleAsync(muteRole);
                            await DiscordHelper.SendMessage(arg.Channel.Id, "<@&860607265827192882> potential scam link");
                            return;
                        }
                    }
                }
            }

            var message = Regex.Replace(arg.Content, @"[^0-9a-zA-Z⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊⠋⠌⠍⠎⠏⠐⠑⠒⠓⠔⠕⠖⠗⠘⠙⠚⠛⠜⠝⠞⠟⠠⠡⠢⠣⠤⠥⠦⠧⠨⠩⠪⠫⠬⠭⠮⠯⠰⠱⠲⠳⠴⠵⠶⠷⠸⠹⠺⠻⠼⠽⠾⠿⡀⡁⡂⡃⡄⡅⡆⡇⡈⡉⡊⡋⡌⡍⡎⡏⡐⡑⡒⡓⡔⡕⡖⡗⡘⡙⡚⡛⡜⡝⡞⡟⡠⡡⡢⡣⡤⡥⡦⡧⡨⡩⡪⡫⡬⡭⡮⡯⡰⡱⡲⡳⡴⡵⡶⡷⡸⡹⡺⡻⡼⡽⡾⡿⢀⢁⢂⢃⢄⢅⢆⢇⢈⢉⢊⢋⢌⢍⢎⢏⢐⢑⢒⢓⢔⢕⢖⢗⢘⢙⢚⢛⢜⢝⢞⢟⢠⢡⢢⢣⢤⢥⢦⢧⢨⢩⢪⢫⢬⢭⢮⢯⢰⢱⢲⢳⢴⢵⢶⢷⢸⢹⢺⢻⢼⢽⢾⢿⣀⣁⣂⣃⣄⣅⣆⣇⣈⣉⣊⣋⣌⣍⣎⣏⣐⣑⣒⣓⣔⣕⣖⣗⣘⣙⣚⣛⣜⣝⣞⣟⣠⣡⣢⣣⣤⣥⣦⣧⣨⣩⣪⣫⣬⣭⣮⣯⣰⣱⣲⣳⣴⣵⣶⣷⣸⣹⣺⣻⣼⣽⣾⣿░█▄▌▀─\p{L}]+", "").ToLower();

            //Racist children
            if (_permBanWords.Any(message.Contains))
            {
                var guild = DiscordConnection.DiscordClient.GetGuild(Config.DiscordGuildID);
                var user = guild.GetUser(arg.Author.Id);
                var muteRole = guild.Roles.First(x => x.Name == "mute");

                await user.AddRoleAsync(muteRole);
                await DiscordHelper.SendMessage(arg.Channel.Id, "<@&860607265827192882> potential racist edgelord child");
                return;
            }

            if (arg.Attachments.Count > 0)
            {
                var attachments = arg.Attachments.ToList();
                Log.Information($"[Discord Message Received] Username: {arg.Author} Image URL: {attachments[0].Url} Channel: {arg.Channel.Name}");
                await Task.Run(async () => await DiscordLevelling.AddDiscordXp(arg.Author.Id, 10, arg.Channel.Id));
                return;
            }

            if (arg.Content.ToLower().StartsWith("stream starts") && arg.Author.Id == Config.DiscordGuildOwner)
            {
                var msg = arg as IUserMessage;
                await msg.PinAsync();
            }

            if (arg.Channel.Id == Config.DiscordGiveawayChannelID && !arg.Content.ToLower().StartsWith("!winner"))
            {
                var builder = new ComponentBuilder()
                    .WithButton(new ButtonBuilder()
                    {
                        Style = ButtonStyle.Primary,
                        CustomId = arg.Id.ToString(),
                        Emote = Emoji.Parse("👶"),
                        Label = "omg hypixel rank"
                    });

                var channel = await DiscordConnection.DiscordClient.GetChannelAsync(Config.DiscordGiveawayChannelID) as IMessageChannel;
                await channel.SendMessageAsync("click the button to role the giveaway", components: builder.Build());
            }

            if (arg.Content.ToLower().Contains("streaming today") || arg.Content.ToLower().Contains("stream today") || arg.Content.ToLower().Contains("when stream"))
            {
                if (arg.Author.Id == 201689212628500481) //bloody bobobu
                {
                    return;
                }

                if (arg.Author.Id == 219623957957967872)
                {
                    await DiscordHelper.SendMessage(arg.Channel.Id, "https://i.imgur.com/pEjmRjb.png");
                    return;
                }
                if (DateTime.Now.DayOfWeek != DayOfWeek.Sunday)
                {
                    await DiscordHelper.SendMessage(arg.Channel.Id, "Looking for when the stream starts? Probably not today as he barely streams (ignore this if it's castles week then check <#354732299180441610>). Check <#754372306222186626> for his days off");
                }
                else
                {
                    await DiscordHelper.SendMessage(arg.Channel.Id, "Looking for when the stream starts? Check the pins in the top right or go to <#754372306222186626> or <#354732299180441610>");
                }

            }

            if (arg.Content.ToLower().Contains("stream late"))
            {
                await DiscordHelper.SendMessage(arg.Channel.Id, "you donut, go complain in the twitch chat");
            }

            if (arg.Content.StartsWith("!"))
            {
                return;
            }

            //limit it so it needs to be a proper sentence
            if (arg.Content.Length >= 3)
            {
                //Check if they are not on cooldown for the xp gains - 5s to prevent spam
                if (!XpCooldownDict.ContainsKey(arg.Author.Id))
                {
                    XpCooldownDict.Add(arg.Author.Id, DateTime.Now);
                    await Task.Run(async () => await DiscordLevelling.AddDiscordXp(arg.Author.Id, 5, arg.Channel.Id));
                    return;
                }

                if (DateTime.Now - TimeSpan.FromSeconds(2) <= XpCooldownDict[arg.Author.Id])
                {
                    return;
                }

                await Task.Run(async () => await DiscordLevelling.AddDiscordXp(arg.Author.Id, 5, arg.Channel.Id));
                XpCooldownDict[arg.Author.Id] = DateTime.Now;
                return;
            }
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
            StreamStatsService.UpdateStreamStat(1, StatTypes.AmountOfDiscordUsersJoined);

            var messageEmbed = new EmbedBuilder()
            {
                Title = "User Joined",
                Timestamp = DateTime.Now,
                Color = new Color(3, 207, 252)
            };

            messageEmbed.AddField("Discord Username", arg.Username);

            var channel = await DiscordConnection.DiscordClient.GetChannelAsync(Config.DiscordEventChannelID) as IMessageChannel;
            await channel.SendMessageAsync(embed: messageEmbed.Build());

            using (var how2LinkGif = new FileStream($"Skins/how2link.gif", FileMode.Open))
            {
                var channel2 = await DiscordConnection.DiscordClient.GetChannelAsync(Config.DiscordLinkingChannelID) as IMessageChannel;
                await channel2.SendFileAsync(how2LinkGif, "how2link.gif", $"Welcome {arg.Mention}! \n\n To access the Discord Server, you must link your Twitch account with Discord by using the ``/link`` command!");
            };

            Log.Information($"[Discord] User joined: {arg.Username}");
        }

        private static async Task UserLeft(SocketGuild arg1, SocketUser arg2)
        {
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

            var channel = await DiscordConnection.DiscordClient.GetChannelAsync(Config.DiscordEventChannelID) as IMessageChannel;
            await channel.SendMessageAsync(embed: messageEmbed.Build());

            Log.Information($"[Discord] User Left: {arg2.Username}");
        }

        private static async Task UserVoiceStateUpdated(global::Discord.WebSocket.SocketUser arg1, global::Discord.WebSocket.SocketVoiceState arg2, global::Discord.WebSocket.SocketVoiceState arg3)
        {
            var guild = DiscordConnection.DiscordClient.GetGuild(Config.DiscordGuildID);
            var user = guild.GetUser(arg1.Id);
            var vcRole = guild.Roles.First(x => x.Name == "VC");

            if (arg2.VoiceChannel == null)
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
