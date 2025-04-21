using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.Daily
{
    public class DiscordDailyPointsData(AppDbContext context, IDiscordHelperService discordHelperService) : IDiscordDailyPointsData
    {
        //TODO: WRITE TESTS FOR METHOD
        public async Task<DiscordEmbedData> HandleDiscordDailyPointsCommand(DiscordCommand command)
        {
            var channel = await context.Channels.FirstAsync(x => x.ChannelConfig.DiscordGuildId == command.GuildId);
            var user = await context.DiscordDailyPoints
                .FirstAsync(x => x.ChannelId == channel.Id && x.User.DiscordUserId == command.UserId);

            // Check if the user has already claimed their daily points
            if (user.DiscordDailyClaimed)
            {
                return new DiscordEmbedData
                {
                    Title = "Daily Points",
                    Description = "Don't be a silly goose, you have already claimed your points today",
                    Colour = new Color(255, 0, 0),
                    Fields = new Dictionary<string, string>
                    {
                        { "Claimed", user.DiscordDailyClaimed.ToString() }
                    }
                };
            }

            var totalBonusesEarned = (int)Math.Floor((decimal)user.DiscordDailyTotalClaims / 5) + 1;

            var bonusPoints = user.DiscordDailyStreak < 4 ? user.DiscordDailyStreak * 100 : PointsToGiveToUser();
            var pointsToGive = (totalBonusesEarned * 700) + 300 + bonusPoints;

            var streakEmoteDescription = user.DiscordDailyStreak switch
            {
                0 => "<:B_Unlocked:797194347492802561> <:O_Locked:797195235985457203> <:N_Locked:797195235717021748> <:U_Locked:797195236316807238> <:S_Locked:797195235989389352>",
                1 => "<:B_Unlocked:797194347492802561> <:O_Unlocked:797194348026265600> <:N_Locked:797195235717021748> <:U_Locked:797195236316807238> <:S_Locked:797195235989389352>",
                2 => "<:B_Unlocked:797194347492802561> <:O_Unlocked:797194348026265600> <:N_Unlocked:797194348478726174> <:U_Locked:797195236316807238> <:S_Locked:797195235989389352>",
                3 => "<:B_Unlocked:797194347492802561> <:O_Unlocked:797194348026265600> <:N_Unlocked:797194348478726174> <:U_Unlocked:797194348369281065> <:S_Locked:797195235989389352>",
                4 => "<:B_Unlocked:797194347492802561> <:O_Unlocked:797194348026265600> <:N_Unlocked:797194348478726174> <:U_Unlocked:797194348369281065> <:S_Unlocked:797194348440715314>",
                _ => "how did we get here"
            };

            // update the streak
            var newStreak = user.DiscordDailyStreak == 4 ? 0 : user.DiscordDailyStreak + 1;
            user.DiscordDailyStreak = newStreak;
            user.DiscordDailyClaimed = true;

            await context.SaveChangesAsync();

            await discordHelperService.AddDiscordXpToUser(command.GuildId, command.ChannelId, command.UserId, 200);
            await discordHelperService.AddPointsToUser(command.GuildId, command.UserId, pointsToGive);

            return new DiscordEmbedData
            {
                Title = "Daily Points",
                Description = streakEmoteDescription,
                Colour = new Color(0, 237, 63),
                Fields = new Dictionary<string, string>
                {
                    { "Claimed", user.DiscordDailyClaimed.ToString() },
                    { "Days Claimed", newStreak == 0 ? "5" : $"{newStreak}" },
                    { "Points Claimed", pointsToGive.ToString("N0") }
                }
            };
        }

        //todo: test method
        public async Task ResetDiscordDailyStreaks()
        {
            var usersReset = await context.DiscordDailyPoints.Where(x => x.DiscordDailyClaimed).ExecuteUpdateAsync(x => x.SetProperty(y => y.DiscordDailyClaimed, false));
            Log.Information($"[Discord Daily Points] Reset {usersReset} users daily points streaks");
        }

        private static long PointsToGiveToUser()
        {
            var random = new Random();

            switch (random.Next(0, 5001))
            {
                case <= 2000: //0-2000
                    return 10000;

                case > 2000 and <= 3500: //2001-3000
                    return 20000;

                case > 3500 and <= 4000:  //3501-4000
                    return 25000;

                case > 4000 and <= 4400: //4001 -4400
                    return 35000;

                case > 4400 and <= 4600: //4401 - 4600
                    return 40000;

                case > 4600 and <= 4750: //4601 - 4750
                    return 50000;

                case > 4750 and <= 4850: //4751 - 4850
                    return 100000;

                case > 4850 and <= 4900: //4851 - 4900
                    return 150000;

                case > 4900 and <= 4950: //4901 - 4950
                    return 200000;

                case > 4950 and <= 4990: //4951 - 4990
                    return 250000;

                case > 4990 and <= 4995: //4991 - 4995
                    return 300000;

                case > 4995 and <= 4998: //4996-4498
                    return 5000000;

                case 4999: //4999
                    return 100000000;

                case 5000: //5000 memes
                    return 1;

                default:
                    return 0;
            }
        }
    }
}
