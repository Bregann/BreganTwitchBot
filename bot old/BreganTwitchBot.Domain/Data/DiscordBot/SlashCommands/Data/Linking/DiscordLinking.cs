using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Data.TwitchBot;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Infrastructure.Database.Models;
using Discord;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Linking
{
    public class DiscordLinking
    {
        public static async Task AddRolesOnInitialVerification(string twitchName)
        {
            Users? user;

            //Check minutes
            using (var context = new DatabaseContext())
            {
                user = context.Users.Include(x => x.Watchtime).Where(x => x.Username == twitchName).FirstOrDefault();

                if (user == null)
                {
                    Log.Information("[Discord Auto Ranking] User is null");
                    return;
                }
            }

            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
            var member = guild.GetUser(user.DiscordUserId);
            var rolesToAdd = new List<IRole>();

            var twitchVerified = guild.Roles.First(x => x.Name == "TwitchVerified");
            rolesToAdd.Add(twitchVerified);

            //Add roles if needed
            if (user.Watchtime.MinutesInStream >= 60)
            {
                var melvin = guild.Roles.First(x => x.Name == "Melvins");
                rolesToAdd.Add(melvin);
                Log.Information("[Discord Auto Ranking] Melvin roles added");
            }

            if (user.Watchtime.MinutesInStream >= 1500)
            {
                var wotCrew = guild.Roles.First(x => x.Name == "WOT crew");
                rolesToAdd.Add(wotCrew);
                Log.Information("[Discord Auto Ranking] Wot Crew roles added");
            }

            if (user.Watchtime.MinutesInStream >= 6000)
            {
                var blocksCrew = guild.Roles.First(x => x.Name == "BLOCKS crew");
                rolesToAdd.Add(blocksCrew);
                Log.Information("[Discord Auto Ranking] blocks crew roles added");
            }

            if (user.Watchtime.MinutesInStream >= 15000)
            {
                var nameOfLegends = guild.Roles.First(x => x.Name == "The Name of Legends");
                rolesToAdd.Add(nameOfLegends);
                Log.Information("[Discord Auto Ranking] Name of Legends roles added");
            }

            if (user.Watchtime.MinutesInStream >= 30000)
            {
                var kingOfStream = guild.Roles.First(x => x.Name == "King of the Stream");
                rolesToAdd.Add(kingOfStream);
                Log.Information("[Discord Auto Ranking] King of Stream roles added");
            }

            foreach (var role in rolesToAdd)
            {
                await member.AddRoleAsync(role);
                Log.Information("[Discord Auto Ranking] Roles added");
            }
        }

        public static async Task ApplyDiscordRankUp(Users user)
        {
            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
            var member = guild.GetUser(user.DiscordUserId);
            var rolesToAdd = new List<IRole>();

            switch (user.Watchtime.MinutesInStream)
            {
                case 60:
                    var melvin = guild.Roles.First(x => x.Name == "Melvins");
                    rolesToAdd.Add(melvin);
                    await DiscordHelper.SendMessage(AppConfig.DiscordRankUpAnnouncementChannelID, $"Congratulations <@{user.DiscordUserId}> on getting the **Melvin** Discord rank from watching the stream for **1** hours!");
                    break;

                case 1500:
                    var wotCrew = guild.Roles.First(x => x.Name == "WOT crew");
                    rolesToAdd.Add(wotCrew);
                    await DiscordHelper.SendMessage(AppConfig.DiscordRankUpAnnouncementChannelID, $"Congratulations <@{user.DiscordUserId}> on getting the **WOT crew** Discord rank from watching the stream for **25** hours!");
                    break;

                case 6000:
                    var blocksCrew = guild.Roles.First(x => x.Name == "BLOCKS crew");
                    rolesToAdd.Add(blocksCrew);
                    await DiscordHelper.SendMessage(AppConfig.DiscordRankUpAnnouncementChannelID, $"Congratulations <@{user.DiscordUserId}> on getting the **BLOCKS Crew** Discord rank from watching the stream for **100** hours!");
                    break;

                case 15000:
                    var nameOfLegends = guild.Roles.First(x => x.Name == "The Name of Legends");
                    rolesToAdd.Add(nameOfLegends);
                    await DiscordHelper.SendMessage(AppConfig.DiscordRankUpAnnouncementChannelID, $"Congratulations <@{user.DiscordUserId}> on getting the **The Name of Legends** Discord rank from watching the stream for **250** hours!");
                    break;

                case 30000:
                    var kingOfStream = guild.Roles.First(x => x.Name == "King of the Stream");
                    rolesToAdd.Add(kingOfStream);
                    await DiscordHelper.SendMessage(AppConfig.DiscordRankUpAnnouncementChannelID, $"Congratulations <@{user.DiscordUserId}> on getting the **King of the Stream** Discord rank from watching the stream for **500** hours!");
                    break;

                default:
                    break;
            }

            foreach (var role in rolesToAdd)
            {
                await member.AddRoleAsync(role);
                Log.Information("[Discord Ranking] Roles added");
            }
        }
    }
}