using BreganTwitchBot.Core.DiscordBot.Commands.Modules.Levelling;
using BreganTwitchBot.Data;
using BreganTwitchBot.Data.Models;
using BreganTwitchBot.DiscordBot.Helpers;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Core.DiscordBot.Commands.Modules.GeneralCommands.DailyPoints
{
    public class Daily
    {
        public static async Task<EmbedBuilder> HandleDailyCommand(SocketInteractionContext context)
        {
            var embed = new EmbedBuilder()
            {
                Timestamp = DateTime.Now,
                Color = new Color(0, 237, 63)
            };

            embed.WithAuthor($"Daily Points - {context.User.Username}", null, context.User.GetAvatarUrl());

            Users user;

            using (var dbContext = new DatabaseContext())
            {
                user = dbContext.Users.Where(x => x.DiscordUserId == context.User.Id).First();
            }

            //Check if user has claimed their points - yes spark you wont get past this
            if (user.DiscordDailyClaimed)
            {
                embed.Title = "❌ Already Claimed";
                embed.Color = new Color(255, 0, 0);
                embed.Description = "Don't be silly you have already claimed your points today";
                return embed;
            }

            var totalBonusesEarned = (int)(Math.Floor((decimal)user.DiscordDailyTotalClaims / 5)) + 1;
            long pointsToGive = 0;

            switch (user.DiscordDailyStreak)
            {
                case 0:
                    pointsToGive = totalBonusesEarned * 700;
                    embed.Description = "<:B_Unlocked:797194347492802561> <:O_Locked:797195235985457203> <:N_Locked:797195235717021748> <:U_Locked:797195236316807238> <:S_Locked:797195235989389352>";
                    embed.AddField("Points claimed", $"{totalBonusesEarned * 700:N0} " + Config.PointsName);
                    embed.AddField("Day's claimed", "1");
                    break;
                case 1:
                    pointsToGive = (totalBonusesEarned * 700) + 100;
                    embed.Description = "<:B_Unlocked:797194347492802561> <:O_Unlocked:797194348026265600> <:N_Locked:797195235717021748> <:U_Locked:797195236316807238> <:S_Locked:797195235989389352>";
                    embed.AddField("Points claimed", $"{(totalBonusesEarned * 700) + 100:N0} " + Config.PointsName);
                    embed.AddField("Day's claimed", "2");
                    break;
                case 2:
                    pointsToGive = (totalBonusesEarned * 700) + 200;
                    embed.Description = "<:B_Unlocked:797194347492802561> <:O_Unlocked:797194348026265600> <:N_Unlocked:797194348478726174> <:U_Locked:797195236316807238> <:S_Locked:797195235989389352>";
                    embed.AddField("Points claimed", $"{(totalBonusesEarned * 700) + 200:N0} " + Config.PointsName);
                    embed.AddField("Day's claimed", "3");
                    break;
                case 3:
                    pointsToGive = (totalBonusesEarned * 700) + 300;
                    embed.Description = "<:B_Unlocked:797194347492802561> <:O_Unlocked:797194348026265600> <:N_Unlocked:797194348478726174> <:U_Unlocked:797194348369281065> <:S_Locked:797195235989389352>";
                    embed.AddField("Points claimed", $"{(totalBonusesEarned * 700) + 300:N0} " + Config.PointsName);
                    embed.AddField("Day's claimed", "4");
                    break;
                case 4:
                    pointsToGive = PointsToGiveToUser() + (totalBonusesEarned * 700) + 300;
                    embed.Description = "<:B_Unlocked:797194347492802561> <:O_Unlocked:797194348026265600> <:N_Unlocked:797194348478726174> <:U_Unlocked:797194348369281065> <:S_Unlocked:797194348440715314>";
                    embed.AddField("Points claimed", $"{pointsToGive:N0} " + Config.PointsName);
                    embed.AddField("Day's claimed", "5");

                    using (var dbContext = new DatabaseContext())
                    {
                        dbContext.Users.Where(x => x.DiscordUserId == context.User.Id).First().DiscordDailyStreak = -1; //-1 as we set it again later (just lazy lol)
                        dbContext.SaveChanges();
                    }
                    break;
            }

            using (var dbContext = new DatabaseContext())
            {
                var userToUpdate = dbContext.Users.Where(x => x.DiscordUserId == context.User.Id).First();
                userToUpdate.DiscordDailyStreak++;
                userToUpdate.DiscordDailyTotalClaims++;
                userToUpdate.DiscordDailyClaimed = true;
                dbContext.SaveChanges();
            }

            DiscordHelper.AddUserPoints(context.User.Id, pointsToGive);
            await Task.Run(async () => await DiscordLevelling.AddDiscordXp(context.User.Id, 100, context.Channel.Id));
            return embed;
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
