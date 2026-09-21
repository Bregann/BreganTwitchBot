using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.GeneralCommands
{
    public class DiscordWhoisData(AppDbContext context) : IDiscordWhoisData
    {
        public async Task<DiscordEmbedData> HandleWhoisCommand(WhoisCommand command)
        {
            var fields = new Dictionary<string, string>();

            var channel = await context.Channels.FirstOrDefaultAsync(x => x.ChannelConfig.DiscordGuildId == command.GuildId);

            if (channel == null)
            {
                return BuildEmbed("This server isn't linked to a channel", fields);
            }

            if (command.DiscordUserId == null && string.IsNullOrWhiteSpace(command.TwitchUsername))
            {
                return BuildEmbed("Give me either a Discord user or a Twitch username to look up", fields);
            }

            var user = command.DiscordUserId != null
                ? await context.ChannelUsers.FirstOrDefaultAsync(x => x.DiscordUserId == command.DiscordUserId)
                : await context.ChannelUsers.FirstOrDefaultAsync(x => x.TwitchUsername.ToLower() == command.TwitchUsername!.ToLower());

            if (user == null)
            {
                // the old bot answered an unknown twitch name with a random joke from a hardcoded
                // list, which read as a real answer. Say plainly that nothing is linked instead.
                var lookedUp = command.DiscordUserId != null ? $"<@{command.DiscordUserId}>" : command.TwitchUsername!.ToLower();
                fields.Add("Looked up", lookedUp);
                fields.Add("Result", "No linked account found");

                return BuildEmbed("Who is", fields);
            }

            fields.Add("Twitch name", user.TwitchUsername);
            fields.Add("Discord name", user.DiscordUserId == 0 ? "Not linked" : $"<@{user.DiscordUserId}>");
            fields.Add("First seen", $"{user.AddedOn:yyyy-MM-dd HH:mm} UTC");
            fields.Add("Last seen", $"{user.LastSeen:yyyy-MM-dd HH:mm} UTC");

            var stats = await context.ChannelUserStats.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);
            fields.Add("Total messages", stats?.TotalMessages.ToString("N0") ?? "0");

            var watchtime = await context.ChannelUserWatchtime.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

            if (watchtime != null)
            {
                fields.Add("Watchtime", $"{watchtime.MinutesInStream:N0} minutes (about {Math.Round(watchtime.MinutesInStream / 60.0, 1)} hours)");
            }

            var userData = await context.ChannelUserData.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

            if (userData != null)
            {
                fields.Add(channel.ChannelConfig.ChannelCurrencyName, userData.Points.ToString("N0"));
            }

            var discordStats = await context.DiscordUserStats.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

            if (discordStats != null)
            {
                fields.Add("Discord level", $"{discordStats.DiscordLevel} ({discordStats.DiscordXp:N0} xp)");
            }

            return BuildEmbed("Who is", fields);
        }

        private static DiscordEmbedData BuildEmbed(string title, Dictionary<string, string> fields)
        {
            return new DiscordEmbedData
            {
                Colour = new Color(238, 255, 46),
                Title = title,
                Description = "",
                Fields = fields
            };
        }
    }
}
