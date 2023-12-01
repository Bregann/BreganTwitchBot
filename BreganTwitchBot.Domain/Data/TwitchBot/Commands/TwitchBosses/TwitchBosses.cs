using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using Serilog;
using TwitchLib.Client.Extensions;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.TwitchBosses
{
    public class TwitchBossDto
    {
        public string UserId { get; set; } = "";
        public string Username { get; set; } = "";
    }
    public class TwitchBosses
    {
        private static List<TwitchBossDto> _viewersJoined = new();
        private static List<string> _twitchModsAndVIPs = new();
        private static bool _bossCountdownEnabled;
        private static bool _bossInProgress;

        public static async Task StartBossFight()
        {
            try
            {
                //Check if enough people joined
                if (_viewersJoined.Count < 5)
                {
                    Log.Information("[Twitch Bosses] Not enough people joined the boss :(");
                    TwitchHelper.SendMessage("Not enough people joined the boss battle :( We need at least 5 to join!");
                    return;
                }

                _bossInProgress = true;

                TwitchBotConnection.Client.GetVIPs(AppConfig.BroadcasterName);
                TwitchBotConnection.Client.GetChannelModerators(AppConfig.BroadcasterName);

                TwitchBotConnection.Client.OnVIPsReceived += OnVIPsReceived;
                TwitchBotConnection.Client.OnModeratorsReceived += OnModeratorsReceived;

                await Task.Delay(5000);

                //Get the data needed
                var bossNames = _twitchModsAndVIPs;
                bossNames.Add(AppConfig.BotName); //incase theres not mods or vips detected
                bossNames.Add(AppConfig.BroadcasterName);

                var bossTitles = new List<string> { "The Terrible", "The Melvin", "The KEKW", "The Smelly", "The Gnoblin", "The Gnome", "The Troll", "The Handsome", "The Rank Beggar", "The Tommyinnit Fan", "The Tree", "The Super", "The Super Super", "The Duck" };
                var elimiationReasons = new List<string> { "got AGS'd for a 73", "got snowballed into the void", "missed their MLG", "missplaced a block", "got stamped on", "had a tree fall onto their head", "got destroyed by lasers" };

                var rnd = new Random();

                //Make the boss name ting
                var rndBossName = rnd.Next(0, bossNames.Count);
                var rndBossTitle = rnd.Next(0, bossTitles.Count);

                var bossName = $"{bossNames[rndBossName]} {bossTitles[rndBossTitle]}";
                TwitchHelper.SendMessage($"Oh no {bossName} has spawned monkaW the fight has begun");

                //Start the fight - stage 1
                await Task.Delay(5000);
                var randomUserToDie = rnd.Next(0, _viewersJoined.Count);
                TwitchHelper.SendMessage($"FeelsBadMan {_viewersJoined[randomUserToDie].Username} has fallen due to the strength of {bossName} :( They are the first player to die");
                _viewersJoined.RemoveAt(randomUserToDie);

                //Stage 2
                await Task.Delay(7500);

                //Amount of users to elimiate
                var amountOfUsersToEliminate = _viewersJoined.Count / 4 + 1;

                //The method of elimiation guinea8S
                TwitchHelper.SendMessage($"{bossName} has done their special attack and {amountOfUsersToEliminate} people {elimiationReasons[rnd.Next(0, elimiationReasons.Count)]}! The end of the fight is near monkaS");
                for (int i = 0; i < amountOfUsersToEliminate; i++)
                {
                    var rndRemoveUser = rnd.Next(0, _viewersJoined.Count);
                    _viewersJoined.RemoveAt(rndRemoveUser);
                }

                //The final stage
                await Task.Delay(10000);
                var rndRandomUserKEKW = rnd.Next(0, _viewersJoined.Count);
                var randomUserToBeKEKWd = _viewersJoined[rndRandomUserKEKW];

                TwitchHelper.SendMessage($"PogChamp {randomUserToBeKEKWd.Username} has done the final hit and destroyed the boss {bossName}!");
                await Task.Delay(5000);

                if (rnd.Next(0, 2) == 1)
                {
                    TwitchHelper.SendMessage($"but wait... the boss has just fallen ontop of {randomUserToBeKEKWd.Username} and they have died KEKW ! KEKW in the chat for {randomUserToBeKEKWd.Username}");
                    _viewersJoined.Remove(randomUserToBeKEKWd);
                }

                //the loot
                var rndPointAmount = rnd.Next(500, 2500);
                var amountToDistribute = (rndPointAmount * _viewersJoined.Count) + 7373;
                await Task.Delay(7000);

                foreach (var viewer in _viewersJoined)
                {
                    await PointsHelper.AddUserPoints(viewer.UserId, amountToDistribute);
                }

                TwitchHelper.SendMessage($"All {_viewersJoined.Count} people who survived the fight against {bossName} have won a grand total of {amountToDistribute:N0} {AppConfig.PointsName} each PogChamp");
                _viewersJoined.Clear();
                _bossCountdownEnabled = false;
                _bossInProgress = false;
            }
            catch (Exception e)
            {
                Log.Information($"[Twitch Bosses] {e}");
            }
        }

        private static void OnModeratorsReceived(object? sender, TwitchLib.Client.Events.OnModeratorsReceivedArgs e)
        {
            foreach (var mod in e.Moderators)
            {
                if (_twitchModsAndVIPs.Contains(mod))
                {
                    continue;
                }

                _twitchModsAndVIPs.Add(mod);
            }
        }

        private static void OnVIPsReceived(object? sender, TwitchLib.Client.Events.OnVIPsReceivedArgs e)
        {
            foreach (var vip in e.VIPs)
            {
                if (_twitchModsAndVIPs.Contains(vip))
                {
                    continue;
                }

                _twitchModsAndVIPs.Add(vip);
            }
        }

        public static void AddUser(string twitchUsername, string userId)
        {
            if (!_bossCountdownEnabled || _bossInProgress)
            {
                return;
            }

            if (_viewersJoined.Any(x => x.UserId == userId))
            {
                return;
            }

            _viewersJoined.Add(new TwitchBossDto { UserId = userId, Username = twitchUsername });
            Log.Information($"[Twitch Bosses] {twitchUsername} added to the boss list");
            return;
        }

        public static void StartBossFightCountdown()
        {
            _viewersJoined = new List<TwitchBossDto>();
            Log.Information("[Twitch Bosses] Twitch boss countdown started");
            HangfireJobs.StartTwitchBossFight();
            _bossCountdownEnabled = true;
            TwitchHelper.SendMessage("A Twitch Boss will be starting in 2 minutes! Type !boss to join PogChamp");
        }
    }
}
