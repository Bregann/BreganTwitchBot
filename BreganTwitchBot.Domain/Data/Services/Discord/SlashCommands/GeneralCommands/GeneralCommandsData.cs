using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.GeneralCommands
{
    // TODO: test this class
    public class GeneralCommandsData(AppDbContext context, IConfigHelperService configHelper, IDiscordHelperService discordHelperService) : IGeneralCommandsData
    {
        public async Task<string> AddUserBirthday(AddBirthdayCommand command)
        {
            // Check if the user has already added their birthday
            var existingBirthday = await context.Birthdays
                .FirstOrDefaultAsync(x => x.User.DiscordUserId == command.UserId && x.Channel.DiscordGuildId == command.GuildId);

            if (existingBirthday != null)
            {
                return "You silly sausage you already have a birthday set! You can unbirthday yourself with /unbirthday";
            }

            // Check that it's a proper day and month
            switch (command.Month)
            {
                //31 days
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    if (command.Day > 31 && command.Day <= 0)
                    {
                        return "Oh deary me that month only has 31 days lol back to school you go! :)";
                    }
                    break;

                //29 days
                case 2:
                    if (command.Day > 29 && command.Day <= 0)
                    {
                        return "Oh deary me that month only has 31 days lol back to school you go! :)";
                    }
                    break;

                //30 days
                case 4:
                case 6:
                case 9:
                case 11:
                    if (command.Day > 30 && command.Day <= 0)
                    {
                        return "Oh deary me that month only has 31 days lol back to school you go! :)";
                    }
                    break;
                //silly people that put over 12 as their year
                default:
                    return "Oh deary me that month only has 31 days lol back to school you go! :)";
            }

            // Add to database
            var channel = await context.Channels
                .FirstAsync(x => x.DiscordGuildId == command.GuildId);

            var channelUser = await context.ChannelUsers.
                FirstAsync(x => x.DiscordUserId == command.UserId);

            await context.Birthdays.AddAsync(new Birthday
            {
                ChannelId = channel.Id,
                ChannelUserId = channelUser.Id,
                Day = command.Day,
                Month = command.Month
            });

            await context.SaveChangesAsync();
            return "Your birthday has been set!";
        }

        public async Task CheckForUserBirthdaysAndSendMessage()
        {
            var today = DateTime.UtcNow;
            var birthdays = await context.Birthdays
                .Where(x => x.Day == today.Day && x.Month == today.Month)
                .ToListAsync();

            if (birthdays.Count == 0)
            {
                Log.Information("No birthdays today");
                return;
            }

            foreach (var birthday in birthdays)
            {
                var config = configHelper.GetDiscordConfig(birthday.Channel.DiscordGuildId ?? 0);

                if (config == null)
                {
                    Log.Information($"No config found for guild {birthday.Channel.DiscordGuildId}");
                    continue;
                }

                if (config.DiscordGeneralChannelId == null)
                {
                    Log.Information($"No general channel found for guild {birthday.Channel.DiscordGuildId}");
                    continue;
                }

                await discordHelperService.SendMessage(config.DiscordGeneralChannelId.Value, $"Happy birthday to you! Happy birthday to you! Happy birthday dear @<{birthday.User.DiscordUserId}>! Happy birthday to you!");
            }
        }
    }
}
