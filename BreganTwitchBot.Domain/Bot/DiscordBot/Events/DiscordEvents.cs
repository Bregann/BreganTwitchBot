using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Bot.DiscordBot.SlashCommands.Data.GeneralCommands.Giveaway;
using BreganTwitchBot.Domain.Bot.DiscordBot.SlashCommands.Data.Levelling;
using BreganTwitchBot.Infrastructure.Database.Models;
using BreganTwitchBot.Services;
using Discord;
using Discord.WebSocket;
using Hangfire;
using Humanizer;
using Humanizer.Localisation;
using Serilog;
using System.Text.RegularExpressions;
using BreganTwitchBot.Domain.Bot.Twitch.Services.Stats;
using BreganTwitchBot.Domain.Bot.Twitch.Services.Stats.Enums;

namespace BreganTwitchBot.DiscordBot.Events
{
    public class DiscordEvents
    {
        public static Dictionary<ulong, DateTime> XpCooldownDict = new Dictionary<ulong, DateTime>();
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

            using (var context = new DatabaseContext())
            {
                _permBanWords = context.Blacklist.Where(x => x.WordType == "word").Select(x => x.Word).ToList();
            }

            _urlDomains = new List<string> { ".xxx", ".tv", ".travel", ".tel", ".ru", ".org", ".net", ".name", ".museum", ".mobi", ".me", ".ly", ".jobs", ".int", ".info", ".gov", ".gg", ".coop", ".xyz", ".com", ".co.uk", ".cat", ".biz", "asia", ".aero", ".gift" };
        }

        private static Task LogError(LogMessage arg)
        {
            Log.Information(arg.Message);
            return Task.CompletedTask;
        }

        private static async Task PresenceUpdated(SocketUser user, SocketPresence previous, SocketPresence newUserUpdate)
        {
            try
            {
                //Don't do anything if the update is nothing
                if (newUserUpdate.Activities.Count == 0)
                {
                    return;
                }

                var previousStatusData = previous.Activities?.Where(x => x.Type == ActivityType.CustomStatus).FirstOrDefault() as CustomStatusGame;
                var newStatusData = newUserUpdate.Activities.Where(x => x.Type == ActivityType.CustomStatus).FirstOrDefault() as CustomStatusGame;

                if (user.Id == AppConfig.DiscordGuildOwner)
                {
                    //Check if the new status data is the same as the known current one
                    if (newStatusData != null)
                    {
                        //Update it if it's different or if it's a different day to the pinned message, it might be the same as yesterdays
                        if (newStatusData.State != AppConfig.PinnedStreamMessage || AppConfig.PinnedMessageDate.Day != DateTime.UtcNow.Day)
                        {
                            //Make sure its about streaming
                            if (newStatusData.State.ToLower().Contains("stream") || newStatusData.State.ToLower().Contains("live"))
                            {
                                //unpin the old message
                                var channel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordGeneralChannel) as IMessageChannel;

                                if (AppConfig.PinnedStreamMessageId != 0)
                                {
                                    var message = await channel.GetMessageAsync(AppConfig.PinnedStreamMessageId) as IUserMessage;
                                    await message.UnpinAsync();
                                }

                                //Send the new message and pin it
                                var newMessage = await channel.SendMessageAsync($"New {AppConfig.BroadcasterName} stream update!!!! Update is: {newStatusData.State}");
                                await newMessage.PinAsync();
                                AppConfig.UpdatePinnedMessageAndMessageId(newStatusData.State, newMessage.Id);
                            }
                        }
                    }
                }

                //If the statuses match then don't process them
                if (previousStatusData?.State == newStatusData?.State)
                {
                    return;
                }

                //We might as well log the statuses for the memes
                Log.Information($"[User Status] Discord status for user {user.Username} changed from {previousStatusData?.State ?? "null"} to {newStatusData?.State ?? "null"} - userId {user.Id}");
            }
            catch (Exception e)
            {
                Log.Fatal($"[User Status] {e}- {e.InnerException}");
                return;
            }
        }

        private static async Task ButtonPressed(SocketMessageComponent arg)
        {
            await arg.DeferAsync();
            bool addedOrRemoved = true;
            string roleName = "";

            if (arg.Channel.Id == AppConfig.DiscordCommandsChannelID)
            {
                var emojiToAdd = "";

                switch (arg.Data.CustomId)
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
                        break;
                }

                var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
                var user = guild.GetUser(arg.User.Id);

                if (emojiToAdd == "")
                {
                    await user.ModifyAsync(user => user.Nickname = null);
                    await arg.FollowupAsync("Your nickname has been cleared!", ephemeral: true);
                    return;
                }
                else
                {
                    string nickNameToSet = "";

                    if (user.Nickname != null)
                    {
                        nickNameToSet = emojiToAdd + user.Nickname + emojiToAdd;
                    }
                    else
                    {
                        nickNameToSet = emojiToAdd + user.Username + emojiToAdd;
                    }

                    await user.ModifyAsync(user => user.Nickname = nickNameToSet);
                    await arg.FollowupAsync("Your nickname has been set!", ephemeral: true);
                    return;
                }
            }

            if (arg.Channel.Id == AppConfig.DiscordReactionRoleChannelID)
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

            if (arg.Channel.Id == AppConfig.DiscordGiveawayChannelID)
            {
                var winner = await GiveawayData.HandleGiveawayButton(arg.User.Id, ulong.Parse(arg.Data.CustomId));

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
            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
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
                    await DiscordHelper.SendMessage(AppConfig.DiscordEventChannelID, $"Message Deleted: {arg1.Value.Content.Replace("@everyone", "").Replace("@here", "")} \nImage Name: {attachment.Filename} \nSent By: {arg1.Value.Author} \nIn Channel: {arg2.Value.Name} \nDeleted at: {DateTime.Now} \n Link: {attachment.Url}");
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

            var channel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordEventChannelID) as IMessageChannel;
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

            _lastAskedMessageCount++;

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
                            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
                            var user = guild.GetUser(arg.Author.Id);
                            var muteRole = guild.Roles.First(x => x.Name == "mute");

                            await user.AddRoleAsync(muteRole);
                            await DiscordHelper.SendMessage(arg.Channel.Id, $"<@&{AppConfig.DiscordBanRole}> potential scam link");
                            return;
                        }
                    }
                }
            }

            var message = Regex.Replace(arg.Content, @"[^0-9a-zA-Z⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊⠋⠌⠍⠎⠏⠐⠑⠒⠓⠔⠕⠖⠗⠘⠙⠚⠛⠜⠝⠞⠟⠠⠡⠢⠣⠤⠥⠦⠧⠨⠩⠪⠫⠬⠭⠮⠯⠰⠱⠲⠳⠴⠵⠶⠷⠸⠹⠺⠻⠼⠽⠾⠿⡀⡁⡂⡃⡄⡅⡆⡇⡈⡉⡊⡋⡌⡍⡎⡏⡐⡑⡒⡓⡔⡕⡖⡗⡘⡙⡚⡛⡜⡝⡞⡟⡠⡡⡢⡣⡤⡥⡦⡧⡨⡩⡪⡫⡬⡭⡮⡯⡰⡱⡲⡳⡴⡵⡶⡷⡸⡹⡺⡻⡼⡽⡾⡿⢀⢁⢂⢃⢄⢅⢆⢇⢈⢉⢊⢋⢌⢍⢎⢏⢐⢑⢒⢓⢔⢕⢖⢗⢘⢙⢚⢛⢜⢝⢞⢟⢠⢡⢢⢣⢤⢥⢦⢧⢨⢩⢪⢫⢬⢭⢮⢯⢰⢱⢲⢳⢴⢵⢶⢷⢸⢹⢺⢻⢼⢽⢾⢿⣀⣁⣂⣃⣄⣅⣆⣇⣈⣉⣊⣋⣌⣍⣎⣏⣐⣑⣒⣓⣔⣕⣖⣗⣘⣙⣚⣛⣜⣝⣞⣟⣠⣡⣢⣣⣤⣥⣦⣧⣨⣩⣪⣫⣬⣭⣮⣯⣰⣱⣲⣳⣴⣵⣶⣷⣸⣹⣺⣻⣼⣽⣾⣿░█▄▌▀─\p{L}]+", "").ToLower();

            //Racist children
            if (_permBanWords.Any(message.Contains))
            {
                var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
                var user = guild.GetUser(arg.Author.Id);
                var muteRole = guild.Roles.First(x => x.Name == "mute");

                await user.AddRoleAsync(muteRole);
                await DiscordHelper.SendMessage(arg.Channel.Id, $"<@&{AppConfig.DiscordBanRole}> potential racist edgelord child");
                return;
            }

            if (arg.Attachments.Count > 0)
            {
                var attachments = arg.Attachments.ToList();
                Log.Information($"[Discord Message Received] Username: {arg.Author} Image URL: {attachments[0].Url} Channel: {arg.Channel.Name}");
                BackgroundJob.Enqueue(() => DiscordLevelling.AddDiscordXp(arg.Author.Id, 10, arg.Channel.Id));
                return;
            }

            if (arg.Content.ToLower().StartsWith("stream starts") && arg.Author.Id == AppConfig.DiscordGuildOwner)
            {
                var msg = arg as IUserMessage;
                await msg.PinAsync();
            }

            if (arg.Channel.Id == AppConfig.DiscordGiveawayChannelID && !arg.Content.ToLower().StartsWith("!winner"))
            {
                var builder = new ComponentBuilder()
                    .WithButton(new ButtonBuilder()
                    {
                        Style = ButtonStyle.Primary,
                        CustomId = arg.Id.ToString(),
                        Emote = Emoji.Parse("👶"),
                        Label = "omg hypixel rank"
                    });

                var channel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordGiveawayChannelID) as IMessageChannel;
                await channel.SendMessageAsync("click the button to role the giveaway", components: builder.Build());
            }

            if (arg.Content.ToLower().Contains("streaming today") || arg.Content.ToLower().Contains("stream today") || arg.Content.ToLower().Contains("when stream"))
            {
                if (_lastAskedMessageCount <= 5)
                {
                    return;
                }

                if (arg.Author.Id == 219623957957967872)
                {
                    await DiscordHelper.SendMessage(arg.Channel.Id, "https://i.imgur.com/pEjmRjb.png");
                    return;
                }

                var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var diff = AppConfig.PinnedMessageDate - origin;
                var epochSeconds = Math.Floor(diff.TotalSeconds);

                if (DateTime.UtcNow.DayOfWeek != DayOfWeek.Sunday)
                {
                    if (AppConfig.PinnedMessageDate.Date != DateTime.UtcNow.Date)
                    {
                        await DiscordHelper.SendMessage(arg.Channel.Id, "Looking for when the stream starts? Probably not today (ignore this if it's castles week then check <#354732299180441610>). Check <#754372306222186626> for his days off");
                        _lastAskedMessageCount = 0;
                        return;
                    }

                    await DiscordHelper.SendMessage(arg.Channel.Id, $"Looking for when the stream starts? The last update I know is: **{AppConfig.PinnedStreamMessage}** which was set <t:{epochSeconds}:R>");
                    _lastAskedMessageCount = 0;
                }
                else
                {
                    if (AppConfig.PinnedMessageDate.Date != DateTime.UtcNow.Date)
                    {
                        await DiscordHelper.SendMessage(arg.Channel.Id, "Looking for when the stream starts? Check the pins in the top right or go to <#754372306222186626> or <#354732299180441610>");
                        _lastAskedMessageCount = 0;
                        return;
                    }

                    await DiscordHelper.SendMessage(arg.Channel.Id, $"Looking for when the stream starts? The last update I know is: **{AppConfig.PinnedStreamMessage}** which was set <t:{epochSeconds}:R>");
                    _lastAskedMessageCount = 0;
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
                    XpCooldownDict.Add(arg.Author.Id, DateTime.UtcNow);
                    BackgroundJob.Enqueue(() => DiscordLevelling.AddDiscordXp(arg.Author.Id, 5, arg.Channel.Id));
                    return;
                }

                if (DateTime.UtcNow - TimeSpan.FromSeconds(2) <= XpCooldownDict[arg.Author.Id])
                {
                    return;
                }

                BackgroundJob.Enqueue(() => DiscordLevelling.AddDiscordXp(arg.Author.Id, 5, arg.Channel.Id));
                XpCooldownDict[arg.Author.Id] = DateTime.UtcNow;
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

            using (var how2LinkGif = new FileStream($"Data/Skins/how2link.gif", FileMode.Open))
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