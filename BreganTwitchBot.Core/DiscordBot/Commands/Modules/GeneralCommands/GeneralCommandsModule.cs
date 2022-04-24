using BreganTwitchBot.DiscordBot.Helpers;
using BreganTwitchBot.Services;
using Discord;
using Discord.Interactions;
using Humanizer;
using Humanizer.Localisation;
using RankBeggarAI;
using Serilog;
using System.Diagnostics;

namespace BreganTwitchBot.Core.DiscordBot.Commands.Modules.GeneralCommands
{
    public class GeneralCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("whois", "whoooooo are they (use either discord user or twitch name")]
        public async Task WhoIsUser([Summary("TwitchUsername", "Twitch username of user")] string twitchName = null, [Summary("DiscordUser", "@ the discord user")] IUser discordUser = null)
        {
            await DeferAsync();

            var response = await Task.Run(async () => await Whois.Whois.HandleWhoisCommand(Context, twitchName, discordUser));

            if (response == null)
            {
                await FollowupAsync("idk the response was null");
                return;
            }

            await FollowupAsync(embed: response.Build());
        }

        [SlashCommand("reboot", "(naughty command) restart the bot incase of issues")]
        public async Task RebootTheBot()
        {
            if (Context.Channel.Id != Config.DiscordEventChannelID)
            {
                await RespondAsync("The bot will now restart!", ephemeral: true);
                return;
            }

            await RespondAsync("Rebooting!");
            Log.Information("Bot will shutdown");
            await DiscordConnection.DiscordClient.LogoutAsync();
            await Task.Delay(10000);
            Environment.Exit(0);
        }

        [SlashCommand("subs", "Check the subcount of the streamer")]
        public async Task SubCount()
        {
            int subCount;
            await DeferAsync();

            try
            {
                var request = await TwitchApiConnection.ApiClient.Helix.Subscriptions.GetBroadcasterSubscriptionsAsync(Config.TwitchChannelID, accessToken: Config.BroadcasterOAuth);
                subCount = request.Total;
            }
            catch (Exception e)
            {
                await FollowupAsync("There has been an error with your request. Please try again");
                Log.Fatal($"[Discord Twitch Sub count] Error getting subcount: {e}");
                return;
            }

            await FollowupAsync($"{Config.BroadcasterName} has {subCount} subs!");
        }

        [SlashCommand("botuptime", "See how long the bot is up for")]
        public async Task BotUptime()
        {
            var botUptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            await RespondAsync($"The bot has been up for {botUptime.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}");
        }

        /*
        [Command("refresh"), ChannelCheck()]
        public async Task RefreshApi(CommandContext ctx)
        {
            if (ctx.User.Id == 196695995868774400 || ctx.User.Id == 219623957957967872)
            {
                try
                {
                    var refresh = await TwitchApiConnection.ApiClient.Auth.RefreshAuthTokenAsync(Config.BroadcasterRefresh, Config.TwitchApiSecret, Config.TwitchAPIOAuth);
                    TwitchApiConnection.ApiClient.Settings.AccessToken = refresh.AccessToken;
                    Config.BroadcasterRefresh = refresh.RefreshToken;
                    Config.BroadcasterOAuth = refresh.AccessToken;

                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["BroadcasterRefresh"].Value = refresh.RefreshToken;
                    config.AppSettings.Settings["BroadcasterOAuth"].Value = refresh.AccessToken;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");
                    Log.Information($"[Refresh Job] Token {refresh.AccessToken} successfully refreshed! Expires in: {refresh.ExpiresIn} | Refresh: {refresh.RefreshToken}");
                }
                catch (Exception e)
                {
                    Log.Fatal($"[Refresh Job] Error refreshing {e}");
                    return;
                }

                //reset pusub
                try
                {
                    PubSubConnection.PubSubClient.Disconnect();
                    PubSubConnection.PubSubClient.ListenToBitsEventsV2(Config.TwitchChannelID);
                    PubSubConnection.PubSubClient.ListenToFollows(Config.TwitchChannelID);
                    PubSubConnection.PubSubClient.ListenToSubscriptions(Config.TwitchChannelID);
                    PubSubConnection.PubSubClient.ListenToChannelPoints(Config.TwitchChannelID);
                    PubSubConnection.PubSubClient.Connect();
                }
                catch (AggregateException e)
                {
                    Log.Fatal($"[Refresh Job] Error disconnecting from pubsub {e}");
                    var pubSub = new PubSubConnection();
                    pubSub.Connect();
                }
            }
        }*/

        [SlashCommand("winner", "omg hypixel rank")]
        public async Task GIVEAWAY([Summary("messageid", "the message ID the bot gave you when starting the giveaway")] string msgId)
        {
            ulong.TryParse(msgId, out ulong convertedMsgId);
            await DeferAsync();

            var response = await Task.Run(async () => await Giveaway.Giveaway.HandleGiveawayCommand(Context, convertedMsgId));

            if (response == "lol")
            {
                await FollowupAsync($"And the winner is... <@{Context.User.Id}>");
                return;
            }

            await FollowupAsync(response);
        }

        [SlashCommand("mute", "mute that melvin")]
        public async Task MuteUser([Summary("discorduser", "the annoying melvin to mute")] IUser userMentioned)
        {
            var isMod = DiscordHelper.IsUserMod(Context.User.Id);

            if (!isMod)
            {
                await RespondAsync("congrats you have muted that user forever", ephemeral: true);
                return;
            }

            await DeferAsync();
            var guild = DiscordConnection.DiscordClient.GetGuild(Config.DiscordGuildID);
            var muteRole = guild.Roles.First(x => x.Name == "mute");
            var naughtyUserToMute = guild.GetUser(userMentioned.Id);

            await naughtyUserToMute.AddRoleAsync(muteRole);

            await FollowupAsync("That melvin has been muted bruv");
        }

        [SlashCommand("unmute", "unmute that melvin")]
        public async Task UnmuteUser([Summary("discorduser", "the probably still annoying melvin to unmute")] IUser userMentioned)
        {
            var isMod = DiscordHelper.IsUserMod(Context.User.Id);

            if (!isMod)
            {
                await RespondAsync("congrats you have unmuted that user forever", ephemeral: true);
                return;
            }

            await DeferAsync();
            var guild = DiscordConnection.DiscordClient.GetGuild(Config.DiscordGuildID);
            var muteRole = guild.Roles.First(x => x.Name == "mute");
            var naughtyUserToMute = guild.GetUser(userMentioned.Id);

            await naughtyUserToMute.RemoveRoleAsync(muteRole);

            await FollowupAsync("That melvin has been unmuted bruv");
        }

        [SlashCommand("socks", "put some beautiful blocks socks on your skin")]
        public async Task GetBlocksSocks([Summary("minecraftign", "Your Minecraft name")] string username)
        {
            await DeferAsync();

            try
            {
                var response = await BlocksSocks.BlocksSocks.GetSkinAndAddSocks(username, Context.User.Id);
                await FollowupAsync(embed: response.Build());
            }
            catch (Exception e)
            {
                await FollowupAsync("Oh no there has been an error  - please try again in a min");
                Log.Fatal($"[BlockSocks] Error with the !socks command - {e}");
                return;
            }
        }

        [SlashCommand("followers", "Check the followcount of the streamer 📉")]
        public async Task TwitchFollowers()
        {
            await DeferAsync();
            try
            {
                var followerCount = await TwitchApiConnection.ApiClient.Helix.Users.GetUsersFollowsAsync(toId: Config.TwitchChannelID);
                await FollowupAsync($"{Config.BroadcasterName} has {followerCount.TotalFollows:N0} followers");
            }
            catch (Exception error)
            {
                await FollowupAsync("lol it broke, try again");
                Log.Warning($"[Followers Command] {error}");
            }
        }

        [SlashCommand("aitest", "Test the rank begging AI (mod only)")]
        public async Task AIPredict([Summary("message", "message to AI test")] string message)
        {
            var user = DiscordConnection.DiscordClient.GetGuild(Config.DiscordGuildID).GetUser(Context.User.Id);
            var isMod = user.Roles.FirstOrDefault(x => x.Name == "Twitch Mot");

            if (isMod == null)
            {
                await RespondAsync("beep boop error you're probably a rank begger anyway lol", ephemeral: true);
                return;
            }

            RankBeggar.ModelInput sampleData = new RankBeggar.ModelInput()
            {
                SentimentText = message.Replace("'", ""),
            };

            // Make a single prediction on the sample data and print results
            var predictionResult = RankBeggar.Predict(sampleData);
            await RespondAsync($"Message: {message} \n Prediction result: {predictionResult.Prediction} \n 0: {predictionResult.Score[0]}% \n 1: {predictionResult.Score[1]}%");
        }

        /*
        [SlashCommand("christmas", "Give yourself some Christmas emojis - enter either present, tree, santa, deer, firework or all")]
        public async Task ChristmasName(InteractionContext ctx, [Option("emojiType", "Select from: present, tree, santa, deer, firework or all (may not work)")] string type)
        {
            var guild = await DiscordConnection.DiscordClient.GetGuildAsync(Config.DiscordGuildID);
            var user = await guild.GetMemberAsync(ctx.User.Id);
            string nickNameToSet = "";
            string emoji = "";

            switch (type.ToLower())
            {
                case "present":
                    emoji = "🎁";
                    break;
                case "tree":
                    emoji = "🎄";
                    break;
                case "santa":
                    emoji = "🎅";
                    break;
                case "deer":
                    emoji = "🦌";
                    break;
                case "firework":
                    emoji = "🎇";
                    break;
                case "all":
                    emoji = "🎁🎄🎅🦌🎇";
                    break;
                default:
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Not a valid option! Select either snowman, tree, santa, snowflake or firework!"));
                    return;
            }

            if (user.Nickname != null)
            {
                nickNameToSet = emoji + user.Nickname.Replace("🎃", "").Replace("👻", "") + emoji;
            }
            else
            {
                nickNameToSet = emoji + user.Username + emoji;
            }

            await user.ModifyAsync(user => user.Nickname = nickNameToSet);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Your nickname has been set!"));

        }*/

        /*
        [Command("poo"), ChannelCheck()]
        public async Task TwitchDying(CommandContext ctx)
        {
            if (ctx.Channel.Id != 475398031861219328)
            {
                return;
            }

            //await ctx.TriggerTypingAsync();
            if (ctx.User.Id == 196695995868774400 || ctx.User.Id == 219623957957967872)
            {
                if (Daily.TwitchBroken)
                {
                    Daily.TwitchBroken = false;
                }
                else
                {
                    Daily.TwitchBroken = true;
                }

                await DiscordConnection.SendMessage(ctx.Channel.Id, $"Twitch broken thing been set to {Daily.TwitchBroken}");
            }
        }*/
    }
}
