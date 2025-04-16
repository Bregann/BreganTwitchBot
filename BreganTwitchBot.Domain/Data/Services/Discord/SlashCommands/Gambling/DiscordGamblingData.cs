using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.DTOs.Discord;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.Gambling
{
    //TODO: Test this class
    public class DiscordGamblingData(AppDbContext context, IDiscordHelperService discordHelperService) : IDiscordGamblingData
    {
        public async Task<DiscordEmbedData> HandleSpinCommand(DiscordCommand command)
        {
            if (command.CommandText == null)
            {
                return new DiscordEmbedData
                {
                    Colour = new Color(255, 0, 0),
                    Title = "Error",
                    Description = "Command text is null.",
                    Fields = []
                };
            }

            var user = await context.ChannelUserData
                .Where(x => x.Channel.DiscordGuildId == command.GuildId && x.ChannelUser.DiscordUserId == command.UserId)
                .FirstAsync();

            if (user.Points < 1000)
            {
                return new DiscordEmbedData
                {
                    Colour = new Color(255, 0, 0),
                    Title = "Error",
                    Description = "Silly goose! You do not have enough points to spin.",
                    Fields = []
                };
            }

            if (command.CommandText.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                return await SpinPoints(user.Points, command.UserId, command.GuildId);
            }
            else
            {
                var pointsBeingGambled = command.CommandText;

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
                    return new DiscordEmbedData
                    {
                        Colour = new Color(255, 0, 0),
                        Title = "Invalid input",
                        Description = "Silly goose! You need to specify a number or do **/spin all** to risk it all!",
                        Fields = []
                    };
                }

                //check if points is more than min spin cost
                if (convertedPoints < 1000)
                {
                    return new DiscordEmbedData
                    {
                        Colour = new Color(255, 0, 0),
                        Title = "Invalid input",
                        Description = $"It is **1,000** points minimum!!!",
                        Fields = []
                    };
                }

                //check if the user has enough points
                if (user.Points < convertedPoints)
                {
                    return new DiscordEmbedData
                    {
                        Colour = new Color(255, 0, 0),
                        Title = "Invalid input",
                        Description = $"Silly goose! You do not have enough points to gamble that much.",
                        Fields = []
                    };
                }

                // do the spin
                return await SpinPoints(convertedPoints, command.UserId, command.GuildId);
            }
        }

        private async Task<DiscordEmbedData> SpinPoints(long pointsGambled, ulong discordId, ulong guildId)
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

            await discordHelperService.RemovePointsFromUser(discordId, guildId, pointsGambled);

            var randomSpin = random.Next(1, 10001);
            long pointsWon = 0;
            var description = @$"**─────────**
                                             **|** EMOTE **|** EMOTE ** |** EMOTE **|**
                                      **─────────**
                                        **─ YOU RESULT ─**"; ;
            var discordXpToAdd = 20;
            var fields = new Dictionary<string, string>();

            switch (randomSpin)
            {
                case 73:
                    description = description.Replace("EMOTE", ":joy:").Replace("RESULT", "WON");
                    fields.Add("Points Won", "this video https://www.youtube.com/watch?v=CqnZjVgDN_g");
                    discordXpToAdd = 1000;
                    break;
                case <= 20:
                    if (random.Next(1, 4) == 1)
                    {
                        description = description.Replace("EMOTE", ":eggplant:").Replace("RESULT", "WON");

                        pointsWon = pointsGambled * 350;
                        discordXpToAdd = 300;
                        fields.Add("Winnings", $"You won {pointsWon:N0} points!");
                        await AddWin(guildId, DiscordGambleWinType.EggplantWins);
                        break;
                    }
                    else
                    {
                        description = description.Replace("EMOTE", ":cheese:").Replace("RESULT", "LOST");

                        pointsWon = 1;
                        discordXpToAdd = 1;
                        fields.Add("Winnings", $"1 lol gg");
                        await AddWin(guildId, DiscordGambleWinType.CheeseWins);
                        break;
                    }
                case <= 50:
                    description = description.Replace("EMOTE", ":cucumber:").Replace("RESULT", "WON");

                    pointsWon = pointsGambled * 80;
                    discordXpToAdd = 100;
                    fields.Add("Winnings", $"You won {pointsWon:N0} points!");
                    await AddWin(guildId, DiscordGambleWinType.CucumberWins);
                    break;
                case <= 150:
                    if (random.Next(1, 3) == 1)
                    {
                        var randomPayout = random.Next(1, 30);

                        pointsWon = pointsGambled * randomPayout;
                        discordXpToAdd = 80;
                        fields.Add("Winnings", $"You won {pointsWon:N0} points! (x{randomPayout})");
                        await AddWin(guildId, DiscordGambleWinType.CherriesWins);
                        break;
                    }
                    else
                    {
                        description = description.Replace("EMOTE", ":pineapple:").Replace("RESULT", "WON");
                        pointsWon = pointsGambled * 30;
                        discordXpToAdd = 80;
                        fields.Add("Winnings", $"You won {pointsWon:N0} points!");
                        await AddWin(guildId, DiscordGambleWinType.PineappleWins);
                        break;
                    }

                case <= 480:
                    description = description.Replace("EMOTE", ":grapes:").Replace("RESULT", "WON");

                    pointsWon = pointsGambled * 15;
                    discordXpToAdd = 30;
                    fields.Add("Winnings", $"You won {pointsWon:N0} points!");
                    await AddWin(guildId, DiscordGambleWinType.GrapesWins);
                    break;
                default:
                    if (random.Next(0, 1000000) == 7373)
                    {
                        description = description.Replace("EMOTE", ":cookie:").Replace("RESULT", "WON");
                        discordXpToAdd = 10000;
                        fields.Add("Winnings", $"this is a 1/1m lol");
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

                    description = @$"**─────────**
                                             **|** :{randomFruits[0]}: **|** :{randomFruits[1]}: ** |** :{randomFruits[2]}: **|**
                                      **─────────**
                                        **─ YOU LOST ─**";

                    discordXpToAdd = 10;
                    fields.Add("Points Lost", $"You lost {pointsGambled:N0} points!");
                    break;
            }

            var embed = new DiscordEmbedData
            {
                Colour = pointsWon > 0 ? new Color(153, 255, 51) : new Color(0, 255, 0),
                Title = "Spin Result",
                Description = description,
                Fields = fields
            };

            if (pointsWon > 0)
            {
                await discordHelperService.AddPointsToUser(guildId, discordId, pointsWon);
            }

            await discordHelperService.AddDiscordXpToUser(guildId, discordId, discordXpToAdd);

            return embed;
        }

        private async Task AddWin(ulong guildId, DiscordGambleWinType winType)
        {
            var stats = await context.DiscordSpinStats.FirstAsync(x => x.Channel.DiscordGuildId == guildId);

            stats.GrapesWins += winType == DiscordGambleWinType.GrapesWins ? 1 : 0;
            stats.PineappleWins += winType == DiscordGambleWinType.PineappleWins ? 1 : 0;
            stats.CherriesWins += winType == DiscordGambleWinType.CherriesWins ? 1 : 0;
            stats.CucumberWins += winType == DiscordGambleWinType.CucumberWins ? 1 : 0;
            stats.EggplantWins += winType == DiscordGambleWinType.EggplantWins ? 1 : 0;
            stats.CheeseWins += winType == DiscordGambleWinType.CheeseWins ? 1 : 0;

            await context.SaveChangesAsync();
        }
    }
}
