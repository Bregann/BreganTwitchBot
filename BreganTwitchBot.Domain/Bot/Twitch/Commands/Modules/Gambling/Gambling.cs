using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Domain.Bot.Twitch.Helpers;
using Serilog;
using TwitchLib.Client.Events;
using BreganTwitchBot.Domain.Data.TwitchBot.Stats;
using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;

namespace BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Gambling
{
    public class Gambling
    {
        private static DateTime _gamblingCooldown;

        public static async Task HandleGamblingCommand(OnChatCommandReceivedArgs command)
        {
            //Check if the stream is even online
            if (!AppConfig.StreamerLive)
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => The streamer is offline so no gambling!");
                return;
            }

            //Check the cooldown
            if (DateTime.UtcNow - TimeSpan.FromSeconds(5) <= _gamblingCooldown && !SuperMods.Supermods.IsUserSupermod(command.Command.ChatMessage.Username.ToLower()))
            {
                Log.Information("[Twitch Commands] !spin command handled successfully (cooldown)");
                return;
            }

            //check if they've done the command
            if (command.Command.ChatMessage.Message.ToLower() == "!spin" || command.Command.ChatMessage.Message.ToLower() == "!slots" || command.Command.ChatMessage.Message.ToLower() == "!gamble")
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => try using !spin <{AppConfig.PointsName}> or !spin all. Remember it is 100 points minimum!");
                return;
            }

            long points;
            //check if they're gambling them all or if they are gambling a set amount
            if (command.Command.ChatMessage.Message.ToLower() == "!spin all")
            {
                points = PointsHelper.GetUserPoints(command.Command.ChatMessage.Username.ToLower());
            }
            else
            {
                long.TryParse(command.Command.ArgumentsAsList[0], out points);
            }

            if (points == 0)
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => try using !spin <{AppConfig.PointsName}> or !spin all. Remember it is 100 points minimum!");
                return;
            }

            if (points < 100)
            {
                TwitchHelper.SendMessage("It's 100 points minimum!");
                return;
            }

            var userPoints = PointsHelper.GetUserPoints(command.Command.ChatMessage.Username.ToLower());

            if (userPoints < points)
            {
                TwitchHelper.SendMessage($"You don't have enough {AppConfig.PointsName}! You only have {userPoints}");
                return;
            }

            //once the checks are done we can gamble their points
            await SpinSlotMachine(command.Command.ChatMessage.Username.ToLower(), points);
            _gamblingCooldown = DateTime.UtcNow;
        }

        public static void HandleSpinStatsCommand(string username)
        {
            using (var context = new DatabaseContext())
            {
                var spinStats = context.SlotMachine.Where(x => x.StreamName == AppConfig.BroadcasterName).First();
                TwitchHelper.SendMessage($"@{username} => Kappa wins: {spinStats.Tier1Wins:N0} | 4Head Wins: {spinStats.Tier2Wins:N0} | LUL Wins: {spinStats.Tier3Wins:N0} | TriHard wins: {spinStats.JackpotAmount:N0} | SMOrc Wins: {spinStats.SmorcWins:N0} | Total Spins: {spinStats.TotalSpins:N0}");
            }
        }

        public static void HandleJackpotCommand(string username)
        {
            var jackpotAmount = GetJackpotAmount();
            TwitchHelper.SendMessage($"@{username} => The current point yackpot is {jackpotAmount:N0}");
        }

        private static async Task SpinSlotMachine(string username, long pointsGambled)
        {
            StreamStatsService.UpdateStreamStat(pointsGambled, StatTypes.PointsGambled);
            PointsHelper.RemoveUserPoints(username, pointsGambled);

            var random = new Random();
            var emoteList = new List<string>();

            for (var i = 0; i < 3; i++)
            {
                var number = random.Next(1, 12);

                if (number <= 4) //1,2,3,4
                {
                    emoteList.Add("Kappa");
                }
                else if (number > 4 && 7 >= number) //5,6,7
                {
                    emoteList.Add("4Head");
                }
                else if (number > 7 && 9 >= number) //8,9
                {
                    emoteList.Add("LUL");
                }
                else if (number == 10)
                {
                    emoteList.Add("TriHard");
                }
                else if (number == 11)
                {
                    emoteList.Add("SMOrc");
                }
            }

            switch (emoteList[0])
            {
                case "Kappa" when emoteList[1] == "Kappa" && emoteList[2] == "Kappa":
                    PointsHelper.AddUserPoints(username, pointsGambled * 10);
                    StreamStatsService.UpdateStreamStat(pointsGambled * 10, StatTypes.PointsWon);
                    StreamStatsService.UpdateStreamStat(1, StatTypes.KappaWins);
                    AddWinToSlotMachineAndUser(username, "tier1Wins", pointsGambled, pointsGambled * 10);
                    TwitchHelper.SendMessage($"@{username} => You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]}. You have won {pointsGambled * 10:N0} {AppConfig.PointsName}!");
                    Log.Information($"[Slot Machine] {username} got a Kappa win!");
                    break;

                case "4Head" when emoteList[1] == "4Head" && emoteList[2] == "4Head":
                    PointsHelper.AddUserPoints(username, pointsGambled * 20);
                    StreamStatsService.UpdateStreamStat(pointsGambled * 20, StatTypes.PointsWon);
                    StreamStatsService.UpdateStreamStat(1, StatTypes.ForeheadWins);
                    AddWinToSlotMachineAndUser(username, "tier2Wins", pointsGambled, pointsGambled * 20);
                    TwitchHelper.SendMessage($"@{username} => You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]}. You have won {pointsGambled * 20:N0} {AppConfig.PointsName}!");
                    Log.Information($"[Slot Machine] {username} got a 4Head win!");
                    break;

                case "LUL" when emoteList[1] == "LUL" && emoteList[2] == "LUL":
                    PointsHelper.AddUserPoints(username, pointsGambled * 40);
                    StreamStatsService.UpdateStreamStat(pointsGambled * 40, StatTypes.PointsWon);
                    StreamStatsService.UpdateStreamStat(1, StatTypes.LULWins);
                    AddWinToSlotMachineAndUser(username, "tier3Wins", pointsGambled, pointsGambled * 40);
                    TwitchHelper.SendMessage($"@{username} => You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]}. You have won {pointsGambled * 40:N0} {AppConfig.PointsName}!");
                    Log.Information($"[Slot Machine] {username} got a LUL win!");
                    break;

                case "TriHard" when emoteList[1] == "TriHard" && emoteList[2] == "TriHard":
                    {
                        var jackpotAmount = GetJackpotAmount();
                        PointsHelper.AddUserPoints(username, jackpotAmount);
                        AddWinToSlotMachineAndUser(username, "jackpotWins", pointsGambled, jackpotAmount);
                        StreamStatsService.UpdateStreamStat(jackpotAmount, StatTypes.PointsWon);
                        StreamStatsService.UpdateStreamStat(1, StatTypes.jackpotWins);

                        using (var context = new DatabaseContext())
                        {
                            context.SlotMachine.Where(x => x.StreamName == AppConfig.BroadcasterName).First().JackpotAmount = 50000;
                            context.SaveChanges();
                        }

                        TwitchHelper.SendMessage($"@{username} => You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]}. DING DING DING JACKPOT!!! You have won {jackpotAmount:N0} {AppConfig.PointsName}!");
                        Log.Information($"[Slot Machine] {username} won the jackpot!");
                        break;
                    }
                case "SMOrc" when emoteList[1] == "SMOrc" && emoteList[2] == "SMOrc":
                    {
                        PointsHelper.AddUserPoints(username, 1);
                        AddWinToSlotMachineAndUser(username, "smorcWins", pointsGambled, 1);
                        StreamStatsService.UpdateStreamStat(1, StatTypes.SMOrcWins);
                        TwitchHelper.SendMessage($"@{username} => You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]}. DING DING DING BUDGET JACKPOT!!! You have won the grand total of 1 WHOLE {AppConfig.PointsName} PogChamp PogChamp PogChamp PogChamp PogChamp PogChamp PogChamp PogChamp PogChamp PogChamp PogChamp PogChamp PogChamp PogChamp ");
                        Log.Information($"[Slot Machine] {username} won the budget SMOrc jackpot!");
                        break;
                    }

                default:
                    TwitchHelper.SendMessage($"@{username} => You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]}. No win :(");
                    var jackpotAmountCheck = GetJackpotAmount();
                    AddLossToJackpotAndUser(username, pointsGambled, pointsGambled / 100 * 60);
                    StreamStatsService.UpdateStreamStat(1, StatTypes.PointsLost);
                    break;
            }

            StreamStatsService.UpdateStreamStat(1, StatTypes.TotalSpins);
        }

        private static void AddWinToSlotMachineAndUser(string username, string winType, long pointsGambled, long pointsWon)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == username).First();
                user.PointsGambled += pointsGambled;
                user.PointsWon += pointsWon;
                user.TotalSpins++;

                var spinStats = context.SlotMachine.Where(x => x.StreamName == AppConfig.BroadcasterName).First();
                spinStats.TotalSpins++;

                switch (winType)
                {
                    case "tier1Wins":
                        user.Tier1Wins++;
                        spinStats.Tier1Wins++;
                        break;
                    case "tier2Wins":
                        user.Tier2Wins++;
                        spinStats.Tier2Wins++;
                        break;
                    case "tier3Wins":
                        user.Tier3Wins++;
                        spinStats.Tier3Wins++;
                        break;
                    case "jackpotWins":
                        user.JackpotWins++;
                        spinStats.JackpotWins++;
                        break;
                    case "smorcWins":
                        user.SmorcWins++;
                        spinStats.SmorcWins++;
                        break;
                }

                context.SaveChanges();
            }
        }

        private static void AddLossToJackpotAndUser(string username, long pointsGambled, long pointsToJackpot)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == username).First();
                user.PointsGambled += pointsGambled;
                user.PointsLost += pointsGambled;
                user.TotalSpins++;

                var spinStats = context.SlotMachine.Where(x => x.StreamName == AppConfig.BroadcasterName).First();
                spinStats.TotalSpins++;
                spinStats.JackpotAmount += pointsToJackpot;

                context.SaveChanges();
            }
        }

        private static long GetJackpotAmount()
        {
            using (var context = new DatabaseContext())
            {
                return context.SlotMachine.Where(x => x.StreamName == AppConfig.BroadcasterName).First().JackpotAmount;
            }
        }
    }
}