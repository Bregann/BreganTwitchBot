﻿using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.GeneralCommands.BlocksSocks;
using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.GeneralCommands.Whois;
using BreganTwitchBot.Domain.Data.TwitchBot;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Infrastructure.Database.Models;
using Discord;
using Discord.Interactions;
using Humanizer;
using Humanizer.Localisation;
using Serilog;
using System.Diagnostics;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Modules.GeneralCommands
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

        [SlashCommand("firework", "Give yourself some fireworks to celebrate fireworks night!")]
        public async Task FireworksName()
        {
            if (DateTime.UtcNow.Month != 11)
            {
                await RespondAsync("You silly firework it's not November!");
            }

            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
            var user = guild.GetUser(Context.User.Id);
            string nickNameToSet = "";

            if (user.Nickname != null)
            {
                nickNameToSet = "🎆" + user.Nickname + "🎇";
            }
            else
            {
                nickNameToSet = "🎆" + user.Username + "🎇";
            }

            await user.ModifyAsync(user => user.Nickname = nickNameToSet);
            await RespondAsync("Your nickname has been set!");
        }

        [SlashCommand("roles", "Give you all the buttons of the roles available")]
        public async Task RoleSelection()
        {
            var builder = new ComponentBuilder()
                .WithButton("Marbles On Stream", "marbolsRole", ButtonStyle.Primary, new Emoji("⚪"))
                .WithButton("Pets", "petsRole", ButtonStyle.Primary, new Emoji("🐱"))
                .WithButton("Programmer", "programmerRole", ButtonStyle.Primary, new Emoji("🖥️"))
                .WithButton("Free Games Ping", "freeGamesRole", ButtonStyle.Primary, new Emoji("🎮"))
                .WithButton("Photography", "photographyRole", ButtonStyle.Primary, new Emoji("📷"))
                .WithButton("Bot Updates", "botUpdatesRole", ButtonStyle.Primary, new Emoji("🤖"))
                .WithButton("Horror Game Ping", "horrorGameRole", ButtonStyle.Primary, new Emoji("😨"))
                .WithButton("Other Games Ping", "otherGamesRole", ButtonStyle.Primary, new Emoji("❓"))
                .WithButton("Food Enjoyer", "foodEnjoyerRole", ButtonStyle.Primary, new Emoji("🤮"));

            await RespondAsync("Click the buttons to add a christmas emoji onto your name!", components: builder.Build());
        }

        [SlashCommand("unspook", "remove your spooky name")]
        public async Task UnspookName()
        {
            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
            var user = guild.GetUser(Context.User.Id);
            string nickNameToSet = "";

            if (user.Nickname != null)
            {
                nickNameToSet = user.Nickname.Replace("🎃", "").Replace("👻", "");
                await user.ModifyAsync(user => user.Nickname = nickNameToSet);
            }

            await RespondAsync("You have been unspooked!");
        }

        [SlashCommand("unfirework", "remove your fireworks name")]
        public async Task UnfireworkName()
        {
            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
            var user = guild.GetUser(Context.User.Id);
            string nickNameToSet = "";

            if (user.Nickname != null)
            {
                nickNameToSet = user.Nickname.Replace("🎆", "").Replace("🎇", "");
                await user.ModifyAsync(user => user.Nickname = nickNameToSet);
            }

            await RespondAsync("You have been unfireworked!");
        }
    }
}