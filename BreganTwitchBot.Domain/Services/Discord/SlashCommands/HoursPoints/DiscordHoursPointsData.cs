using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.HoursPoints
{
    public class DiscordHoursPointsData(AppDbContext context, IDiscordHelperService discordHelperService) : IDiscordHoursPointsData
    {
        public async Task<DiscordEmbedData> HandleHoursCommandAsync(HoursPointsCommand command)
        {
            var (channel, user, error) = await ResolveUser(command);

            if (error != null)
            {
                return BuildEmbed("Hours check", error, []);
            }

            var watchtime = await context.ChannelUserWatchtime.FirstOrDefaultAsync(x => x.ChannelUserId == user!.Id && x.ChannelId == channel!.Id);
            var minutes = watchtime?.MinutesInStream ?? 0;

            return BuildEmbed($"Hours check - {user!.TwitchUsername}", "", new Dictionary<string, string>
            {
                { "Time watched", $"{minutes:N0} minutes (about {Math.Round(minutes / 60.0, 2):N2} hours)" }
            });
        }

        public async Task<DiscordEmbedData> HandlePointsCommandAsync(HoursPointsCommand command)
        {
            var (channel, user, error) = await ResolveUser(command);

            if (error != null)
            {
                return BuildEmbed("Points check", error, []);
            }

            var pointsName = channel!.ChannelConfig.ChannelCurrencyName;
            var userData = await context.ChannelUserData.FirstOrDefaultAsync(x => x.ChannelUserId == user!.Id && x.ChannelId == channel.Id);
            var points = userData?.Points ?? 0;

            return BuildEmbed($"{pointsName} check - {user!.TwitchUsername}", "", new Dictionary<string, string>
            {
                { pointsName, $"{points:N0}" }
            });
        }

        public async Task<string> HandlePrestigeCommandAsync(HoursPointsCommand command)
        {
            var channel = await GetChannelForGuild(command.GuildId);

            if (channel == null)
            {
                return "This server isn't linked to a channel";
            }

            var user = await context.ChannelUsers.FirstOrDefaultAsync(x => x.DiscordUserId == command.CallerDiscordUserId);

            if (user == null)
            {
                return "You need to link your Twitch account first!";
            }

            var pointsName = channel.ChannelConfig.ChannelCurrencyName;
            var prestigeCost = channel.ChannelConfig.CurrencyPointCap;
            var userData = await context.ChannelUserData.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

            if (userData == null || userData.Points < prestigeCost)
            {
                return $"You don't have enough {pointsName} to prestige! You need {prestigeCost:N0} {pointsName}";
            }

            var discordStats = await context.DiscordUserStats.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == channel.Id);

            if (discordStats == null)
            {
                return "You don't have any Discord stats yet, try chatting first!";
            }

            userData.Points -= prestigeCost;
            discordStats.PrestigeLevel++;
            await context.SaveChangesAsync();

            Log.Information($"[Discord Prestige] {user.TwitchUsername} prestiged to level {discordStats.PrestigeLevel}");

            await discordHelperService.AddDiscordXpToUser(command.GuildId, command.ChannelId, command.CallerDiscordUserId, 7500);

            return $"You have prestiged! You are now prestige level {discordStats.PrestigeLevel}";
        }

        /// <summary>
        /// Works out which user the command is asking about - a named twitch user, a named discord
        /// user, or the caller. Returns the reason instead when they can't be looked up.
        /// </summary>
        private async Task<(Channel? Channel, ChannelUser? User, string? Error)> ResolveUser(HoursPointsCommand command)
        {
            var channel = await GetChannelForGuild(command.GuildId);

            if (channel == null)
            {
                return (null, null, "This server isn't linked to a channel");
            }

            ChannelUser? user;

            if (!string.IsNullOrWhiteSpace(command.TwitchUsername))
            {
                user = await context.ChannelUsers.FirstOrDefaultAsync(x => x.TwitchUsername.ToLower() == command.TwitchUsername.ToLower());

                if (user == null)
                {
                    return (channel, null, $"I don't know anybody called {command.TwitchUsername.ToLower()}");
                }
            }
            else
            {
                var discordUserId = command.DiscordUserId ?? command.CallerDiscordUserId;
                user = await context.ChannelUsers.FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId);

                if (user == null)
                {
                    return (channel, null, command.DiscordUserId == null
                        ? "You need to link your Twitch account first!"
                        : "That user hasn't linked their Twitch account");
                }
            }

            return (channel, user, null);
        }

        private async Task<Channel?> GetChannelForGuild(ulong guildId)
        {
            return await context.Channels.FirstOrDefaultAsync(x => x.ChannelConfig.DiscordGuildId == guildId);
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
