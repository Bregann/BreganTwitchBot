using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.Giveaway
{
    public class DiscordGiveawayData(AppDbContext context) : IDiscordGiveawayData
    {
        public async Task<(string? GiveawayId, string Response)> StartGiveaway(ulong guildId, ulong channelId, ulong startedByUserId, int minimumWatchtimeHours, string? requiredRankName)
        {
            var channel = await GetChannelForGuild(guildId);

            if (channel == null)
            {
                return (null, "This server isn't linked to a channel");
            }

            if (channel.ChannelConfig.DiscordGiveawayChannelId != channelId)
            {
                return (null, "Your giveaway has been unstarted! (wrong channel)");
            }

            if (minimumWatchtimeHours < 0)
            {
                return (null, "The minimum watchtime can't be negative");
            }

            ChannelRank? requiredRank = null;

            if (!string.IsNullOrWhiteSpace(requiredRankName))
            {
                requiredRank = await context.ChannelRanks.FirstOrDefaultAsync(x => x.ChannelId == channel.Id && x.RankName.ToLower() == requiredRankName.ToLower());

                if (requiredRank == null)
                {
                    return (null, $"There isn't a rank called {requiredRankName} in this channel");
                }
            }

            var giveawayId = Guid.NewGuid().ToString("N");

            await context.DiscordGiveaways.AddAsync(new DiscordGiveaway
            {
                ChannelId = channel.Id,
                GiveawayId = giveawayId,
                StartedByDiscordUserId = startedByUserId,
                StartedAt = DateTime.UtcNow,
                Active = true,
                MinimumWatchtimeMinutes = minimumWatchtimeHours * 60,
                RequiredRankId = requiredRank?.Id
            });

            await context.SaveChangesAsync();

            var requirements = BuildRequirementsText(minimumWatchtimeHours, requiredRank?.RankName);
            return (giveawayId, $"A new giveaway has started! Click the button to enter.{requirements}");
        }

        public async Task<(string Response, bool Ephemeral)> EnterGiveaway(ulong guildId, ulong userId, string giveawayId)
        {
            var giveaway = await GetGiveaway(giveawayId);

            if (giveaway == null || !giveaway.Active)
            {
                return ("That giveaway has finished!", true);
            }

            if (await context.DiscordGiveawayEntries.AnyAsync(x => x.DiscordGiveawayId == giveaway.Id && x.DiscordUserId == userId))
            {
                return ("don't be silly, you have already entered the giveaway!", true);
            }

            var user = await context.ChannelUsers
                .FirstOrDefaultAsync(x => x.DiscordUserId == userId);

            if (user == null)
            {
                return ("You need to link your Twitch account before you can enter!", true);
            }

            var watchtime = await context.ChannelUserWatchtime.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == giveaway.ChannelId);
            var minutesWatched = watchtime?.MinutesInStream ?? 0;

            // the old bot silently entered users below the watchtime bar with entries that could
            // never win. The bar is now explicit per giveaway, and users below it are told so.
            if (minutesWatched < giveaway.MinimumWatchtimeMinutes)
            {
                var hoursNeeded = giveaway.MinimumWatchtimeMinutes / 60.0;
                var hoursHad = minutesWatched / 60.0;
                return ($"You need **{hoursNeeded:N0} hours** of watchtime to enter this giveaway. You currently have **{hoursHad:N1} hours**", true);
            }

            var ranksAchieved = await context.ChannelUserRankProgress
                .Where(x => x.ChannelUserId == user.Id && x.ChannelId == giveaway.ChannelId)
                .ToListAsync();

            if (giveaway.RequiredRankId != null && !ranksAchieved.Any(x => x.ChannelRankId == giveaway.RequiredRankId))
            {
                return ($"You need the **{giveaway.RequiredRank?.RankName}** rank to enter this giveaway", true);
            }

            var config = await GetOrCreateConfig(giveaway.ChannelId);
            var discordStats = await context.DiscordUserStats.FirstOrDefaultAsync(x => x.ChannelUserId == user.Id && x.ChannelId == giveaway.ChannelId);

            var watchtimeEntries = config.MinutesPerEntry > 0 ? minutesWatched / config.MinutesPerEntry : 0;
            var rankEntries = config.RanksGrantEntries ? ranksAchieved.Count : 0;
            var xpEntries = 0;

            if (config.XpPerEntry > 0 && discordStats != null)
            {
                xpEntries = (int)Math.Min(discordStats.DiscordXp / config.XpPerEntry, config.MaxXpEntries);
            }

            // everyone who meets the requirements gets at least one entry
            var totalEntries = Math.Max(1, watchtimeEntries + rankEntries + xpEntries);

            await context.DiscordGiveawayEntries.AddAsync(new DiscordGiveawayEntry
            {
                DiscordGiveawayId = giveaway.Id,
                DiscordUserId = userId,
                Entries = totalEntries,
                EnteredAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            Log.Information($"[Discord Giveaways] User {userId} entered giveaway {giveawayId} with {totalEntries} entries");

            return ($"You have entered the giveaway! You have **{totalEntries}** entries " +
                    $"({watchtimeEntries} from watchtime, {rankEntries} from ranks, {xpEntries} from Discord xp). " +
                    "You can earn more by watching the stream and levelling up in the Discord", true);
        }

        public async Task<(string Response, bool Ephemeral)> CheckEntries(ulong guildId, ulong userId, string giveawayId)
        {
            var giveaway = await GetGiveaway(giveawayId);

            if (giveaway == null)
            {
                return ("I can't find that giveaway!", true);
            }

            // whoever started it sees the totals instead of their own entries
            if (userId == giveaway.StartedByDiscordUserId)
            {
                var peopleEntered = await context.DiscordGiveawayEntries.CountAsync(x => x.DiscordGiveawayId == giveaway.Id);
                var totalEntries = await context.DiscordGiveawayEntries.Where(x => x.DiscordGiveawayId == giveaway.Id).SumAsync(x => x.Entries);

                return ($"There are currently **{peopleEntered}** people in the giveaway with a total of **{totalEntries}** entries", true);
            }

            var entry = await context.DiscordGiveawayEntries.FirstOrDefaultAsync(x => x.DiscordGiveawayId == giveaway.Id && x.DiscordUserId == userId);

            return entry == null
                ? ("You haven't entered the giveaway yet! Try clicking the other button", true)
                : ($"You have entered the giveaway! You have **{entry.Entries}** entries in the giveaway", true);
        }

        public async Task<(string Response, bool Ephemeral)> DrawWinner(ulong guildId, ulong userId, string giveawayId)
        {
            var giveaway = await GetGiveaway(giveawayId);

            if (giveaway == null)
            {
                return ("I can't find that giveaway!", true);
            }

            if (userId != giveaway.StartedByDiscordUserId)
            {
                return ("Only whoever started the giveaway can draw the winner!", true);
            }

            var entries = await context.DiscordGiveawayEntries
                .Where(x => x.DiscordGiveawayId == giveaway.Id)
                .ToListAsync();

            if (entries.Count == 0)
            {
                return ("Nobody entered the giveaway :(", true);
            }

            // weight the draw by entry count without materialising a row per entry
            var totalEntries = entries.Sum(x => x.Entries);
            var winningTicket = Random.Shared.Next(0, totalEntries);
            var runningTotal = 0;
            var winnerId = entries[^1].DiscordUserId;

            foreach (var entry in entries)
            {
                runningTotal += entry.Entries;

                if (winningTicket < runningTotal)
                {
                    winnerId = entry.DiscordUserId;
                    break;
                }
            }

            giveaway.Active = false;
            giveaway.WinnerDiscordUserId = winnerId;
            await context.SaveChangesAsync();

            Log.Information($"[Discord Giveaways] Giveaway {giveawayId} won by {winnerId} out of {totalEntries} entries");

            return ($"The winner of the giveaway is... <@{winnerId}>", false);
        }

        private async Task<Channel?> GetChannelForGuild(ulong guildId)
        {
            return await context.Channels.FirstOrDefaultAsync(x => x.ChannelConfig.DiscordGuildId == guildId);
        }

        private async Task<DiscordGiveaway?> GetGiveaway(string giveawayId)
        {
            return await context.DiscordGiveaways.FirstOrDefaultAsync(x => x.GiveawayId == giveawayId);
        }

        /// <summary>
        /// Gets the channel's entry weighting, creating it with the values the old bot hardcoded
        /// the first time a channel runs a giveaway
        /// </summary>
        private async Task<DiscordGiveawayConfig> GetOrCreateConfig(int channelId)
        {
            var config = await context.DiscordGiveawayConfigs.FirstOrDefaultAsync(x => x.ChannelId == channelId);

            if (config != null)
            {
                return config;
            }

            config = new DiscordGiveawayConfig
            {
                ChannelId = channelId,
                MinutesPerEntry = 3600,
                XpPerEntry = 1000,
                MaxXpEntries = 30,
                RanksGrantEntries = true
            };

            await context.DiscordGiveawayConfigs.AddAsync(config);
            await context.SaveChangesAsync();

            return config;
        }

        private static string BuildRequirementsText(int minimumWatchtimeHours, string? requiredRankName)
        {
            var requirements = new List<string>();

            if (minimumWatchtimeHours > 0)
            {
                requirements.Add($"**{minimumWatchtimeHours} hours** of watchtime");
            }

            if (!string.IsNullOrWhiteSpace(requiredRankName))
            {
                requirements.Add($"the **{requiredRankName}** rank");
            }

            return requirements.Count == 0 ? "" : $" You need {string.Join(" and ", requirements)} to enter.";
        }
    }
}
