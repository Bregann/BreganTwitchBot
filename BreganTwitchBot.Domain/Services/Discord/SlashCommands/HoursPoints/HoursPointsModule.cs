using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.HoursPoints
{
    public class HoursPointsModule(IDiscordHoursPointsData discordHoursPointsData) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("hours", "Check your or another user's stream hours")]
        public async Task GetUserHours(
            [Summary("twitchusername", "Twitch username of the user")] string? twitchUsername = null,
            [Summary("discorduser", "The Discord user")] IUser? discordUser = null)
        {
            await DeferAsync();

            var embedData = await discordHoursPointsData.HandleHoursCommand(BuildCommand(twitchUsername, discordUser));
            await FollowupAsync(embed: BuildEmbed(embedData).Build());
        }

        [SlashCommand("points", "Check your or another user's points")]
        public async Task GetUserPoints(
            [Summary("twitchusername", "Twitch username of the user")] string? twitchUsername = null,
            [Summary("discorduser", "The Discord user")] IUser? discordUser = null)
        {
            await DeferAsync();

            var embedData = await discordHoursPointsData.HandlePointsCommand(BuildCommand(twitchUsername, discordUser));
            await FollowupAsync(embed: BuildEmbed(embedData).Build());
        }

        [SlashCommand("prestige", "Spend your points to gain a prestige level")]
        public async Task Prestige()
        {
            await DeferAsync();

            var response = await discordHoursPointsData.HandlePrestigeCommand(BuildCommand(null, null));
            await FollowupAsync($"{Context.User.Mention} => {response}");
        }

        private HoursPointsCommand BuildCommand(string? twitchUsername, IUser? discordUser)
        {
            return new HoursPointsCommand
            {
                GuildId = Context.Guild.Id,
                ChannelId = Context.Channel.Id,
                CallerDiscordUserId = Context.User.Id,
                DiscordUserId = discordUser?.Id,
                TwitchUsername = twitchUsername
            };
        }

        private static EmbedBuilder BuildEmbed(DiscordEmbedData embedData)
        {
            var embed = new EmbedBuilder
            {
                Timestamp = DateTime.Now,
                Color = embedData.Colour,
                Title = embedData.Title,
                Description = embedData.Description
            };

            foreach (var field in embedData.Fields)
            {
                embed.AddField(field.Key, field.Value);
            }

            return embed;
        }
    }
}
