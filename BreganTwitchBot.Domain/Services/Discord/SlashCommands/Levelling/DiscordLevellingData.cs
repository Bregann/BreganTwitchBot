using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.Services.Helpers;
using Discord;
using BreganTwitchBot.Domain.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.Levelling
{
    //TODO: WRITE TESTS FOR THIS class
    public class DiscordLevellingData(AppDbContext context) : IDiscordLevellingData
    {
        public async Task<string> HandleToggleLevelUpCommand(DiscordCommand command)
        {
            var channel = await context.GetRequiredChannelForGuild(command.GuildId);
            var user = await context.DiscordUserStats.FirstAsync(x => x.User.DiscordUserId == command.UserId && x.ChannelId == channel.Id);

            // change level ups toggle and return the enabled or disabled message
            user.DiscordLevelUpNotifsEnabled = !user.DiscordLevelUpNotifsEnabled;
            await context.SaveChangesAsync();

            return user.DiscordLevelUpNotifsEnabled ? "Level up notifications have been enabled!" : "Level up notifications have been disabled!";
        }

        public async Task<DiscordEmbedData> HandleLevelCommand(ulong guildId, ulong discordUserId, string displayName)
        {
            var channel = await context.Channels.FirstOrDefaultAsync(x => x.ChannelConfig.DiscordGuildId == guildId);

            if (channel == null)
            {
                return BuildEmbed("Discord level", "This server isn't linked to a channel", []);
            }

            var user = await context.DiscordUserStats
                .FirstOrDefaultAsync(x => x.User.DiscordUserId == discordUserId && x.ChannelId == channel.Id);

            if (user == null)
            {
                // the old bot answered this with "dunno don't think it exists lol"
                return BuildEmbed("Discord level", $"{displayName} hasn't earned any xp in this server yet", []);
            }

            var nextLevelXp = DiscordLevelHelper.GetXpNeededForNextLevel(user.DiscordLevel);
            var progress = DiscordLevelHelper.GetProgressPercentage(user.DiscordLevel, user.DiscordXp);

            var fields = new Dictionary<string, string>
            {
                { "Level", $"{user.DiscordLevel:N0}" },
                { "XP", $"{user.DiscordXp:N0}" },
                { "Next level at", $"{nextLevelXp:N0} xp" },
                { "Progress", $"{progress:N1}%" }
            };

            if (user.PrestigeLevel > 0)
            {
                fields.Add("Prestige", $"{user.PrestigeLevel:N0}");
            }

            return BuildEmbed($"Discord level - {displayName}", "", fields);
        }

        private static DiscordEmbedData BuildEmbed(string title, string description, Dictionary<string, string> fields)
        {
            return new DiscordEmbedData
            {
                Colour = new Color(238, 255, 46),
                Title = title,
                Description = description,
                Fields = fields
            };
        }
    }
}
