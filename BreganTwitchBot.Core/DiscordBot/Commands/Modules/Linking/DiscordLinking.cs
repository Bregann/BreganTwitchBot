using BreganTwitchBot.Data;
using BreganTwitchBot.Data.Models;
using BreganTwitchBot.DiscordBot.Helpers;
using BreganTwitchBot.Services;
using Discord;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace BreganTwitchBot.Core.DiscordBot.Commands.Modules.Linking
{
    public class DiscordLinking
    {
        public static async Task<string> NewLinkRequest(string twitchName, ulong discordId)
        {
            var doesUserExist = false;

            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == twitchName).FirstOrDefault();

                if (user != null)
                {
                    doesUserExist = true;
                }
            }

            //Check if user exists
            if (!doesUserExist)
            {
                GetUsersResponse getUserID = null;

                try
                {
                    getUserID = await TwitchApiConnection.ApiClient.Helix.Users.GetUsersAsync(logins: new List<string> { twitchName });
                }
                catch (Exception)
                {
                    return "There has been an error! Please try again shortly";
                }

                if (getUserID.Users.Length == 0 || getUserID == null)
                {
                    return "Twitch user does not exist!";
                }

                using(var context = new DatabaseContext())
                {
                    var newUser = new Users
                    {
                        TwitchUserId = getUserID.Users[0].Id,
                        Username = twitchName,
                        InStream = true,
                        IsSub = false,
                        MinutesInStream = 0,
                        Points = 0,
                        IsSuperMod = false,
                        TotalMessages = 0,
                        DiscordUserId = 0,
                        LastSeenDate = DateTime.Now,
                        PointsGambled = 0,
                        PointsWon = 0,
                        PointsLost = 0,
                        TotalSpins = 0,
                        Tier1Wins = 0,
                        Tier2Wins = 0,
                        Tier3Wins = 0,
                        JackpotWins = 0,
                        SmorcWins = 0,
                        CurrentStreak = 0,
                        HighestStreak = 0,
                        TotalTimesClaimed = 0,
                        TotalPointsClaimed = 0,
                        PointsLastClaimed = new DateTime(0),
                        PointsClaimedThisStream = false,
                        GiftedSubsThisMonth = 0,
                        BitsDonatedThisMonth = 0,
                        MarblesWins = 0,
                        DiceRolls = 0,
                        BonusDiceRolls = 0,
                        DiscordDailyStreak = 0,
                        DiscordDailyTotalClaims = 0,
                        DiscordDailyClaimed = false,
                        DiscordLevel = 0,
                        DiscordXp = 0,
                        DiscordLevelUpNotifsEnabled = true,
                        PrestigeLevel = 0,
                        MinutesWatchedThisStream = 0,
                        MinutesWatchedThisWeek = 0,
                        MinutesWatchedThisMonth = 0,
                        BossesDone = 0,
                        BossesPointsWon = 0,
                        TimeoutStrikes = 0,
                        WarnStrikes = 0
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();
                }
            }

            var isUserLinked = false;

            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == twitchName).First();

                if (user.DiscordUserId != 0)
                {
                    isUserLinked = true;
                }
            }

            //Check if user has already linked
            if (isUserLinked)
            {
                return "That Twitch user is already linked!";
            }

            //insert into db
            using (var context = new DatabaseContext())
            {
                //Check if there is already a link request for the user
                var existingRequest = context.DiscordLinkRequests.Where(x => x.DiscordID == discordId || x.TwitchName == twitchName).FirstOrDefault();

                if (existingRequest != null)
                {
                    context.DiscordLinkRequests.Remove(existingRequest);
                }

                var linkRequest = new DiscordLinkRequests
                {
                    DiscordID = discordId,
                    TwitchName = twitchName
                };

                context.DiscordLinkRequests.Add(linkRequest);
                context.SaveChanges();

            }

            return "Head over to https://twitch.tv/blocksssssss and type in !link in the Twitch chat to link your account!";
        }

        public static async Task<string> ManualLinkRequest(ulong discordId, string twitchName)
        {
            var doesUserExist = false;

            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == twitchName).FirstOrDefault();

                if (user != null)
                {
                    doesUserExist = true;
                }
            }

            //Check if user exists
            if (doesUserExist)
            {
                return "Twitch user does not exist!";
            }

            var isUserLinked = false;

            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == twitchName).First();

                if (user.DiscordUserId != 0)
                {
                    isUserLinked = true;
                }
            }

            //Check if user has already linked
            if (isUserLinked)
            {
                return "That Twitch user is already linked!";
            }

            using(var context = new DatabaseContext())
            {
                context.Users.Where(x => x.Username == twitchName).First().DiscordUserId = discordId;
                context.SaveChanges();
            }

            await AddRolesOnInitialVerification(twitchName);

            Log.Information($"[Discord Auto Ranks] {twitchName} has been manually linked");
            return "user successfully linked";
        }

        public static async Task AddRolesOnInitialVerification(string twitchName)
        {
            Users user;

            //Check minutes
            using (var context = new DatabaseContext())
            {
                user = context.Users.Where(x => x.Username == twitchName).FirstOrDefault();

                if (user == null)
                {
                    Log.Information("[Discord Auto Ranking] User is null");
                    return;
                }
            }

            var guild = DiscordConnection.DiscordClient.GetGuild(Config.DiscordGuildID);
            var member = guild.GetUser(user.DiscordUserId);
            var rolesToAdd = new List<IRole>();

            var twitchVerified = guild.Roles.First(x => x.Name == "TwitchVerified");
            rolesToAdd.Add(twitchVerified);

            //Add roles if needed
            if (user.MinutesInStream >= 60)
            {
                var melvin = guild.Roles.First(x => x.Name == "Melvins");
                rolesToAdd.Add(melvin);
                Log.Information("[Discord Auto Ranking] Melvin roles added");
            }

            if (user.MinutesInStream >= 1500)
            {
                var wotCrew = guild.Roles.First(x => x.Name == "WOT crew");
                rolesToAdd.Add(wotCrew);
                Log.Information("[Discord Auto Ranking] Wot Crew roles added");
            }

            if (user.MinutesInStream >= 6000)
            {
                var blocksCrew = guild.Roles.First(x => x.Name == "BLOCKS crew");
                rolesToAdd.Add(blocksCrew);
                Log.Information("[Discord Auto Ranking] blocks crew roles added");
            }

            if (user.MinutesInStream >= 15000)
            {
                var nameOfLegends = guild.Roles.First(x => x.Name == "The Name of Legends");
                rolesToAdd.Add(nameOfLegends);
                Log.Information("[Discord Auto Ranking] Name of Legends roles added");
            }

            if (user.MinutesInStream >= 30000)
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
            var guild = DiscordConnection.DiscordClient.GetGuild(Config.DiscordGuildID);
            var member = guild.GetUser(user.DiscordUserId);
            var rolesToAdd = new List<IRole>();

            switch (user.MinutesInStream)
            {
                case 60:
                    var melvin = guild.Roles.First(x => x.Name == "Melvins");
                    rolesToAdd.Add(melvin);
                    await DiscordHelper.SendMessage(Config.DiscordRankUpAnnouncementChannelID, $"Congratulations <@{user.DiscordUserId}> on getting the **Melvin** Discord rank from watching the stream for **1** hours!");
                    break;
                case 1500:
                    var wotCrew = guild.Roles.First(x => x.Name == "WOT crew");
                    rolesToAdd.Add(wotCrew);
                    await DiscordHelper.SendMessage(Config.DiscordRankUpAnnouncementChannelID, $"Congratulations <@{user.DiscordUserId}> on getting the **WOT crew** Discord rank from watching the stream for **25** hours!");
                    break;
                case 6000:
                    var blocksCrew = guild.Roles.First(x => x.Name == "BLOCKS crew");
                    rolesToAdd.Add(blocksCrew);
                    await DiscordHelper.SendMessage(Config.DiscordRankUpAnnouncementChannelID, $"Congratulations <@{user.DiscordUserId}> on getting the **BLOCKS Crew** Discord rank from watching the stream for **100** hours!");
                    break;
                case 15000:
                    var nameOfLegends = guild.Roles.First(x => x.Name == "The Name of Legends");
                    rolesToAdd.Add(nameOfLegends);
                    await DiscordHelper.SendMessage(Config.DiscordRankUpAnnouncementChannelID, $"Congratulations <@{user.DiscordUserId}> on getting the **The Name of Legends** Discord rank from watching the stream for **250** hours!");
                    break;
                case 30000:
                    var kingOfStream = guild.Roles.First(x => x.Name == "King of the Stream");
                    rolesToAdd.Add(kingOfStream);
                    await DiscordHelper.SendMessage(Config.DiscordRankUpAnnouncementChannelID, $"Congratulations <@{user.DiscordUserId}> on getting the **King of the Stream** Discord rank from watching the stream for **500** hours!");
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
