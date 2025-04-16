using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
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
    }
}