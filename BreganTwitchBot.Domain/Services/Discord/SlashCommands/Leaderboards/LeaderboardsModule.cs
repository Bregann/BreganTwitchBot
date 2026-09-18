using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.Leaderboards
{
    public class LeaderboardsModule(IDiscordLeaderboardsData discordLeaderboardsData) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("pointslb", "Get the top 24 users by points")]
        public async Task GetPointsLeaderboard()
        {
            await RespondWithLeaderboard(DiscordLeaderboardType.Points, "Points Leaderboard", value => $"{value:N0}");
        }

        [SlashCommand("hourslb", "Get the top 24 users by all time hours")]
        public async Task GetHoursLeaderboard()
        {
            await RespondWithLeaderboard(DiscordLeaderboardType.AllTimeHours, "Hours Leaderboard", FormatHours);
        }

        [SlashCommand("streamhourslb", "Get the top 24 users by hours watched this stream")]
        public async Task GetStreamHoursLeaderboard()
        {
            await RespondWithLeaderboard(DiscordLeaderboardType.StreamHours, "Stream Hours Leaderboard", FormatHours);
        }

        [SlashCommand("weeklyhourslb", "Get the top 24 users by hours watched this week")]
        public async Task GetWeeklyHoursLeaderboard()
        {
            await RespondWithLeaderboard(DiscordLeaderboardType.WeeklyHours, "Weekly Hours Leaderboard", FormatHours);
        }

        [SlashCommand("monthlyhourslb", "Get the top 24 users by hours watched this month")]
        public async Task GetMonthlyHoursLeaderboard()
        {
            await RespondWithLeaderboard(DiscordLeaderboardType.MonthlyHours, "Monthly Hours Leaderboard", FormatHours);
        }

        [SlashCommand("marbleslb", "Get the top 24 users by marbles wins")]
        public async Task GetMarblesLeaderboard()
        {
            await RespondWithLeaderboard(DiscordLeaderboardType.Marbles, "Marbles Wins Leaderboard", value => $"{value:N0}");
        }

        [SlashCommand("dailystreaklb", "Get the top 24 users by daily streak")]
        public async Task GetDailyStreakLeaderboard()
        {
            await RespondWithLeaderboard(DiscordLeaderboardType.DailyStreak, "Daily Streak Leaderboard", value => $"{value:N0}");
        }

        [SlashCommand("levellb", "Get the top 24 users by Discord level")]
        public async Task GetDiscordLevelLeaderboard()
        {
            await RespondWithLeaderboard(DiscordLeaderboardType.DiscordLevel, "Discord Level Leaderboard", value => $"Level {value:N0}");
        }

        [SlashCommand("xplb", "Get the top 24 users by Discord xp")]
        public async Task GetDiscordXpLeaderboard()
        {
            await RespondWithLeaderboard(DiscordLeaderboardType.DiscordXp, "Discord XP Leaderboard", value => $"{value:N0} xp");
        }

        private async Task RespondWithLeaderboard(DiscordLeaderboardType type, string title, Func<long, string> formatValue)
        {
            await DeferAsync();

            var entries = await discordLeaderboardsData.GetLeaderboardAsync(Context.Guild.Id, type);

            if (entries.Count == 0)
            {
                await FollowupAsync("There's nothing on that leaderboard yet!");
                return;
            }

            var embed = new EmbedBuilder
            {
                Title = title,
                Timestamp = DateTime.Now,
                Color = new Color(238, 255, 46)
            };

            foreach (var entry in entries)
            {
                embed.AddField($"#{entry.Position} - {entry.Username}", formatValue(entry.Value), true);
            }

            await FollowupAsync(embed: embed.Build());
        }

        private static string FormatHours(long minutes)
        {
            return $"{Math.Round(minutes / 60.0, 2):N2} hours";
        }
    }
}
