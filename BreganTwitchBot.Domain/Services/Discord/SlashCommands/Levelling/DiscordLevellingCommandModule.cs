using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.Levelling
{
    public class LevellingCommandModule(IDiscordLevellingData discordLevellingData) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("togglelevelups", "Disable or enable level ups")]
        public async Task ToggleLevelUpNotifs()
        {
            var command = new DiscordCommand
            {
                UserId = Context.User.Id,
                GuildId = Context.Guild.Id,
                ChannelId = Context.Channel.Id
            };

            var response = await discordLevellingData.HandleToggleLevelUpCommand(command);
            await RespondAsync(response);
        }
        [SlashCommand("level", "Check your or another user's Discord level")]
        public async Task GetLevel([Summary("discorduser", "The Discord user")] IUser? discordUser = null)
        {
            await DeferAsync();

            var target = discordUser ?? Context.User;
            var embedData = await discordLevellingData.HandleLevelCommand(Context.Guild.Id, target.Id, target.Username);

            var embed = new EmbedBuilder
            {
                Timestamp = DateTime.Now,
                Color = embedData.Colour,
                Title = embedData.Title,
                Description = embedData.Description
            };

            foreach (var field in embedData.Fields)
            {
                embed.AddField(field.Key, field.Value, true);
            }

            await FollowupAsync(embed: embed.Build());
        }

    }
}
