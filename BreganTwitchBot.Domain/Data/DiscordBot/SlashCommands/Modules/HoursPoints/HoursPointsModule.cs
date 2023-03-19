using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.HoursPoints;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Modules.HoursPoints
{
    public class HoursPointsModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("hours", "Check your or another users stream hours")]
        public async Task GetUserHours([Summary("TwitchUsername", "Twitch username of user")] string twitchName = null, [Summary("DiscordUser", "the discord user")] IUser discordUser = null)
        {
            await DeferAsync();
            var response = await Task.Run(async () => await HoursPointsData.HandleHoursCommand(Context, twitchName, discordUser));

            if (response == null)
            {
                await FollowupAsync("broked");
                return;
            }

            await FollowupAsync(embed: response.Build());
        }

        [SlashCommand("points", "Check your or another users points")]
        public async Task GetUserPoints([Summary("TwitchUsername", "Twitch username of user")] string twitchName = null, [Summary("DiscordUser", "the discord user")] IUser discordUser = null)
        {
            await DeferAsync();
            var response = await Task.Run(async () => await HoursPointsData.HandlePointsCommand(Context, twitchName, discordUser));

            if (response == null)
            {
                await FollowupAsync("broked");
                return;
            }

            await FollowupAsync(embed: response.Build());
        }

        [SlashCommand("prestige", "When hit max points convert all your points into a prestige level")]
        public async Task Prestige()
        {
            var response = await Task.Run(async () => await HoursPointsData.HandlePrestigeCommand(Context));
            await RespondAsync(response);
        }
    }
}