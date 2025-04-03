using BreganTwitchBot.Domain.DTOs.Twitch.Commands.TwitchBosses;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.TwitchBosses
{
    public class TwitchBossesDataService(ITwitchHelperService twitchHelperService) : ITwitchBossesDataService
    {
        private Dictionary<string, BossState> _bossState = [];

        public async Task StartBossFight(string broadcasterId, string broadcasterName)
        {
            // get the boss state
            var bossState = _bossState.GetValueOrDefault(broadcasterId);

            if (bossState == null)
            {
                Log.Warning($"[Twitch Bosses] No boss state found for {broadcasterName}");
                return;
            }

            //Check if enough people joined
            if (bossState.ViewersJoined.Count < 5)
            {
                Log.Information($"[Twitch Bosses] Not enough people joined the boss for broadcaster {broadcasterName} :(");
                await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, broadcasterName, "Not enough people joined the boss battle :( We need at least 5 to join!");
                return;
            }

            _bossState[broadcasterId].BossInProgress = true;

            await Task.Delay(5000);

            //Get the data needed
            //TODO: GET THE MODS
            var bossSuffixes = new List<string> { "The Terrible", "The Melvin", "The KEKW", "The Smelly", "The Gnoblin", "The Gnome", "The Troll", "The Handsome", "The Tree", "The Duck" };
            var elimiationReasons = new List<string> { "got AGS'd for a 73", "got snowballed into the void", "missed their MLG", "missplaced a block", "got stamped on", "had a tree fall onto their head", "got destroyed by lasers" };

            var rnd = new Random();

            //Make the boss name ting
            var rndBossName = rnd.Next(0, bossState.TwitchMods.Count);
            var rndBossTitle = rnd.Next(0, bossSuffixes.Count);

            var bossName = $"{bossState.TwitchMods[rndBossName]} {bossSuffixes[rndBossTitle]}";
            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, broadcasterName, $"A wild {bossName} has appeared! {bossState.ViewersJoined.Count} people have joined the fight! Lets gooooo PogChamp");

            //Start the fight - stage 1
            await Task.Delay(5000);
            var randomUserToDie = rnd.Next(0, bossState.ViewersJoined.Count);
            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, broadcasterName, $"FeelsBadMan {bossState.ViewersJoined[randomUserToDie].Username} has fallen due to the strength of {bossName} :( They are the first player to die");
            bossState.ViewersJoined.RemoveAt(randomUserToDie);

            //Stage 2
            await Task.Delay(7500);

            //Amount of users to elimiate
            var amountOfUsersToEliminate = bossState.ViewersJoined.Count / 4 + 1;

            //The method of elimiation guinea8S
            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, broadcasterName, $"The boss has done a special attack and {amountOfUsersToEliminate} people {elimiationReasons[rnd.Next(0, elimiationReasons.Count)]}! The end of the fight is near monkaS");
            for (int i = 0; i < amountOfUsersToEliminate; i++)
            {
                var rndRemoveUser = rnd.Next(0, bossState.ViewersJoined.Count);
                bossState.ViewersJoined.RemoveAt(rndRemoveUser);
            }

            //The final stage
            await Task.Delay(10000);
            var rndRandomUserKEKW = rnd.Next(0, bossState.ViewersJoined.Count);
            var randomUserToBeKEKWd = bossState.ViewersJoined[rndRandomUserKEKW];

            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, broadcasterName, $"The boss is on low health! {randomUserToBeKEKWd.Username} has done the final hit and destroyed the boss {bossName}!");
            await Task.Delay(5000);

            if (rnd.Next(0, 2) == 1)
            {
                await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, broadcasterName, $"But wait... {randomUserToBeKEKWd.Username} has been hit by a stray arrow and has died KEKW ! KEKW in the chat for {randomUserToBeKEKWd.Username}");
                bossState.ViewersJoined.Remove(randomUserToBeKEKWd);
            }

            //the loot
            var rndPointAmount = rnd.Next(5000, 25000);
            var amountToDistribute = (rndPointAmount * bossState.ViewersJoined.Count) + 7373;
            await Task.Delay(7000);

            foreach (var viewer in bossState.ViewersJoined)
            {
                await twitchHelperService.AddPointsToUser(broadcasterId, viewer.UserId, rndPointAmount, broadcasterName, viewer.Username);
            }

            var pointsName = await twitchHelperService.GetPointsName(broadcasterId, broadcasterName);
            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, broadcasterName, $"The boss has been defeated! {bossState.ViewersJoined.Count} people have survived the fight and have won {rndPointAmount:N0} {pointsName} each! {amountToDistribute:N0} {pointsName} have been distributed in total! PogChamp");

            //Reset the boss state
            _bossState[broadcasterId].ViewersJoined.Clear();
            _bossState[broadcasterId].TwitchMods.Clear();
            _bossState[broadcasterId].BossCountdownEnabled = false;
            _bossState[broadcasterId].BossInProgress = false;
        }
    }
}
