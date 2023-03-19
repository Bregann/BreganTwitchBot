using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Leaderboards;
using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Leaderboards.Enums;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Modules.Leaderboards
{
    public class LeaderboardsModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("pointslb", "Get top 24 user points")]
        public async Task GetPointsLb()
        {
            var dict = DiscordLeaderboards.GetLeaderboard(DiscordLeaderboardType.Points);
            var embed = new EmbedBuilder
            {
                Title = $"{AppConfig.PointsName} Leaderboard",
                Description = "Top 250 found at https://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{dict.Values.ElementAt(i):N0}", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("hourslb", "Get top 24 user hours")]
        public async Task GetHoursLb()
        {
            var dict = DiscordLeaderboards.GetHoursLeaderboard(DiscordLeaderboardType.AllTimeHours);
            var embed = new EmbedBuilder
            {
                Title = $"Hours Leaderboard",
                Description = "Top 250 found at https://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                var hours = (double)dict.Values.ElementAt(i) / 60;
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{Math.Round(hours, 2):N} hours", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("marbleslb", "Get top 24 marbles wins")]
        public async Task GetMarblesWinsLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetLeaderboard(DiscordLeaderboardType.Marbles);
            var embed = new EmbedBuilder
            {
                Title = $"Marbles Wins Leaderboard",
                Description = "Top 250 found at https://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{dict.Values.ElementAt(i):N0}", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("dailystreaklb", "Get top 24 twitch daily streaks")]
        public async Task GetStreakLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetLeaderboard(DiscordLeaderboardType.DailyStreak);
            var embed = new EmbedBuilder
            {
                Title = $"Current Daily Streak Leaderboard",
                Description = "Top 250 found at https://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{dict.Values.ElementAt(i):N0}", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("discordlevellb", "Get top 24 discord user levels")]
        public async Task GetDiscordLevelLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetLeaderboard(DiscordLeaderboardType.DiscordLevel);
            var embed = new EmbedBuilder
            {
                Title = $"Discord Level Leaderboard",
                Description = "Top 250 found at https://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{dict.Values.ElementAt(i):N0}", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("discordxplb", "Get top 24 discord user xp")]
        public async Task GetDiscordXPLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetLeaderboard(DiscordLeaderboardType.DiscordXp);
            var embed = new EmbedBuilder
            {
                Title = $"Discord XP Leaderboard",
                Description = "Top 250 found at https://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{dict.Values.ElementAt(i):N0}xp", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("streamhourslb", "Get top 24 current twitch stream hours")]
        public async Task GetStreamHoursLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetHoursLeaderboard(DiscordLeaderboardType.StreamHours);
            var embed = new EmbedBuilder
            {
                Title = $"Stream Hours Leaderboard",
                Description = "Top 250 found at https://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                var hours = (double)dict.Values.ElementAt(i) / 60;
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{Math.Round(hours, 2):N} hours", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("weeklyhourslb", "Get top 24 weekly twitch stream hours")]
        public async Task GetWeeklyHoursLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetHoursLeaderboard(DiscordLeaderboardType.WeeklyHours);
            var embed = new EmbedBuilder
            {
                Title = $"Weekly Stream Hours Leaderboard",
                Description = "Top 250 found at https://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                var hours = (double)dict.Values.ElementAt(i) / 60;
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{Math.Round(hours, 2):N} hours", true);
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("monthlyhourslb", "Get top 24 monthly twitch stream hours")]
        public async Task GetMonthlyHoursLb()
        {
            //await ctx.TriggerTypingAsync();
            var dict = DiscordLeaderboards.GetHoursLeaderboard(DiscordLeaderboardType.MonthlyHours);
            var embed = new EmbedBuilder
            {
                Title = $"Monthly Stream Hours Leaderboard",
                Description = "Top 250 found at https://bot.bregan.me"
            };

            for (int i = 0; i < dict.Count; i++)
            {
                var hours = (double)dict.Values.ElementAt(i) / 60;
                embed.AddField($"#{i + 1} - {dict.Keys.ElementAt(i)}", $"{Math.Round(hours, 2):N} hours", true);
            }

            await RespondAsync(embed: embed.Build());
        }
    }
}