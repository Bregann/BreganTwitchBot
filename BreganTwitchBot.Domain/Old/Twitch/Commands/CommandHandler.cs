using BreganTwitchBot.Core.Twitch.Commands.Modules.EightBall;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.CustomCommands;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.DadJoke;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.DailyPoints;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.DiceRoll;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.FollowAge;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Gambling;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Hours;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Leaderboards;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Leaderboards.Enums;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Linking;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Marbles;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Points;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.StreamInfo;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Subathon;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.SuperMods;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.TwitchBosses;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Uptime;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.WordBlacklist;
using Serilog;
using TwitchLib.Client.Events;
using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Domain.Data.TwitchBot;
using BreganTwitchBot.Domain.Data.TwitchBot.WordBlacklist;

namespace BreganTwitchBot.Domain.Bot.Twitch.Commands
{
    public class CommandHandler
    {
        private static List<string> _customCommands;

        public static void HandleCustomCommand(OnMessageReceivedArgs msg)
        {
            //Commands beginning with ! are already handled below
            if (msg.ChatMessage.Message.StartsWith("!"))
            {
                return;
            }

            //If its null then load the command list
            if (_customCommands == null)
            {
                using (var context = new DatabaseContext())
                {
                    _customCommands = context.Commands.Select(x => x.CommandName).ToList();
                }
            }

            var commandName = msg.ChatMessage.Message.Split(" ")[0].ToLower();
            SendCustomCommandAndUpdateUsage(commandName, msg.ChatMessage.UserId, msg.ChatMessage.Username);
        }

        public static async Task HandleCommand(OnChatCommandReceivedArgs command)
        {
            StreamStatsService.UpdateStreamStat(1, StatTypes.CommandsSent);
            Log.Information($"[Twitch Commands] !{command.Command.CommandText.ToLower()} receieved from {command.Command.ChatMessage.Username}");

            //If its null then load the command list
            if (_customCommands == null)
            {
                using (var context = new DatabaseContext())
                {
                    _customCommands = context.Commands.Select(x => x.CommandName).ToList();
                }
            }

            //Handle custom command
            var commandName = command.Command.ChatMessage.Message.Split(" ")[0].ToLower();
            SendCustomCommandAndUpdateUsage(commandName, command.Command.ChatMessage.UserId, command.Command.ChatMessage.Username);

            //The main commands
            switch (command.Command.CommandText.ToLower())
            {
                case "dadjoke":
                    await DadJokes.HandleDadJokeCommand(command.Command.ChatMessage.Username);
                    break;

                case "8ball":
                    EightBall.Handle8BallCommand(command.Command.ChatMessage.Username);
                    break;

                case "commands":
                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => Bot commands + much more can be found at https://bot.bregan.me/ ! :)");
                    break;

                case "uptime":
                    await Uptime.HandleUptimeCommand(command.Command.ChatMessage.Username, command.Command.ChatMessage.UserId);
                    break;

                case "botuptime":
                    Uptime.HandleBotUptimeCommand(command.Command.ChatMessage.Username);
                    break;

                case "shoutout" when command.Command.ChatMessage.IsModerator:
                case "shoutout" when command.Command.ChatMessage.IsBroadcaster:
                case "so" when command.Command.ChatMessage.IsModerator:
                case "so" when command.Command.ChatMessage.IsBroadcaster:
                    if (command.Command.ArgumentsAsList.Count == 0)
                    {
                        return;
                    }

                    TwitchHelper.SendMessage($"Hey go check out {command.Command.ArgumentsAsList[0].Replace("@", "")} at twitch.tv/{command.Command.ArgumentsAsList[0].Replace("@", "").Trim()} for some great content!");
                    break;

                case "addcmd" when command.Command.ChatMessage.IsModerator:
                case "addcmd" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "cmdadd" when command.Command.ChatMessage.IsModerator:
                case "cmdadd" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                    CustomCommands.HandleAddCommand(command);
                    break;

                case "delcmd" when command.Command.ChatMessage.IsModerator:
                case "delcmd" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "cmddel" when command.Command.ChatMessage.IsModerator:
                case "cmddel" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                    CustomCommands.HandleRemoveCommand(command);
                    break;

                case "editcmd" when command.Command.ChatMessage.IsModerator:
                case "editcmd" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "cmdedit" when command.Command.ChatMessage.IsModerator:
                case "cmdedit" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                    CustomCommands.HandleEditCommandCommand(command);
                    break;

                case "title":
                    await Title.HandleTitleCommand(command);
                    break;

                case "game":
                    await Game.HandleGameCommand(command);
                    break;

                case "subs":
                case "subcount":
                    await Subs.HandleSubsCommand(command.Command.ChatMessage.Username);
                    break;

                case "followage":
                case "howlong":
                    await FollowAge.HandleFollowageCommand(command);
                    break;

                case "followsince":
                    await FollowAge.HandleFollowSinceCommand(command);
                    break;

                case "addsupermod" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                    await Supermods.HandleAddSupermodCommand(command);
                    break;

                case "removesupermod" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                    await Supermods.HandleRemoveSupermodCommand(command);
                    break;

                case "points":
                case "pooants":
                case "coins":
                    Points.HandlePointsCommand(command);
                    break;

                case "hours":
                case "houres":
                case "hrs":
                    Hours.HandleHoursCommand(command);
                    break;

                case "streamhours":
                case "streamhrs":
                    Hours.HandleStreamHoursCommand(command);
                    break;

                case "weeklyhours":
                case "weeklyhrs":
                    Hours.HandleWeeklyHoursCommand(command);
                    break;

                case "monthlyhours":
                case "monthlyhrs":
                    Hours.HandleMonthlyHoursCommand(command);
                    break;

                case "addbadword" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "addtempword" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "addwarningword" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "addstrikeword" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                    WordBlacklist.HandleAddWordCommand(command);
                    break;

                case "removebadword" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "removetempword" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "removewarningword" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "removestrikeword" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                    WordBlacklist.HandleRemoveWordCommand(command);
                    break;

                case "removebadword" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "removetempword" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "removewarningword" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                    WordBlacklist.HandleRemoveWordCommand(command);
                    break;

                case "aitoggle" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "toggleai" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                    WordBlacklist.HandleToggleAiCommand(command);
                    break;

                case "spin":
                case "slots":
                case "gamble":
                    await Gambling.HandleGamblingCommand(command);
                    break;

                case "spinstats":
                    Gambling.HandleSpinStatsCommand(command.Command.ChatMessage.Username);
                    break;

                default:
                    break;

                case "daily":
                case "dailt":
                case "claim":
                case "dailypoints":
                case "collect":
                    DailyPoints.HandleClaimPointsCommand(command);
                    break;

                case "streak":
                case "steak":
                    DailyPoints.HandleStreakCommand(command);
                    break;

                case "addpoints" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                    Points.HandleAddPointsCommand(command);
                    break;

                case "pointslb":
                    Leaderboards.HandleLeaderboardCommand(command.Command.ChatMessage.Username, LeaderboardType.Points);
                    break;

                case "hrslb":
                case "hourslb":
                    Leaderboards.HandleLeaderboardCommand(command.Command.ChatMessage.Username, LeaderboardType.AllTimeHours);
                    break; ;
                case "pointswonlb":
                    Leaderboards.HandleLeaderboardCommand(command.Command.ChatMessage.Username, LeaderboardType.PointsWon);
                    break;

                case "pointslostlb":
                    Leaderboards.HandleLeaderboardCommand(command.Command.ChatMessage.Username, LeaderboardType.PointsLost);
                    break;

                case "pointsgambledlb":
                    Leaderboards.HandleLeaderboardCommand(command.Command.ChatMessage.Username, LeaderboardType.PointsGambled);
                    break;

                case "totalspinslb":
                    Leaderboards.HandleLeaderboardCommand(command.Command.ChatMessage.Username, LeaderboardType.TotalSpins);
                    break;

                case "dailystreaklb":
                case "streaklb":
                    Leaderboards.HandleLeaderboardCommand(command.Command.ChatMessage.Username, LeaderboardType.CurrentStreak);
                    break;

                case "higheststreaklb":
                    Leaderboards.HandleLeaderboardCommand(command.Command.ChatMessage.Username, LeaderboardType.HighestStreak);
                    break;

                case "totalclaimslb":
                    Leaderboards.HandleLeaderboardCommand(command.Command.ChatMessage.Username, LeaderboardType.TotalTimesClaimed);
                    break;

                case "streamhrslb":
                case "streamhourslb":
                    Leaderboards.HandleLeaderboardCommand(command.Command.ChatMessage.Username, LeaderboardType.StreamHours);
                    break;

                case "weeklyhrslb":
                case "weeklyhourslb":
                    Leaderboards.HandleLeaderboardCommand(command.Command.ChatMessage.Username, LeaderboardType.WeeklyHours);
                    break;

                case "monthlyhrslb":
                case "monthlyhourslb":
                    Leaderboards.HandleLeaderboardCommand(command.Command.ChatMessage.Username, LeaderboardType.MonthlyHours);
                    break;

                case "marbleswinslb":
                case "marblewinslb":
                case "marbleslb":
                case "marbolslb":
                    Leaderboards.HandleLeaderboardCommand(command.Command.ChatMessage.Username, LeaderboardType.MarblesRacesWon);
                    break;

                case "jackpot":
                case "yackpot":
                    Gambling.HandleJackpotCommand(command.Command.ChatMessage.Username);
                    break;

                case "boss":
                    TwitchBosses.AddUser(command.Command.ChatMessage.Username.ToLower());
                    break;

                case "startboss" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                    await TwitchBosses.StartBossFightCountdown();
                    break;

                case "link":
                    await Linking.HandleLinkingCommand(command);
                    break;

                case "addmarbleswin" when Supermods.IsUserSupermod(command.Command.ChatMessage.UserId):
                    await Marbles.HandleAddMarblesWinCommand(command);
                    break;

                case "subathon":
                    Subathon.HandleSubathonCommand(command.Command.ChatMessage.Username);
                    break;
            }

            Log.Information($"[Twitch Commands] !{command.Command.CommandText.ToLower()} completed");
        }
    }
}