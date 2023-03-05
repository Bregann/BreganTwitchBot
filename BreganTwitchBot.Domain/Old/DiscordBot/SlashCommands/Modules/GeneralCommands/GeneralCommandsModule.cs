using BreganTwitchBot.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Bot.DiscordBot.SlashCommands.Data.GeneralCommands.BlocksSocks;
using BreganTwitchBot.Domain.Bot.DiscordBot.SlashCommands.Data.GeneralCommands.Giveaway;
using BreganTwitchBot.Domain.Bot.DiscordBot.SlashCommands.Data.GeneralCommands.Whois;
using BreganTwitchBot.Domain.Data.TwitchBot;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Infrastructure.Database.Models;
using BreganTwitchBot.Services;
using Discord;
using Discord.Interactions;
using Humanizer;
using Humanizer.Localisation;
using Serilog;
using System.Diagnostics;

namespace BreganTwitchBot.Domain.Bot.DiscordBot.SlashCommands.Modules.GeneralCommands
{
    public class GeneralCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("whois", "whoooooo are they (use either discord user or twitch name")]
        public async Task WhoIsUser([Summary("TwitchUsername", "Twitch username of user")] string twitchName = null, [Summary("DiscordUser", "@ the discord user")] IUser discordUser = null)
        {
            await DeferAsync();

            var response = await Task.Run(async () => await WhoisData.HandleWhoisCommand(Context, twitchName, discordUser));

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
            if (Context.Channel.Id != AppConfig.DiscordEventChannelID)
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
                var request = await TwitchApiConnection.ApiClient.Helix.Subscriptions.GetBroadcasterSubscriptionsAsync(AppConfig.TwitchChannelID, accessToken: AppConfig.BroadcasterOAuth);
                subCount = request.Total;
            }
            catch (Exception e)
            {
                await FollowupAsync("There has been an error with your request. Please try again");
                Log.Fatal($"[Discord Twitch Sub count] Error getting subcount: {e}");
                return;
            }

            await FollowupAsync($"{AppConfig.BroadcasterName} has {subCount} subs!");
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

            var response = await Task.Run(async () => await GiveawayData.HandleGiveawayCommand(Context, convertedMsgId));

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
            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
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
            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
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
                var response = await BlocksSocksData.GetSkinAndAddSocks(username, Context.User.Id);
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
                var followerCount = await TwitchApiConnection.ApiClient.Helix.Users.GetUsersFollowsAsync(toId: AppConfig.TwitchChannelID);
                await FollowupAsync($"{AppConfig.BroadcasterName} has {followerCount.TotalFollows:N0} followers");
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
            var user = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID).GetUser(Context.User.Id);
            var isMod = user.Roles.FirstOrDefault(x => x.Name == "Twitch Mot");

            if (isMod == null)
            {
                await RespondAsync("beep boop error you're probably a rank begger anyway lol", ephemeral: true);
                return;
            }

            var sampleData = new BreganTwitchBot_Domain.RankBeggar.ModelInput()
            {
                Message = message.Replace("'", ""),
            };

            // Make a single prediction on the sample data and print results
            var predictionResult = BreganTwitchBot_Domain.RankBeggar.Predict(sampleData);
            await RespondAsync($"Message: {message} \n Prediction result: {predictionResult.AiResult} \n 0: {predictionResult.Score[0]}% \n 1: {predictionResult.Score[1]}%");
        }

        [SlashCommand("birthday", "Set your birthday to get a Happy Birthday announcement")]
        public async Task Birthday([Summary("day", "the day you were born (number)")] int day, [Summary("month", "the month you were born (number)")] int month)
        {
            //Check if user has already set their birthday
            using (var context = new DatabaseContext())
            {
                var user = context.Birthdays.Where(x => x.DiscordId == Context.User.Id).FirstOrDefault();

                if (user != null)
                {
                    await RespondAsync("You silly sausage you already have a birthday set!");
                    return;
                }
            }

            //Check that it's a proper day and month
            switch (month)
            {
                //31 days
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    if (day > 31 && day <= 0)
                    {
                        await RespondAsync("Oh deary me that month only has 31 days lol back to school you go! :)");
                        return;
                    }
                    break;

                //29 days
                case 2:
                    if (day > 29 && day <= 0)
                    {
                        await RespondAsync("Oh deary me that month only has 28 days (or 29 if you want to be awkward)");
                        return;
                    }
                    break;

                //30 days
                case 4:
                case 6:
                case 9:
                case 11:
                    if (day > 30 && day <= 0)
                    {
                        await RespondAsync("Oh deary me that month only has 30 days lol back to school you go! :)");
                        return;
                    }
                    break;
                //silly people that put over 12 as their year
                default:
                    await RespondAsync(".... silly you - I'm not even going to tell you what you've done here");
                    return;
            }

            //Insert to db
            using (var context = new DatabaseContext())
            {
                context.Birthdays.Add(new Birthdays
                {
                    DiscordId = Context.User.Id,
                    Day = day,
                    Month = month
                });

                context.SaveChanges();
            }

            //say its successful
            await RespondAsync("Your birthday has been set!");
        }

        [SlashCommand("christmas", "get yourself some Christmas emojis for your name")]
        public async Task ChristmasName()
        {

            var builder = new ComponentBuilder()
                .WithButton(" ", "christmas-snowman", ButtonStyle.Danger, new Emoji("⛄"))
                .WithButton(" ", "christmas-gift", ButtonStyle.Success, new Emoji("🎁"))
                .WithButton(" ", "christmas-tree", ButtonStyle.Danger, new Emoji("🎄"))
                .WithButton(" ", "christmas-santa", ButtonStyle.Success, new Emoji("🎅"))
                .WithButton(" ", "christmas-mrsanta", ButtonStyle.Danger, new Emoji("🤶"))
                .WithButton(" ", "christmas-star", ButtonStyle.Success, new Emoji("🌟"))
                .WithButton(" ", "christmas-socks", ButtonStyle.Danger, new Emoji("🧦"))
                .WithButton(" ", "christmas-bell", ButtonStyle.Success, new Emoji("🔔"))
                .WithButton(" ", "christmas-deer", ButtonStyle.Danger, new Emoji("🦌"))
                .WithButton(" ", "christmas-resetusername", ButtonStyle.Success, new Emoji("♻️"));

            await RespondAsync("Click the buttons to add a christmas emoji onto your name!", components: builder.Build());
        }

        [SlashCommand("spook", "Give yourself a pumpkin AND spooky ghost")]
        public async Task SpookyName()
        {
            if (DateTime.UtcNow.Month != 10)
            {
                await RespondAsync("You silly pumpkin it's not October!");
            }

            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
            var user = guild.GetUser(Context.User.Id);
            string nickNameToSet = "";

            if (user.Nickname != null)
            {
                nickNameToSet = "🎃" + user.Nickname + "👻";
            }
            else
            {
                nickNameToSet = "🎃" + user.Username + "👻";
            }

            await user.ModifyAsync(user => user.Nickname = nickNameToSet);
            await RespondAsync("Your nickname has been set!");
        }
    }
}