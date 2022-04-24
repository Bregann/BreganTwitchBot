using BreganTwitchBot.Core.DiscordBot.Commands.Modules.Levelling;
using BreganTwitchBot.Data;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Core.DiscordBot.Commands.Modules.HoursPoints
{
    public class HoursPoints
    {
        public static async Task<EmbedBuilder> HandleHoursCommand(SocketInteractionContext ctx, string optUsername, IUser discordUser)
        {
            var embed = new EmbedBuilder()
            {
                Title = "Hours check",
                Timestamp = DateTime.Now,
                Color = new Color(238, 255, 46)
            };

            if (optUsername != null)
            {
                var twitchUserMinutes = TimeSpan.FromMinutes(0);

                using(var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.Username == optUsername).FirstOrDefault();

                    if (user != null)
                    {
                        twitchUserMinutes = TimeSpan.FromMinutes(user.MinutesInStream);
                    }
                }

                if (twitchUserMinutes.TotalMinutes == 0)
                {
                    embed.WithDescription("User does not exist!");
                    return embed;
                }

                embed.Title = $"Hours check - {optUsername.ToLower()}";
                embed.AddField("Time watched", $"{twitchUserMinutes.TotalMinutes:N0} minutes (about {Math.Round(twitchUserMinutes.TotalMinutes / 60, 2):N0} hours)");
                return embed;
            }

            var discordUserMinutes = TimeSpan.FromMinutes(0);

            if (discordUser == null)
            {
                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.DiscordUserId == ctx.User.Id).FirstOrDefault();

                    if (user != null)
                    {
                        discordUserMinutes = TimeSpan.FromMinutes(user.MinutesInStream);
                    }
                }
            }
            else
            {
                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.DiscordUserId == discordUser.Id).FirstOrDefault();

                    if (user != null)
                    {
                        discordUserMinutes = TimeSpan.FromMinutes(user.MinutesInStream);
                    }
                }
            }

            if (discordUserMinutes.TotalMinutes == 0)
            {
                embed.WithDescription("User does not exist!");
                return embed;
            }

            if (discordUser == null)
            {
                embed.Title = $"Hours check - {ctx.User.Username}";
                embed.AddField("Time watched", $"{discordUserMinutes.TotalMinutes:N0} minutes (about {Math.Round(discordUserMinutes.TotalMinutes / 60, 2):N0} hours)");
            }
            else
            {
                embed.Title = $"Hours check - {discordUser.Username}";
                embed.AddField("Time watched", $"{discordUserMinutes.TotalMinutes:N0} minutes (about {Math.Round(discordUserMinutes.TotalMinutes / 60, 2):N0} hours)");
            }

            return embed;
        }

        public static async Task<EmbedBuilder> HandlePointsCommand(SocketInteractionContext ctx, string optUsername, IUser discordUser)
        {
            var embed = new EmbedBuilder()
            {
                Title = $"{Config.PointsName} check",
                Timestamp = DateTime.Now,
                Color = new Color(238, 255, 46)
            };

            if (optUsername != null)
            {
                long twitchNamePoints = 0;

                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.Username == optUsername).FirstOrDefault();

                    if (user != null)
                    {
                        twitchNamePoints = user.Points;
                    }
                }

                if (twitchNamePoints == 0)
                {
                    embed.WithDescription("User does not exist!");
                    return embed;
                }

                embed.Title = $"{Config.PointsName} check - {optUsername.ToLower()}";
                embed.AddField($"{Config.PointsName}", $"{twitchNamePoints:N0} points");
                return embed;
            }

            long discordUserPoints = 0;

            if (discordUser == null)
            {
                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.DiscordUserId == ctx.User.Id).FirstOrDefault();

                    if (user != null)
                    {
                        discordUserPoints = user.Points;
                    }
                }
            }
            else
            {
                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => x.DiscordUserId == discordUser.Id).FirstOrDefault();

                    if (user != null)
                    {
                        discordUserPoints = user.Points;
                    }
                }
            }

            if (discordUserPoints == 0)
            {
                embed.WithDescription("User does not exist!");
                return embed;
            }

            if (discordUser == null)
            {
                embed.Title = $"{Config.PointsName} check - {ctx.User.Username}";
                embed.AddField($"{Config.PointsName}", $"{discordUserPoints:N0}");
            }
            else
            {
                embed.Title = $"{Config.PointsName} check - {discordUser.Username}";
                embed.AddField($"{Config.PointsName}", $"{discordUserPoints:N0}");
            }


            return embed;
        }

        public static async Task<string> HandlePrestigeCommand(SocketInteractionContext ctx)
        {
            long userPoints = 0;

            using (var context = new DatabaseContext())
            {
                userPoints = context.Users.Where(x => x.DiscordUserId == ctx.User.Id).First().Points;
            }

            if (userPoints < Config.PrestigeCap)
            {
                return $"{ctx.User.Mention} => You don't have enough points to prestige! You need {Config.PrestigeCap:N0} {Config.PointsName}!";
            }

            using(var context = new DatabaseContext())
            {
                context.Users.Where(x => x.DiscordUserId == ctx.User.Id).First().PrestigeLevel++;
                context.Users.Where(x => x.DiscordUserId == ctx.User.Id).First().Points -= Config.PrestigeCap;
                context.SaveChanges();
            }

            await DiscordLevelling.AddDiscordXp(ctx.User.Id, 7500, ctx.Channel.Id);
            return $"{ctx.User.Mention} => You have prestieged!";
        }
    }
}
