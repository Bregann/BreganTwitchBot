using BreganTwitchBot.Core.DiscordBot.Commands.Modules.Leaderboards;
using BreganTwitchBot.Core.DiscordBot.Commands.Modules.Leaderboards.Enums;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Core.Twitch.Commands.Modules.Leaderboards
{
    public class LeaderboardsModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("pointslb", "Get top 25 user points")]
        public async Task GetPointsLb()
        {
            var dict = DiscordLeaderboards.GetLeaderboard(DiscordLeaderboardType.Points);
            var embed = new EmbedBuilder
            {
                Title = $"{Config.PointsName} Leaderboard",
                Description = "Top 250 found at http://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{dict.Values.ElementAt(i):N0}", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("hourslb", "Get top 25 user hours")]
        public async Task GetHoursLb()
        {
            var dict = DiscordLeaderboards.GetHoursLeaderboard(DiscordLeaderboardType.AllTimeHours);
            var embed = new EmbedBuilder
            {
                Title = $"Hours Leaderboard",
                Description = "Top 250 found at http://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                var hours = ((double)dict.Values.ElementAt(i)) / 60;
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{Math.Round(hours, 2):N} hours", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("marbleslb", "Get top 25 marbles wins")]
        public async Task GetMarblesWinsLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetLeaderboard(DiscordLeaderboardType.Marbles);
            var embed = new EmbedBuilder
            {
                Title = $"Marbles Wins Leaderboard",
                Description = "Top 250 found at http://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{dict.Values.ElementAt(i):N0}", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("dailystreaklb", "Get top 25 twitch daily streaks")]
        public async Task GetStreakLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetLeaderboard(DiscordLeaderboardType.DailyStreak);
            var embed = new EmbedBuilder
            {
                Title = $"Current Daily Streak Leaderboard",
                Description = "Top 250 found at http://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{dict.Values.ElementAt(i):N0}", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("discordlevellb", "Get top 25 discord user levels")]
        public async Task GetDiscordLevelLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetLeaderboard(DiscordLeaderboardType.DiscordLevel);
            var embed = new EmbedBuilder
            {
                Title = $"Discord Level Leaderboard",
                Description = "Top 250 found at http://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{dict.Values.ElementAt(i):N0}", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("discordxplb", "Get top 25 discord user xp")]
        public async Task GetDiscordXPLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetLeaderboard(DiscordLeaderboardType.DiscordXp);
            var embed = new EmbedBuilder
            {
                Title = $"Discord XP Leaderboard",
                Description = "Top 250 found at http://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{dict.Values.ElementAt(i):N0}xp", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("streamhourslb", "Get top 25 current twitch stream hours")]
        public async Task GetStreamHoursLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetHoursLeaderboard(DiscordLeaderboardType.StreamHours);
            var embed = new EmbedBuilder
            {
                Title = $"Stream Hours Leaderboard",
                Description = "Top 250 found at http://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                var hours = ((double)dict.Values.ElementAt(i)) / 60;
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{Math.Round(hours, 2):N} hours", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("weeklyhourslb", "Get top 25 weekly twitch stream hours")]
        public async Task GetWeeklyHoursLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetHoursLeaderboard(DiscordLeaderboardType.WeeklyHours);
            var embed = new EmbedBuilder
            {
                Title = $"Weekly Stream Hours Leaderboard",
                Description = "Top 250 found at http://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                var hours = ((double)dict.Values.ElementAt(i)) / 60;
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{Math.Round(hours, 2):N} hours", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("monthlyhourslb", "Get top 25 monthly twitch stream hours")]
        public async Task GetMonthlyHoursLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetHoursLeaderboard(DiscordLeaderboardType.MonthlyHours);
            var embed = new EmbedBuilder
            {
                Title = $"Monthly Stream Hours Leaderboard",
                Description = "Top 250 found at http://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                var hours = ((double)dict.Values.ElementAt(i)) / 60;
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{Math.Round(hours, 2):N} hours", true);
            }

            await RespondAsync(embed: embed.Build());
        }
    }
}
