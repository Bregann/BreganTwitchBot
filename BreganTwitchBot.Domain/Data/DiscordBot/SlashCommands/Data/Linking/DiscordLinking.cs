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
                GetUsersResponse getUserID;

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

                using (var context = new DatabaseContext())
                {
                    var newUser = new Users
                    {
                        TwitchUserId = getUserID.Users[0].Id,
                        Username = twitchName,
                        InStream = true,
                        IsSub = false,
                        Points = 0,
                        IsSuperMod = false,
                        TotalMessages = 0,
                        DiscordUserId = 0,
                        LastSeenDate = DateTime.UtcNow,
                        GiftedSubsThisMonth = 0,
                        BitsDonatedThisMonth = 0,
                        MarblesWins = 0,
                        BossesDone = 0,
                        BossesPointsWon = 0,
                        TimeoutStrikes = 0,
                        WarnStrikes = 0,
                        DailyPoints = new Infrastructure.Database.Models.DailyPoints
                        {
                            CurrentStreak = 0,
                            DiscordDailyClaimed = false,
                            DiscordDailyStreak = 0,
                            DiscordDailyTotalClaims = 0,
                            HighestStreak = 0,
                            PointsClaimedThisStream = false,
                            PointsLastClaimed = DateTime.UtcNow,
                            TotalPointsClaimed = 0,
                            TotalTimesClaimed = 0
                        },
                        DiscordUserStats = new DiscordUserStats
                        {
                            DiscordLevel = 0,
                            DiscordLevelUpNotifsEnabled = true,
                            DiscordXp = 0,
                            PrestigeLevel = 0
                        },
                        UserGambleStats = new UserGambleStats
                        {
                            JackpotWins = 0,
                            PointsGambled = 0,
                            PointsLost = 0,
                            PointsWon = 0,
                            SmorcWins = 0,
                            Tier1Wins = 0,
                            Tier2Wins = 0,
                            Tier3Wins = 0,
                            TotalSpins = 0
                        },
                        Watchtime = new Infrastructure.Database.Models.Watchtime
                        {
                            Rank1Applied = false,
                            Rank2Applied = false,
                            Rank3Applied = false,
                            Rank4Applied = false,
                            Rank5Applied = false,
                            MinutesInStream = 0,
                            MinutesWatchedThisMonth = 0,
                            MinutesWatchedThisStream = 0,
                            MinutesWatchedThisWeek = 0
                        }
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

            using (var context = new DatabaseContext())
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