﻿using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Gambling
{
    public class GamblingDataService(ITwitchHelperService twitchHelperService, AppDbContext context) : IGamblingDataService
    {
        public async Task<string> HandleSpinCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var streamerLive = await twitchHelperService.IsBroadcasterLive(msgParams.BroadcasterChannelName);

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
                return await SpinSlotMachine(userPoints, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId, msgParams.ChatterChannelName, msgParams.BroadcasterChannelName, pointsName);
            }

            if (!long.TryParse(msgParams.MessageParts[1], out long pointsGambled))
            {
                throw new InvalidCommandException("You need to specify a valid amount to gamble! Usage: !spin <amount> or if you are an absolute mad lad do !spin all");
            }

            if (pointsGambled > userPoints)
            {
                throw new InvalidCommandException($"You do not have enough {pointsName} to gamble that amount! You have {userPoints:N0} {pointsName}");
            }

            return await SpinSlotMachine(pointsGambled, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId, msgParams.ChatterChannelName, msgParams.BroadcasterChannelName, pointsName);
        }

        private async Task<string> SpinSlotMachine(long pointsGambled, string twitchUserId, string broadcasterId, string twitchUsername, string broadcasterUsername, string pointsName)
        {
            await twitchHelperService.RemovePointsFromUser(broadcasterId, twitchUserId, pointsGambled, broadcasterUsername, twitchUsername);

            var random = new Random();
            var emoteList = new List<string>();

            for (var i = 0; i < 3; i++)
            {
                var number = random.Next(1, 13); // 1 to 12

                if (number <= 4) // 33.33% (1/27)
                {
                    emoteList.Add("Kappa");
                }
                else if (number <= 7) // 25.00% (1/64)
                {
                    emoteList.Add("4Head");
                }
                else if (number <= 10) // 25.00% (1/64)
                {
                    emoteList.Add("📖");
                }
                else if (number <= 12)  // 16.67% (1/216)
                {
                    emoteList.Add("LUL");
                }
                else if (number == 13) // 8.33% (1/1728)
                {
                    emoteList.Add("TriHard");
                }
                else // 8.33% (1/1728)
                {
                    emoteList.Add("SMOrc");
                }
            }

            var slotMachineDbData = await context.TwitchSlotMachineStats.FirstAsync(x => x.Channel.BroadcasterTwitchChannelId == broadcasterId);

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

            // Check if all emotes are the same
            var firstEmote = emoteList[0];

            if (emoteList.All(x => x == firstEmote)) // Check if all items in the list match the first emote
            {
                if (winMultipliers.ContainsKey(firstEmote))
                {
                    // Special case for TriHard and SMOrc where custom messages are needed
                    if (firstEmote == "TriHard")
                    {
                        winAmount = slotMachineDbData.JackpotAmount;
                        slotMachineDbData.JackpotAmount = 20000;
                        slotMachineDbData.JackpotWins++;

                        spinResultMessage = $"You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]} . DING DING DING JACKPOT!!! You have won {winAmount:N0} {pointsName}!";
                        Log.Information($"{twitchUsername} got a TriHard win in channel {broadcasterUsername}");
                    }
                    else if (firstEmote == "SMOrc")
                    {
                        winAmount = 1;
                        slotMachineDbData.SmorcWins++;

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
                                break;
                            case "4Head":
                                slotMachineDbData.Tier2Wins++;
                                break;
                            case "📖":
                                slotMachineDbData.BookWins++;
                                break;
                            case "LUL":
                                slotMachineDbData.Tier3Wins++;
                                break;
                        }

                        // Gotta have some fun with the book emoji
                        if (firstEmote == "📖")
                        {
                            spinResultMessage = $"BOOK BOOK BOOK 📖📖📖 You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]} . You have won {winAmount} {pointsName}! 📖📖📖";
                        }
                    }
                }
            }
            else
            {
                Log.Information($"{twitchUsername} got a loss in channel {broadcasterUsername}");
                spinResultMessage = $"You have spun {emoteList[0]} | {emoteList[1]} | {emoteList[2]} . You are not lucky :( Have this to increase your luck! 🍀 🌳";
            }

            if (winAmount > 0)
            {
                await twitchHelperService.AddPointsToUser(broadcasterId, twitchUserId, winAmount, broadcasterUsername, twitchUsername);
            }
            else
            {
                // add 20% of the points gambled to the jackpot
                slotMachineDbData.JackpotAmount = slotMachineDbData.JackpotAmount + (long)(pointsGambled * 0.2);
            }

            slotMachineDbData.TotalSpins++;
            await context.SaveChangesAsync();

            return spinResultMessage;
        }
    }
}
