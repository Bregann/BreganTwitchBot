using BreganTwitchBot.Domain.Services.Twitch.Commands.TwitchBosses;
using BreganTwitchBot.Domain.DTOs.Twitch.Commands.TwitchBosses;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.DomainTests.Helpers;
using Hangfire;
using Moq;

namespace BreganTwitchBot.DomainTests.Twitch.Commands
{
    [TestFixture]
    public class TwitchBossesTests
    {
        private Mock<ITwitchHelperService> _twitchHelperService;

        private TwitchBossesDataService _twitchBossesDataService;

        [SetUp]
        public void Setup()
        {
            _twitchHelperService = new Mock<ITwitchHelperService>();

            _twitchHelperService
                .Setup(x => x.SendTwitchMessageToChannel(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()
                ))
                .Returns(Task.CompletedTask);

            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            _twitchBossesDataService = new TwitchBossesDataService(_twitchHelperService.Object, mockBackgroundJobClient.Object);
        }

        [Test]
        public async Task StartBossFight_WhenBossStateIsNull_BossFightDoesntStart()
        {
            await _twitchBossesDataService.StartBossFight(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);

            Assert.Multiple(() =>
            {
                Assert.That(_twitchHelperService.Invocations, Is.Empty);
                Assert.That(_twitchBossesDataService._bossState, Is.Empty);
            });
        }

        [Test]
        public async Task StartBossFIght_WhenLessThan5Users_BossFightDoesntStart()
        {
            _twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId] = new BossState
            {
                BossCountdownEnabled = true,
                ViewersJoined = new List<(string, string)>
                {
                (DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName)
                },
                TwitchMods = new(),
                BossInProgress = false
            };

            await _twitchBossesDataService.StartBossFight(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);

            Assert.Multiple(() =>
            {
                Assert.That(_twitchHelperService.Invocations, Has.Count.EqualTo(1));
                Assert.That(_twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId].BossInProgress, Is.False);
            });
        }

        [Test]
        public async Task StartBossFight_WhenBossInProgress_BossFightDoesntStart()
        {
            _twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId] = new BossState
            {
                BossCountdownEnabled = true,
                ViewersJoined = new List<(string, string)>
                {
                    (DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName)
                },
                TwitchMods = new(),
                BossInProgress = true
            };

            await _twitchBossesDataService.StartBossFight(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);

            Assert.Multiple(() =>
            {
                Assert.That(_twitchHelperService.Invocations, Has.Count.EqualTo(0));
                Assert.That(_twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId].BossInProgress, Is.True);
            });
        }

        [Test]
        public async Task StartBossFight_WhenBossCountdownEnabled_BossFightStarts()
        {
            _twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId] = new BossState
            {
                BossCountdownEnabled = true,
                ViewersJoined = new List<(string, string)>
                {
                    (DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName),
                    (DatabaseSeedHelper.Channel2BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel2BroadcasterTwitchChannelName),
                    ("aaa", "bbb"),
                    ("aaaa", "bbbb"),
                    ("aaaaa", "bbbbb")
                },
                TwitchMods = new(),
                BossInProgress = false
            };
            await _twitchBossesDataService.StartBossFight(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName);
            Assert.Multiple(() =>
            {
                Assert.That(_twitchHelperService.Invocations, Has.Count.EqualTo(8));
                Assert.That(_twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId].BossInProgress, Is.False);
                Assert.That(_twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId].BossCountdownEnabled, Is.False);
                Assert.That(_twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId].ViewersJoined, Is.Empty);
            });
        }

        [Test]
        public void HandleBossCommand_WhenBossStateIsNull_CorrectMessageReturned()
        {
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!boss", "123", ["!boss"]);
            var response = _twitchBossesDataService.HandleBossCommand(msgParams);

            Assert.That(response, Is.EqualTo("There is no boss active!"));
        }

        [Test]
        public void HandleBossCommand_WhenBossInProgress_CorrectMessageReturned()
        {
            _twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId] = new BossState
            {
                BossCountdownEnabled = true,
                ViewersJoined = new List<(string, string)>
                {
                    (DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName)
                },
                TwitchMods = new(),
                BossInProgress = true
            };

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!boss", "123", ["!boss"]);
            var response = _twitchBossesDataService.HandleBossCommand(msgParams);

            Assert.That(response, Is.EqualTo("The boss countdown is not enabled or the boss is already in progress!"));
        }

        [Test]
        public void HandleBossCommand_WhenBossNoCountdownEnabled_CorrectMessageReturned()
        {
            _twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId] = new BossState
            {
                BossCountdownEnabled = false,
                ViewersJoined = new List<(string, string)>
                {
                    (DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName)
                },
                TwitchMods = new(),
                BossInProgress = false
            };
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!boss", "123", ["!boss"]);
            var response = _twitchBossesDataService.HandleBossCommand(msgParams);

            Assert.That(response, Is.EqualTo("The boss countdown is not enabled or the boss is already in progress!"));
        }

        [Test]
        public void HandleBossCommand_WhenBossCountdownEnabled_CorrectMessageReturned()
        {
            _twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId] = new BossState
            {
                BossCountdownEnabled = true,
                ViewersJoined = new List<(string, string)>
                {
                    (DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName)
                },
                TwitchMods = new(),
                BossInProgress = false
            };
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!boss", "123", ["!boss"]);
            var response = _twitchBossesDataService.HandleBossCommand(msgParams);

            Assert.Multiple(() =>
            {
                Assert.That(response, Is.EqualTo($"{msgParams.ChatterChannelName} has joined the boss fight! {_twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId].ViewersJoined.Count} people have joined the fight! You can join to by doing !boss"));
                Assert.That(_twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId].ViewersJoined, Has.Count.EqualTo(2));
            });
        }

        [Test]
        public void HandleBossCommand_WhenUserAlreadyInBossFight_CorrectMessageReturned()
        {
            _twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId] = new BossState
            {
                BossCountdownEnabled = true,
                ViewersJoined = new List<(string, string)>
                {
                    (DatabaseSeedHelper.Channel1User1TwitchUsername, DatabaseSeedHelper.Channel1User1TwitchUserId)
                },
                TwitchMods = new(),
                BossInProgress = false
            };

            var msgParams = MessageParamsHelper.CreateChatMessageParams("!boss", "123", ["!boss"]);
            var response = _twitchBossesDataService.HandleBossCommand(msgParams);
            Assert.That(response, Is.EqualTo($"You already joined the boss fight! Daft user!"));
        }

        [Test]
        public async Task StartBossFightCountdown_WhenUserIsNotSupermod_IsFalse()
        {
            _twitchHelperService
                .Setup(x => x.IsUserSuperModInChannel(
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(false);
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!boss", "123", ["!boss"]);
            var response = await _twitchBossesDataService.StartBossFightCountdown(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, msgParams);
            Assert.That(response, Is.False);
        }

        [Test]
        public async Task StartBossFightCountdown_WhenUserIsSupermodAndBossStateIsNull_IsTrue()
        {
            _twitchHelperService
                .Setup(x => x.IsUserSuperModInChannel(
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(true);
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!boss", "123", ["!boss"]);
            var response = await _twitchBossesDataService.StartBossFightCountdown(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, msgParams);
            Assert.That(response, Is.True);
        }

        [Test]
        public async Task StartBossFightCountdown_WhenUserIsSupermodAndBossStateIsNotNull_IsTrue()
        {
            _twitchHelperService
                .Setup(x => x.IsUserSuperModInChannel(
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(true);
            _twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId] = new BossState
            {
                BossCountdownEnabled = false,
                ViewersJoined = new List<(string, string)>
                {
                    (DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName)
                },
                TwitchMods = new(),
                BossInProgress = false
            };
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!boss", "123", ["!boss"]);
            var response = await _twitchBossesDataService.StartBossFightCountdown(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, msgParams);
            Assert.That(response, Is.True);
        }

        [Test]
        public async Task StartBossFightCountdown_WhenCountdownHasAlreadyStarted_IsFalse()
        {
            _twitchHelperService
                .Setup(x => x.IsUserSuperModInChannel(
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .ReturnsAsync(true);
            _twitchBossesDataService._bossState[DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId] = new BossState
            {
                BossCountdownEnabled = true,
                ViewersJoined = new List<(string, string)>
                {
                    (DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName)
                },
                TwitchMods = new(),
                BossInProgress = false
            };
            var msgParams = MessageParamsHelper.CreateChatMessageParams("!boss", "123", ["!boss"]);
            var response = await _twitchBossesDataService.StartBossFightCountdown(DatabaseSeedHelper.Channel1BroadcasterTwitchChannelId, DatabaseSeedHelper.Channel1BroadcasterTwitchChannelName, msgParams);
            Assert.That(response, Is.False);
        }
    }
}
