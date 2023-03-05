using BreganTwitchBot.Core.Twitch.Commands.Modules.EightBall;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.DailyPoints;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.DiceRoll;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Leaderboards;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Leaderboards.Enums;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Linking;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Marbles;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Subathon;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.TwitchBosses;
using Serilog;
using TwitchLib.Client.Events;
using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Domain.Data.TwitchBot;
using BreganTwitchBot.Domain.Data.TwitchBot.WordBlacklist;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.DadJoke;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.Uptime;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.CustomCommands;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.StreamInfo;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.Supermods;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.FollowAge;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.Points;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.Hours;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.WordBlacklist;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.Gambling;

namespace BreganTwitchBot.Domain.Bot.Twitch.Commands
{
    public class CommandHandlerOld
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
            command.Command.Arg
            //The main commands
            switch (command.Command.CommandText.ToLower())
            {
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
                case "dumblethon":
                    Subathon.HandleSubathonCommand(command.Command.ChatMessage.Username);
                    break;
            }

            Log.Information($"[Twitch Commands] !{command.Command.CommandText.ToLower()} completed");
        }
    }
}