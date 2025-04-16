using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord
{
    public class DiscordHelperService(IDiscordService discordService, IConfigHelperService configHelperService, IServiceProvider serviceProvider) : IDiscordHelperService
    {
        public async Task SendMessage(ulong channelId, string message)
        {
            try
            {
                var channel = discordService.Client.GetChannel(channelId) as IMessageChannel;

                if (channel != null)
                {
                    await channel.TriggerTypingAsync();
                    await channel.SendMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"[Discord Message Send] Failed to send message - {e}");
            }
        }

        public async Task SendEmbedMessage(ulong channelId, EmbedBuilder embed)
        {
            try
            {
                var channel = discordService.Client.GetChannel(channelId) as IMessageChannel;

                if (channel != null)
                {
                    await channel.TriggerTypingAsync();
                    await channel.SendMessageAsync(embed: embed.Build());
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"[Discord Message Send] Failed to send message - {ex}");
            }
        }

        public bool IsUserMod(ulong serverId, ulong userId)
        {
            var user = discordService.Client.GetGuild(serverId).GetUser(userId);

            if (user == null)
            {
                return false;
            }

            var discordConfig = configHelperService.GetDiscordConfig(serverId);
            if (discordConfig != null)
            {
                var modRole = discordConfig.DiscordModeratorRoleId;
                return user.Roles.Any(role => role.Id == modRole);
            }

            return false;
        }

        //TODO: WRITE TESTS FOR THIS METHOD
        public string? GetTwitchUsernameFromDiscordUser(ulong userId)
        {
            using(var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var user = context.ChannelUsers.FirstOrDefault(x => x.DiscordUserId == userId);

                return user?.TwitchUsername;
            }
        }

        //TODO: WRITE TESTS FOR THIS METHOD
        public async Task AddDiscordXpToUser(ulong guildId, ulong channelId, ulong userId, long baseXpToAdd)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var user = await context.DiscordUserStats.FirstOrDefaultAsync(x => x.User.DiscordUserId == userId && x.Channel.DiscordGuildId == guildId);

                if (user != null)
                {
                    user.DiscordXp += baseXpToAdd;
                    await context.SaveChangesAsync();

                    //TODO: check for level up
                    long xpNeededForLevelUp;
                    var baseXp = 10;
                    var userLevelledUp = false;

                    //Check if they have levelled up - could be mutliple to slap it in a while loop
                    while (true)
                    {
                        switch (user.DiscordLevel)
                        {
                            case 0:
                                xpNeededForLevelUp = 5;
                                break;
                            case 1:
                                xpNeededForLevelUp = 10;
                                break;
                            default:
                                var lastLevelXp = baseXp * (user.DiscordLevel - 1);
                                xpNeededForLevelUp = (long)Math.Round(lastLevelXp * 1.08 * user.DiscordLevel);
                                break;
                        }

                        if (user.DiscordXp >= xpNeededForLevelUp)
                        {
                            user.DiscordLevel++;
                            await AddPointsToUser(guildId, userId, user.DiscordLevel * 2000);
                            Log.Information($"[Discord XP Manager Service] {userId} has levelled up to level {user.DiscordLevel} in {guildId}");
                            await context.SaveChangesAsync();
                            userLevelledUp = true;
                            continue;
                        }

                        break;
                    }

                    Log.Information("[Discord Levelling] Levelling done");

                    if (userLevelledUp && user.DiscordLevelUpNotifsEnabled)
                    {
                        Log.Information("[Discord Levelling] Sending message");
                        if (user.DiscordLevel == 1 || user.DiscordLevel == 2)
                        {
                            return;
                        }

                        var config = configHelperService.GetDiscordConfig(guildId);
                        await SendMessage(channelId, $"**GG** <@{user.User.DiscordUserId}> you have levelled up to level **{user.DiscordLevel}**! You have gained **{user.DiscordLevel * 2000:N0}** pooants! (you can disable level up messages by doing /togglelevelups in <#{config.DiscordUserCommandsChannelId}>");
                    }
                }
            }
        }

        //TODO: WRITE TESTS FOR THIS METHOD
        public async Task AddPointsToUser(ulong serverId, ulong userId, long pointsToAdd)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var channel = await context.Channels.FirstAsync(x => x.DiscordGuildId == serverId);

                var userPoints = await context.ChannelUserData.FirstOrDefaultAsync(x => x.Channel.DiscordGuildId == serverId && x.ChannelUser.DiscordUserId == userId);

                if (userPoints == null)
                {
                    Log.Warning("[Discord Helper Service] User points not found. No points added");
                    return;
                }

                // check if the new amount of points will make the user above the points cap
                if (userPoints.Points + pointsToAdd > channel.ChannelConfig.CurrencyPointCap)
                {
                    userPoints.Points = channel.ChannelConfig.CurrencyPointCap;
                    await context.SaveChangesAsync();

                    Log.Information($"[Discord Helper Service] Added {pointsToAdd} points to {userId} in {serverId}, but capped at {channel.ChannelConfig.CurrencyPointCap}");
                    return;
                }

                userPoints.Points += pointsToAdd;
                await context.SaveChangesAsync();

                Log.Information($"[Discord Helper Service] Added {userId} points to {userId} in {serverId}");
            }
        }

        //TODO: WRITE TESTS FOR THIS METHOD
        public async Task RemovePointsFromUser(ulong serverId, ulong userId, long pointsToRemove)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var channel = await context.Channels.FirstAsync(x => x.DiscordGuildId == serverId);
                var userPoints = await context.ChannelUserData.FirstAsync(x => x.Channel.DiscordGuildId == serverId && x.ChannelUser.DiscordUserId == userId);

                // check if the new amount of points will make the user below 0
                if (userPoints.Points - pointsToRemove < 0)
                {
                    userPoints.Points = 0;
                    await context.SaveChangesAsync();
                    Log.Information($"[Discord Helper Service] Removed {pointsToRemove} points from {userId} in {serverId}, but capped at 0");
                    return;
                }

                userPoints.Points -= pointsToRemove;
                await context.SaveChangesAsync();
                Log.Information($"[Discord Helper Service] Removed {pointsToRemove} points from {userId} in {serverId}");
            }
        }
    }
}
