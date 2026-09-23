using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Gambling
{
    public class GamblingDataService(ITwitchHelperService twitchHelperService, AppDbContext context) : IGamblingDataService
    {
        public async Task<string> HandleSpinCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var streamerLive = await twitchHelperService.IsBroadcasterLive(msgParams.BroadcasterChannelId);

            if (!streamerLive)
            {
                return "The streamer is offline so no gambling! No book book book for you";
            }

            if (msgParams.MessageParts.Length == 1)
            {
                throw new InvalidCommandException("You need to specify an amount to gamble! Usage: !spin <amount> or if you are an absolute mad lad do !spin all");
            }

            var userPoints = await twitchHelperService.GetPointsForUser(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId, msgParams.BroadcasterChannelName, msgParams.ChatterChannelName);
            var pointsName = await twitchHelperService.GetPointsName(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            if (msgParams.MessageParts[1].ToLower() == "all")
            {
                if (userPoints < 100)
                {
                    throw new InvalidCommandException($"You need to have at least 100 {pointsName} to gamble all! Usage: !spin <amount> or if you are an absolute mad lad do !spin all");
                }

                return await SpinSlotMachine(userPoints, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId, msgParams.ChatterChannelName, msgParams.BroadcasterChannelName, pointsName);
            }

            if (!long.TryParse(msgParams.MessageParts[1], out long pointsGambled))
            {
                throw new InvalidCommandException("You need to specify a valid amount to gamble! Usage: !spin <amount> or if you are an absolute mad lad do !spin all");
            }

            if (pointsGambled < 100)
            {
                throw new InvalidCommandException($"You have to gamble at least 100 {pointsName}! Usage: !spin <amount> or if you are an absolute mad lad do !spin all");
            }

            if (pointsGambled > userPoints)
            {
                throw new InvalidCommandException($"You do not have enough {pointsName} to gamble that amount! You have {userPoints:N0} {pointsName}");
            }

            return await SpinSlotMachine(pointsGambled, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId, msgParams.ChatterChannelName, msgParams.BroadcasterChannelName, pointsName);
        }

        public async Task<string> GetJackpotAmount(ChannelChatMessageReceivedParams msgParams)
        {
            var slotMachineDbData = await context.TwitchSlotMachineStats.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId);
            var pointsName = await twitchHelperService.GetPointsName(msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            return $"The current jackpot is {slotMachineDbData.JackpotAmount:N0} {pointsName}";
        }

        /// <summary>
        /// Each reel lands on one of these many positions. It used to roll 1-12, so the
        /// TriHard (13) and SMOrc (14) positions could never come up and nobody could win
        /// either jackpot.
        /// </summary>
        public const int ReelPositions = 14;

        /// <summary>
        /// The emote on a reel position. The odds are per reel, then for all three matching.
        /// </summary>
        public static string GetReelEmote(int position)
        {
            return position switch
            {
                <= 4 => "Kappa",    // 4/14, 1 in 43 to win
                <= 7 => "4Head",    // 3/14, 1 in 102
                <= 10 => "📖",      // 3/14, 1 in 102
                <= 12 => "LUL",     // 2/14, 1 in 343
                13 => "TriHard",    // 1/14, 1 in 2,744 - the jackpot
                _ => "SMOrc"        // 1/14, 1 in 2,744 - the budget jackpot
            };
        }

        private async Task<string> SpinSlotMachine(long pointsGambled, string twitchUserId, string broadcasterId, string twitchUsername, string broadcasterUsername, string pointsName)
        {
            await twitchHelperService.RemovePointsFromUser(broadcasterId, twitchUserId, pointsGambled, broadcasterUsername, twitchUsername);

            var random = new Random();
            var emoteList = new List<string>();

            for (var i = 0; i < 3; i++)
            {
                emoteList.Add(GetReelEmote(random.Next(1, ReelPositions + 1)));
            }

            var slotMachineDbData = await context.TwitchSlotMachineStats.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId);
            var userSlotMachineDbData = await context.ChannelUserGambleStats.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId && x.ChannelUser.TwitchUserId == twitchUserId);

            var winMultipliers = new Dictionary<string, long>
            {
                { "Kappa", 10 },
                { "4Head", 20 },
                { "📖", 20 },
                { "LUL", 40 },
                { "TriHard", slotMachineDbData.JackpotAmount }, // Special jackpot for TriHard
                { "SMOrc", 1 } // Budget jackpot
            };

            long winAmount = 0;
            var spinResultMessage = $"You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]} . You have won {winAmount} {pointsName}!";

            var firstEmote = emoteList[0];

            // Check if all items in the list match the first emote
            if (emoteList.All(x => x == firstEmote))
            {
                if (winMultipliers.ContainsKey(firstEmote))
                {
                    // Special case for TriHard and SMOrc where custom messages are needed
                    if (firstEmote == "TriHard")
                    {
                        winAmount = slotMachineDbData.JackpotAmount;
                        slotMachineDbData.JackpotAmount = 20000;
                        slotMachineDbData.JackpotWins++;
                        userSlotMachineDbData.JackpotWins++;

                        spinResultMessage = $"You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]} . DING DING DING JACKPOT!!! You have won {winAmount:N0} {pointsName}!";
                        Log.Information($"{twitchUsername} got a TriHard win in channel {broadcasterUsername}");
                    }
                    else if (firstEmote == "SMOrc")
                    {
                        winAmount = 1;
                        slotMachineDbData.SmorcWins++;
                        userSlotMachineDbData.SmorcWins++;

                        spinResultMessage = $"You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]} . DING DING DING BUDGET JACKPOT!!! You have won the grand total of 1 WHOLE {pointsName} PogChamp PogChamp PogChamp";
                        Log.Information($"{twitchUsername} got a SMOrc win in channel {broadcasterUsername}");
                    }
                    else
                    {
                        winAmount = pointsGambled * winMultipliers[firstEmote];
                        Log.Information($"{twitchUsername} got a {firstEmote} win in channel {broadcasterUsername}");

                        switch (firstEmote)
                        {
                            case "Kappa":
                                slotMachineDbData.Tier1Wins++;
                                userSlotMachineDbData.Tier1Wins++;
                                break;
                            case "4Head":
                                slotMachineDbData.Tier2Wins++;
                                userSlotMachineDbData.Tier2Wins++;
                                break;
                            case "📖":
                                slotMachineDbData.BookWins++;
                                userSlotMachineDbData.BookWins++;
                                break;
                            case "LUL":
                                slotMachineDbData.Tier3Wins++;
                                userSlotMachineDbData.Tier3Wins++;
                                break;
                        }

                        // the message has to be built after winAmount is set - it used to keep the
                        // "won 0" message made before the spin. The winnings themselves are paid
                        // once, below, along with every other win
                        spinResultMessage = $"You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]} . You have won {winAmount:N0} {pointsName}!";

                        // Gotta have some fun with the book emoji
                        if (firstEmote == "📖")
                        {
                            spinResultMessage = $"BOOK BOOK BOOK 📖📖📖 You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]} . You have won {winAmount:N0} {pointsName}! 📖📖📖";
                        }
                    }
                }
            }
            else
            {
                Log.Information($"{twitchUsername} got a loss in channel {broadcasterUsername}");
                spinResultMessage = $"You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]} . You are not lucky :( Have this to increase your luck! 🍀 🌳";
                userSlotMachineDbData.PointsLost += pointsGambled;
            }

            if (winAmount > 0)
            {
                await twitchHelperService.AddPointsToUser(broadcasterId, twitchUserId, winAmount, broadcasterUsername, twitchUsername);
                userSlotMachineDbData.PointsWon += winAmount;
            }
            else
            {
                // add 20% of the points gambled to the jackpot
                slotMachineDbData.JackpotAmount = slotMachineDbData.JackpotAmount + (long)(pointsGambled * 0.2);
            }

            slotMachineDbData.TotalSpins++;
            userSlotMachineDbData.TotalSpins++;
            userSlotMachineDbData.PointsGambled += pointsGambled;
            await context.SaveChangesAsync();

            return spinResultMessage;
        }
    }
}
