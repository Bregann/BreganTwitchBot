using BreganTwitchBot.Core.DiscordBot.Commands.Modules.Gambling.Enums;
using BreganTwitchBot.Core.DiscordBot.Commands.Modules.Levelling;
using BreganTwitchBot.Core.Twitch.Services;
using BreganTwitchBot.Data;
using BreganTwitchBot.Data.Models;
using BreganTwitchBot.DiscordBot.Helpers;
using Discord;
using Discord.Interactions;
using Serilog;

namespace BreganTwitchBot.Core.DiscordBot.Commands.Modules.Gambling
{
    public class Gambling
    {
        public static async Task<EmbedBuilder> HandleSpinCommand(string pointsBeingGambled, SocketInteractionContext context)
        {
            Users user;

            var embed = new EmbedBuilder()
            {
                Timestamp = DateTime.Now,
                Color = new Color(0, 237, 63)
            };

            //Check if user is gambling all their points
            if (pointsBeingGambled.ToLower() == "all")
            {
                using (var dbContext = new DatabaseContext())
                {
                    user = dbContext.Users.Where(x => x.DiscordUserId == context.User.Id).FirstOrDefault();
                }

                if (user.Points < 1000)
                {
                    embed.Title = $"❌ Not enough {Config.PointsName}";
                    embed.Color = new Color(255, 0, 0);
                    embed.Description = $"You only have **{user.Points:N0}** {Config.PointsName}";
                    return embed;
                }

                embed.WithAuthor($"Spin - {context.User.Username} - {user.Points:N0} {Config.PointsName} Gambled", null, context.User.GetAvatarUrl());
                return await SpinPoints(user.Points, context.User.Id, embed, context.Channel.Id);
            }

            var pointsGambled = long.TryParse(pointsBeingGambled, out long convertedPoints);
            var split = pointsBeingGambled.Split(".");
            pointsBeingGambled = pointsBeingGambled.ToLower(); //make it to lower for the m,k,b matching
            //also try and convert incase they have done something like 73.3m
            if (split.Count() == 2) //check if its just one decimal
            {
                if (split[1].ToCharArray().Count() <= 3) //check if its 2 or less decimal places
                {
                    var type = split[1].ToLower().ToCharArray();
                    switch (type[type.Count() - 1]) //check the type and convert it as nesseacary
                    {
                        case 'k':
                            if (type.Count() == 3)
                            {
                                long.TryParse(pointsBeingGambled.Replace("k", "0").Replace(".", ""), out convertedPoints);
                            }
                            else
                            {
                                long.TryParse(pointsBeingGambled.Replace("k", "00").Replace(".", ""), out convertedPoints);
                            }
                            break;
                        case 'm':
                            if (type.Count() == 3)
                            {
                                long.TryParse(pointsBeingGambled.Replace("m", "0000").Replace(".", ""), out convertedPoints);
                            }
                            else
                            {
                                long.TryParse(pointsBeingGambled.Replace("m", "00000").Replace(".", ""), out convertedPoints);
                            }
                            break;
                        case 'b':
                            if (type.Count() == 3)
                            {
                                long.TryParse(pointsBeingGambled.Replace("b", "0000000").Replace(".", ""), out convertedPoints);
                            }
                            else
                            {
                                long.TryParse(pointsBeingGambled.Replace("b", "00000000").Replace(".", ""), out convertedPoints);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                if (split.Count() == 1)
                {
                    var type = split[0].ToCharArray();

                    switch (type[type.Count() - 1])
                    {
                        case 'k':
                            long.TryParse(pointsBeingGambled.Replace("k", "000"), out convertedPoints);
                            break;
                        case 'm':
                            long.TryParse(pointsBeingGambled.Replace("m", "000000"), out convertedPoints);
                            break;
                        case 'b':
                            long.TryParse(pointsBeingGambled.Replace("b", "000000000"), out convertedPoints);
                            break;
                        default:
                            break;
                    }
                }
            }

            //check if points is valid
            if (convertedPoints <= 0)
            {
                embed.Title = "❌ Invalid input";
                embed.Color = new Color(255, 0, 0);
                embed.Description = "You need to specify a number or do **!spin all** to risk it all!";
                return embed;
            }

            //check if points is more than min spin cost
            if (convertedPoints < 1000)
            {
                embed.Title = "❌ Not enough gambled";
                embed.Color = new Color(255, 0, 0);
                embed.Description = $"It is **1,000** {Config.PointsName} minimum!";
                return embed;
            }

            //Check if they have enough points
            using (var dbContext = new DatabaseContext())
            {
                user = dbContext.Users.Where(x => x.DiscordUserId == context.User.Id).FirstOrDefault();
            }

            if (user.Points < convertedPoints)
            {
                embed.Title = $"❌ Not enough {Config.PointsName}";
                embed.Color = new Color(255, 0, 0);
                embed.Description = $"You only have **{user.Points:N0}** {Config.PointsName}";
                return embed;
            }

            //if they have over 1m then they need to gamble more
            if (user.Points > 1000000 && 5000 > convertedPoints)
            {
                embed.Title = $"❌ Not enough gambled";
                embed.Color = new Color(255, 0, 0);
                embed.Description = $"You only have to gamble at least 5,000 of your {Config.PointsName}";
                return embed;
            }

            //if they have over 10m then they need to gamble more
            var onePercentOfUserPoints = user.Points / 100;
            if (user.Points > 10000000 && onePercentOfUserPoints > convertedPoints)
            {
                embed.Title = $"❌ Not enough gambled";
                embed.Color = new Color(255, 0, 0);
                embed.Description = $"You only have to gamble at least 1% of your {Config.PointsName} ({user.Points / 100:N0})";
                return embed;
            }

            embed.WithAuthor($"Spin - {context.User.Username} - {convertedPoints:N0} {Config.PointsName} Gambled", null, context.User.GetAvatarUrl());
            return await SpinPoints(convertedPoints, context.User.Id, embed, context.Channel.Id);
        }

        private static async Task<EmbedBuilder> SpinPoints(long points, ulong discordId, EmbedBuilder embed, ulong channelId)
        {
            var random = new Random();
            var emoteList = new List<string>();

            /*
                grapeWins: 1/27 (3703)
                pineappleWins: 1/125 (800)
                cherryWins: 1/125 (797)
                cucumberWins: 1/421 (237)
                eggplantWins: 1/3448 (29)
                cheeseWins: 1/3448 (29) 
             */
            DiscordHelper.RemoveUserPoints(discordId, points);

            long userPoints;

            using(var context = new DatabaseContext())
            {
                userPoints = context.Users.Where(x => x.DiscordUserId == discordId).First().Points;
            }

            var randomSpin = random.Next(1, 10001);
            switch (randomSpin)
            {
                case 73:
                    embed.Description = @$"**─────────**
                                             **|** :joy: **|** :joy: ** |** :joy: **|**
                                      **─────────**
                                        **─ YOU WON ─**";

                    embed.AddField("Winnings", "this video https://www.youtube.com/watch?v=CqnZjVgDN_g");
                    embed.AddField(Config.PointsName, $"{userPoints:N0}");
                    await Task.Run(async () => await DiscordLevelling.AddDiscordXp(discordId, 500, channelId));
                    break;
                case <= 15:
                    if (random.Next(1, 3) == 1)
                    {
                        embed.Description = @$"**─────────**
                                             **|** :cheese: **|** :cheese: ** |** :cheese: **|**
                                      **─────────**
                                        **─ YOU WON ─**";

                        embed.AddField("Winnings", "1 lol gg");
                        DiscordHelper.AddUserPoints(discordId, 1);
                        AddWin(WinTypes.CheeseWins);

                        embed.AddField(Config.PointsName, $"{userPoints + 1:N0}");
                        await Task.Run(async () => await DiscordLevelling.AddDiscordXp(discordId, 1, channelId));
                    }
                    else
                    {
                        embed.Description = @$"**─────────**
                                             **|** :eggplant: **|** :eggplant: ** |** :eggplant: **|**
                                      **─────────**
                                        **─ YOU WON ─**";

                        embed.AddField("Winnings", $"{points * 350:N0}");
                        DiscordHelper.AddUserPoints(discordId, points * 350);
                        AddWin(WinTypes.EggplantWins);

                        embed.AddField(Config.PointsName, $"{(points * 350) + userPoints:N0}");
                        await Task.Run(async () => await DiscordLevelling.AddDiscordXp(discordId, 100, channelId));
                    }
                    break;
                case <= 50:
                    embed.Description = @$"**─────────**
                                             **|** :cucumber: **|** :cucumber: ** |** :cucumber: **|**
                                      **─────────**
                                        **─ YOU WON ─**";

                    embed.AddField("Winnings", $"{points * 80:N0}");
                    DiscordHelper.AddUserPoints(discordId, points * 80);
                    AddWin(WinTypes.CucumberWins);

                    embed.AddField(Config.PointsName, $"{(points * 80) + userPoints:N0}");
                    await Task.Run(async () => await DiscordLevelling.AddDiscordXp(discordId, 50, channelId));
                    break;
                case <= 150:
                    if (random.Next(1, 3) == 1)
                    {
                        embed.Description = @$"**─────────**
                                             **|** :cherries: **|** :cherries: ** |** :cherries: **|**
                                      **─────────**
                                        **─ YOU WON ─**";
                        var randomPayout = random.Next(35, 50);

                        embed.AddField("Winnings", $"{points * randomPayout:N0} (x{randomPayout})");
                        DiscordHelper.AddUserPoints(discordId, points * randomPayout);
                        AddWin(WinTypes.CherriesWins);

                        embed.AddField(Config.PointsName, $"{(points * randomPayout) + userPoints:N0} Remaining");
                        await Task.Run(async () => await DiscordLevelling.AddDiscordXp(discordId, 40, channelId));
                    }
                    else
                    {
                        embed.Description = @$"**─────────**
                                             **|** :pineapple: **|** :pineapple: ** |** :pineapple: **|**
                                      **─────────**
                                        **─ YOU WON ─**";

                        embed.AddField("Winnings", $"{points * 30:N0}");
                        DiscordHelper.AddUserPoints(discordId, points * 30);
                        AddWin(WinTypes.PineappleWins);

                        embed.AddField(Config.PointsName, $"{(points * 30) + userPoints:N0}");
                        await Task.Run(async () => await DiscordLevelling.AddDiscordXp(discordId, 40, channelId));
                    }
                    break;
                case <= 550:
                    embed.Description = @$"**─────────**
                                             **|** :grapes: **|** :grapes: ** |** :grapes: **|**
                                      **─────────**
                                        **─ YOU WON ─**";

                    embed.AddField("Winnings", $"{points * 15:N0}");
                    DiscordHelper.AddUserPoints(discordId, points * 15);
                    AddWin(WinTypes.GrapesWins);

                    embed.AddField(Config.PointsName, $"{(points * 15) + userPoints:N0}");
                    await Task.Run(async () => await DiscordLevelling.AddDiscordXp(discordId, 30, channelId));
                    break;
                default:
                    if (random.Next(0, 1000000) == 7373)
                    {
                        embed.Description = @$"**─────────**
                                             **|** :cookie: **|** :cookie: ** |** :cookie: **|**
                                      **─────────**
                                        **─ YOU WON ─**";

                        embed.AddField("Winnings", "this is a 1/1m lol");
                        await Task.Run(async () => await DiscordLevelling.AddDiscordXp(discordId, 10000, channelId));
                        break;
                    }

                    var fruits = new List<string> { "cheese", "eggplant", "cherries", "cucumber", "pineapple", "grapes", "cheese", "eggplant", "cherries", "cucumber", "pineapple", "grapes" };
                    var randomFruits = new List<string>();

                    for (int i = 0; i < 3; i++)
                    {
                        var rnd = random.Next(0, fruits.Count - 1);
                        randomFruits.Add(fruits[rnd]);
                        fruits.RemoveAt(rnd);
                    }

                    embed.Description = @$"**─────────**
                                             **|** :{randomFruits[0]}: **|** :{randomFruits[1]}: ** |** :{randomFruits[2]}: **|**
                                      **─────────**
                                        **─ YOU LOST ─**";
                    embed.AddField("Winnings", $"-{points:N0}");

                    embed.AddField(Config.PointsName, $"{userPoints:N0}");
                    await Task.Run(async () => await DiscordLevelling.AddDiscordXp(discordId, 5, channelId));
                    break;
            }

            Log.Information($"[Discord Gambling] Number generated: {randomSpin}");

            using(var context = new DatabaseContext())
            {
                context.SlotMachine.First().DiscordTotalSpins++;
            }

            return embed;
        }

        private static void AddWin(WinTypes type)
        {
            using(var context = new DatabaseContext())
            {
                switch (type)
                {
                    case WinTypes.GrapesWins:
                        context.SlotMachine.First().GrapesWins++;
                        break;
                    case WinTypes.PineappleWins:
                        context.SlotMachine.First().PineappleWins++;
                        break;
                    case WinTypes.CherriesWins:
                        context.SlotMachine.First().CherriesWins++;
                        break;
                    case WinTypes.CucumberWins:
                        context.SlotMachine.First().CucumberWins++;
                        break;
                    case WinTypes.EggplantWins:
                        context.SlotMachine.First().EggplantWins++;
                        break;
                    case WinTypes.CheeseWins:
                        context.SlotMachine.First().CheeseWins++;
                        break;
                }

                context.SaveChanges();
            }
        }
    }
}
