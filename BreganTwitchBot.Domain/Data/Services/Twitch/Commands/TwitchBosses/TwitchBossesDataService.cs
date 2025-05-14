using BreganTwitchBot.Domain.DTOs.Twitch.Commands.TwitchBosses;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Hangfire;
using Serilog;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.TwitchBosses
{
    public class TwitchBossesDataService(ITwitchHelperService twitchHelperService, IBackgroundJobClient backgroundJobClient) : ITwitchBossesDataService
    {
        internal Dictionary<string, BossState> _bossState = [];

        public async Task StartBossFight(string broadcasterId, string broadcasterName)
        {
            // get the boss state
            var bossState = _bossState.GetValueOrDefault(broadcasterId);

            if (bossState == null)
            {
                Log.Warning($"[Twitch Bosses] No boss state found for {broadcasterName}");
                return;
            }

            //Check if the boss is in progress
            if (bossState.BossInProgress)
            {
                Log.Information($"[Twitch Bosses] Boss is already in progress for {broadcasterName}");
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
            //TODO: GET THE MODS, temp atm
            bossState.TwitchMods.Add(broadcasterName);
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
                await twitchHelperService.AddPointsToUser(broadcasterId, viewer.UserId, amountToDistribute, broadcasterName, viewer.Username);
            }

            var pointsName = await twitchHelperService.GetPointsName(broadcasterId, broadcasterName);
            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, broadcasterName, $"The boss has been defeated! {bossState.ViewersJoined.Count} people have survived the fight and have won {amountToDistribute:N0} {pointsName} each! {amountToDistribute * bossState.ViewersJoined.Count:N0} {pointsName} have been distributed in total! PogChamp");

            //Reset the boss state
            _bossState[broadcasterId].ViewersJoined.Clear();
            _bossState[broadcasterId].TwitchMods.Clear();
            _bossState[broadcasterId].BossCountdownEnabled = false;
            _bossState[broadcasterId].BossInProgress = false;
        }

        public string HandleBossCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var boss = _bossState.GetValueOrDefault(msgParams.BroadcasterChannelId);

            if (boss == null)
            {
                Log.Warning($"[Twitch Bosses] No boss state found for {msgParams.BroadcasterChannelName}");
                return "There is no boss active!";
            }

            if (!boss.BossCountdownEnabled || boss.BossInProgress)
            {
                Log.Warning($"[Twitch Bosses] Boss countdown is not enabled or boss is in progress for {msgParams.BroadcasterChannelName}");
                return "The boss countdown is not enabled or the boss is already in progress!";
            }

            if (boss.ViewersJoined.Any(x => x.UserId == msgParams.ChatterChannelId))
            {
                Log.Information($"[Twitch Bosses] {msgParams.ChatterChannelName} is already in the boss fight for {msgParams.BroadcasterChannelName}");
                return $"You already joined the boss fight! Daft user!";
            }

            _bossState[msgParams.BroadcasterChannelId].ViewersJoined.Add((msgParams.ChatterChannelName, msgParams.ChatterChannelId));
            Log.Information($"[Twitch Bosses] {msgParams.ChatterChannelName} has joined the boss fight for {msgParams.BroadcasterChannelName}");
            return $"{msgParams.ChatterChannelName} has joined the boss fight! {_bossState[msgParams.BroadcasterChannelId].ViewersJoined.Count} people have joined the fight! You can join to by doing !boss";
        }

        public async Task<bool> StartBossFightCountdown(string broadcasterId, string broadcasterName, ChannelChatMessageReceivedParams? msgParams = null)
        {
            // check if the msgParams is null, if not, check if the user is a supermod or broadcaster
            if (msgParams != null)
            {
                var isSuperMod = await twitchHelperService.IsUserSuperModInChannel(broadcasterId, msgParams.ChatterChannelId);
                var isBroadcaster = msgParams.IsBroadcaster;
                if (!isSuperMod && !isBroadcaster)
                {
                    Log.Information($"[Twitch Bosses] {msgParams.ChatterChannelName} is not a supermod or broadcaster for {broadcasterName}");
                    return false;
                }
            }

            var boss = _bossState.GetValueOrDefault(broadcasterId);

            if (boss == null)
            {
                Log.Information($"[Twitch Bosses] No boss state found for {broadcasterName}. Setting up new state");
                _bossState[broadcasterId] = new BossState
                {
                    ViewersJoined = [],
                    TwitchMods = [],
                    BossCountdownEnabled = true,
                    BossInProgress = false
                };

                Log.Information($"[Twitch Bosses] Boss state created for {broadcasterName} and boss started");
                backgroundJobClient.Schedule(() => twitchHelperService.SendAnnouncementMessageToChannel(broadcasterId, broadcasterName, "This is your 1 minute warning! You only have 1 minute left to join the boss fight with !boss"), TimeSpan.FromMinutes(1));
                backgroundJobClient.Schedule<ITwitchBossesDataService>(
                    x => x.StartBossFight(broadcasterId, broadcasterName),
                    TimeSpan.FromMinutes(2));

                await twitchHelperService.SendAnnouncementMessageToChannel(broadcasterId, broadcasterName, "The boss countdown has started! You can join the fight by doing !boss");
                return true;
            }

            if (boss.BossCountdownEnabled || boss.BossInProgress)
            {
                Log.Warning($"[Twitch Bosses] Boss countdown is already enabled or boss is in progress for {broadcasterName}");
                return false;
            }

            boss.BossCountdownEnabled = true;
            Log.Information($"[Twitch Bosses] Boss countdown started for {broadcasterName}");
            backgroundJobClient.Schedule(() => twitchHelperService.SendAnnouncementMessageToChannel(broadcasterId, broadcasterName, "This is your 1 minute warning! You only have 1 minute left to join the boss fight with !boss"), TimeSpan.FromMinutes(1));
            backgroundJobClient.Schedule<ITwitchBossesDataService>(
                x => x.StartBossFight(broadcasterId, broadcasterName),
                TimeSpan.FromMinutes(2));

            await twitchHelperService.SendAnnouncementMessageToChannel(broadcasterId, broadcasterName, "The boss countdown has started! You can join the fight by doing !boss");
            return true;
        }
    }
}
